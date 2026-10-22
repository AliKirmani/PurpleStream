param([string]$PublishDirectory='artifacts/publish',[string]$Destination='artifacts/release')
$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.IO.Compression.FileSystem
$repo=Split-Path $PSScriptRoot -Parent
$publish=(Resolve-Path $PublishDirectory).Path
$dest=[IO.Path]::GetFullPath($Destination)
New-Item -ItemType Directory -Force $dest | Out-Null
$version='0.4.3'
$zipPath=Join-Path $dest "PurpleStream-v$version-Windows-x64.zip"
if(Test-Path $zipPath){throw 'Destination ZIP already exists. Choose a fresh output directory.'}
foreach($license in 'LICENSE','LGPL-2.1.txt','THIRD-PARTY-NOTICES.txt'){Copy-Item (Join-Path $repo $license) $publish}
$zip=[IO.Compression.ZipFile]::Open($zipPath,[IO.Compression.ZipArchiveMode]::Create)
try {
 foreach($file in Get-ChildItem $publish -Recurse -File){
  $rel=$file.FullName.Substring($publish.Length+1).Replace('\','/')
  if($rel -match '^libvlc/(win-x86|win-arm64)/' -or $rel -match '\.(pdb|dat|tmp|log|m3u|m3u8|part)$' -or $rel -match '(?i)(^|/)(tests|ui-tests|ui-exception)\.txt$'){continue}
  [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$file.FullName,"PurpleStream-v$version-Windows-x64/$rel",[IO.Compression.CompressionLevel]::Optimal)|Out-Null
 }
}finally{$zip.Dispose()}
$exe=Join-Path $dest "PurpleStream-Setup-v$version.exe"
$csc=Join-Path $env:WINDIR 'Microsoft.NET/Framework64/v4.0.30319/csc.exe'
$options=@('/nologo','/target:winexe','/platform:x64','/optimize+',"/out:$exe","/win32icon:$repo/src/PurpleStream/PurpleStream.ico","/win32manifest:$repo/src/PurpleStream/app.manifest",'/reference:System.Windows.Forms.dll','/reference:System.Drawing.dll','/reference:System.IO.Compression.dll','/reference:System.IO.Compression.FileSystem.dll',"/resource:$zipPath,PurpleStream.Payload.zip","$repo/installer/Setup.cs")
& $csc @options
if($LASTEXITCODE -ne 0){throw 'Installer compilation failed'}
$hashes=Get-FileHash $exe,$zipPath -Algorithm SHA256 | ForEach-Object {"$($_.Hash.ToLowerInvariant())  $(Split-Path $_.Path -Leaf)"}
[IO.File]::WriteAllLines((Join-Path $dest 'SHA256SUMS.txt'),$hashes)
