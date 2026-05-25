import { useEffect, useState } from 'react';
import { messaging, getToken, onMessage } from '../config/firebase';
import { authApi } from '../services/api';

export const useFCM = (isAuthenticated: boolean) => {
  const [fcmToken, setFcmToken] = useState<string | null>(null);

  useEffect(() => {
    if (!isAuthenticated) return;

    const requestPermissionAndGetToken = async () => {
      try {
        const permission = await Notification.requestPermission();
        if (permission === 'granted') {
          // You may need to pass vapidKey here if getToken fails: getToken(messaging, { vapidKey: "YOUR_PUBLIC_VAPID_KEY" })
          const token = await getToken(messaging);
          if (token) {
            setFcmToken(token);
            // Send token to backend
            await authApi.put('/users/me/fcm-token', { token });
            console.log("FCM Token sent to backend:", token);
          } else {
            console.log('No registration token available. Request permission to generate one.');
          }
        } else {
          console.log('Unable to get permission to notify.');
        }
      } catch (error) {
        console.error('An error occurred while retrieving token. ', error);
      }
    };

    requestPermissionAndGetToken();

    // Lắng nghe thông báo khi ứng dụng đang mở (Foreground)
    const unsubscribe = onMessage(messaging, (payload) => {
      console.log('Message received. ', payload);
      // Có thể custom việc hiển thị toast notification ở đây nếu muốn
    });

    return () => {
      unsubscribe();
    };
  }, [isAuthenticated]);

  return { fcmToken };
};
