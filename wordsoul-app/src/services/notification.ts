
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import type { NotificationDto } from "../types/NotificationDto";
import { authApi, BASE_URL, endpoints } from "./api";


export const fetchNotifications = async (): Promise<NotificationDto[]> => {
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
  // Lấy base URL (bỏ phần /api ở cuối nếu có) để tạo SignalR hub URL
  const hubBaseUrl = BASE_URL.replace(/\/api$/, "");
  return new HubConnectionBuilder()
    .withUrl(`${hubBaseUrl}/notificationHub`, {
      accessTokenFactory: () => localStorage.getItem("accessToken") || "",
    })
    .configureLogging(LogLevel.Information)
    .withAutomaticReconnect()
    .build();
};