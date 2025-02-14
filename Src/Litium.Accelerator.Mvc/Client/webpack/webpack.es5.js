const path = require('path');
const BundleAnalyzerPlugin = require('webpack-bundle-analyzer')
    .BundleAnalyzerPlugin;

const ROOT = path.resolve(__dirname, '../');
const JS_DIR = path.resolve(ROOT, 'Scripts');
const BUILD_DIR = path.resolve(ROOT, '../wwwroot/ui');

const common = require('./webpack.common.js');
const { merge } = require('webpack-merge');

const NEED_COMPILE_NODE_MODULES = ['yup', '@react-hook'];

module.exports = merge(common, {
    target: ['web', 'es5'],
    entry: {
        app: [
            'whatwg-fetch',
            'abortcontroller-polyfill/dist/abortcontroller-polyfill-only',
            path.resolve(JS_DIR, 'index.js'),
        ],
    },
    output: {
        path: path.resolve(BUILD_DIR, 'es5'),
        publicPath: '/ui/es5/',
    },
    module: {
        rules: [
            {
                test: /\.js(x?)$/,
                include: [
                    JS_DIR,
                    ...NEED_COMPILE_NODE_MODULES.map((module) =>
                        path.resolve(ROOT, '..', 'node_modules', module)
                    ),
                ],
                use: [
                    {
                        loader: 'babel-loader',
                        options: {
                            presets: [
                                [
                                    '@babel/preset-env',
                                    {
                                        targets: 'defaults, ie >= 11',
                                        useBuiltIns: 'usage',
                                        corejs: '3.10',
                                    },
                                ],
                                '@babel/preset-react',
                            ],
                            plugins: [],
                        },
                    },
                ],
            },
        ],
    },
    plugins: [
        // new BundleAnalyzerPlugin(),
    ],
});
