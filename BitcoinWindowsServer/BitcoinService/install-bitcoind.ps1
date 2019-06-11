$localtion = $PSScriptRoot
$exePath = Join-Path -Path $localtion -ChildPath "BitcoinService.exe"
$binaryPath = $exePath
New-Service -Name "bitcoind" -BinaryPathName $binaryPath -DisplayName "Bitcoin service" -Description "Bitcoin deamon service" -StartupType Automatic -Credential "NT AUTHORITY\LOCAL SERVICE"