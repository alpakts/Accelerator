const webpack = require('webpack');
const { merge } = require('webpack-merge');
const TerserPlugin = require('terser-webpack-plugin');
const CssMinimizerPlugin = require('css-minimizer-webpack-plugin');
const configEs5 = require('./webpack.es5.js');
const configEs6 = require('./webpack.es6.js');

const ENV = (process.env.ENV = 'production');

module.exports = (env) => {
    return merge(env.ES5 ? configEs5 : configEs6, {
        mode: ENV,
        output: {
            filename: '[name].[contenthash].js',
        },
        optimization: {
            minimizer: [new TerserPlugin(), new CssMinimizerPlugin()],
        },
        plugins: [
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': JSON.stringify(ENV),
            }),
        ],
    });
};
