importScripts('https://www.gstatic.com/firebasejs/10.8.0/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.8.0/firebase-messaging-compat.js');

// You need to replace this with your actual config from firebase.ts
const firebaseConfig = {
  apiKey: "AIzaSyARowuD4tMrzwarl9U9knSr79mQQfiRnZw",
  authDomain: "vocamon-7932b.firebaseapp.com",
  projectId: "vocamon-7932b",
  storageBucket: "vocamon-7932b.firebasestorage.app",
  messagingSenderId: "767175855154",
  appId: "1:767175855154:web:422679e8f21f8ca0a8c02b",
  measurementId: "G-KJVT7K72Z4"
};

firebase.initializeApp(firebaseConfig);

const messaging = firebase.messaging();

// Handle background messages for Data-Only payload
messaging.onBackgroundMessage((payload) => {
  console.log('[firebase-messaging-sw.js] Received background message ', payload);
  
  // Trích xuất dữ liệu từ data payload thay vì notification
  const notificationTitle = payload.data.title || 'Thông báo từ Vocamon';
  const notificationOptions = {
    body: payload.data.body || 'Bạn có một thông báo mới',
    icon: '/logo.png',
    data: { actionUrl: payload.data.actionUrl || '/' }
  };

  self.registration.showNotification(notificationTitle, notificationOptions);
});

// Xử lý sự kiện click vào thông báo
self.addEventListener('notificationclick', function(event) {
  event.notification.close();
  const urlToOpen = event.notification.data?.actionUrl || '/';
  
  event.waitUntil(
    clients.matchAll({ type: 'window', includeUncontrolled: true }).then((windowClients) => {
      let matchingClient = null;
      for (let i = 0; i < windowClients.length; i++) {
        const windowClient = windowClients[i];
        if (windowClient.url.includes(urlToOpen)) {
          matchingClient = windowClient;
          break;
        }
      }

      if (matchingClient) {
        return matchingClient.focus();
      } else {
        return clients.openWindow(urlToOpen);
      }
    })
  );
});
