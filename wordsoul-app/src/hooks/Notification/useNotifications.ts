import { useEffect, useState } from "react";
import { createHubConnection } from "../../services/notification";
import type { NotificationDto } from "../../types/NotificationDto";

export const useNotifications = (userId?: number) => {
  const [connection, setConnection] = useState<ReturnType<typeof createHubConnection> | null>(null);
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);

  useEffect(() => {
    const newConnection = createHubConnection();
    setConnection(newConnection);

    newConnection
      .start()
      .then(() => {
        if (userId) {
          newConnection.on("ReceiveNotification", (notification: NotificationDto) => {
            setNotifications((prev) => [notification, ...prev]);
          });
        }
      })
      .catch(() => {
        // SignalR reconnect behavior is handled by the connection configuration.
      });

    // Cleanup on unmount
    return () => {
      newConnection.stop();
    };
  }, [userId]);

  return { connection, notifications, setNotifications };
};
