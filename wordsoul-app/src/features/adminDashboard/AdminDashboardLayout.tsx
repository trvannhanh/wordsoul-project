import { Outlet, Link, useLocation } from 'react-router-dom';
import { Menu, X } from 'lucide-react'; // Sử dụng icon từ lucide-react (cài đặt: npm install lucide-react)
import { useState } from 'react';

const DashboardLayout = () => {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const location = useLocation();

  const navItems = [
    { path: '/admin', label: 'Users' },
    { path: '/admin/vocabulary-sets', label: 'Vocabulary Sets' },
    { path: '/admin/vocabularies', label: 'Vocabularies' },
    { path: '/admin/pets', label: 'Pets' },
    { path: '/admin/activities', label: 'Activity Log' },
  ];

  return (
    <div className="flex h-screen bg-gray-100 relative">
      {/* Sidebar */}
      <div
        className={`fixed inset-y-0 left-0 transform ${isSidebarOpen ? 'translate-x-0' : '-translate-x-full'
          } md:relative md:translate-x-0 w-64 bg-blue-800 text-white transition duration-200 ease-in-out z-50 md:z-auto`}
      >
        <div className="p-4">
          <h2 className="text-2xl font-bold">Admin Panel</h2>
          <button
            onClick={() => setIsSidebarOpen(false)}
            className="md:hidden absolute top-4 right-4 text-white"
          >
            <X size={24} />
          </button>
        </div>
        <nav className="mt-6">
          {navItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={`block px-4 py-2 hover:bg-blue-700 ${location.pathname === item.path ? 'bg-blue-700' : ''
                }`}
            >
              {item.label}
            </Link>
          ))}
        </nav>
      </div>

      {/* Main Content */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header */}
        <header className="bg-white shadow p-4 flex justify-between items-center md:hidden">
          <button onClick={() => setIsSidebarOpen(true)} className="text-blue-600">
            <Menu size={24} />
          </button>
          <h1 className="text-xl font-semibold">Admin Dashboard</h1>
        </header>

        {/* Content Area */}
        <main className="flex-1 overflow-y-auto p-4">
          <Outlet />
        </main>
      </div>
    </div>
  );
};

export default DashboardLayout;