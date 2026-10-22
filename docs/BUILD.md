# Build and test on Windows

Install .NET 8 SDK (x64); Node.js is needed only to generate the original test video. Restore requires NuGet network access. From the repository root in PowerShell:

```powershell
dotnet restore src/PurpleStream/PurpleStream.csproj
dotnet publish src/PurpleStream/PurpleStream.csproj -c Release -r win-x64 --self-contained true -o artifacts/publish --no-restore
```

The project pins runtime 8.0.32 and VideoLAN.LibVLC.Windows 3.0.24. Review runtime/dependency security updates before distributing future builds. Keep all required runtime files and libvlc/win-x64 together; x86/ARM64 LibVLC payloads can be omitted from this x64 distribution.

```powershell
node src/PurpleStream/tests/generate-fixture.cjs
Start-Process artifacts/publish/PurpleStream.exe -ArgumentList '--tests' -Wait
Start-Process artifacts/publish/PurpleStream.exe -ArgumentList '--ui-tests' -Wait
```

Run from the root: the generator writes work/test-video.avi. Read artifacts/publish/tests.txt and ui-tests.txt and check for failures; UI tests need an interactive Windows session and native playback/audio availability. Do not treat process launch alone as a passing test. --directory-tests runs organization regression checks. --demo opens the sample live library.

To reproduce documentation captures, run the newly built app with --screenshots from the root. It writes gallery/*.png without reading saved account data. Copy reviewed images to docs/images and site/images. No video is streamed by the gallery.

Build an installer and portable ZIP:

```powershell
./scripts/package.ps1 -PublishDirectory artifacts/publish -Destination artifacts/release
```

This compiles installer/Setup.cs using the Windows .NET Framework C# compiler and embeds the portable ZIP. The installer is unsigned; there is no code-signing certificate in this repository. Do not bypass a Windows security block; use trusted signing/distribution procedures. Original application code is MIT; preserve all third-party notices and LibVLC licensing information in published binaries.