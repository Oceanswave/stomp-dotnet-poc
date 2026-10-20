const assert = require('node:assert/strict');
const crypto = require('node:crypto');
const fs = require('node:fs');
const path = require('node:path');
const { test } = require('node:test');
const build = path.join(__dirname, '..', '.next');

function digest(file) {
  return crypto.createHash('sha256').update(fs.readFileSync(path.join(build, file))).digest('hex');
}

test('the actual production client assets are minified and match compiler evidence', () => {
  const proof = JSON.parse(fs.readFileSync(path.join(build, 'static/build-validation.json'), 'utf8'));
  assert.equal(proof.minimize, true);
  assert.ok(proof.assets.some((asset) => /pages\/index-/.test(asset.name)));
  assert.ok(proof.assets.some((asset) => /framework-/.test(asset.name)));
  for (const asset of proof.assets) {
    assert.equal(asset.minimized, true, `production asset must be minimized: ${asset.name}`);
    assert.equal(digest(asset.name), asset.sha256, `emitted asset must match compilation: ${asset.name}`);
  }
});

test('production polyfills exist, match compiled content and are referenced by rendered HTML', () => {
  const manifest = JSON.parse(fs.readFileSync(path.join(build, 'build-manifest.json'), 'utf8'));
  const proof = JSON.parse(fs.readFileSync(path.join(build, 'static/build-validation.json'), 'utf8'));
  assert.equal(manifest.polyfillFiles.length, 1);
  const file = manifest.polyfillFiles[0];
  assert.match(file, /^static\/chunks\/polyfills-[a-f0-9]+\.js$/);
  const asset = proof.assets.find((asset) => asset.name === file);
  assert.ok(asset, 'polyfill must exist in the actual compilation');
  assert.equal(digest(file), asset.sha256);
  assert.equal(asset.minimized, true);
  const html = fs.readFileSync(path.join(build, 'server/pages/index.html'), 'utf8');
  assert.ok(html.includes(`/_next/${file}`), 'rendered page must reference the emitted polyfill');
});
