# Privacy

The distributed source and release payload contain no preset provider account, playlist or subscription. The default account fields are blank. Remembered account/settings, favorites (including URLs) and viewing history are encrypted using Windows DPAPI for the current Windows user, stored in `%LOCALAPPDATA%\PurpleStream\settings.dat`.

This protects local storage; it does not make an unencrypted HTTP provider connection secure. Your IPTV provider receives account and stream requests. Public country-flag requests to Flagcdn contain country codes, not provider credentials; drawn offline flags are available as a fallback. No app analytics or cloud account service is implemented. Operating-system and dependency behavior is outside this statement.

Downloads, recordings and snapshots are ordinary local media files. Anyone with access to those files may view them. Do not upload settings, personal playlists, logs containing URLs or account screenshots to GitHub. Review diagnostic files before sharing. The documentation gallery uses isolated synthetic data and does not load saved accounts.