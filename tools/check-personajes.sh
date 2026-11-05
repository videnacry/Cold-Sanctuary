#!/usr/bin/env bash
# Comprueba las reglas de personajes sin abrir Unity (docs/reglas-personajes.md, P2 y P7):
#   · cada persona del mesocosmos tiene su ficha docs/personajes/<nombre>.md;
#   · cada entrada de Professions.All tiene su base en Professions.Basis.
# USO: tools/check-personajes.sh   (desde cualquier carpeta). Devuelve 1 si falta algo.
set -uo pipefail
cd "$(dirname "$0")/.." || exit 2
WD=Assets/Scripts/Progression/WorkdayDirector.cs
PR=Assets/Scripts/Progression/Professions.cs

# El personal: de «Staff = {» hasta «};». Quita los comentarios antes de sacar los nombres.
staff="$(sed -n '/public static readonly string\[\] Staff = {/,/};/p' "$WD" | sed 's|//.*||' | grep -o '"[A-Za-z]*"' | tr -d '"')"
fail=0
n=0
for who in $staff Magnate Neo; do
  n=$((n + 1))
  case "$who" in
    Player) doc=kushal ;;
    JefeConstruccion) doc=tote ;;
    *) doc="$(echo "$who" | tr '[:upper:]' '[:lower:]')" ;;
  esac
  if [ ! -f "docs/personajes/$doc.md" ]; then echo "FALTA la ficha de $who: docs/personajes/$doc.md"; fail=1; fi
done

entries="$(grep -o 'new Entry("[A-Za-z]*"' "$PR" | sed 's/new Entry("//; s/"//')"
m=0
for who in $entries; do
  m=$((m + 1))
  if ! sed -n '/Dictionary<string, string> Basis/,/};/p' "$PR" | grep -q "{ \"$who\", \""; then echo "FALTA la base del puesto de $who en Professions.Basis"; fail=1; fi
done

echo "$n personas y $m puestos revisados."
[ "$fail" -eq 0 ] && echo "RESULTADO: PASS" || echo "RESULTADO: FAIL"
exit "$fail"
