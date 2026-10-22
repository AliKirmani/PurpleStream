# Architecture

The Windows Forms shell (`Main`, `Windows`, `Theme`) owns navigation and dialogs. `Provider` and `ProviderTransport` implement account/catalog/guide requests. `Core`, `ChannelDirectory` and `FlagIcons` parse, classify and organize entries. `Browser` and `ChannelPicker` provide virtualized lists and background filtering.

`PlayerView` displays playback state and controls. `Playback` confines LibVLC native operations, getters and disposal to an `AsyncPlayback` worker; the UI reads cached state. `Multiscreen` owns a separate worker per slot and keeps desired audio state per player. `Transfers` manages downloads and recording finalization. `SettingsStore` encrypts the saved payload for the current Windows user.

Tests use synthetic provider responses and an original generated AVI. `Gallery.cs` adds a maintainer-only --screenshots command that creates UI captures in a gallery folder using isolated demo state; this helper is not present in the previously tested 0.4.3 release binaries. It does not change normal app behavior.