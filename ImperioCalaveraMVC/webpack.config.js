const path = require('path');

module.exports = {
    entry: './wwwroot/js/appointments.js',
    output: {
        filename: 'appointments.bundle.js',
        path: path.resolve(__dirname, 'wwwroot/js'),
       // clean: true
    },
    mode: 'development',
    module: {
        rules: [
            {
                test: /\.js$/,
                exclude: /node_modules/,
                use: {
                    loader: 'babel-loader'
                }
            },
            {
                test: /\.css$/i,
                use: ['style-loader', 'css-loader']
            }
        ]
    },
    devtool: 'source-map'
};