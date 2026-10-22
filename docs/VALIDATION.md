# Validation and release provenance

The existing 0.4.3 Windows binaries passed the prior local native/fixture and WinForms checks, including independent multiscreen audio, layouts, country/category filtering and a 12,000-entry synthetic picker dataset. Tests are synthetic; real subscription authorization, archive availability, codecs and provider quotas are not comprehensively verified.

The previous packaging audit reported zero known personal-account credential matches and zero settings/playlist files in the payload. Installer test extraction matched the published executable hash; executable icon extraction and directory regression checks passed. Full installation/uninstallation in a real Windows user profile was not exercised in that audit.

This repository copies that application source and adds documentation, workflows, packaging scripts and a developer screenshot helper. It does not replace the already-tested release binaries. New builds should run the suites in BUILD.md before release. Visual captures use synthetic data; blank multiscreen slots are intentional. No claim of error-free operation is made.

## Repository preparation rerun (2026-10-22)

The copied source built successfully with one existing high-DPI manifest warning. Six real application gallery images were generated. The interactive UI suite passed its catalog navigation, player/fullscreen controls, settings, four native audio outputs, screen layouts, 12,000-entry picker, failed-connection responsiveness and guide/archive checks. The non-UI suite passed directory/network/provider/XMLTV/download checks, then failed at the native video playback checkpoint (`Native playback not playing`). Capture and recording checks later in that suite were therefore not rerun successfully. This remains an unresolved local validation limitation; previous 0.4.3 release assets are retained unchanged rather than replaced by this documentation build.
