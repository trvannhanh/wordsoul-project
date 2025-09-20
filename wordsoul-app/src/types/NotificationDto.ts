export interface NotificationDto {
  id: number;
  userId?: number;
  title: string;
  type: string;
  message: string;
  isRead: boolean;
  createdAt: Date;
}