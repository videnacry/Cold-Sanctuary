#!/usr/bin/env python3
"""Genera docs/CODEMAP.md — un índice de TODAS las clases/enums/structs/interfaces bajo Assets/, con su resumen
(la primera frase del /// XML-doc), agrupado por carpeta de primer nivel. Objetivo: que el contexto del proyecto
esté disponible de un vistazo (sin tener que "redescubrir" leyendo archivos). Un CI lo regenera y verifica en cada PR.

Uso:  python3 tools/gen_codemap.py       (reescribe docs/CODEMAP.md)
"""
import os, re

ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
ASSETS = os.path.join(ROOT, "Assets")

DECL = re.compile(
    r'^\s*(?:\[[^\]]*\]\s*)*'                                   # atributos sueltos
    r'(?:public|internal|protected|private)?\s*'
    r'(?:static\s+|abstract\s+|sealed\s+|partial\s+|readonly\s+)*'
    r'(class|struct|interface|enum)\s+([A-Za-z0-9_]+)')

def summary_above(lines, i):
    """Reúne el bloque /// contiguo por encima de la línea i; devuelve la 1ª frase (sin tags XML)."""
    buf = []
    j = i - 1
    while j >= 0:
        s = lines[j].strip()
        if s.startswith("///"):
            buf.append(re.sub(r'<[^>]+>', '', s.lstrip('/').strip()))
        elif s == "" or s.startswith("["):
            pass
        else:
            break
        j -= 1
    text = " ".join(reversed([b for b in buf if b])).strip()
    text = re.sub(r'\s+', ' ', text)
    # primera frase / recorte
    m = re.split(r'(?<=[.:])\s', text, maxsplit=1)
    text = (m[0] if m else text).strip()
    return text[:200]

def main():
    groups = {}
    for dirpath, _, files in os.walk(ASSETS):
        for fn in sorted(files):
            if not fn.endswith(".cs"):
                continue
            path = os.path.join(dirpath, fn)
            rel = os.path.relpath(path, ROOT)
            top = os.path.relpath(dirpath, ASSETS).split(os.sep)[0]
            try:
                lines = open(path, encoding="utf-8", errors="replace").read().splitlines()
            except OSError:
                continue
            for i, ln in enumerate(lines):
                m = DECL.match(ln)
                if m:
                    kind, name = m.group(1), m.group(2)
                    groups.setdefault(top, []).append((name, kind, rel, summary_above(lines, i)))
                    break   # solo el primer tipo de primer nivel por archivo

    md = ["# CODEMAP — índice de clases del proyecto (GENERADO — no editar a mano)",
          "",
          "> Regenerar con `python3 tools/gen_codemap.py`. Un CI (`.github/workflows/codemap.yml`) lo verifica en cada PR.",
          "> Es el índice a nivel de CLASE (complementa la tabla de sistemas de `CLAUDE.md` y el índice de `docs/`).",
          ""]
    total = 0
    for top in sorted(groups):
        md.append(f"## {top}")
        md.append("")
        for name, kind, rel, summ in sorted(groups[top], key=lambda t: t[0].lower()):
            total += 1
            md.append(f"- **{name}** ({kind}) — `{rel}`" + (f" — {summ}" if summ else ""))
        md.append("")
    md.insert(4, f"_Total: {total} tipos._")
    md.insert(5, "")

    with open(os.path.join(ROOT, "docs", "CODEMAP.md"), "w", encoding="utf-8") as f:
        f.write("\n".join(md) + "\n")
    print(f"docs/CODEMAP.md generado — {total} tipos.")

if __name__ == "__main__":
    main()
