const crypto = require('node:crypto');

// Audit the real client compilation after Next's native minimizer has run.
// This emits test metadata; it does not replace or configure a compiler/minifier.
class BuildValidationPlugin {
  apply(compiler) {
    compiler.hooks.thisCompilation.tap('BuildValidationPlugin', (compilation) => {
      compilation.hooks.processAssets.tap({
        name: 'BuildValidationPlugin',
        stage: compiler.webpack.Compilation.PROCESS_ASSETS_STAGE_REPORT,
      }, () => {
        const assets = compilation.getAssets().filter((asset) => asset.name.endsWith('.js')).map((asset) => {
          const content = Buffer.from(asset.source.source());
          return {
            name: asset.name,
            size: content.length,
            sha256: crypto.createHash('sha256').update(content).digest('hex'),
            minimized: asset.info.minimized === true,
          };
        });
        compilation.emitAsset('static/build-validation.json', new compiler.webpack.sources.RawSource(JSON.stringify({
          minimize: compiler.options.optimization.minimize,
          assets,
        })));
      });
    });
  }
}

module.exports = BuildValidationPlugin;
