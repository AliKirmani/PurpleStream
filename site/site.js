(() => {
  const repo = 'https://github.com/AliKirmani/PurpleStream';
  document.querySelectorAll('[data-repo]').forEach(a => a.href = repo);
  document.querySelectorAll('[data-release]').forEach(a => a.href = repo + '/releases');
  const note = document.getElementById('repo-note');
  if (note) { note.textContent = ''; const a = document.createElement('a'); a.href = repo; a.textContent = 'View source, documentation and release downloads on GitHub →'; note.append(a); }
})();
