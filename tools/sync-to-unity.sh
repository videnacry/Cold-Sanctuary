#!/usr/bin/env bash
# Sincroniza los `.cs` de este repo git al PROYECTO UNITY, que es una carpeta APARTE.
#
# POR QUÉ: el repo solo versiona `.cs`+docs (ver CLAUDE.md). Las escenas, prefabs, `.meta`,
# `ProjectSettings/` y `Packages/manifest.json` viven solo en el proyecto Unity. Son dos copias
# distintas del código, y si no se sincronizan, el Editor compila código viejo: en 2026-10-11 el
# proyecto Unity estaba 57 archivos por detrás del repo (un mes de PRs sin verificar).
#
# USO:  tools/sync-to-unity.sh [--prune] [ruta-del-proyecto-unity]
#   sin --prune : copia nuevos/modificados y solo AVISA de los `.cs` que sobran en Unity.
#   con --prune : además BORRA esos `.cs` sobrantes (y su `.meta`). Úsalo cuando el repo haya
#                 borrado/renombrado archivos: un `.cs` obsoleto que define una clase retirada
#                 rompe la compilación (p. ej. `SimpleAnima.cs`/`YogaPortal.cs` en 2026-10-11).
# Por defecto usa $COLD_SANCTUARY_UNITY_PROJECT, y si no, C:/Users/Blein/COLD-SANCTUARY.
#
# Después de sincronizar: `tools/verify-compile.sh` comprueba que compila.

set -uo pipefail

PRUNE=0
if [ "${1:-}" = "--prune" ]; then PRUNE=1; shift; fi
PROJECT="${1:-${COLD_SANCTUARY_UNITY_PROJECT:-C:/Users/Blein/COLD-SANCTUARY}}"
REPO="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"

if [ ! -d "$PROJECT/Assets" ]; then
  echo "ERROR: '$PROJECT' no parece un proyecto Unity (no hay Assets/)." >&2
  exit 2
fi

# `.cs` que el proyecto Unity puede tener legítimamente sin estar en git: la plantilla de Unity y
# los scripts de trabajo local que nunca se comitearon.
is_whitelisted() {
  case "$1" in
    Assets/TutorialInfo/*) return 0 ;;
    *) return 1 ;;
  esac
}

cd "$REPO" || exit 2
new=0; upd=0
while read -r f; do
  dst="$PROJECT/$f"
  if [ ! -f "$dst" ]; then
    mkdir -p "$(dirname "$dst")"; cp "$f" "$dst"; new=$((new+1)); echo "  + $f"
  elif ! cmp -s "$f" "$dst"; then
    cp "$f" "$dst"; upd=$((upd+1)); echo "  ~ $f"
  fi
done < <(find Assets -name "*.cs" | sort)

echo "Sincronizados: $new nuevos, $upd actualizados  ->  $PROJECT"

# `.cs` que están en Unity pero ya no en el repo.
extra=0
while read -r f; do
  rel="${f#$PROJECT/}"
  [ -f "$REPO/$rel" ] && continue
  is_whitelisted "$rel" && continue
  extra=$((extra+1))
  if [ "$PRUNE" -eq 1 ]; then
    rm -f "$f" "$f.meta"; echo "  - BORRADO $rel (+ .meta)"
  else
    echo "  ! SOBRA en Unity (no esta en git): $rel"
  fi
done < <(find "$PROJECT/Assets" -name "*.cs" | sort)

if [ "$extra" -gt 0 ] && [ "$PRUNE" -eq 0 ]; then
  echo "Hay $extra .cs que sobran. Si el repo los borro/renombro, re-ejecuta con --prune"
  echo "(un .cs obsoleto que define una clase retirada ROMPE la compilacion)."
fi
