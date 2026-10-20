const TerserPlugin = require('terser-webpack-plugin');

module.exports = {
  webpack5: false,
  webpack(config, { defaultLoaders, webpack, config: nextConfig }) {
    // Use supported hashing on current Node without a legacy OpenSSL provider.
    config.output.hashFunction = 'sha256';
    config.plugins = config.plugins.map((plugin) => {
      if (plugin instanceof webpack.HashedModuleIdsPlugin) {
        return new webpack.HashedModuleIdsPlugin({ hashFunction: 'sha256' });
      }
      if (plugin.constructor.name === 'CopyFilePlugin' &&
          plugin.name === 'static/chunks/polyfills-[hash].js') {
        return new plugin.constructor({
          filePath: plugin.filePath,
          cacheKey: `${plugin.cacheKey}:sha256`,
          name: 'static/chunks/polyfills-[sha256:hash].js',
          info: plugin.info,
        });
      }
      return plugin;
    });
    // Scope hoisting in the vendored Webpack 4 uses native MD4 internally.
    config.optimization.concatenateModules = false;
    defaultLoaders.babel.options.cache = false;
    // Next 11's first minimizer is JS; retain its CSS minimizer and all others.
    if (!Array.isArray(config.optimization.minimizer) ||
        config.optimization.minimizer.length < 2 ||
        typeof config.optimization.minimizer[0] !== 'function') {
      throw new Error('Unexpected Next 11 minimizer configuration');
    }
    config.optimization.minimizer = [new TerserPlugin({
      cache: false,
      parallel: nextConfig.experimental.cpus,
      sourceMap: true,
      extractComments: false,
      // Preserve the exact Next 11.1.3 Terser configuration (Webpack 4 mode).
      terserOptions: {
        ecma: 5,
        module: false,
        parse: { ecma: 8 },
        compress: { ecma: 5, warnings: false, comparisons: false, inline: 2 },
        mangle: { safari10: true },
        output: { beautify: false, ecma: 5, safari10: true, comments: false, ascii_only: true },
      },
    }), ...config.optimization.minimizer.slice(1)];
    return config;
  },
};
