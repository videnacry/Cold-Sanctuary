# construir-web.ps1 - CONSTRUIR LA WEB CONTINUA SIN ABRIR EL EDITOR (2026-11-16).
#
# Corre Unity en modo batch: WebLigeraBuild.BuildContinuaCI (el santuario, los grupos de microcosmos y las paginas, en Builds\Web\sitio), y al
# terminar deja el proyecto en StandaloneWindows64. Con el EDITOR CERRADO (un proyecto no se abre dos veces); con el editor abierto, usa el
# menu Tools > Cold Sanctuary > Web > Web continua: construir todo.
# Antes: sincroniza el codigo de la rama al proyecto (tools\sync-to-unity.sh) y genera las escenas que hayan cambiado.
#
# Uso: powershell -ExecutionPolicy Bypass -File tools\web\construir-web.ps1 [-Horas 8] [-Compresion gzip|ninguna]
# La espera esta acotada (-Horas); el registro, en Builds\Web\construir.log.

param(
    [string]$Proyecto = "C:\Users\Blein\COLD-SANCTUARY",
    [string]$Unity = "",
    [double]$Horas = 8,
    [ValidateSet("gzip", "ninguna")][string]$Compresion = "gzip"   # gzip con descompresion de reserva (por defecto) o sin comprimir
)

$ErrorActionPreference = "Stop"
if (-not $Unity) {
    $ver = ((Get-Content (Join-Path $Proyecto "ProjectSettings\ProjectVersion.txt")) | Where-Object { $_ -like "m_EditorVersion:*" }) -replace "m_EditorVersion:\s*", ""
    $Unity = "C:\Program Files\Unity\Hub\Editor\$ver\Editor\Unity.exe"
}
if (-not (Test-Path $Unity)) { Write-Error "No encuentro Unity.exe ($Unity)."; exit 2 }
if (Test-Path (Join-Path $Proyecto "Temp\UnityLockfile")) {
    try { [IO.File]::Open((Join-Path $Proyecto "Temp\UnityLockfile"), "Open", "ReadWrite", "None").Close() }
    catch { Write-Error "El editor tiene el proyecto abierto: cierralo, o usa el menu Web continua: construir todo."; exit 2 }
}
$log = Join-Path $Proyecto "Builds\Web\construir.log"
New-Item -ItemType Directory -Force (Split-Path $log) | Out-Null
Write-Host "Construyendo la web continua ($Compresion, hasta $Horas h). Registro: $log"
$p = Start-Process -FilePath $Unity -ArgumentList @("-batchmode", "-quit", "-projectPath", "`"$Proyecto`"", "-buildTarget", "WebGL", "-executeMethod", "WebLigeraBuild.BuildContinuaCI", "-webCompresion", $Compresion, "-logFile", "`"$log`"") -PassThru -WindowStyle Hidden
if (-not $p.WaitForExit([int]($Horas * 3600 * 1000))) { try { $p.Kill() } catch {}; Write-Error "Sin terminar en $Horas h: cortado (mira $log)."; exit 3 }
Get-Content $log -ErrorAction SilentlyContinue | Where-Object { $_ -like "*[Web]*" } | Select-Object -Last 40 | ForEach-Object { Write-Host $_ }
Write-Host "Unity salio con $($p.ExitCode)."
exit $p.ExitCode
