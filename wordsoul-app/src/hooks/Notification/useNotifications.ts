/* eslint-disable @typescript-eslint/no-explicit-any */
import { useEffect, useState } from "react";
import { createHubConnection } from "../../services/notification";
import type { NotificationDto } from "../../types/NotificationDto";

export const useNotifications = (userId?: number) => {
  const [connection, setConnection] = useState<null | any>(null); // Adjust type based on your SignalR types
  const [notifications, setNotifications] = useState<NotificationDto[]>([]);

  useEffect(() => {
    const newConnection = createHubConnection();
    setConnection(newConnection);

    newConnection
      .start()
      .then(() => {
        console.log("SignalR Connected!");
        if (userId) {
          newConnection.on("ReceiveNotification", (notification: NotificationDto) => {
            setNotifications((prev) => [notification, ...prev]);
          });
        }
      })
      .catch((e: Error) => console.log("Connection failed: ", e));

    // Cleanup on unmount
    return () => {
      newConnection.stop();
    };
  }, [userId]);

  return { connection, notifications, setNotifications };
};