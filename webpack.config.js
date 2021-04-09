// Note this only includes basic configuration for development mode.
// For a more comprehensive configuration check:
// https://github.com/fable-compiler/webpack-config-template

const path = require("path");
const HtmlWebpackPlugin = require("html-webpack-plugin");
const MiniCssExtractPlugin = require("mini-css-extract-plugin");
const FixStyleOnlyEntriesPlugin = require("webpack-fix-style-only-entries");
const OptimizeCSSAssetsPlugin = require("optimize-css-assets-webpack-plugin");

module.exports = {
    mode: "development",
    entry: {
        bundle: "./src/App.fsproj",
        styles: ["./public/main.css", "./public/bootstrap.css"],
    },
    output: {
        path: path.join(__dirname, "./public"),
        filename: "[name].js",
    },
    devServer: {
        contentBase: "./public",
        host: '0.0.0.0',
        port: 8080,
        disableHostCheck: true
    },
    module: {
        rules: [{
            test: /\.fs(x|proj)?$/,
            use: "fable-loader"
        }, {
            test: /\.css$/,
            use: [MiniCssExtractPlugin.loader, "css-loader"]
        }, {
            test: /\.(ttf|eot|svg|png|jpg|gif|ico)(\?v=[0-9]\.[0-9]\.[0-9])?$/,
            loader: 'file-loader'

        }]
    },
    plugins: [
        new HtmlWebpackPlugin({template: "public/index.html"}),
        new MiniCssExtractPlugin({filename: "bootstrap.css"}),
        new FixStyleOnlyEntriesPlugin(),
        new OptimizeCSSAssetsPlugin({})
    ]
}