export interface NotificationDto {
  id: number;
  userId?: number;
  title: string;
  type: string;
  message: string;
  actionUrl?: string;
  isRead: boolean;
  createdAt: Date;
}