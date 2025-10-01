
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import type { NotificationDto } from "../types/NotificationDto";
import { authApi, endpoints } from "./api";


export const fetchNotifications = async () : Promise<NotificationDto[]> => {
  const response = await authApi.get<NotificationDto[]>(endpoints.notification);
  return response.data;
};

export const markReadAllNotifications = async () => {
  const response = await authApi.put(endpoints.markReadAllNotification);
  return response.data;
};

export const deleteNotification = async (notificationId: number) => {
  const response = await authApi.delete(endpoints.deleteNotification(notificationId));
  return response.data;
};

export const markReadNotifications = async (notificationId: number) => {
  const response = await authApi.put(endpoints.markReadNotification(notificationId));
  return response.data;
};

export const createHubConnection = () => {
  return new HubConnectionBuilder()
    .withUrl(`https://localhost:7272/notificationHub`, {
      accessTokenFactory: () => localStorage.getItem("accessToken") || "",
    })
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();
};