#!/usr/bin/env bash
# EL COMPROBADOR DE QA SIN UNITY (docs/qa/README.md; Assets/Scripts/QaCheck.cs, lógica pura). Compila QaCheck.cs con el dotnet que trae el
# Editor de Unity (sin licencia ni proyecto abierto) y lo corre sobre el repositorio: «[QA] OK … 0 problemas» o la lista de lo que no cuadra.
# USO: tools/qa-check.sh [ruta-del-repo]   (por defecto, la raíz de este repo). Devuelve 0 si cuadra, 1 si hay problemas.
set -uo pipefail
REPO="$(cd "${1:-$(dirname "$0")/..}" && pwd)"
E="$(ls -d "/c/Program Files/Unity/Hub/Editor/"*/Editor/Data/DotNetSdk 2>/dev/null | sort | tail -1)"
RT="$(ls -d "$E"/shared/Microsoft.NETCore.App/* | sort | tail -1)"
CSC="$(ls -d "$E"/sdk/*/Roslyn/bincore/csc.dll | head -1)"
[ -n "$E" ] && [ -n "$CSC" ] || { echo "ERROR: no encuentro el dotnet del Editor de Unity." >&2; exit 2; }
W="$(mktemp -d)"; trap 'rm -rf "$W"' EXIT
echo 'public static class QaMain { public static int Main(string[] a) => QaCheck.Cli(a, System.Console.Out); }' > "$W/Main.cs"
: > "$W/refs.rsp"
while IFS= read -r d; do echo "-r:\"$(cygpath -m "$d")\"" >> "$W/refs.rsp"; done < <(ls "$RT"/System*.dll "$RT"/netstandard.dll "$RT"/mscorlib.dll | grep -v Native)
"$E/dotnet.exe" "$(cygpath -m "$CSC")" -nologo -target:exe -out:"$(cygpath -m "$W/qa.dll")" "@$(cygpath -m "$W/refs.rsp")" "$(cygpath -m "$W/Main.cs")" "$(cygpath -m "$REPO/Assets/Scripts/QaCheck.cs")" | grep -E "error" && exit 2
printf '{ "runtimeOptions": { "tfm": "net8.0", "framework": { "name": "Microsoft.NETCore.App", "version": "%s" } } }\n' "$(basename "$RT")" > "$W/qa.runtimeconfig.json"
"$E/dotnet.exe" exec "$(cygpath -m "$W/qa.dll")" --root "$(cygpath -m "$REPO")"
