const webpack = require('webpack');
const { merge } = require('webpack-merge');
const configEs5 = require('./webpack.es5.js');
const configEs6 = require('./webpack.es6.js');

const ENV = (process.env.ENV = 'development');

module.exports = (env) => {
    return merge(env.ES5 ? configEs5 : configEs6, {
        mode: ENV,
        plugins: [
            new webpack.DefinePlugin({
                'process.env.NODE_ENV': JSON.stringify(ENV),
            }),
        ],
    });
};
