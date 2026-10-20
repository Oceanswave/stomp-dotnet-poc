const assert = require('node:assert/strict');
const { execFileSync } = require('node:child_process');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');

for (const mode of ['runtime', 'external', 'compiler']) {
  test(`actual Next ${mode} handles the Babel advisory replacement pattern`, () => {
    // A vulnerable helper hangs on this bounded offline input. Isolate it so a
    // regression fails within five seconds instead of blocking the test runner.
    const output = execFileSync(process.execPath, [path.join(__dirname, 'fixtures/babel-helper-probe.js'), mode], {
      timeout: 5000,
      encoding: 'utf8',
    });
    const result = JSON.parse(output);
    const version = result.version.split('.').map(Number);
    assert.ok(version[0] > 7 || (version[0] === 7 && (version[1] > 26 || (version[1] === 26 && version[2] >= 10))));
  });
}

test('the actual native Next SWC minimizer produces executable minified code', async () => {
  const { minify, loadBindings } = require('next/dist/build/swc');
  await loadBindings();
  const result = await minify(Buffer.from(`/*! MINIFIER_LICENSE_MARKER */
    (function () {
    function MINIFIER_PROBE_UNMINIFIED(firstNumber, secondNumber) {
      return firstNumber + secondNumber;
    }
    console.log(MINIFIER_PROBE_UNMINIFIED(20, 22));
    })();
  `), { compress: true, mangle: true, module: false, format: { comments: false } });
  assert.ok(!result.code.includes('MINIFIER_PROBE_UNMINIFIED'));
  assert.ok(!result.code.includes('MINIFIER_LICENSE_MARKER'));
  let value;
  vm.runInNewContext(result.code, { console: { log(result) { value = result; } } });
  assert.equal(value, 42);
});
