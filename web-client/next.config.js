module.exports = {
  webpack(config, { defaultLoaders }) {
    // Next 9's bundled Babel cache hashes with native MD4, unavailable on modern Node.
    // Compile normally without that disk cache rather than enabling legacy OpenSSL.
    defaultLoaders.babel.options.cache = false;
    return config;
  },
};
