const assert = require('node:assert/strict');
const vm = require('node:vm');
const mode = process.argv[2];
const substitution = '$<'.repeat(150000) + 'tail';

if (mode === 'runtime' || mode === 'external') {
  const prefix = mode === 'runtime' ? 'next/dist/compiled/@babel/runtime' : '@babel/runtime';
  const wrapRegExp = require(`${prefix}/helpers/wrapRegExp`);
  const expression = wrapRegExp(/(a)/, { word: 1 });
  assert.equal('a'.replace(expression, '$<word>'), 'a');
  assert.equal('a'.replace(expression, substitution), substitution);
  console.log(JSON.stringify({ mode, version: require(`${prefix}/package.json`).version }));
} else if (mode === 'compiler') {
  const babel = require('next/dist/compiled/babel/core');
  const preset = require('next/dist/compiled/babel/preset-env');
  const result = babel.transformSync('globalThis.result = "a".replace(/(?<word>a)/, globalThis.substitution);', {
    configFile: false,
    babelrc: false,
    presets: [[preset, { targets: { ie: '11' }, modules: false }]],
  });
  for (const replacement of ['$<word>', substitution]) {
    const context = { substitution: replacement };
    vm.runInNewContext(result.code, context, { timeout: 2500 });
    assert.equal(context.result, replacement === '$<word>' ? 'a' : replacement);
  }
  console.log(JSON.stringify({ mode, version: babel.version }));
} else {
  throw new Error(`Unknown helper probe: ${mode}`);
}
