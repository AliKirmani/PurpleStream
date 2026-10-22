# Contributing

Open an issue describing the problem or proposed change before large work. Keep reports reproducible with synthetic data. Never submit paid playlists, provider passwords, tokens, personal settings or copyrighted media.

Build and run the relevant checks in docs/BUILD.md. Playback/UI changes need interactive native tests; directory changes should include representative synthetic metadata. Keep native calls off the UI thread, preserve independent multiscreen state, and do not log credential-bearing URLs. Explain behavior changes and validation in the pull request template.

Use original assets or material with compatible licensing and document third-party dependencies. Contributions are provided under this project's MIT application-code license unless a file explicitly carries another license. Maintainers review changes before merging; response times are not guaranteed.