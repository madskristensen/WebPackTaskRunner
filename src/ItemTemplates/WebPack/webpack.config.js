module.exports = {
    entry: "./src/file.js",
    output: {
        filename: "./dist/bundle.js"
    },
    module: {
        loaders: [
            {
                test: /\.js$/,
                loader: 'jsx-loader'
            },
        ]
    }
}