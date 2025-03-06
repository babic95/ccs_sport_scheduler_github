/* eslint-disable no-restricted-globals */

import { clientsClaim } from 'workbox-core';
import { ExpirationPlugin } from 'workbox-expiration';
import { precacheAndRoute, createHandlerBoundToURL } from 'workbox-precaching';
import { registerRoute } from 'workbox-routing';
import { StaleWhileRevalidate } from 'workbox-strategies';

clientsClaim();

precacheAndRoute(self.__WB_MANIFEST);

const fileExtensionRegexp = new RegExp('/[^/?]+\\.[^/]+$');
registerRoute(
    ({ request, url }) => {
        if (request.mode !== 'navigate') {
            return false;
        }
        if (url.pathname.startsWith('/_')) {
            return false;
        }
        if (url.pathname.match(fileExtensionRegexp)) {
            return false;
        }
        return true;
    },
    createHandlerBoundToURL('%PUBLIC_URL%/index.html')
);

registerRoute(
    ({ url }) => url.origin === self.location.origin && url.pathname.endsWith('.png'),
    new StaleWhileRevalidate({
        cacheName: 'images',
        plugins: [
            new ExpirationPlugin({ maxEntries: 50 }),
        ],
    })
);

self.addEventListener('install', (event) => {
    event.waitUntil(
        caches.open('ccs-sport-scheduler-v1').then((cache) => {
            return cache.addAll([
                '/',
                '/index.html',
                '/manifest.json',
                '/assets/logo.png',
                '/assets/logo_512x512.png',
                '/assets/logo_192x192.png',
                // Dodaj ostale resurse koje treba keširati
            ]);
        })
    );
});

self.addEventListener('message', (event) => {
    if (event.data && event.data.type === 'SKIP_WAITING') {
        self.skipWaiting();
    }
});

self.addEventListener('notificationclick', (event) => {
    event.notification.close();
    event.waitUntil(
        self.clients.matchAll({ type: 'window', includeUncontrolled: true }).then((clientList) => {
            for (const client of clientList) {
                if (client.url.includes('tksirmium.com') && 'focus' in client) {
                    return client.focus();
                }
            }
            if (self.clients.openWindow) {
                return self.clients.openWindow('/');
            }
        })
    );
});

self.addEventListener('fetch', (event) => {
    if (event.request.mode === 'navigate') {
        event.respondWith(
            (async () => {
                const clientList = await self.clients.matchAll({ type: 'window', includeUncontrolled: true });
                for (const client of clientList) {
                    const urlsToMatch = [
                        'tksirmium.com/schedule',
                        'tksirmium.com/profile',
                        'tksirmium.com/financial-card',
                        'tksirmium.com/financial-card-all',
                        'tksirmium.com/new-notification',
                    ];
                    if (urlsToMatch.some(url => client.url.includes(url))) {
                        client.focus();
                        return new Response('', { status: 200, statusText: 'OK' });
                    }
                }
                return fetch(event.request);
            })()
        );
    }
});