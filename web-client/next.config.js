const BuildValidationPlugin = require('./build-validation-plugin');

module.exports = {
  webpack(config, { dev, isServer }) {
    if (!dev && !isServer && process.env.BUILD_VALIDATION === '1') {
      config.plugins.push(new BuildValidationPlugin());
    }
    return config;
  },
};
