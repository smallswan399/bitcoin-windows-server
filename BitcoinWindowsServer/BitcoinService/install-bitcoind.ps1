$dataDir = "D:\btcdata"
$localtion = $PSScriptRoot
$exePath = Join-Path -Path $localtion -ChildPath "BitcoinService.exe"
$binaryPath = $exePath + " -datadir=" + $dataDir
New-Service -Name "bitcoind" -BinaryPathName $binaryPath -DisplayName "Bitcoin service" -Description "Bitcoin deamon service" -StartupType Automatic -Credential "NT AUTHORITY\LOCAL SERVICE"