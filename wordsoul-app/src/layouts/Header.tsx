import { Link } from "react-router-dom";
import { useEffect, useMemo, useState } from "react";
import { deleteNotification, fetchNotifications, markReadAllNotifications, markReadNotifications } from "../services/notification";
import { useNotifications } from "../hooks/Notification/useNotifications";
import { useAuth } from "../hooks/Auth/useAuth";

const Header: React.FC = () => {
    const { user, logout } = useAuth();
    const { notifications, setNotifications } = useNotifications(user?.id);
    const [isNotificationSidebarOpen, setIsNotificationSidebarOpen] = useState(false);
    const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
    const [isDarkMode, setIsDarkMode] = useState<boolean>(() => {
        // Khôi phục chế độ từ localStorage, mặc định là dark mode
        const savedTheme = localStorage.getItem("theme");
        return savedTheme ? savedTheme === "dark" : true; // Mặc định là true (dark mode) nếu không có giá trị trong localStorage
    });

    const unreadCount = useMemo(() => notifications.filter((n) => !n.isRead).length, [notifications]);

    // Áp dụng theme và lưu vào localStorage
    useEffect(() => {
        if (isDarkMode) {
            document.documentElement.classList.add("dark");
            localStorage.setItem("theme", "dark");
        } else {
            document.documentElement.classList.remove("dark");
            localStorage.setItem("theme", "light");
        }
    }, [isDarkMode]);

    // Toggle dark/light mode
    const toggleTheme = () => {
        setIsDarkMode(!isDarkMode);
    };

    // Fetch initial notifications
    useEffect(() => {
        if (user?.id) {
            fetchNotifications()
                .then((data) => setNotifications(data))
                .catch((error) => console.error("Failed to fetch notifications:", error));
        }
    }, [user?.id, setNotifications]);

    // Toggle sidebar thông báo
    const toggleNotificationSidebar = () => {
        setIsNotificationSidebarOpen(!isNotificationSidebarOpen);
    };

    // Toggle menu mobile
    const toggleMobileMenu = () => {
        setIsMobileMenuOpen(!isMobileMenuOpen);
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
            {/* Header chính */}
            <div className="flex items-center justify-between background-color text-color px-4 sm:px-6 lg:px-10 py-2 shadow-sm fixed top-0 left-0 z-50 w-full">
                <div className="container mx-auto flex items-center justify-between w-full sm:w-10/12 lg:w-7/12 max-w-7xl">
                    {/* Logo Section */}
                    <h2 className="flex items-center cursor-pointer">
                        <div>
                            <img
                                src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759222012/egg-logo_pflvdz.png"
                                width="35"
                                height="35"
                                alt="coin logo"
                                className="animate-pulse "
                            />
                        </div>
                        {user ? (
                            <Link to="/home" className="ml-2 text-xs sm:text-sm font-press hover:text-blue-700 custom-cursor">
                                WordSoul
                            </Link>
                        ) : (
                            <Link to="/" className="ml-2 text-xs sm:text-sm font-press  hover:text-blue-700 custom-cursor">
                                WordSoul
                            </Link>
                        )}
                    </h2>

                    {/* Hamburger Menu cho mobile */}
                    <div className="md:hidden">
                        <button onClick={toggleMobileMenu} className="custom-cursor">
                            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 6h16M4 12h16M4 18h16" />
                            </svg>
                        </button>
                    </div>

                    {/* Navigation Links */}
                    <div className="hidden md:flex items-center justify-center gap-2 text-xs">
                        <div className="relative">
                            <Link to="/vocabularySet">
                                <button className="flex items-center gap-1 px-3 py-2  hover:text-blue-400 hover:bg-gray-200 dark:hover:bg-gray-800 rounded-md custom-cursor">
                                    Bộ từ vựng
                                    <span>
                                        <svg width="16" height="16" viewBox="0 0 20 20" fill="none">
                                            <path fillRule="evenodd" clipRule="evenodd" d="M5.83317 6.6665H4.1665V8.33317H5.83317V9.99984H7.49984V11.6665H9.1665V13.3332H10.8332V11.6665H12.4998V9.99984H14.1665V8.33317H15.8332V6.6665H14.1665V8.33317H12.4998V9.99984H10.8332V11.6665H9.1665V9.99984H7.49984V8.33317H5.83317V6.6665Z" fill="#64748B" />
                                        </svg>
                                    </span>
                                </button>
                            </Link>
                        </div>
                        <div className="relative">
                            <Link to="/home">
                                <button className="flex items-center gap-1 px-3 py-2 hover:text-blue-400 hover:bg-gray-200 dark:hover:bg-gray-800 rounded-md custom-cursor">
                                    Ôn tập
                                    <span>
                                        <svg width="16" height="16" viewBox="0 0 20 20" fill="none">
                                            <path fillRule="evenodd" clipRule="evenodd" d="M5.83317 6.6665H4.1665V8.33317H5.83317V9.99984H7.49984V11.6665H9.1665V13.3332H10.8332V11.6665H12.4998V9.99984H14.1665V8.33317H15.8332V6.6665H14.1665V8.33317H12.4998V9.99984H10.8332V11.6665H9.1665V9.99984H7.49984V8.33317H5.83317V6.6665Z" fill="#64748B" />
                                        </svg>
                                    </span>
                                </button>
                            </Link>
                        </div>
                        <div>
                            <Link to="/pets">
                                <button className="px-3 py-2 hover:text-blue-400 hover:bg-gray-200 dark:hover:bg-gray-800 rounded-md custom-cursor">
                                    Pokédex
                                </button>
                            </Link>
                        </div>
                        <div className="relative">
                            <Link to="/community">
                                <button className="flex items-center gap-1 px-3 py-2  hover:text-blue-400 hover:bg-gray-200 dark:hover:bg-gray-800 rounded-md custom-cursor">
                                    Cộng đồng
                                    <span>
                                        <svg width="16" height="16" viewBox="0 0 20 20" fill="none">
                                            <path fillRule="evenodd" clipRule="evenodd" d="M5.83317 6.6665H4.1665V8.33317H5.83317V9.99984H7.49984V11.6665H9.1665V13.3332H10.8332V11.6665H12.4998V9.99984H14.1665V8.33317H15.8332V6.6665H14.1665V8.33317H12.4998V9.99984H10.8332V11.6665H9.1665V9.99984H7.49984V8.33317H5.83317V6.6665Z" fill="#64748B" />
                                        </svg>
                                    </span>
                                </button>
                            </Link>
                        </div>
                    </div>

                    {/* User Actions */}
                    <div className="flex items-center gap-2 sm:gap-4">
                        {/* Notification Bell */}
                        {user ? (
                            <div className="relative icon-container nes-pointer">
                                <svg
                                    xmlns="http://www.w3.org/2000/svg"
                                    width="20"
                                    height="20"
                                    viewBox="0 0 24 24"
                                    fill="none"
                                    onClick={toggleNotificationSidebar}
                                    className="custom-cursor"
                                >
                                    <path
                                        fillRule="evenodd"
                                        clipRule="evenodd"
                                        d="M14 4V2H9.99999V4H5.00018V6H19.0002V4H14ZM18.9999 16H4.99994V12H2.99994V16V18L7.99975 18V22H9.99975V18H13.9998V20H10V22H13.9998V22H15.9998V18L20.9999 18V16L21 12H19V6H17V14H18.9999V16ZM5.00018 6V14H7.00018V6H5.00018Z"
                                        fill={notifications.some((n) => !n.isRead) ? "#FFD700" : "#94A3B8"}
                                    />
                                </svg>
                                {unreadCount > 0 && (
                                    <span className="absolute top-0 right-0 inline-flex items-center justify-center w-3 h-3 text-xs font-bold bg-red-500 rounded-full -mt-1 -mr-1">
                                        {unreadCount}
                                    </span>
                                )}
                            </div>
                        ) : null}

                        {/* Dark/Light Mode Toggle */}
                        <button onClick={toggleTheme} className="hidden sm:block icon-container custom-cursor">
                            {isDarkMode ? (
                                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g clip-path="url(#clip0_681_9546)"><g clip-path="url(#clip1_681_9546)"><path fill-rule="evenodd" clip-rule="evenodd" d="M13 0H11V4H13V0ZM0 11V13H4V11H0ZM24 11V13H20V11H24ZM13 24H11V20H13V24ZM8 6H16V8H8V6ZM6 8H8V16H6V8ZM8 18V16H16V18H8ZM18 16H16V8H18V16ZM20 2H22V4H20V2ZM20 4V6H18V4H20ZM22 22H20V20H22V22ZM20 20H18V18H20V20ZM4 2H2V4H4V6H6V4H4V2ZM2 22H4V20H6V18H4V20H2V22Z" fill="#64748B"></path></g></g><defs><clipPath id="clip0_681_9546"><rect width="24" height="24" fill="white"></rect></clipPath><clipPath id="clip1_681_9546"><rect width="24" height="24" fill="white"></rect></clipPath></defs></svg>
                            ) : (
                                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg"><g clip-path="url(#clip0_681_6745)"><path fill-rule="evenodd" clip-rule="evenodd" d="M6 2H14V4H12V6H10V4H6V2ZM4 6V4H6V6H4ZM4 16H2V6H4V16ZM6 18H4V16H6V18ZM8 20H6V18H8V20ZM18 20V22H8V20H18ZM20 18V20H18V18H20ZM18 14H20V18H22V10H20V12H18V14ZM12 14V16H18V14H12ZM10 12H12V14H10V12ZM10 12V6H8V12H10Z" fill="#94A3B8"></path></g><defs><clipPath id="clip0_681_6745"><rect width="24" height="24" fill="white"></rect></clipPath></defs></svg>
                            )}
                        </button>


                        {/* Login/Logout button */}
                        {user ? (
                            <button
                                onClick={logout}
                                className="px-2 py-1 text-xs bg-yellow-300 text-black rounded hover:bg-yellow-200 font-pixel custom-cursor"
                            >
                                Đăng xuất
                            </button>
                        ) : (
                            <Link to="/login" className="no-underline">
                                <button className="px-2 py-1 text-xs bg-yellow-300 text-black rounded hover:bg-yellow-200 font-pixel custom-cursor">
                                    Đăng nhập
                                </button>
                            </Link>
                        )}
                    </div>
                </div>
            </div>

            {/* Mobile Menu Dropdown */}
            {isMobileMenuOpen && (
                <div className="md:hidden bg-white dark:bg-gray-800 text-gray-900 dark:text-white px-4 py-2 absolute top-[48px] left-0 w-full z-40">
                    <div className="flex flex-col gap-2">
                        <Link to="/vocabularySet" className="py-2 hover:text-blue-400" onClick={toggleMobileMenu}>
                            Bộ từ vựng
                        </Link>
                        <Link to="/home" className="py-2 hover:text-blue-400" onClick={toggleMobileMenu}>
                            Ôn tập
                        </Link>
                        <Link to="/pets" className="py-2 hover:text-blue-400" onClick={toggleMobileMenu}>
                            Pokédex
                        </Link>
                        <Link to="/community" className="py-2 hover:text-blue-400" onClick={toggleMobileMenu}>
                            Cộng đồng
                        </Link>
                    </div>
                </div>
            )}

            {/* Notification Sidebar */}
            <div
                className={`fixed top-0 right-0 h-full sidebar-color shadow-lg z-50 w-full sm:w-80 md:w-96 transform transition-transform duration-300 ease-in-out ${isNotificationSidebarOpen ? "translate-x-0" : "translate-x-full"
                    }`}
            >
                <div className="flex flex-col h-full">
                    {/* Header của Sidebar */}
                    <div className="flex justify-between items-center p-4 border-b border-gray-200 dark:border-gray-700">
                        <h3 className="text-xl text-color font-semibold">Thông báo</h3>
                        <div className="flex gap-2">
                            <button
                                onClick={handleMarkAllRead}
                                className="text-xs text-blue-500 hover:text-blue-700 custom-cursor"
                            >
                                Đánh dấu tất cả đã đọc
                            </button>
                            <button
                                onClick={toggleNotificationSidebar}
                                className="text-color custom-cursor"
                            >
                                <svg className="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                                </svg>
                            </button>
                        </div>
                    </div>

                    {/* Nội dung thông báo */}
                    <div className="flex-1 overflow-y-auto p-4">
                        {notifications.length > 0 ? (
                            notifications.map((notif) => (
                                <div
                                    key={notif.id}
                                    className={`p-3 border-b border-gray-200 dark:border-gray-700 ${!notif.isRead ? "background-color" : "sidebar-color"}`}
                                >
                                    <div className="text-sm text-color font-medium">{notif.title}</div>
                                    <div className="text-xs text-gray-600 dark:text-gray-400">{notif.message}</div>
                                    <div className="text-xs text-gray-400 dark:text-gray-500">
                                        {new Date(notif.createdAt).toLocaleTimeString()}
                                    </div>
                                    <div className="flex gap-2 mt-2 flex-wrap">
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
                            <div className="text-xs text-color text-center py-4">Không có thông báo</div>
                        )}
                    </div>
                </div>
            </div>

            {/* Overlay khi sidebar mở */}
            {isNotificationSidebarOpen && (
                <div
                    className="fixed inset-0 bg-black bg-opacity-50 z-40 md:hidden"
                    onClick={toggleNotificationSidebar}
                />
            )}
        </>
    );
};

export default Header;