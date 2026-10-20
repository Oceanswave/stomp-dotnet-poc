const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const fs = require('node:fs');
const path = require('node:path');
const { test } = require('node:test');

test('production polyfill SHA256 filename matches content and rendered page reference', () => {
  const build = path.join(__dirname, '..', '.next');
  const manifest = JSON.parse(fs.readFileSync(path.join(build, 'build-manifest.json'), 'utf8'));
  assert.equal(manifest.polyfillFiles.length, 1);
  const file = manifest.polyfillFiles[0];
  const match = /^static\/chunks\/polyfills-([a-f0-9]{64})\.js$/.exec(file);
  assert.ok(match, 'polyfills must use the configured SHA256 filename');
  const content = fs.readFileSync(path.join(build, file));
  assert.equal(crypto.createHash('sha256').update(content).digest('hex'), match[1]);
  const html = fs.readFileSync(path.join(build, 'server', 'pages', 'index.html'), 'utf8');
  assert.ok(html.includes(`/_next/${file}`), 'rendered page must reference the emitted polyfill');
});
