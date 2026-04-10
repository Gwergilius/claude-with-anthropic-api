// Production service worker — caches all app assets for offline support.
// The build process generates service-worker-assets.js with the asset manifest.

self.importScripts('./service-worker-assets.js');

self.addEventListener('install', event => event.waitUntil(onInstall(event)));
self.addEventListener('activate', event => event.waitUntil(onActivate(event)));
self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

const cacheNamePrefix = 'blazor-chat-';
const cacheName = `${cacheNamePrefix}${self.assetsManifest.version}`;

const offlineAssetsInclude = [
    /\.dll$/, /\.pdb$/, /\.wasm/, /\.html/, /\.js$/,
    /\.json$/, /\.css$/, /\.woff2?$/, /\.png$/, /\.jpe?g$/, /\.gif$/, /\.ico$/, /\.blat$/, /\.dat$/
];
const offlineAssetsExclude = [/^service-worker\.js$/];

async function onInstall(event) {
    console.info('Service worker: Install');
    self.skipWaiting();

    const assetsRequests = self.assetsManifest.assets
        .filter(asset => offlineAssetsInclude.some(p => p.test(asset.url)))
        .filter(asset => !offlineAssetsExclude.some(p => p.test(asset.url)))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

    const cache = await caches.open(cacheName);
    await cache.addAll(assetsRequests);
}

async function onActivate(event) {
    console.info('Service worker: Activate');
    const keys = await caches.keys();
    await Promise.all(
        keys.filter(k => k.startsWith(cacheNamePrefix) && k !== cacheName)
            .map(k => caches.delete(k))
    );
}

async function onFetch(event) {
    if (event.request.method !== 'GET') return fetch(event.request);

    const request = event.request.mode === 'navigate'
        ? new Request('index.html')
        : event.request;

    const cache = await caches.open(cacheName);
    const cached = await cache.match(request);
    return cached || fetch(event.request);
}
