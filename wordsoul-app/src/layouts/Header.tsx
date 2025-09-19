import { Link } from "react-router-dom"
import { useAuth } from "../store/AuthContext";
import type { NotificationDto } from "../types/Dto";
import { useEffect, useMemo, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { deleteNotification, fetchNotifications, markReadAllNotifications, markReadNotifications } from "../services/notification";

const Header: React.FC = () => {
    const { user, logout } = useAuth();
    const [notifications, setNotifications] = useState<NotificationDto[]>([]);
    const [isDropdownOpen, setIsDropdownOpen] = useState(false);
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const [connection, setConnection] = useState<any>(null);

    // Initialize SignalR connection
    useEffect(() => {
        const newConnection = new HubConnectionBuilder()
            .withUrl("https://localhost:7272/notificationHub", {
                accessTokenFactory: () => localStorage.getItem("accessToken") || "", // Use your token storage
            })
            .configureLogging(LogLevel.Information)
            .withAutomaticReconnect()
            .build();

        setConnection(newConnection);
    }, []);

    const unreadCount = useMemo(() => notifications.filter((n) => !n.isRead).length, [notifications]);

    // Start connection and handle real-time notifications
    useEffect(() => {
        if (connection) {
            connection
                .start()
                .then(() => {
                    console.log("SignalR Connected!");
                    if (user?.id) {
                        connection.on("ReceiveNotification", (notification: NotificationDto) => {
                            setNotifications((prev) => [notification, ...prev]); // Add new notification
                        });
                    }
                })
                .catch((e: Error) => console.log("Connection failed: ", e));

            return () => {
                connection.stop(); // Cleanup on unmount
            };
        }
    }, [connection, user?.id]);

    // Fetch initial notifications when user is logged in
    useEffect(() => {
        if (user?.id) {
            fetchNotifications()
                .then((data) => setNotifications(data))
                .catch((error) => console.error("Failed to fetch notifications:", error));
        }
    }, [user?.id]);

    // Toggle dropdown
    const toggleDropdown = () => {
        setIsDropdownOpen(!isDropdownOpen);
    };

    // Mark a notification as read
    const handleMarkRead = (id: number) => {
        markReadNotifications(id)
            .then(() => {
                setNotifications((prev) =>
                    prev.map((n) => (n.id === id ? { ...n, isRead: true } : n))
                );
            })
            .catch((error) => console.error("Failed to mark as read:", error));
    };

    // Delete a notification
    const handleDelete = (id: number) => {
        deleteNotification(id)
            .then(() => {
                setNotifications((prev) => prev.filter((n) => n.id !== id));
            })
            .catch((error) => console.error("Failed to delete notification:", error));
    };

    // Mark all as read
    const handleMarkAllRead = () => {
        markReadAllNotifications()
            .then(() => {
                setNotifications((prev) => prev.map((n) => ({ ...n, isRead: true })));
            })
            .catch((error) => console.error("Failed to mark all as read:", error));
    };

    return (
        <>
            <div className="flex items-center justify-between background-color px-10 py-2 shadow-sm fixed top-0 left-0 z-1000 w-screen">
                <div className="container mx-auto flex items-center justify-between w-7/12" >
                    {/* Logo Section */}
                    <h2 className="flex items-center cursor-pointer ">
                        <div>
                            <img
                                src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756452551/coin-logo_ysauhp.png"
                                width="25"
                                height="28"
                                alt="coin logo"
                                className="animate-pulse"
                            />
                        </div>
                        <Link to="/" className="ml-2 text-s font-press text-white hover:text-blue-700 custom-cursor">
                            WordSoul
                        </Link>
                    </h2>

                    {/* Navigation Links and User Actions */}
                    <div className="flex items-center justify-center gap-4 ">
                        {/* Navigation Links */}
                        <div className="flex items-center gap-2 text-xs">
                            <div className="relative">
                                <Link to="/vocabularySet">
                                    <button className="flex items-center gap-1 px-4 py-2 text-white hover:text-blue-400 hover:bg-slate-800 rounded-md custom-cursor">
                                        Bộ từ vựng
                                        <span>
                                            <svg
                                                width="20"
                                                height="20"
                                                viewBox="0 0 20 20"
                                                fill="none"
                                                xmlns="http://www.w3.org/2000/svg"
                                            >
                                                <path
                                                    fillRule="evenodd"
                                                    clipRule="evenodd"
                                                    d="M5.83317 6.6665H4.1665V8.33317H5.83317V9.99984H7.49984V11.6665H9.1665V13.3332H10.8332V11.6665H12.4998V9.99984H14.1665V8.33317H15.8332V6.6665H14.1665V8.33317H12.4998V9.99984H10.8332V11.6665H9.1665V9.99984H7.49984V8.33317H5.83317V6.6665Z"
                                                    fill="#64748B"
                                                />
                                            </svg>
                                        </span>
                                    </button>
                                </Link>
                            </div>
                            <div className="relative">
                                <button className="flex items-center gap-1 px-4 py-2 text-white hover:text-blue-400 hover:bg-slate-800 rounded-md custom-cursor " >
                                    Ôn tập
                                    <span>
                                        <svg
                                            width="20"
                                            height="20"
                                            viewBox="0 0 20 20"
                                            fill="none"
                                            xmlns="http://www.w3.org/2000/svg"
                                        >
                                            <path
                                                fillRule="evenodd"
                                                clipRule="evenodd"
                                                d="M5.83317 6.6665H4.1665V8.33317H5.83317V9.99984H7.49984V11.6665H9.1665V13.3332H10.8332V11.6665H12.4998V9.99984H14.1665V8.33317H15.8332V6.6665H14.1665V8.33317H12.4998V9.99984H10.8332V11.6665H9.1665V9.99984H7.49984V8.33317H5.83317V6.6665Z"
                                                fill="#64748B"
                                            />
                                        </svg>
                                    </span>
                                </button>
                            </div>
                            <div>
                                <Link to="/pets">
                                    <button className="px-4 py-2 text-white hover:text-blue-400 hover:bg-slate-800 rounded-md custom-cursor " >
                                        Linh Thú
                                    </button>
                                </Link>
                            </div>
                            <div className="relative">
                                <Link to="/community">
                                    <button className="flex items-center gap-1 px-4 py-2 text-white hover:text-blue-400 hover:bg-slate-800 rounded-md custom-cursor">
                                        Cộng đồng
                                        <span>
                                            <svg
                                                width="20"
                                                height="20"
                                                viewBox="0 0 20 20"
                                                fill="none"
                                                xmlns="http://www.w3.org/2000/svg"
                                            >
                                                <path
                                                    fillRule="evenodd"
                                                    clipRule="evenodd"
                                                    d="M5.83317 6.6665H4.1665V8.33317H5.83317V9.99984H7.49984V11.6665H9.1665V13.3332H10.8332V11.6665H12.4998V9.99984H14.1665V8.33317H15.8332V6.6665H14.1665V8.33317H12.4998V9.99984H10.8332V11.6665H9.1665V9.99984H7.49984V8.33317H5.83317V6.6665Z"
                                                    fill="#64748B"
                                                />
                                            </svg>
                                        </span>
                                    </button>
                                </Link>
                            </div>
                        </div>
                    </div>
                    <div className="flex items-center justify-center gap-4">
                        {/* User Actions */}
                        <div className="flex items-center gap-4">
                            {/* Notification Bell with Dropdown */}
                            {user ? (
                                <div className="relative icon-container nes-pointer ">
                                    <svg
                                        xmlns="http://www.w3.org/2000/svg"
                                        width="24"
                                        height="24"
                                        viewBox="0 0 24 24"
                                        fill="none"
                                        onClick={toggleDropdown}
                                        className="custom-cursor"
                                    >
                                        <path
                                            fillRule="evenodd"
                                            clipRule="evenodd"
                                            d="M14 4V2H9.99999V4H5.00018V6H19.0002V4H14ZM18.9999 16H4.99994V12H2.99994V16V18L7.99975 18V22H9.99975V18H13.9998V20H10V22H13.9998V22H15.9998V18L20.9999 18V16L21 12H19V6H17V14H18.9999V16ZM5.00018 6V14H7.00018V6H5.00018Z"
                                            fill={notifications.some((n) => !n.isRead) ? "#FFD700" : "#94A3B8"} // Yellow if unread
                                        />
                                    </svg>
                                    {unreadCount > 0 && (
                                        <span className="absolute top-0 right-0 inline-flex items-center justify-center w-4 h-4 text-xs font-bold text-white bg-red-500 rounded-full -mt-1 -mr-1">
                                            {unreadCount}
                                        </span>
                                    )}
                                    {isDropdownOpen && (
                                        <div className="absolute right-0 mt-2 w-64 bg-white rounded-md shadow-lg z-50">
                                            <div className="p-2">
                                                <div className="flex justify-between items-center mb-2">
                                                    <h3 className="text-sm font-semibold">Thông báo</h3>
                                                    <button
                                                        onClick={handleMarkAllRead}
                                                        className="text-xs text-blue-500 hover:text-blue-700 custom-cursor"
                                                    >
                                                        Đánh dấu tất cả đã đọc
                                                    </button>
                                                </div>
                                                {notifications.length > 0 ? (
                                                    notifications.map((notif) => (
                                                        <div
                                                            key={notif.id}
                                                            className={`p-2 border-b ${!notif.isRead ? "bg-gray-100" : ""}`}
                                                        >
                                                            <div className="text-sm">{notif.title}</div>
                                                            <div className="text-xs text-gray-600">{notif.message}</div>
                                                            <div className="text-xs text-gray-400">
                                                                {new Date(notif.createdAt).toLocaleTimeString()}
                                                            </div>
                                                            <div className="flex gap-2 mt-1">
                                                                {!notif.isRead && (
                                                                    <button
                                                                        onClick={() => handleMarkRead(notif.id)}
                                                                        className="text-xs text-blue-500 hover:text-blue-700 custom-cursor"
                                                                    >
                                                                        Đánh dấu đã đọc
                                                                    </button>
                                                                )}
                                                                <button
                                                                    onClick={() => handleDelete(notif.id)}
                                                                    className="text-xs text-red-500 hover:text-red-700 custom-cursor"
                                                                >
                                                                    Xóa
                                                                </button>
                                                            </div>
                                                        </div>
                                                    ))
                                                ) : (
                                                    <div className="text-xs text-gray-500 text-center">Không có thông báo</div>
                                                )}
                                            </div>
                                        </div>
                                    )}
                                </div>
                            ) : null}
                            <div className="custom-cursor">
                                <svg
                                    width="24"
                                    height="24"
                                    viewBox="0 0 24 24"
                                    fill="none"
                                    xmlns="http://www.w3.org/2000/svg"
                                >
                                    <g clipPath="url(#clip0_681_6745)">
                                        <path
                                            fillRule="evenodd"
                                            clipRule="evenodd"
                                            d="M6 2H14V4H12V6H10V4H6V2ZM4 6V4H6V6H4ZM4 16H2V6H4V16ZM6 18H4V16H6V18ZM8 20H6V18H8V20ZM18 20V22H8V20H18ZM20 18V20H18V18H20ZM18 14H20V18H22V10H20V12H18V14ZM12 14V16H18V14H12ZM10 12H12V14H10V12ZM10 12V6H8V12H10Z"
                                            fill="#94A3B8"
                                        />
                                    </g>
                                    <defs>
                                        <clipPath id="clip0_681_6745">
                                            <rect width="24" height="24" fill="white" />
                                        </clipPath>
                                    </defs>
                                </svg>
                            </div>
                            {user ? (
                                <button
                                    onClick={logout}
                                    className="relative flex items-center px-2 py-1.5 bg-yellow-300 text-black rounded-xs hover:bg-yellow-200 custom-cursor"
                                >
                                    <span className="absolute left-0 w-0.5 h-full bg-yellow-500" />
                                    <span className="mx-1 text-xs font-pixel">Đăng xuất</span>
                                    <span className="absolute right-0 w-0.5 h-full bg-yellow-500" />
                                    <span className="absolute top-6.5 right-0 w-full h-1 bg-yellow-500" />
                                    <span className="absolute bottom-6.5 right-0 w-full h-0.5 bg-yellow-500" />
                                </button>
                            ) : (
                                <Link to="/login" className="no-underline">
                                    <button className="relative flex items-center px-2 py-1.5 bg-yellow-300 text-black rounded-xs hover:bg-yellow-200 custom-cursor">
                                        <span className="absolute left-0 w-0.5 h-full bg-yellow-500" />
                                        <span className="mx-1 text-xs font-pixel">Đăng nhập</span>
                                        <span className="absolute right-0 w-0.5 h-full bg-yellow-500" />
                                        <span className="absolute top-6.5 right-0 w-full h-1 bg-yellow-500" />
                                        <span className="absolute bottom-6.5 right-0 w-full h-0.5 bg-yellow-500" />
                                    </button>
                                </Link>
                            )}
                        </div>
                    </div>
                </div>
            </div>

        </>

    );
};

export default Header;