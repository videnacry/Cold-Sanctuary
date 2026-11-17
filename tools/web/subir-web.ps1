# subir-web.ps1 - SUBIR LA WEB CONTINUA (2026-11-16).
#
# 1) Copia el sitio construido (Builds\Web\sitio del proyecto Unity: santuario, grupos, una pagina por microcosmos, portal, escenas.json)
#    al repositorio de GitHub Pages (cs-web-pages), espejo exacto (robocopy /MIR, sin tocar .git).
# 2) Comprueba que ningun archivo pase de 100 MB (GitHub Pages no los sirve; git los rechaza).
# 3) Commit local (con el trailer Co-Authored-By) y push a origin main.
# 4) Espera (acotado) a que Pages publique la version nueva (version.txt) y corre tools\web\probar-web.ps1 contra la web en vivo.
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File tools\web\subir-web.ps1 -Mensaje "Web: sitio desde web-continua <commit>"
#   ... -SinProbar          (solo subir)
#   ... -SoloProbar         (no copia ni sube: solo prueba la web en vivo)
#   ... -Squash             (ATENCION: reescribe la historia de cs-web-pages con un solo commit y hace push --force. Cada subida anade
#                            cientos de MB al historial de git; con -Squash el repositorio no crece. Solo con el visto bueno de Beron.)
# Todo lo que espera esta acotado.

param(
    [string]$Origen = "C:\Users\Blein\COLD-SANCTUARY\Builds\Web\sitio",
    [string]$Repo = "C:\Users\Blein\Documents\GitHub\cs-web-pages",
    [string]$Url = "https://videnacry.github.io/Cold-sanctuary-web/",
    [string]$Mensaje = "",
    [switch]$SinProbar,
    [switch]$SoloProbar,
    [switch]$Squash,
    [int]$EsperaPages = 900
)

$ErrorActionPreference = "Stop"
$aqui = Split-Path -Parent $MyInvocation.MyCommand.Path
if (-not $Url.EndsWith("/")) { $Url += "/" }

if (-not $SoloProbar) {
    if (-not (Test-Path (Join-Path $Origen "index.html")) -or -not (Test-Path (Join-Path $Origen "version.txt"))) { Write-Error "No hay sitio en $Origen (construyelo antes: Tools > Cold Sanctuary > Web > Web continua: construir todo)."; exit 2 }
    if (-not (Test-Path (Join-Path $Repo ".git"))) { Write-Error "$Repo no es un repositorio git."; exit 2 }
    $grandes = @(Get-ChildItem -Path $Origen -Recurse -File | Where-Object { $_.Length -gt 100MB -and $_.FullName -notlike "*_BurstDebugInformation_DoNotShip*" })
    if ($grandes.Count -gt 0) { $grandes | ForEach-Object { Write-Host ("  {0}: {1:0.0} MB" -f $_.FullName, ($_.Length / 1MB)) }; Write-Error "Hay archivos de mas de 100 MB: GitHub Pages no los sirve. Vuelve a construir (el plan parte los grupos grandes)."; exit 2 }
    $version = (Get-Content (Join-Path $Origen "version.txt") -Raw).Trim()
    $total = (Get-ChildItem -Path $Origen -Recurse -File | Where-Object { $_.FullName -notlike "*_BurstDebugInformation_DoNotShip*" } | Measure-Object Length -Sum).Sum
    Write-Host ("Sitio {0}: {1:0} MB. Copiando a {2}..." -f $version, ($total / 1MB), $Repo)

    # 1) Espejo (robocopy: 0-7 = bien; 8+ = error). /XD .git: el repositorio se queda.
    & robocopy $Origen $Repo /MIR /XD (Join-Path $Repo ".git") "*_BurstDebugInformation_DoNotShip" /XF ".git" /R:2 /W:2 /NFL /NDL /NJH /NJS /NP | Out-Null
    if ($LASTEXITCODE -ge 8) { Write-Error "robocopy fallo ($LASTEXITCODE)."; exit 2 }

    # 2) Commit y push.
    if (-not $Mensaje) { $Mensaje = "Web: sitio continuo ($version)" }
    $msgFile = Join-Path $env:TEMP ("cs-web-msg-" + [guid]::NewGuid().ToString("N") + ".txt")
    [IO.File]::WriteAllText($msgFile, "$Mensaje`n`nCo-Authored-By: Claude Opus 5.5 <noreply@anthropic.com>`n", (New-Object Text.UTF8Encoding $false))
    git -C $Repo add -A
    if ($Squash) {
        Write-Host "ATENCION: -Squash: historia nueva de un solo commit y push --force a origin main."
        git -C $Repo checkout --orphan web-squash
        git -C $Repo add -A
        git -C $Repo commit -q -F $msgFile
        git -C $Repo branch -D main
        git -C $Repo branch -m main
        git -C $Repo push --force origin main
    }
    else {
        git -C $Repo diff --cached --quiet
        if ($LASTEXITCODE -eq 0) { Write-Host "Nada que subir (el sitio no ha cambiado)." }
        else {
            git -C $Repo commit -q -F $msgFile
            git -C $Repo push origin main
            if ($LASTEXITCODE -ne 0) { Remove-Item $msgFile -ErrorAction SilentlyContinue; Write-Error "push fallo."; exit 2 }
        }
    }
    Remove-Item $msgFile -ErrorAction SilentlyContinue
    $gitMB = (Get-ChildItem -Path (Join-Path $Repo ".git") -Recurse -File -ErrorAction SilentlyContinue | Measure-Object Length -Sum).Sum / 1MB
    Write-Host ("Subido: {0} (commit {1}). El .git de cs-web-pages ocupa {2:0} MB{3}." -f $version, (git -C $Repo rev-parse --short HEAD), $gitMB, $(if ($gitMB -gt 900) { " - CERCA DEL LIMITE DE GITHUB (1 GB): considera -Squash" } else { "" }))

    # 3) Esperar a que GitHub Pages publique esta version (acotado).
    if (-not $SinProbar) {
        Write-Host "Esperando a que GitHub Pages publique $version (hasta $EsperaPages s)..."
        $hasta = (Get-Date).AddSeconds($EsperaPages); $vivo = ""
        while ((Get-Date) -lt $hasta) {
            try { $vivo = (Invoke-WebRequest -Uri ($Url + "version.txt?t=" + [DateTime]::UtcNow.Ticks) -UseBasicParsing -TimeoutSec 20).Content.Trim() } catch { $vivo = "" }
            if ($vivo -eq $version) { break }
            Start-Sleep -Seconds 15
        }
        if ($vivo -ne $version) { Write-Error "Pages no publico $version en $EsperaPages s (en vivo: '$vivo'). Prueba luego con -SoloProbar."; exit 3 }
        Write-Host "Publicado."
    }
}

# 4) La prueba en vivo.
if ($SinProbar) { exit 0 }
& (Join-Path $aqui "probar-web.ps1") -Base $Url -Salida (Join-Path $env:TEMP "cs-web-resultados.json")
exit $LASTEXITCODE
