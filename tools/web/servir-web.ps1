# servir-web.ps1 - SERVIDOR ESTATICO MINIMO PARA PROBAR LA WEB EN LOCAL (2026-11-17). Sin instalar nada.
#
# Sirve una carpeta (por defecto Builds\Web\sitio del proyecto Unity) en http://127.0.0.1:<puerto>/ con un TcpListener (no HttpListener:
# este no necesita permisos de URL ni de administrador). Tipos MIME de Unity WebGL (.wasm, .js, .data, .unityweb...) y SIN
# Content-Encoding: con gzip y "decompression fallback", el cargador de Unity descomprime el solo.
# Atiende de una en una (Connection: close); el navegador pone en cola el resto. Todo acotado: -MaxSec en total, 15 s por peticion.
# Para pararlo: crea el fichero -Stop, o espera -MaxSec. probar-web.ps1 -Local lo arranca y lo para el solo.
#
# Uso: powershell -ExecutionPolicy Bypass -File tools\web\servir-web.ps1 [-Root <carpeta>] [-Port 18080] [-MaxSec 7200] [-Stop <fichero>]

param(
    [string]$Root = "C:\Users\Blein\COLD-SANCTUARY\Builds\Web\sitio",
    [int]$Port = 18080,
    [int]$MaxSec = 7200,
    [string]$Stop = ""
)

$ErrorActionPreference = "Stop"
$Root = (Resolve-Path $Root).Path.TrimEnd("\")
$tipos = @{
    ".html" = "text/html; charset=utf-8"; ".htm" = "text/html; charset=utf-8"; ".js" = "application/javascript"; ".mjs" = "application/javascript"
    ".json" = "application/json"; ".css" = "text/css"; ".txt" = "text/plain; charset=utf-8"; ".wasm" = "application/wasm"
    ".data" = "application/octet-stream"; ".unityweb" = "application/octet-stream"; ".bin" = "application/octet-stream"
    ".png" = "image/png"; ".jpg" = "image/jpeg"; ".ico" = "image/x-icon"; ".svg" = "image/svg+xml"; ".mem" = "application/octet-stream"
}

function Tipo([string]$path) {
    $n = [IO.Path]::GetFileName($path).ToLowerInvariant()
    # Archivos de Unity con doble extension: X.wasm.unityweb, X.framework.js.unityweb, X.data.unityweb (gzip de reserva: binario).
    if ($n.EndsWith(".unityweb")) { return "application/octet-stream" }
    $e = [IO.Path]::GetExtension($n)
    if ($tipos.ContainsKey($e)) { return $tipos[$e] }
    return "application/octet-stream"
}

function Responder($stream, [int]$code, [string]$status, [string]$ctype, [long]$len, [bool]$head) {
    $h = "HTTP/1.1 $code $status`r`nContent-Type: $ctype`r`nContent-Length: $len`r`nCache-Control: no-cache`r`nAccess-Control-Allow-Origin: *`r`nConnection: close`r`n`r`n"
    $b = [Text.Encoding]::ASCII.GetBytes($h)
    $stream.Write($b, 0, $b.Length)
}

$listener = New-Object System.Net.Sockets.TcpListener ([Net.IPAddress]::Loopback, $Port)
$listener.Start(64)
Write-Host "Sirviendo $Root en http://127.0.0.1:$Port/ (hasta $MaxSec s$(if ($Stop) { " o hasta que exista $Stop" }))"
$t0 = Get-Date
$n = 0
try {
    while (((Get-Date) - $t0).TotalSeconds -lt $MaxSec) {
        if ($Stop -and (Test-Path $Stop)) { break }
        if (-not $listener.Pending()) { Start-Sleep -Milliseconds 50; continue }
        $client = $listener.AcceptTcpClient()
        try {
            $client.ReceiveTimeout = 15000; $client.SendTimeout = 15000
            $stream = $client.GetStream()
            $reader = New-Object IO.StreamReader($stream, [Text.Encoding]::ASCII, $false, 8192, $true)
            $line = $reader.ReadLine()
            while ($true) { $hl = $reader.ReadLine(); if ($null -eq $hl -or $hl -eq "") { break } }   # (las cabeceras no se usan)
            if (-not $line) { continue }
            $parts = $line.Split(" ")
            $method = $parts[0]; $target = if ($parts.Length -gt 1) { $parts[1] } else { "/" }
            $q = $target.IndexOf("?"); if ($q -ge 0) { $target = $target.Substring(0, $q) }
            $rel = [Uri]::UnescapeDataString($target).TrimStart("/").Replace("/", "\")
            $full = [IO.Path]::GetFullPath((Join-Path $Root $rel))
            if (Test-Path $full -PathType Container) { $full = Join-Path $full "index.html" }
            $n++
            if (($method -ne "GET" -and $method -ne "HEAD") -or -not $full.StartsWith($Root, [StringComparison]::OrdinalIgnoreCase) -or -not (Test-Path $full -PathType Leaf)) {
                $msg = [Text.Encoding]::UTF8.GetBytes("404 $target")
                Responder $stream 404 "Not Found" "text/plain; charset=utf-8" $msg.Length $false
                $stream.Write($msg, 0, $msg.Length)
                continue
            }
            $fi = New-Object IO.FileInfo $full
            Responder $stream 200 "OK" (Tipo $full) $fi.Length ($method -eq "HEAD")
            if ($method -eq "GET") { $fs = [IO.File]::OpenRead($full); try { $fs.CopyTo($stream, 1048576) } finally { $fs.Dispose() } }
            $stream.Flush()
        }
        catch { Write-Host ("  (peticion cortada: " + $_.Exception.Message + ")") }
        finally { $client.Close() }
    }
}
finally {
    $listener.Stop()
    Write-Host "Servidor parado tras $n peticiones."
}
