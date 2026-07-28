import { useEffect, useState } from 'react';
import { messaging, getToken, onMessage } from '../config/firebase';
import { authApi } from '../services/api';
import { toast } from '../shared/toast';

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
          }
        }
      } catch (error) {
        if (import.meta.env.DEV) {
          console.error('Unable to configure push notifications.', error);
        }
      }
    };

    requestPermissionAndGetToken();

    // Lắng nghe thông báo khi ứng dụng đang mở (Foreground)
    const unsubscribe = onMessage(messaging, (payload) => {
      toast.info(
        payload.notification?.body ?? 'Bạn có một thông báo mới.',
        {
          id: payload.messageId,
          description: payload.notification?.title,
        },
      );
    });

    return () => {
      unsubscribe();
    };
  }, [isAuthenticated]);

  return { fcmToken };
};
