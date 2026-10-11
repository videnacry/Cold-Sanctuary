#!/usr/bin/env bash
# Lanza la PRUEBA EN VIVO (blockout + Play + salir) en el proyecto Unity, dejando el entorno limpio
# antes de arrancar. Envuelve `HeadlessLive.RunLive` (Assets/Editor/HeadlessLive.cs).
#
# POR QUÉ EXISTE LA LIMPIEZA: dos trampas que colgaron sendas corridas el 2026-10-11, ambas por
# restos de una ejecución anterior:
#   · `Temp/__Backupscenes/*.backup` → al abrir, Unity muestra el modal **"Recovering Scene Backups"**
#     y se queda ahí esperando un clic humano PARA SIEMPRE (el log se corta justo tras licenciar, con
#     el proceso a ~0% de CPU). Pasa siempre que se mató un Editor con cambios de escena respaldados.
#   · `Temp/UnityLockfile` retenido por un Unity zombi → ningún Editor nuevo puede abrir el proyecto.
# Los backups son desechables: `HeadlessLive` guarda la escena, y el blockout lo regenera el builder.
#
# USO:  tools/run-live-test.sh [segundos] [ruta-del-informe]
#   segundos : cuánto dejar correr el Play (por defecto 180)
#   informe  : fichero con flush por línea donde salen [LIVE]/[TEST]/[ECO] y los errores
#              (por defecto ./live-report.log)
# Proyecto Unity: $COLD_SANCTUARY_UNITY_PROJECT, o C:/Users/Blein/COLD-SANCTUARY.
#
# Sincroniza primero (`tools/sync-to-unity.sh`) y comprueba que compila (`tools/verify-compile.sh`):
# si los scripts no compilan, Unity NO invoca el executeMethod y la corrida no arranca.

set -uo pipefail

SECONDS_ARG="${1:-180}"
REPORT="${2:-$PWD/live-report.log}"
PROJECT="${COLD_SANCTUARY_UNITY_PROJECT:-C:/Users/Blein/COLD-SANCTUARY}"

if [ ! -d "$PROJECT/Assets" ]; then
  echo "ERROR: '$PROJECT' no parece un proyecto Unity (no hay Assets/)." >&2
  exit 2
fi

VERSION="$(sed -n 's/^m_EditorVersion: //p' "$PROJECT/ProjectSettings/ProjectVersion.txt" 2>/dev/null)"
UNITY="/c/Program Files/Unity/Hub/Editor/$VERSION/Editor/Unity.exe"
if [ ! -x "$UNITY" ]; then
  echo "ERROR: no encuentro Unity $VERSION en $UNITY." >&2
  exit 2
fi

# ── Limpieza: Editor colgado + lockfile + backups de escena ─────────────────────────────────────
echo "Limpiando restos de corridas anteriores..."
powershell -NoProfile -Command "
  \$procs = Get-CimInstance Win32_Process -Filter \"Name='Unity.exe'\" |
            Where-Object { \$_.CommandLine -like '*$(basename "$PROJECT")*' }
  foreach (\$p in \$procs) {
    Write-Output ('  matando Unity PID ' + \$p.ProcessId + ' (resto de otra corrida)')
    Stop-Process -Id \$p.ProcessId -Force -ErrorAction SilentlyContinue
  }
  Start-Sleep -Milliseconds 1500
" 2>/dev/null

rm -f  "$PROJECT/Temp/UnityLockfile"
rm -rf "$PROJECT/Temp/__Backupscenes"
echo "  lockfile y __Backupscenes retirados (evita el modal 'Recovering Scene Backups')"

# Las EditorPrefs de la máquina de estados se borran al terminar, pero si la corrida anterior murió a
# medias quedan sucias y la siguiente arrancaría creyendo estar en mitad de un Play.
powershell -NoProfile -Command "
  \$k = 'HKCU:\Software\Unity Technologies\Unity Editor 5.x'
  if (Test-Path \$k) {
    (Get-Item \$k).GetValueNames() | Where-Object { \$_ -like 'ColdSanctuary_HeadlessLive*' } | ForEach-Object {
      Remove-ItemProperty -Path \$k -Name \$_ -Force -ErrorAction SilentlyContinue
      Write-Output ('  EditorPref limpiada: ' + \$_)
    }
  }
" 2>/dev/null

# ── Lanzar ──────────────────────────────────────────────────────────────────────────────────────
REPORT_WIN="$(cygpath -w "$REPORT" 2>/dev/null || echo "$REPORT")"
EDITOR_LOG="${REPORT%.log}-editor.log"
EDITOR_LOG_WIN="$(cygpath -w "$EDITOR_LOG" 2>/dev/null || echo "$EDITOR_LOG")"
rm -f "$REPORT" "$EDITOR_LOG"

echo "Lanzando Unity $VERSION (GUI, nunca -batchmode) — Play de ${SECONDS_ARG}s"
echo "  informe:    $REPORT"
echo "  Editor.log: $EDITOR_LOG"
"$UNITY" -projectPath "$(cygpath -w "$PROJECT" 2>/dev/null || echo "$PROJECT")" \
         -logFile "$EDITOR_LOG_WIN" \
         -executeMethod HeadlessLive.RunLive \
         -liveSeconds "$SECONDS_ARG" \
         -liveReport "$REPORT_WIN"
rc=$?

echo "Unity salio con codigo $rc"
if [ -f "$REPORT" ]; then
  echo "──────── informe ────────"
  cat "$REPORT"
else
  echo "AVISO: no se genero el informe. Si el Editor.log se corta justo tras licenciar y el proceso"
  echo "       esta a ~0% de CPU, lo mas probable es un MODAL esperando un clic: comprueba el titulo"
  echo "       de la ventana con:  Get-Process Unity | Select MainWindowTitle"
fi
exit "$rc"
