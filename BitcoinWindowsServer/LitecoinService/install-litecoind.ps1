$dataDir = "D:\ltcdata"
$localtion = $PSScriptRoot
$exePath = Join-Path -Path $localtion -ChildPath "LitecoinService.exe"
$binaryPath = $exePath + " -datadir=" + $dataDir
New-Service -Name "litecoind" -BinaryPathName $binaryPath -DisplayName "Litecoin service" -Description "Litecoin deamon service" -StartupType Automatic -Credential "NT AUTHORITY\LOCAL SERVICE"