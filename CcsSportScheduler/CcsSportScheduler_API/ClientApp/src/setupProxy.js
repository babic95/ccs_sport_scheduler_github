const { createProxyMiddleware } = require('http-proxy-middleware');
const { env } = require('process');

const target = env.ASPNETCORE_HTTPS_PORT ? `https://localhost:${env.ASPNETCORE_HTTPS_PORT}` :
    env.ASPNETCORE_URLS ? env.ASPNETCORE_URLS.split(';')[0] : 'http://localhost:54172';

const context = [
    "/swagger",
    "/api/klubs",
    "/api/users",
    "/api/obavestenjas",
    "/api/racuns",
    "/api/termins",
    "/api/uplatas",
];

const onError = (err, req, resp, target) => {
    console.error(`Proxy error: ${err.message}`);
}

module.exports = function (app) {
    const appProxy = createProxyMiddleware(context, {
        target: target,
        onError: onError,
        secure: false,
        changeOrigin: true,
        headers: {
            Connection: 'Keep-Alive'
        }
    });

    app.use(appProxy);
};
