// service-worker.js
importScripts('https://storage.googleapis.com/workbox-cdn/releases/5.1.2/workbox-sw.js');

const CACHE_NAME = 'market-analytic-hub-cache-v1';
const offlineFallbackPage = "offline.html";
const urlsToCache = ['/', offlineFallbackPage];

self.addEventListener("message", (event) => {
  if (event.data && event.data.type === "SKIP_WAITING") {
    self.skipWaiting();
  }
});

// Install event: cache necessary resources
self.addEventListener('install', event => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then(cache => {
        return cache.addAll(urlsToCache);
      })
      .catch(err => {
        console.error('Failed to cache resources during install:', err);
      })
  );
});

// Handle Push Event

self.addEventListener('push', (event) => {
  let options = {
    body: 'Default body text', // Fallback text if payload is missing
    icon: '/assets/icons/marketanalytic_hub_icon_48x48.png',
    data: { path: '/' } // Default path
  };

  if (event.data) {
    let payload;
    try {
      // Attempt to parse as JSON
      payload = event.data.json();
    } catch (err) {
      console.error('Failed to parse push event data as JSON:', err);
      // If parsing fails, treat the data as text
      payload = { title: 'Notification', body: event.data.text() };
    }

    options.body = payload.body || options.body;
    options.data.path = payload.path || options.data.path;
    options.icon = payload.icon || options.icon;

    event.waitUntil(
      self.registration.showNotification(payload.title || 'Notification Title', options)
    );
  } else {
    event.waitUntil(
      self.registration.showNotification('Notification Title', options)
    );
  }
});


// Handle Notification Click
self.addEventListener('notificationclick', (event) => {
  event.notification.close();
  const fullPath = new URL(event.notification.data.path, self.location.origin).href;

  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true }).then(windowClients => {
      for (let client of windowClients) {
        if (client.url === fullPath && 'focus' in client) {
          return client.focus();
        }
      }
      if (clients.openWindow) {
        return clients.openWindow(fullPath);
      }
    }).catch(err => {
      console.error('Failed to open window:', err);
    })
  );
});

// Cache and Return Requests
self.addEventListener('fetch', event => {
  event.respondWith(
    caches.match(event.request)
      .then(response => {
        if (response) {
          return response;
        }
        return fetch(event.request);
      })
      .catch(err => {
        console.error('Fetch failed; returning offline page:', err);
        if (event.request.mode === 'navigate') {
          return caches.match(offlineFallbackPage);
        }
      })
  );
});

// Update a service worker
self.addEventListener('activate', event => {
  const cacheWhitelist = [CACHE_NAME];
  event.waitUntil(
    caches.keys().then(cacheNames => {
      return Promise.all(
        cacheNames.map(cacheName => {
          if (!cacheWhitelist.includes(cacheName)) {
            return caches.delete(cacheName);
          }
        })
      );
    }).then(() => {
      return self.clients.claim();
    }).catch(err => {
      console.error('Failed to activate service worker:', err);
    })
  );
});
