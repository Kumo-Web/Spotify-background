"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
// server.ts
var https_1 = require("https");
var url_1 = require("url");
var next_1 = require("next");
var fs_1 = require("fs");
var path_1 = require("path");
var port = 5000;
var dev = process.env.NODE_ENV !== "production";
var app = (0, next_1.default)({ dev: dev });
var handle = app.getRequestHandler();
var httpsOptions = {
    key: fs_1.default.readFileSync(path_1.default.join(__dirname, "localhost-key.pem")),
    cert: fs_1.default.readFileSync(path_1.default.join(__dirname, "localhost.pem")),
};
app.prepare().then(function () {
    (0, https_1.createServer)(httpsOptions, function (req, res) {
        var parsedUrl = (0, url_1.parse)(req.url, true);
        handle(req, res, parsedUrl);
    }).listen(port, function () {
        console.log("> Ready on https://localhost:".concat(port));
    });
});
