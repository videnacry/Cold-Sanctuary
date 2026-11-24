#!/usr/bin/env python3
"""Índice, búsqueda y comprobación de la documentación de Cold Sanctuary.

Cada doc de docs/ (menos los registros de qa/runs y qa/migracion) empieza con una cabecera:

    ---
    resumen: Una frase que diga qué hay dentro y cuándo abrirlo.
    temas: [yoga, asanas]
    estado: vigente            # vigente | propuesta | historico | generado
    codigo: [AsanaPoseSpell, SunSalutation]   # clases que lo implementan (opcional)
    modelos: []                # .fbx / prefabs que usa (opcional)
    animaciones: []            # clips del Animator (opcional)
    retos: []                  # misiones, retos o casos de QA ligados (opcional)
    ---

USO (desde la raíz del repo):
    python3 tools/docs.py index           regenera docs/README.md (el índice; no editarlo a mano)
    python3 tools/docs.py check           falla si hay enlaces rotos, cabeceras que faltan o el índice no está al día
    python3 tools/docs.py find yoga       docs cuyo tema, título, resumen o clase coincide (mínimo contexto)
    python3 tools/docs.py de AnimaController   docs que citan esa clase en `codigo` (código → doc)
    python3 tools/docs.py temas           temas con su número de docs
"""
import os
import re
import sys

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
DOCS = os.path.join(ROOT, "docs")
INDEX = os.path.join(DOCS, "README.md")
SKIP_DIRS = ("qa/runs", "qa/migracion")          # registros: no llevan cabecera ni van al índice
NO_HEADER = {"README.md", "CODEMAP.md"}           # generados
ESTADOS = {"vigente", "propuesta", "historico", "generado"}
LISTAS = ("temas", "codigo", "modelos", "animaciones", "retos")

# Qué va en cada carpeta: lo que el índice enseña arriba de cada grupo.
CARPETAS = [
    ("", "Puertas de entrada", "Empezar aquí: mapa del código, tablero de tareas y pendientes."),
    ("reglas", "Reglas", "Reglas de diseño y de trabajo que no tienen excepciones (manual W/J/M, animales, personajes, plataformas)."),
    ("sistemas", "Sistemas", "Cómo funciona cada mecánica y su código: Anima, mente, emoción, hechizos, UI, cámara, jornada."),
    ("areas", "Áreas y edificios", "Cada área del santuario (mesocosmos): edificios, pisos, máquinas, medidas reales y su investigación."),
    ("fauna", "Fauna y ecología", "Especies, biología real y ecología de los santuarios."),
    ("microcosmos", "Microcosmos", "Los mundos a escala insecto: eras, historias, biomas, mapas y pueblos."),
    ("historia", "Historia y novela", "La narración, las historias de los personajes y los héroes por época."),
    ("personajes", "Personajes", "Una ficha por personaje (comprobadas por tools/check-personajes.sh)."),
    ("qa", "Pruebas y QA", "Casos con ID, registro, hallazgos (Hn), bugs (B-nn), checklists de pruebas en Unity."),
    ("conocimiento", "Conocimiento (lo que ya nos mordió)", "Lecciones de Unity, del editor, del PC y de las ramas: leer ANTES de reintentar algo que falló."),
    ("plataforma", "Plataforma", "Unity, modelos 3D, builds web, visor y rendimiento."),
    ("codeship", "CodeShip", "Datos compartidos con CodeShip."),
    ("historial", "Historial", "Auditorías y documentos superados: solo para contexto, no describen el código de hoy."),
]

LINK = re.compile(r"\]\(([^)\s]+)\)")


def rel(p):
    return os.path.relpath(p, ROOT).replace(os.sep, "/")


def all_md():
    out = []
    for d, _, files in os.walk(DOCS):
        r = rel(d)
        for f in files:
            if f.endswith(".md"):
                out.append(os.path.join(d, f))
    return sorted(out)


def indexed(path):
    r = rel(path)[len("docs/"):]
    return not r.startswith(SKIP_DIRS) and os.path.basename(path) not in NO_HEADER


def read(path):
    with open(path, encoding="utf-8") as fh:
        return fh.read()


def parse(text):
    """Devuelve (cabecera dict o None, cuerpo)."""
    if not text.startswith("---\n"):
        return None, text
    end = text.find("\n---\n", 4)
    if end < 0:
        return None, text
    meta = {}
    for line in text[4:end].splitlines():
        if not line.strip() or ":" not in line:
            continue
        k, v = line.split(":", 1)
        k, v = k.strip(), v.strip()
        if not v.startswith(("'", '"')):
            v = v.split(" #", 1)[0].rstrip()   # comentario solo fuera de comillas
        if k in LISTAS:
            v = v.strip("[]")
            meta[k] = [x.strip().strip("'\"") for x in v.split(",") if x.strip()]
        else:
            meta[k] = v.strip("'\"")
    return meta, text[end + 5:]


def title(body, path):
    m = re.search(r"^# (.+)$", body, re.M)
    return m.group(1).strip() if m else os.path.basename(path)[:-3]


def load():
    docs = []
    for p in all_md():
        if not indexed(p):
            continue
        meta, body = parse(read(p))
        docs.append({"path": p, "rel": rel(p), "meta": meta, "title": title(body, p)})
    return docs


def folder_of(d):
    r = d["rel"][len("docs/"):]
    return r.split("/")[0] if "/" in r else ""


def build_index(docs):
    out = [
        "# Índice de la documentación",
        "",
        "> **GENERADO** por `python3 tools/docs.py index` desde la cabecera de cada doc: no editar a mano.",
        "> Buscar sin abrir nada: `python3 tools/docs.py find <tema>` · código→doc: `python3 tools/docs.py de <Clase>`.",
        "> Antes de reintentar algo que falló: [conocimiento/](conocimiento/README.md) y [qa/hallazgos.md](qa/hallazgos.md).",
        "",
    ]
    by = {}
    for d in docs:
        by.setdefault(folder_of(d), []).append(d)
    known = [c[0] for c in CARPETAS]
    order = CARPETAS + [(f, f, "") for f in sorted(by) if f not in known]
    for key, name, desc in order:
        group = by.get(key)
        if not group:
            continue
        out += [f"## {name}" + (f" (`{key}/`)" if key else ""), ""]
        if desc:
            out += [desc, ""]
        for d in sorted(group, key=lambda x: x["rel"]):
            m = d["meta"] or {}
            link = d["rel"][len("docs/"):]
            extra = []
            if m.get("estado") and m["estado"] != "vigente":
                extra.append(m["estado"])
            if m.get("temas"):
                extra.append(", ".join(m["temas"]))
            tail = f" _({' · '.join(extra)})_" if extra else ""
            out.append(f"- [{d['title']}]({link}) — {m.get('resumen', '')}{tail}")
        out.append("")
    return "\n".join(out).rstrip() + "\n"


def broken_links():
    bad = []
    roots = [os.path.join(ROOT, f) for f in ("CLAUDE.md", "CHANGELOG.md", "DEVLOG.md") if os.path.exists(os.path.join(ROOT, f))]
    for p in roots + all_md():
        text = read(p)
        text = re.sub(r"```.*?```", "", text, flags=re.S)
        text = re.sub(r"`[^`\n]*`", "", text)
        for t in LINK.findall(text):
            if re.match(r"^[a-z]+:", t) or t.startswith("#") or t.startswith("<"):
                continue
            target = t.split("#", 1)[0].split("?", 1)[0]
            if not target:
                continue
            full = os.path.normpath(os.path.join(os.path.dirname(p), target))
            if not os.path.exists(full):
                bad.append((rel(p), t))
    return bad


def classes():
    path = os.path.join(DOCS, "CODEMAP.md")
    return set(re.findall(r"^- \*\*(\w+)\*\*", read(path), re.M)) if os.path.exists(path) else set()


def cmd_check():
    docs = load()
    errors, warns = [], []
    for d in docs:
        m = d["meta"]
        if m is None:
            errors.append(f"{d['rel']}: falta la cabecera (--- resumen/temas/estado ---)")
            continue
        if not m.get("resumen"):
            errors.append(f"{d['rel']}: la cabecera no tiene resumen")
        if m.get("estado") not in ESTADOS:
            errors.append(f"{d['rel']}: estado «{m.get('estado')}» (vale: {', '.join(sorted(ESTADOS))})")
    known = classes()
    for d in docs:
        for c in (d["meta"] or {}).get("codigo", []):
            if known and c not in known:
                warns.append(f"{d['rel']}: la clase `{c}` no está en CODEMAP")
    for src, t in broken_links():
        if src.startswith(tuple("docs/" + s for s in SKIP_DIRS)):
            warns.append(f"{src}: enlace roto {t}")
        else:
            errors.append(f"{src}: enlace roto {t}")
    if os.path.exists(INDEX) and read(INDEX) != build_index(docs):
        errors.append("docs/README.md no está al día: corre `python3 tools/docs.py index`")
    for w in warns:
        print("aviso:", w)
    for e in errors:
        print("ERROR:", e)
    print(f"{len(docs)} docs, {len(errors)} errores, {len(warns)} avisos.")
    return 1 if errors else 0


def cmd_find(words):
    q = [w.lower() for w in words]
    for d in load():
        m = d["meta"] or {}
        hay = " ".join([d["rel"], d["title"], m.get("resumen", "")] + m.get("temas", []) + m.get("codigo", [])).lower()
        if all(w in hay for w in q):
            print(f"{d['rel']} — {m.get('resumen', d['title'])}")


def cmd_de(cls):
    for d in load():
        if cls in (d["meta"] or {}).get("codigo", []):
            print(f"{d['rel']} — {(d['meta'] or {}).get('resumen', '')}")


def cmd_temas():
    count = {}
    for d in load():
        for t in (d["meta"] or {}).get("temas", []):
            count[t] = count.get(t, 0) + 1
    for t, n in sorted(count.items(), key=lambda x: (-x[1], x[0])):
        print(f"{n:4d}  {t}")


def main(argv):
    if not argv or argv[0] in ("-h", "--help"):
        print(__doc__)
        return 0
    cmd, rest = argv[0], argv[1:]
    if cmd == "index":
        with open(INDEX, "w", encoding="utf-8", newline="\n") as fh:
            fh.write(build_index(load()))
        print("docs/README.md regenerado")
        return 0
    if cmd == "check":
        return cmd_check()
    if cmd == "find" and rest:
        cmd_find(rest)
        return 0
    if cmd == "de" and rest:
        cmd_de(rest[0])
        return 0
    if cmd == "temas":
        cmd_temas()
        return 0
    print(__doc__)
    return 2


if __name__ == "__main__":
    sys.exit(main(sys.argv[1:]))
