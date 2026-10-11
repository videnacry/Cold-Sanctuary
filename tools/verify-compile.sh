#!/usr/bin/env bash
# Verifica que TODO el código del proyecto COMPILA, sin abrir el Editor y SIN NECESITAR LICENCIA
# de Unity. Existe porque:
#   · el bridge MCP de Unity no conecta de forma fiable (5 ciclos de checklist sin poder compilar), y
#   · `Unity.exe -batchmode` exige una licencia válida (si caducó: return code 198,
#     "No valid Unity Editor license found") — esta vía no la necesita.
#
# CÓMO FUNCIONA: Unity (Bee) deja en `Library/Bee/artifacts/*.dag/` los ficheros de respuesta
# (`Assembly-CSharp.rsp`, `Assembly-CSharp-Editor.rsp`) con la línea de compilación EXACTA que usó:
# los ~366 `-r:` a los DLL de Unity y de los paquetes, los `-define:` y los analizadores. El script
# reaprovecha esos .rsp, les sustituye la lista de fuentes por los `.cs` que hay AHORA en disco, y
# llama al Roslyn que viene con el Editor. Resultado: los mismos `error CS` que daría el Editor.
#
# REQUISITO: que Unity haya compilado el proyecto al menos una vez (para que existan los .rsp).
# Si cambian los paquetes de `Packages/manifest.json`, hay que abrir Unity una vez para
# regenerar los .rsp; mientras tanto este script compila contra el juego de referencias anterior.
#
# USO:  tools/verify-compile.sh [ruta-del-proyecto-unity]
# Por defecto usa $COLD_SANCTUARY_UNITY_PROJECT, y si no, C:/Users/Blein/COLD-SANCTUARY.
# El repo git solo versiona `.cs`+docs, así que el proyecto Unity (con escenas/prefabs/Packages)
# es una carpeta APARTE: hay que sincronizar los `.cs` allí antes de compilar (ver tools/sync-to-unity.sh).
#
# SALIDA: lista los `error CS` y devuelve 1 si hay alguno; 0 si todo compila.

set -uo pipefail

PROJECT="${1:-${COLD_SANCTUARY_UNITY_PROJECT:-C:/Users/Blein/COLD-SANCTUARY}}"
WORK="$(mktemp -d)"
trap 'rm -rf "$WORK"' EXIT

if [ ! -d "$PROJECT/Assets" ]; then
  echo "ERROR: '$PROJECT' no parece un proyecto Unity (no hay Assets/)." >&2
  exit 2
fi

# ── Editor que corresponde al proyecto ──────────────────────────────────────────
VERSION="$(sed -n 's/^m_EditorVersion: //p' "$PROJECT/ProjectSettings/ProjectVersion.txt" 2>/dev/null)"
if [ -z "$VERSION" ] || [ "$VERSION" = "UnknownUnityVersion" ]; then
  echo "ERROR: no se pudo leer la version de Unity en ProjectSettings/ProjectVersion.txt." >&2
  exit 2
fi
EDITOR_DIR="/c/Program Files/Unity/Hub/Editor/$VERSION/Editor"
DOTNET="$EDITOR_DIR/Data/DotNetSdk/dotnet.exe"
CSC="$(ls -d "$EDITOR_DIR"/Data/DotNetSdk/sdk/*/Roslyn/bincore/csc.dll 2>/dev/null | head -1)"
if [ ! -x "$DOTNET" ] || [ -z "$CSC" ]; then
  echo "ERROR: no encuentro el compilador de Unity $VERSION (dotnet/csc) en $EDITOR_DIR." >&2
  exit 2
fi
# csc.dll se pasa a dotnet.exe como ruta Windows
CSC_WIN="$(cygpath -m "$CSC" 2>/dev/null || echo "$CSC")"

# ── .rsp de Bee más recientes ───────────────────────────────────────────────────
DAG="$(ls -dt "$PROJECT"/Library/Bee/artifacts/*.dag 2>/dev/null | head -1)"
if [ -z "$DAG" ] || [ ! -f "$DAG/Assembly-CSharp.rsp" ]; then
  echo "ERROR: no hay .rsp de Bee en $PROJECT/Library/Bee/artifacts/*.dag/." >&2
  echo "       Abre el proyecto en Unity una vez para generarlos y vuelve a intentarlo." >&2
  exit 2
fi

OUT_WIN="$(cygpath -m "$WORK" 2>/dev/null || echo "$WORK")"
cd "$PROJECT" || exit 2

# Partición por defecto de Unity: todo `.cs` bajo una carpeta llamada Editor va al
# ensamblado de editor; el resto, al de runtime.
find Assets -name "*.cs" -path "*/Editor/*" | sort | awk '{print "\""$0"\""}' > "$WORK/src_editor.txt"
find Assets -name "*.cs" ! -path "*/Editor/*" | sort | awk '{print "\""$0"\""}' > "$WORK/src_runtime.txt"
echo "Compilando $(wc -l < "$WORK/src_runtime.txt") fuentes de runtime y $(wc -l < "$WORK/src_editor.txt") de editor"
echo "  proyecto: $PROJECT"
echo "  Unity:    $VERSION"

# Reusa el .rsp cambiando solo las salidas y la lista de fuentes.
make_rsp() {  # $1=rsp origen  $2=rsp destino  $3=nombre del ensamblado  $4=lista de fuentes
  grep -v '^"Assets' "$1" \
    | sed "s|^-out:.*|-out:\"$OUT_WIN/$3.dll\"|" \
    | sed "s|^-refout:.*|-refout:\"$OUT_WIN/$3.ref.dll\"|" \
    | sed "s|^-r:\"Library/Bee/artifacts/[^/]*\.dag/Assembly-CSharp\.ref\.dll\"|-r:\"$OUT_WIN/Assembly-CSharp.ref.dll\"|" \
    > "$2"
  cat "$4" >> "$2"
}

make_rsp "$DAG/Assembly-CSharp.rsp"        "$WORK/rt.rsp" "Assembly-CSharp"        "$WORK/src_runtime.txt"
make_rsp "$DAG/Assembly-CSharp-Editor.rsp" "$WORK/ed.rsp" "Assembly-CSharp-Editor" "$WORK/src_editor.txt"

status=0
for pair in "runtime:$WORK/rt.rsp" "editor:$WORK/ed.rsp"; do
  name="${pair%%:*}"; rsp="${pair#*:}"
  echo "── $name ──────────────────────────────────────────"
  # El ensamblado de editor referencia al de runtime: si runtime no compiló, aquí solo saldría un
  # CS0006 ("Metadata file ... could not be found") que despista. Se salta.
  if [ "$name" = "editor" ] && [ "$status" -ne 0 ]; then
    echo "OMITIDO: runtime no compila, asi que el editor no puede referenciarlo. Arregla runtime primero."
    continue
  fi
  "$DOTNET" "$CSC_WIN" "@$rsp" > "$WORK/$name.out" 2>&1
  n_err="$(grep -c 'error CS' "$WORK/$name.out" || true)"
  n_warn="$(grep -c 'warning CS' "$WORK/$name.out" || true)"
  if [ "$n_err" -gt 0 ]; then
    status=1
    echo "FAIL: $n_err error(es) de compilacion:"
    grep 'error CS' "$WORK/$name.out"
  else
    echo "OK: compila (0 errores, $n_warn warnings)"
  fi
done

echo "───────────────────────────────────────────────────"
if [ "$status" -eq 0 ]; then echo "RESULTADO: PASS — todo el codigo compila."
else echo "RESULTADO: FAIL — hay errores de compilacion (arriba)."; fi
exit "$status"
