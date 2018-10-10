Add-Type -AssemblyName System.IO.Compression.FileSystem

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
Invoke-WebRequest "https://github.com/rprichard/winpty/releases/download/0.4.3/winpty-0.4.3-msvc2015.zip" -OutFile winpty.zip
function Unzip
{
    param([string]$zipfile, [string]$outpath)
    [System.IO.Compression.ZipFile]::ExtractToDirectory($zipfile, $outpath)
}

Unzip "$PSScriptRoot\winpty.zip" "$PSScriptRoot\tmp\winpty\"

New-Item -ItemType Directory -Force -Path src\TerminalVelocity.WinPty\winpty\x86
New-Item -ItemType Directory -Force -Path src\TerminalVelocity.WinPty\winpty\x64

Copy-Item -Recurse -Force tmp\winpty\ia32\bin\* src\TerminalVelocity.WinPty\winpty\x86
Copy-Item -Recurse -Force tmp\winpty\x64\bin\* src\TerminalVelocity.WinPty\winpty\x64

Remove-Item -Recurse -Force -Path tmp\
Remove-Item -Recurse -Force -Path winpty.zip