import { Link } from "react-router-dom"
import { useAuth } from "../store/AuthContext";

const Header: React.FC = () => {
    const { user, logout } = useAuth();
    
    return (
        <>
            <div className="flex items-center justify-between background-color px-10 py-2  shadow-sm fixed top-0 left-0 z-1000 w-screen">
                <div className="container mx-auto flex items-center justify-between w-7/12 " >
                    {/* Logo Section */}
                    <h2 className="flex items-center cursor-pointer w-1/5">
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
                    <div className="flex items-center justify-between gap-4 w-4/5">
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
                            <div>
                                <Link to="/dashboard">
                                    <button className="px-4 py-2 text-white hover:text-blue-400 hover:bg-slate-800 rounded-md custom-cursor">
                                        Quản trị
                                    </button>
                                </Link>
                            </div>
                        </div>

                        {/* User Actions */}
                        <div className="flex items-center gap-4">
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