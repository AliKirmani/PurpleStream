# Publish on GitHub

1. Create or select the intended repository. Upload the contents of this folder as source, excluding bin/obj and all local settings. Do not commit installer/portable binaries into Git history.
2. Set the repository description to: **Independent native Windows IPTV player with country/genre browsing, multiscreen, EPG, catch-up and recording.** Suggested topics: `iptv`, `windows`, `winforms`, `csharp`, `libvlc`, `m3u`, `xtream-codes`.
3. Open Releases → Draft a new release; use tag `v0.4.3`, title **Purple Stream 0.4.3**, and the text in docs/RELEASE-v0.4.3.md. Attach the existing setup EXE, portable ZIP and SHA256SUMS.txt from the separate release upload folder. Review and publish when ready. GitHub's source ZIP is generated automatically from the repository tag.
4. For GitHub Pages, open Settings → Pages → Source → GitHub Actions, then Actions → Deploy GitHub Pages → Run workflow. The static site contains screenshots and links to https://github.com/AliKirmani/PurpleStream. Review the resulting URL in the deployment output. A custom domain requires updating the site's repository links. Pages availability/visibility depends on your GitHub plan and repository configuration.
5. Enable private vulnerability reporting in repository Settings where available. Use issue forms for normal reports; never accept passwords or playlists in public issues.

The Windows build workflow runs on pushes and pull requests. The manual release-build workflow creates downloadable workflow artifacts, not a public release. Existing tested 0.4.3 assets may be uploaded directly; newly built assets require validation. No workflow automatically publishes a release.
