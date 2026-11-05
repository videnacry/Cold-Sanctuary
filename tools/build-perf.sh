#!/usr/bin/env bash
# Mide los FPS FUERA DEL EDITOR (docs/rendimiento.md). Dos partes:
#   tools/build-perf.sh build [final|dev]   construye solo SampleScene en Builds/Perf(Dev). Unity en batchmode: el editor
#                                           debe estar CERRADO y hace falta licencia. Con el editor abierto, usar el menú
#                                           Tools/Cold Sanctuary/Rendimiento/Build de medida.
#   tools/build-perf.sh run [final|dev]     lanza el ejecutable con -perf: a los 45 s mide con y sin lotes, escribe
#                                           [Rendimiento] en su log y se cierra. Ventana de 1280 × 720.
# Toda espera está acotada: la construcción a 2 h, la medida a 15 min.
set -uo pipefail
P="${COLD_SANCTUARY_UNITY_PROJECT:-C:/Users/Blein/COLD-SANCTUARY}"
KIND="${2:-final}"
DIR="$P/Builds/Perf"; METHOD=RendimientoPerfil.BuildFinal
[ "$KIND" = dev ] && { DIR="$P/Builds/PerfDev"; METHOD=RendimientoPerfil.BuildDevelopment; }
case "${1:-}" in
  build)
    VERSION="$(sed -n 's/^m_EditorVersion: //p' "$P/ProjectSettings/ProjectVersion.txt")"
    UNITY="/c/Program Files/Unity/Hub/Editor/$VERSION/Editor/Unity.exe"
    timeout 7200 "$UNITY" -batchmode -quit -projectPath "$P" -executeMethod "$METHOD" -logFile "$P/Logs/build-perf.log"
    code=$?
    grep -a '\[Rendimiento\] build' "$P/Logs/build-perf.log" | tail -1
    exit $code ;;
  run)
    timeout 900 "$DIR/ColdSanctuary.exe" -perf -screen-fullscreen 0 -screen-width 1280 -screen-height 720 -logFile "$DIR/perf.log"
    grep -a '\[Rendimiento\]' "$DIR/perf.log" | grep -a 'A/B\|FPS\|fase \|Debug.Log\|renderers que' | cut -c1-170 ;;
  *) echo "uso: tools/build-perf.sh build|run [final|dev]"; exit 2 ;;
esac
