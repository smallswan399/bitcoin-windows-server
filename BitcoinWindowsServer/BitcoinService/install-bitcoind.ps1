$dataDir = "D:\btcdata"
$binaryPath = "BitcoinService.exe -datadir=" + $dataDir

New-Service -Name "bitcoind" -BinaryPathName $binaryPath -DisplayName "Bitcoin service" -Description "Bitcoin deamon service" -StartupType Automatic -Credential "NT AUTHORITY\LOCAL SERVICE"