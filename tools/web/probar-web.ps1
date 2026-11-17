# probar-web.ps1 - PRUEBA DE LA WEB EN EDGE SIN VENTANA (2026-11-16). Sin instalar nada: usa el Edge del PC.
#
# Abre cada pagina de la web (escenas.json del sitio) con ?prueba=1 en Edge headless, con tamano de movil, espera (acotado) a que la
# pagina ponga su titulo en "OK" o "FALLOS n" (lo hace WebPrueba dentro del juego) y lee <pre id="resultado"> (JSON). Imprime una tabla
# por escena y sale con 0 si todas dieron OK.
#
# Dos modos:
#   -Modo cdp      (por defecto) Edge con --remote-debugging-port: navega y pregunta el titulo cada pocos segundos (DevTools Protocol,
#                  con el WebSocket de .NET; nada que instalar). Espera real, acotada por -TimeoutEscena.
#   -Modo dumpdom  msedge --headless --dump-dom --virtual-time-budget: mas simple, pero el tiempo virtual y WebGL se llevan mal; de reserva.
#
# Uso:
#   powershell -ExecutionPolicy Bypass -File tools\web\probar-web.ps1
#   powershell -ExecutionPolicy Bypass -File tools\web\probar-web.ps1 -Base https://videnacry.github.io/Cold-sanctuary-web/ -Solo Buda,santuario
#   Sitio local: sirve Builds/Web/sitio (p. ej. un HttpListener de PowerShell en el puerto 18080) y pasa -Base http://127.0.0.1:18080/
#   (-Base es la raiz del sitio: de alli lee escenas.json y abre cada pagina con ?prueba=1)
# Todo lo que espera esta acotado: Edge y DevTools 30 s, cada orden 30 s, cada escena -TimeoutEscena s, todo -TimeoutTotal s.

param(
    [string]$Base = "https://videnacry.github.io/Cold-sanctuary-web/",
    [string[]]$Solo = @(),
    [ValidateSet("cdp", "dumpdom")][string]$Modo = "cdp",
    [int]$TimeoutEscena = 420,
    [int]$TimeoutTotal = 14400,
    [int]$Ancho = 812,
    [int]$Alto = 375,
    [int]$Puerto = 9333,
    [string]$Edge = "",
    [string]$Salida = "",
    [string[]]$Urls = @(),   # direcciones sueltas (en vez de escenas.json): se les anade prueba=1
    [switch]$Local,            # sirve -Root con tools\web\servir-web.ps1 (lo arranca y lo para solo) y prueba alli
    [string]$Root = "C:\Users\Blein\COLD-SANCTUARY\Builds\Web\sitio",
    [int]$PuertoWeb = 18080
)

$ErrorActionPreference = "Stop"
if (-not $Base.EndsWith("/")) { $Base += "/" }
if (-not $Edge) {
    foreach ($c in @("${env:ProgramFiles(x86)}\Microsoft\Edge\Application\msedge.exe", "$env:ProgramFiles\Microsoft\Edge\Application\msedge.exe")) { if (Test-Path $c) { $Edge = $c; break } }
}
if (-not $Edge -or -not (Test-Path $Edge)) { Write-Error "No encuentro msedge.exe (pasa -Edge <ruta>)."; exit 2 }
$t0 = Get-Date

# -- -Local: el servidor estatico (tools\web\servir-web.ps1), en otro proceso, acotado; se para al salir --------------------------------
$script:servidor = $null; $script:stopFile = $null
function Parar-Servidor { if ($script:stopFile) { New-Item -ItemType File -Force $script:stopFile | Out-Null }; if ($script:servidor) { if (-not $script:servidor.WaitForExit(5000)) { try { $script:servidor.Kill() } catch {} } }; if ($script:stopFile) { Remove-Item $script:stopFile -ErrorAction SilentlyContinue } }
function Salir([int]$code) { Parar-Servidor; exit $code }
if ($Local) {
    if (-not (Test-Path (Join-Path $Root "index.html"))) { Write-Error "No hay sitio en $Root."; exit 2 }
    $pw = $PuertoWeb
    for ($i = 0; $i -lt 20; $i++) { $libre = $true; try { $t = New-Object System.Net.Sockets.TcpListener ([Net.IPAddress]::Loopback, ($PuertoWeb + $i)); $t.Start(); $t.Stop() } catch { $libre = $false }; if ($libre) { $pw = $PuertoWeb + $i; break } }
    $script:stopFile = Join-Path $env:TEMP ("cs-servir-" + [guid]::NewGuid().ToString("N") + ".stop")
    $aqui = Split-Path -Parent $MyInvocation.MyCommand.Path
    $script:servidor = Start-Process -FilePath "powershell.exe" -ArgumentList @("-NoProfile", "-ExecutionPolicy", "Bypass", "-File", "`"$(Join-Path $aqui "servir-web.ps1")`"", "-Root", "`"$Root`"", "-Port", $pw, "-MaxSec", ($TimeoutTotal + 600), "-Stop", "`"$script:stopFile`"") -PassThru -WindowStyle Hidden
    $Base = "http://127.0.0.1:$pw/"
    $listo = $false; $hasta = (Get-Date).AddSeconds(30)
    while (-not $listo -and (Get-Date) -lt $hasta) { try { $null = Invoke-WebRequest -Uri ($Base + "version.txt") -UseBasicParsing -TimeoutSec 5; $listo = $true } catch { Start-Sleep -Milliseconds 500 } }
    if (-not $listo) { Write-Error "El servidor local no responde en 30 s ($Base)."; Salir 2 }
    Write-Host "Servidor local: $Base (sirve $Root)"
}

# -- La lista de paginas ------------------------------------------------------------------------------------------------------------
function UrlOf($e) { $u = if ($e.full) { $e.full } else { $Base + $e.url }; if ($u.Contains("?")) { $u + "&prueba=1" } else { $u + "?prueba=1" } }
$lista = $null
if ($Urls.Count -gt 0) { $lista = @($Urls | ForEach-Object { [pscustomobject]@{ nombre = ($_ -replace "^https?://[^/]+/", ""); full = $_ } }) }
else { try { $lista = (Invoke-RestMethod -Uri ($Base + "escenas.json?t=" + [DateTime]::UtcNow.Ticks) -TimeoutSec 30).escenas }
catch { Write-Error "No pude leer $($Base)escenas.json: $($_.Exception.Message)"; Salir 2 } }
if ($Solo.Count -gt 0) { $want = $Solo | ForEach-Object { $_ -split "," } | ForEach-Object { $_.Trim() }; $lista = @($lista | Where-Object { $want -contains $_.nombre }) }
Write-Host ("Probando {0} paginas de {1} (modo {2}, {3}x{4}, {5} s por escena)" -f @($lista).Count, $Base, $Modo, $Ancho, $Alto, $TimeoutEscena)

$filas = New-Object System.Collections.ArrayList
function Add-Fila($e, $estado, $json, $segs, $nota = "") {
    $r = $null
    if ($json) { try { $r = $json | ConvertFrom-Json } catch { $r = $null } }
    $fallas = ""
    if ($r -and $r.checks) { $fallas = (@($r.checks | Where-Object { -not $_.ok } | ForEach-Object { $_.nombre + ": " + $_.detalle }) -join " | ") }
    elseif ($r -and $r.error) { $fallas = $r.error }
    if ($nota) { $fallas = if ($fallas) { $fallas + " | " + $nota } else { $nota } }
    [void]$filas.Add([pscustomobject]@{
        escena  = $e.nombre
        estado  = $estado
        fallos  = if ($r) { $r.fallos } else { "" }
        gente   = if ($r) { $r.gente } else { "" }
        flora   = if ($r) { ("{0}/{1}" -f $r.flora, $r.floraEsperada) } else { "" }
        fauna   = if ($r) { ("{0}/{1}" -f $r.fauna, $r.faunaEsperada) } else { "" }
        segundos = [int]$segs
        detalle = if ($fallas.Length -gt 400) { $fallas.Substring(0, 400) + "..." } else { $fallas }
        json    = $json
    })
}

$perfil = Join-Path $env:TEMP ("cs-edge-" + [guid]::NewGuid().ToString("N"))
$comunes = @("--headless=new", "--user-data-dir=$perfil", "--window-size=$Ancho,$Alto", "--no-first-run", "--no-default-browser-check", "--mute-audio",
             "--use-angle=swiftshader", "--enable-unsafe-swiftshader", "--ignore-gpu-blocklist", "--disable-background-timer-throttling",
             "--disable-renderer-backgrounding", "--disable-backgrounding-occluded-windows", "--autoplay-policy=no-user-gesture-required")

if ($Modo -eq "dumpdom") {
    foreach ($e in $lista) {
        if (((Get-Date) - $t0).TotalSeconds -gt $TimeoutTotal) { Add-Fila $e "SIN TIEMPO" $null 0; continue }
        $url = UrlOf $e
        $out = Join-Path $env:TEMP ("cs-dom-" + [guid]::NewGuid().ToString("N") + ".html")
        $s0 = Get-Date
        $p = Start-Process -FilePath $Edge -ArgumentList ($comunes + @("--dump-dom", "--virtual-time-budget=$($TimeoutEscena * 1000)", $url)) -RedirectStandardOutput $out -PassThru -WindowStyle Hidden
        if (-not $p.WaitForExit(($TimeoutEscena + 60) * 1000)) { try { $p.Kill() } catch {} }
        $dom = if (Test-Path $out) { Get-Content $out -Raw -Encoding UTF8 } else { "" }
        Remove-Item $out -ErrorAction SilentlyContinue
        $title = if ($dom -match "<title>(.*?)</title>") { $Matches[1] } else { "" }
        $json = if ($dom -match '<pre id="resultado"[^>]*>([\s\S]*?)</pre>') { [System.Net.WebUtility]::HtmlDecode($Matches[1]) } else { $null }
        $estado = if ($title -match "^(OK|FALLOS.*)$") { $title } else { "SIN RESULTADO" }
        Add-Fila $e $estado $json ((Get-Date) - $s0).TotalSeconds
        Write-Host ("  {0,-28} {1}" -f $e.nombre, $estado)
    }
}
else {
    # MODO CDP, A PRUEBA DE CAIDAS (2026-11-17): una PESTANA NUEVA por escena (se cierra al acabar: la memoria de WebGL no se acumula);
    # si la conexion o la pestana mueren, se reintenta con otra pestana y, si hace falta, se relanza Edge; la escena queda "ERROR runner"
    # y se sigue con la siguiente. Cada pagina guarda las ultimas lineas de su consola (window.__csLog) para cuando no hay resultado.
    $script:proc = $null; $script:ws = $null; $script:id = 0
    $consolaHook = "(function(){ if (window.__csLog) return; window.__csLog = []; ['log','info','warn','error'].forEach(function(k){ var o = console[k]; console[k] = function(){ try { var s = Array.prototype.join.call(arguments, ' '); window.__csLog.push(k[0].toUpperCase() + ' ' + s.substring(0, 400)); if (window.__csLog.length > 200) window.__csLog.shift(); } catch (e) {} return o.apply(console, arguments); }; }); window.addEventListener('error', function(ev){ try { window.__csLog.push('X ' + ev.message + ' @ ' + ev.filename + ':' + ev.lineno); } catch (e) {} }); })();"

    function Start-Edge {
        if ($script:proc) { try { Stop-Process -Id $script:proc.Id -Force -ErrorAction SilentlyContinue } catch {} }
        Get-CimInstance Win32_Process -Filter "Name = 'msedge.exe'" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*$perfil*" } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
        $script:proc = Start-Process -FilePath $Edge -ArgumentList ($comunes + @("--remote-debugging-port=$Puerto", "about:blank")) -PassThru -WindowStyle Hidden
        $hasta = (Get-Date).AddSeconds(30)
        while ((Get-Date) -lt $hasta) { try { $null = Invoke-RestMethod -Uri "http://127.0.0.1:$Puerto/json/version" -TimeoutSec 5; return } catch { Start-Sleep -Milliseconds 500 } }
        throw "Edge no abrio DevTools en 30 s."
    }
    function Close-Ws { if ($script:ws) { try { [void]$script:ws.CloseAsync([Net.WebSockets.WebSocketCloseStatus]::NormalClosure, "fin", [Threading.CancellationToken]::None).Wait(3000) } catch {}; try { $script:ws.Dispose() } catch {}; $script:ws = $null } }
    function Open-Tab {
        # Una pestana nueva (about:blank) y su WebSocket. PUT /json/new (Edge reciente); si no, GET.
        $t = $null
        try { $t = Invoke-RestMethod -Method Put -Uri "http://127.0.0.1:$Puerto/json/new?about:blank" -TimeoutSec 10 } catch { $t = Invoke-RestMethod -Uri "http://127.0.0.1:$Puerto/json/new?about:blank" -TimeoutSec 10 }
        Close-Ws
        $script:ws = New-Object System.Net.WebSockets.ClientWebSocket
        $script:ws.Options.KeepAliveInterval = [TimeSpan]::FromSeconds(20)
        if (-not $script:ws.ConnectAsync([Uri]$t.webSocketDebuggerUrl, [Threading.CancellationToken]::None).Wait(10000)) { throw "No conecta con la pestana." }
        return $t.id
    }
    function Close-Tab($tabId) { Close-Ws; if ($tabId) { try { $null = Invoke-RestMethod -Uri "http://127.0.0.1:$Puerto/json/close/$tabId" -TimeoutSec 10 } catch {} } }
    function Cdp([string]$method, $params) {
        if (-not $script:ws -or $script:ws.State -ne [Net.WebSockets.WebSocketState]::Open) { throw "WebSocket cerrado." }
        $script:id++
        $myId = $script:id
        $msg = @{ id = $myId; method = $method; params = $params } | ConvertTo-Json -Depth 8 -Compress
        $bytes = [Text.Encoding]::UTF8.GetBytes($msg)
        if (-not $script:ws.SendAsync((New-Object ArraySegment[byte] -ArgumentList @(, $bytes)), [Net.WebSockets.WebSocketMessageType]::Text, $true, [Threading.CancellationToken]::None).Wait(30000)) { throw "DevTools no responde (envio)." }
        $hasta = (Get-Date).AddSeconds(30)
        $buf = New-Object byte[] 1048576
        while ((Get-Date) -lt $hasta) {
            $sb = New-Object System.Text.StringBuilder
            do {
                $task = $script:ws.ReceiveAsync((New-Object ArraySegment[byte] -ArgumentList @(, $buf)), [Threading.CancellationToken]::None)
                if (-not $task.Wait(30000)) { throw "DevTools no responde (lectura)." }
                if ($task.Result.MessageType -eq [Net.WebSockets.WebSocketMessageType]::Close) { throw "DevTools cerro la conexion." }
                [void]$sb.Append([Text.Encoding]::UTF8.GetString($buf, 0, $task.Result.Count))
            } while (-not $task.Result.EndOfMessage)
            $r = $sb.ToString() | ConvertFrom-Json
            if ($r.id -eq $myId) { return $r }
        }
        throw "DevTools: sin respuesta a $method en 30 s."
    }
    function Eval([string]$expr) { return (Cdp "Runtime.evaluate" @{ expression = $expr; returnByValue = $true }).result.result.value }
    function Save-Consola($e, [string]$why) {
        $tail = ""
        try { $tail = Eval "(window.__csLog || []).slice(-40).join('\n')" } catch { $tail = "(no se pudo leer la consola: $($_.Exception.Message))" }
        $file = Join-Path $env:TEMP ("cs-web-consola-" + ($e.nombre -replace "[^\w\-]", "_") + ".txt")
        Set-Content -Path $file -Value ("$why`r`n" + $tail) -Encoding UTF8
        Write-Host ("    consola ({0}): {1}" -f $why, $file)
        $last = @($tail -split "`n" | Where-Object { $_ } | Select-Object -Last 3) -join " / "
        return $last
    }

    try {
        Start-Edge
        $hechas = 0
        foreach ($e in $lista) {
            if (((Get-Date) - $t0).TotalSeconds -gt $TimeoutTotal) { Add-Fila $e "SIN TIEMPO" $null 0; continue }
            if ($hechas -gt 0 -and $hechas % 15 -eq 0) { try { Start-Edge } catch {} }   # (cada 15 escenas, Edge limpio)
            $hechas++
            $url = UrlOf $e
            $s0 = Get-Date
            $estado = "SIN RESULTADO"; $json = $null; $nota = ""
            $intentos = 0
            while ($intentos -lt 2) {
                $intentos++
                $tab = $null
                try {
                    $tab = Open-Tab
                    [void](Cdp "Emulation.setDeviceMetricsOverride" @{ width = $Ancho; height = $Alto; deviceScaleFactor = 2; mobile = $true })
                    [void](Cdp "Emulation.setTouchEmulationEnabled" @{ enabled = $true; maxTouchPoints = 5 })
                    [void](Cdp "Page.addScriptToEvaluateOnNewDocument" @{ source = $consolaHook })
                    [void](Cdp "Page.navigate" @{ url = $url })
                    $hasta = $s0.AddSeconds($TimeoutEscena)
                    $titulo = ""; $got = $false
                    while ((Get-Date) -lt $hasta) {
                        Start-Sleep -Seconds 5
                        $v = (Eval "JSON.stringify({t: document.title, r: (document.getElementById('resultado') || {}).textContent || '', u: location.href})") | ConvertFrom-Json
                        $titulo = $v.t
                        if ($v.t -match "^(OK|FALLOS.*)$") { $estado = $v.t; $json = $v.r; $got = $true; break }
                    }
                    if (-not $got) {
                        $estado = "SIN RESULTADO (titulo: $titulo)"
                        $nota = Save-Consola $e "sin resultado en $TimeoutEscena s; ultima URL: $($v.u)"
                    }
                    Close-Tab $tab
                    break
                }
                catch {
                    $err = $_.Exception; while ($err.InnerException) { $err = $err.InnerException }
                    Write-Host ("    {0}: error del probador (intento {1}): {2}" -f $e.nombre, $intentos, $err.Message)
                    try { $nota = Save-Consola $e "error del probador: $($err.Message)" } catch {}
                    Close-Tab $tab
                    $estado = "ERROR runner"; $json = $null
                    if ($nota -eq "") { $nota = $err.Message }
                    # La pestana o Edge murieron: Edge nuevo y, si queda tiempo para la escena, otro intento.
                    try { Start-Edge } catch { Write-Host "    no se pudo relanzar Edge: $($_.Exception.Message)" }
                    if (((Get-Date) - $s0).TotalSeconds -gt $TimeoutEscena * 0.8) { break }
                }
            }
            Add-Fila $e $estado $json ((Get-Date) - $s0).TotalSeconds $nota
            Write-Host ("  {0,-28} {1}  ({2:0} s)" -f $e.nombre, $estado, ((Get-Date) - $s0).TotalSeconds)
        }
    }
    catch { Write-Host "El probador se paro: $($_.Exception.Message)" }
    finally {
        Close-Ws
        if ($script:proc) { try { Stop-Process -Id $script:proc.Id -Force -ErrorAction SilentlyContinue } catch {} }
        Get-CimInstance Win32_Process -Filter "Name = 'msedge.exe'" -ErrorAction SilentlyContinue | Where-Object { $_.CommandLine -like "*$perfil*" } | ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
        Start-Sleep -Seconds 1
        Remove-Item $perfil -Recurse -Force -ErrorAction SilentlyContinue
        Parar-Servidor   # (tambien si algo fallo a medias: el servidor local no se queda encendido)
    }
}

# -- La tabla -----------------------------------------------------------------------------------------------------------------------
$filas | Format-Table escena, estado, fallos, gente, flora, fauna, segundos, detalle -AutoSize -Wrap | Out-String -Width 220 | Write-Host
$ok = @($filas | Where-Object { $_.estado -eq "OK" }).Count
Write-Host ("{0} de {1} OK en {2:0} min." -f $ok, $filas.Count, ((Get-Date) - $t0).TotalMinutes)
if ($Salida) { $filas | ConvertTo-Json -Depth 6 | Set-Content -Path $Salida -Encoding UTF8; Write-Host "Resultados: $Salida" }
if ($ok -eq $filas.Count -and $filas.Count -gt 0) { Salir 0 } else { Salir 1 }
