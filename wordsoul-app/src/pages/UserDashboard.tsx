import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import type { UserDashboardDto } from "../types/Dto";
import { getUserDashboard } from "../services/userService";
import { Chart as ChartJS, CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend } from "chart.js";
import { Bar } from "react-chartjs-2";
import { createReviewSession } from "../services/learningSession";


// đăng ký các component ChartJS
ChartJS.register(CategoryScale, LinearScale, BarElement, Title, Tooltip, Legend);

const UserDashboard: React.FC = () => {
    const [dashboard, setDashboard] = useState<UserDashboardDto | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const navigate = useNavigate();


    const handleCreateReviewSession = async () => {
        setError(null);
        setLoading(true);
        try {
            const session = await createReviewSession();
            navigate(`/learningSession/${session.id}?mode=review`);
            // eslint-disable-next-line @typescript-eslint/no-explicit-any
        } catch (error: any) {
            setError(error?.response?.data?.message || "Lỗi tạo phiên ôn tập");
        } finally {
            setLoading(false);
        }
    };


    useEffect(() => {
        const fetchData = async () => {
            try {
                const data = await getUserDashboard();

                // đảm bảo luôn có 5 level
                const filledStats = Array.from({ length: 5 }, (_, i) => {
                    const level = i + 1;
                    const found = data.vocabularyStats.find((s) => s.level === level);
                    return { level, count: found ? found.count : 0 };
                });

                setDashboard({ ...data, vocabularyStats: filledStats });
                // eslint-disable-next-line @typescript-eslint/no-unused-vars
            } catch (err) {
                setError("Không thể tải dữ liệu dashboard");
            } finally {
                setLoading(false);
            }
        };
        fetchData();
    }, []);

    if (loading) return <div className="text-white">Đang tải...</div>;
    if (error) return <div className="text-red-500">{error}</div>;

    return (
        <div className="bg-black w-full h-screen text-white">
            <div className="container mx-auto w-7/12 flex items-start justify-center gap-15 relative top-[6rem] p-7">
                <div className="w-8/12 h-full">
                    {/* Box thông báo review */}
                    <div className="background-color2 border-1 border-gray-500 rounded-xl w-full h-5/12 flex-col items-center justify-center text-center">
                        <div className="w-full p-7 flex items-center justify-center">
                            <img
                                src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756453095/boy_c1k3lt.gif"
                                alt="minh hoa"
                                className="w-20 h-20 object-cover"
                            />
                        </div>
                        <h2 className="font-pixel text-4xl p-2">Welcome to WordSoul</h2>

                        {dashboard && dashboard.reviewWordCount > 0 ? (
                            <>
                                <p className="p-2 font-sans text-xs">
                                    Bạn có {dashboard.reviewWordCount} từ cần ôn tập{" "}
                                    {dashboard.nextReviewTime &&
                                        `sau ${new Date(dashboard.nextReviewTime).toLocaleTimeString()}`}
                                </p>
                                <div className="w-full flex items-center justify-center p-4">
                                    <button className="relative flex items-center justify-center w-35 px-2 py-3 bg-blue-500 rounded-xs hover:bg-yellow-200 custom-cursor" onClick={handleCreateReviewSession} disabled={loading}>
                                        <span className="mx-1 text-xs font-bold font-sans">Ôn tập</span>
                                    </button>
                                </div>
                            </>
                        ) : (
                            <>
                                <p className="p-2 font-sans text-xs">
                                    Hành trình của bạn chỉ mới bắt đầu, cùng khám phá nào!!
                                </p>
                                <div className="w-full flex items-center justify-center p-4">
                                    <button className="relative flex items-center justify-center w-35 px-2 py-3 bg-blue-500 rounded-xs hover:bg-yellow-200 custom-cursor">
                                        <span className="mx-1 text-xs font-bold font-sans">Học</span>
                                    </button>
                                </div>
                            </>
                        )}
                    </div>

                    {/* Biểu đồ thống kê theo level */}
                    <div className="background-color2 border-1 border-gray-500 rounded-xl w-full h-5/12 flex-col items-center justify-center text-center p-4 mt-10">
                        <h3 className="font-pixel text-lg mb-2">Thống kê từ vựng</h3>
                        {dashboard && (
                            <Bar
                                data={{
                                    labels: dashboard.vocabularyStats.map((s) => `Lv ${s.level}`),
                                    datasets: [
                                        {
                                            label: "Số lượng từ",
                                            data: dashboard.vocabularyStats.map((s) => s.count),
                                            backgroundColor: "rgba(59, 130, 246, 0.7)", // xanh
                                            borderColor: "rgba(59, 130, 246, 1)",
                                            borderWidth: 1,
                                        },
                                    ],
                                }}
                                options={{
                                    responsive: true,
                                    plugins: {
                                        legend: {
                                            display: false,
                                        },
                                        title: {
                                            display: false,
                                        },
                                    },
                                    scales: {
                                        y: {
                                            beginAtZero: true,
                                            ticks: {
                                                stepSize: 1,
                                            },
                                        },
                                    },
                                }}
                            />
                        )}
                    </div>
                </div>

                {/* Card profile */}
                <div className="w-4/12 h-full">
                    <div className="border-1 border-gray-500 p-4 rounded-xl background-color">
                        <div>
                            <div className="flex gap-2 mb-3">
                                <div className="w-10 h-10">
                                    <img
                                        src={
                                            dashboard?.avatarUrl ??
                                            "https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756453095/boy_c1k3lt.gif"
                                        }
                                        alt="avatar"
                                        className="w-full h-full object-cover"
                                    />
                                </div>
                                <div>
                                    <div className="font-pixel">{dashboard?.username}</div>
                                    <div className="font-extralight text-xs">
                                        level {dashboard?.level}
                                    </div>
                                </div>
                            </div>

                            <div className="w-full h-4/12 grid grid-cols-2 gap-4">
                                <StatCard label="Total XP" value={dashboard?.totalXP ?? 0} />
                                <StatCard label="Total AP" value={dashboard?.totalAP ?? 0} />
                                <StatCard label="Pets" value={dashboard?.petCount ?? 0} />
                                <StatCard label="Streak" value={dashboard?.streakDays ?? 0} />
                            </div>

                            <Link to="/profile" className="no-underline">
                                <button className="relative flex items-center justify-center w-full px-2 py-1.5 border-1 border-gray-500 rounded-l hover:bg-neutral-900 custom-cursor mt-5">
                                    <span className="mx-1 text-xs font-bold font-sans">
                                        View Profile
                                    </span>
                                </button>
                            </Link>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

const StatCard: React.FC<{ label: string; value: number }> = ({ label, value }) => (
    <div className="flex items-center justify-start gap-1">
        {icons[label]}
        <div className="flex-col">
            <div className="text-xs font-pixel">{value}</div>
            <div className="text-xs font-sans">{label}</div>
        </div>
    </div>
);

const icons: Record<string, React.ReactNode> = {
    "Total XP": (
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32" fill="none">
            <g clip-path="url(#clip0_240_18579)">
                <path fill-rule="evenodd" clip-rule="evenodd" d="M14 0H18V2H14V0ZM12 6V2H14V6H12ZM10 10V6H12V10H10ZM6 12V10H10V12H6ZM2 14V12H6V14H2ZM2 18H0V14H2V18ZM6 20H2V18H6V20ZM10 22H6V20H10V22ZM12 26H10V22H12V26ZM14 30V26H12V30H14ZM18 30V32H14V30H18ZM20 26V30H18V26H20ZM22 22V26H20V22H22ZM26 20V22H22V20H26ZM30 18V20H26V18H30ZM30 14H32V18H30V14ZM26 12H30V14H26V12ZM22 10H26V12H22V10ZM20 6H22V10H20V6ZM20 6V2H18V6H20Z" fill="#020617"></path>
                <path fill-rule="evenodd" clip-rule="evenodd" d="M14 2H18V6H20V10H22V12H26V14H30V18H26V20H22V22H20V26H18V30H14V26H12V22H10V20H6V18H2V14H6V12H10V10H12V6H14V2Z" fill="#EAB308"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M18 2H16V16H18H20H22H26V12H22V10H20V6H18V2Z" fill="#FDE047"></path>
                <rect x="26" y="14" width="4" height="2" fill="#FDE047"></rect>
                <path fill-rule="evenodd" clip-rule="evenodd" d="M18 2H16V6H18V10H20V12H22V14H26V16H30V14H26V12H22V10H20V6H18V2Z" fill="white"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M2 16H14H16V30H14V26H12V22H10V20H6V18H2V16Z" fill="#CA8A04"></path>
                <path fill-rule="evenodd" clip-rule="evenodd" d="M16 16H2V18H6V20H10V22H12V26H14V30H16V26V22V20V18V16Z" fill="#CA8A04"></path>
            </g>
            <defs><clipPath id="clip0_240_18579">
                <rect width="32" height="32" fill="white"></rect></clipPath>
            </defs>
        </svg>
    ),
    "Total AP": (
        <svg xmlns="http://www.w3.org/2000/svg" width="32" height="32" viewBox="0 0 32 32" fill="none"><path fill-rule="evenodd" clip-rule="evenodd" d="M20 2H12V4H8V6H6V8H4V12H2V20H4V24H6V26H8V28H12V30H20V28H24V26H26V24H28V20H30V12H28V8H26V6H24V4H20V2ZM20 4V6H24V8H26V12H28V20H26V24H24V26H20V28H12V26H8V24H6V20H4V12H6V8H8V6H12V4H20Z" fill="#020617"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M20 4H12V6H8V8H6V12H4V20H6V24H8V26H12V28H20V26H24V24H26V20H28V12H26V8H24V6H20V4Z" fill="#14ADFF"></path><rect x="4" y="12" width="6" height="8" fill="#75D1FF"></rect><rect x="10" y="10" width="12" height="12" fill="#EFF8FF"></rect><path fill-rule="evenodd" clip-rule="evenodd" d="M12 26H8V24H6V20H12V22V24V26Z" fill="#0080D4"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M12 6H8V8H6V12H12V10V8V6Z" fill="#2CBAFF"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M20 6H24V8H26V12H20V10V8V6Z" fill="#2CBAFF"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M20 26H24V24H26V20H20V22V24V26Z" fill="#75D1FF"></path><rect x="22" y="12" width="6" height="8" fill="#B6E4FF"></rect><rect x="12" y="4" width="8" height="6" fill="#75D1FF"></rect></svg>
    ),
    "Pets": (
        <svg xmlns="http://www.w3.org/2000/svg" width="33" height="32" viewBox="0 0 33 32" fill="none">
            <path fill="currentColor" d="M5 2h4v2H7v2H5V2Zm0 10H3V6h2v6Zm2 2H5v-2h2v2Zm2 2v-2H7v2H3v-2H1v2h2v2h4v4h2v-4h2v-2H9Zm0 0v2H7v-2h2Zm6-12v2H9V4h6Zm4 2h-2V4h-2V2h4v4Zm0 6V6h2v6h-2Zm-2 2v-2h2v2h-2Zm-2 2v-2h2v2h-2Zm0 2h-2v-2h2v2Zm0 0h2v4h-2v-4Z" />
        </svg>
    ),
    "Streak": (
        <svg xmlns="http://www.w3.org/2000/svg" width="33" height="32" viewBox="0 0 33 32" fill="none">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M18.5 0H16.5V2H14.5V8H12.5V14H10.5V12H8.5V10H6.5V12H4.5V16H2.5V22H4.5V26H6.5V28H8.5V30H10.5V32H22.5V30H24.5V28H26.5V26H28.5V22H30.5V16H28.5V12H26.5V8H24.5V6H22.5V4H20.5V2H18.5V0ZM18.5 2V4H20.5V6H22.5V8H24.5V12H26.5V16H28.5V22H26.5V26H24.5V28H22.5V30H10.5V28H8.5V26H6.5V22H4.5V16H6.5V12H8.5V14H10.5V16H12.5V14H14.5V8H16.5V2H18.5Z" fill="#020617"></path>
            <path fill-rule="evenodd" clip-rule="evenodd" d="M18.5 2H16.5V8H14.5V14H12.5V16H10.5V14H8.5V12H6.5V16H4.5V22H6.5V26H8.5V28H10.5V30H12.5H14.5H16.5H18.5H20.5H22.5V28H24.5V26H26.5V22H28.5V16H26.5V12H24.5V8H22.5V6H20.5V4H18.5V2Z" fill="#FF7A00"></path><path fill-rule="evenodd" clip-rule="evenodd" d="M18.5 10H20.5V12H22.5V16H24.5V24H22.5V26H20.5V28H18.5H12.5V26H10.5V24H8.5V18H10.5V20H12.5H14.5V18H16.5V12H18.5V10Z" fill="#FDE047"></path>
        </svg>
    ),
};

export default UserDashboard;
