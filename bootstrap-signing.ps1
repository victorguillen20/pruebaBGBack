$ErrorActionPreference = "Stop"
$cert = New-SelfSignedCertificate -Subject "CN=BG Invoice Dev" -Type CodeSigningCert -CertStoreLocation "Cert:\CurrentUser\My" -NotAfter (Get-Date).AddYears(5)
$store = New-Object System.Security.Cryptography.X509Certificates.X509Store("TrustedPeople", "CurrentUser")
$store.Open("ReadWrite")
$store.Add($cert)
$store.Close()
Write-Host "Cert creado y agregado a CurrentUser\TrustedPeople (thumbprint $($cert.Thumbprint))"
Write-Host "Ahora correr .\sign-dlls.ps1 después de cada build"
