const assert = require('node:assert/strict');
const fs = require('node:fs');
const os = require('node:os');
const path = require('node:path');
const vm = require('node:vm');
const { test } = require('node:test');
const webpack = require('webpack');
const TerserPlugin = require('terser-webpack-plugin');
const nextConfig = require('../next.config');

test('legacy build uses SHA256 and still minifies executable production assets', async () => {
  const directory = fs.mkdtempSync(path.join(os.tmpdir(), 'stomp-minifier-probe-'));
  try {
    const input = path.join(directory, 'input.js');
    fs.writeFileSync(input, `/*! MINIFIER_LICENSE_MARKER */
      function MINIFIER_PROBE_UNMINIFIED(firstNumber, secondNumber) {
        return firstNumber + secondNumber;
      }
      console.log(MINIFIER_PROBE_UNMINIFIED(20, 22));
    `);
    let otherMinimizerApplied = false;
    const otherMinimizer = { apply() { otherMinimizerApplied = true; } };
    const config = nextConfig.webpack({
      mode: 'production',
      context: directory,
      entry: input,
      output: { path: path.join(directory, 'output'), filename: 'probe.js' },
      optimization: { minimize: true, minimizer: [() => { throw new Error('Old minifier must be replaced'); }, otherMinimizer] },
      plugins: [new webpack.HashedModuleIdsPlugin()],
    }, {
      defaultLoaders: { babel: { options: { cache: true } } },
      webpack,
      config: { experimental: { cpus: 1 } },
    });
    assert.equal(config.optimization.minimize, true);
    assert.ok(config.optimization.minimizer[0] instanceof TerserPlugin);
    assert.equal(config.optimization.minimizer[0].options.cache, false);
    assert.equal(config.optimization.minimizer[0].options.extractComments, false);
    assert.equal(config.optimization.minimizer[0].options.parallel, 1);
    assert.equal(config.optimization.minimizer[1], otherMinimizer);
    assert.equal(config.plugins[0].options.hashFunction, 'sha256');
    await new Promise((resolve, reject) => {
      webpack(config).run((error, stats) => {
        if (error) return reject(error);
        if (stats.hasErrors()) return reject(new Error(stats.toString({ all: false, errors: true })));
        resolve();
      });
    });
    assert.equal(otherMinimizerApplied, true);
    const asset = fs.readFileSync(path.join(directory, 'output', 'probe.js'), 'utf8');
    assert.ok(!asset.includes('MINIFIER_PROBE_UNMINIFIED'), 'Terser must compress/mangle the fixture');
    assert.ok(!asset.includes('MINIFIER_LICENSE_MARKER'), 'Next comment behavior must be preserved');
    let value;
    vm.runInNewContext(asset, { console: { log(result) { value = result; } } });
    assert.equal(value, 42);
  } finally {
    fs.rmSync(directory, { recursive: true, force: true });
  }
});
