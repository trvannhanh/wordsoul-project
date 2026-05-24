import { authApi, endpoints } from './api';
import type { NotificationDto } from '../types/NotificationDto';

export const fetchNotifications = async (): Promise<NotificationDto[]> => {
  const response = await authApi.get<NotificationDto[]>(
    endpoints.notification,
  );
  return response.data;
};

export const markAllRead = async (): Promise<void> => {
  await authApi.put(endpoints.markReadAllNotification);
};

export const markRead = async (notificationId: number): Promise<void> => {
  await authApi.put(endpoints.markReadNotification(notificationId));
};

export const deleteNotification = async (
  notificationId: number,
): Promise<void> => {
  await authApi.delete(endpoints.deleteNotification(notificationId));
};
