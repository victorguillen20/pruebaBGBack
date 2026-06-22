$ErrorActionPreference = "Stop"
$certSubject = "CN=BG Invoice Dev"
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("My", "CurrentUser")
$store.Open("ReadOnly")
$cert = $store.Certificates | Where-Object { $_.Subject -eq $certSubject } | Select-Object -First 1
$store.Close()
if (-not $cert) {
    Write-Error "Cert '$certSubject' no encontrado en CurrentUser\My. Correr bootstrap-signing.ps1 primero."
    exit 1
}
$binPath = "src\BG.Invoice.Api\bin\Debug\net10.0"
if (-not (Test-Path $binPath)) {
    Write-Host "Bin no existe aun ($binPath). Se firmara despues del primer build."
    exit 0
}
$count = 0
Get-ChildItem $binPath -Filter "*.dll" | ForEach-Object {
    Set-AuthenticodeSignature -FilePath $_.FullName -Certificate $cert | Out-Null
    $count++
}
Write-Host "Firmados $count DLLs en $binPath"
