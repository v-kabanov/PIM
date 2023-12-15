[string]$scriptDir = $PSScriptRoot;
$now = [System.DateTime]::Now;

$backupDir = [System.IO.Path]::Combine($scriptDir, [System.DateTime]::Now.ToString("yyyy\\MM-MMM"));
[System.IO.Directory]::CreateDirectory($backupDir) | Out-Null;

$backupFilePath = [System.IO.Path]::Combine($backupDir, "pim-dump.7z");

#$env:PGPASSWORD="***";
# -Fc is custom format

#&"c:\Program Files\PostgreSQL\16\bin\pg_dump.exe" --no-password -Z6 -U postgres -d pim > $backupFilePath;

$7z ="c:\Program Files\7-Zip\7z.exe";

&"c:\Program Files\PostgreSQL\16\bin\pg_dump.exe" --no-password -d pim | &$7z a -r -t7z -bd -si $backupFilePath;