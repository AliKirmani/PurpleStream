# Features and practical limits

| Area | Implemented in 0.4.3 | Requirement / limit |
| --- | --- | --- |
| Native desktop | Windows Forms, Windows 10/11 x64, bundled runtime | No Android emulator; other operating systems are not supported |
| Account | Xtream Codes, M3U URL or local playlist, remember option | Valid provider access and reachable server required |
| Dashboard | Six tiles, toolbar, counts, expiry when supplied, continue watching | GPU/temperature may display unavailable; counts reflect loaded catalogs |
| Organization | Country flags, country → genre → title, search, manual corrections | Inference depends on provider metadata and naming |
| Movies / Series | Separate libraries, seasons/episodes, lazy Xtream loading | M3U episodes use explicit groups/types, /series/ paths and common SxxExx names |
| Favorites / history | Local favorites and resume history | Resume is useful for seekable VOD; live streams start live |
| Playback | Pause, timeline, ±10s, Ctrl-arrow ±60s, volume, mute, speed, audio/subtitle tracks, fullscreen, snapshot | Seeking and tracks must be supported by the stream |
| Responsiveness | Native playback calls run on dedicated workers; catalog parsing/search off UI thread | Network failure and unavailable streams can still cause playback errors |
| Multiscreen | 2/3/4 slots, equal or main/sub layout, Make main, independent initially muted audio, Mute all | Hidden slots stop; each playing slot uses a provider connection and decoder |
| Channel picker | Country/category tree, flags, combined keyword search, favorites/recent filters, virtual results | Quality of categorization depends on metadata |
| EPG | Xtream short EPG and XMLTV URL/file | Plain XML or HTTP content encoding; standalone .gz files are not supported |
| Catch-up | Xtream timeshift and common M3U {utc}/{utcend}/{duration} templates | Requires advertised archives and matching schedules |
| Downloads | Direct HTTP VOD download; VLC capture for HLS/DASH | Format/source dependent; no DRM bypass or guaranteed offline access |
| Recording | Live MPEG-TS capture, manual/timed stop and finalization | Preview stops by default to free a connection; simultaneous preview needs another connection |
| Privacy | Current-user Windows DPAPI encrypted settings, favorites and history | Does not encrypt provider network traffic; HTTP remains HTTP |
| Installation | Per-user installer, uninstaller, Start Menu and optional Desktop shortcuts | Windows taskbar pinning is performed by the user |

## Future work — not implemented

Scheduled recording, automatic episode subscriptions, cloud sync, provider account management, universal catch-up formats, DRM support, and guaranteed hardware telemetry are not included. There is no promise of support for every provider or codec. Roadmap ideas are not release commitments.