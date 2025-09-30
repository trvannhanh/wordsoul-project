import React from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import StatCard from '../../components/UserProfile/StatCard';
import { useAuth } from '../../hooks/Auth/useAuth';


const ProfilePage: React.FC = () => {
  const { user } = useAuth();

  if (!user) {
    return (
      <div className="pixel-background font-pixel text-white min-h-screen flex items-center justify-center">
        <div className="text-center text-red-500">Please log in to view your profile.</div>
      </div>
    );
  }

  return (
    <div className="pixel-background font-pixel text-white min-h-screen w-full flex justify-center items-center">
      <div className="container mx-auto p-4 w-7/12">
        <motion.div
          initial={{ opacity: 0, y: -20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5 }}
          className="bg-gradient-to-br from-gray-700 to-gray-900 pixel-border rounded-xl p-6 shadow-lg"
        >
          {/* Header with Avatar and Username */}
          <div className="flex items-center gap-6 mb-6">
            <motion.div
              className="w-24 h-24"
              whileHover={{ scale: 1.1 }}
              transition={{ duration: 0.3 }}
            >
              <img
                src={
                  user.avatarUrl ??
                  'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756453095/boy_c1k3lt.gif'
                }
                alt="avatar"
                className="w-full h-full object-cover pixelated rounded-md border-2 border-yellow-300"
              />
            </motion.div>
            <div>
              <h1 className="text-3xl font-pokemon text-yellow-300">{user.username}</h1>
              <div className="text-lg text-gray-300">Level {user.level}</div>
              <div className="text-sm text-gray-400">{user.email}</div>
            </div>
          </div>

          {/* User Info Section */}
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
            <div className="bg-gray-800 bg-opacity-80 p-4 rounded-md pixel-border">
              <h2 className="text-xl font-bold text-blue-400 mb-2">Account Details</h2>
              <div className="text-sm">
                <p>
                  <span className="font-bold">Role:</span>{' '}
                  {user.role.charAt(0).toUpperCase() + user.role.slice(1)}
                </p>
                <p>
                  <span className="font-bold">Joined:</span>{' '}
                  {new Date(user.createdAt).toLocaleDateString()}
                </p>
                <p>
                  <span className="font-bold">Status:</span>{' '}
                  {user.isActive ? 'Active' : 'Inactive'}
                </p>
              </div>
            </div>

            {/* Stats Section */}
            <div className="bg-gray-800 bg-opacity-80 p-4 rounded-md pixel-border">
              <h2 className="text-xl font-bold text-blue-400 mb-2">Trainer Stats</h2>
              <div className="grid grid-cols-2 gap-2">
                <StatCard label="Total XP" value={user.totalXP} />
                <StatCard label="Total AP" value={user.totalAP} />
                <StatCard label="Pets" value={user.petCount ?? 0} />
                <StatCard label="Streak" value={user.streakDays} />
              </div>
            </div>
          </div>

          {/* Action Buttons */}
          <div className="flex justify-center gap-4">
            <Link to="/community" className="no-underline">
              <motion.button
                className="px-6 py-2 bg-blue-500 text-white font-pokemon text-sm rounded pixel-border hover:bg-blue-400"
                whileHover={{ scale: 1.05, boxShadow: '0 0 10px rgba(59, 130, 246, 0.7)' }}
                whileTap={{ scale: 0.95 }}
              >
                View Leaderboard
              </motion.button>
            </Link>
            <Link to="/pets" className="no-underline">
              <motion.button
                className="px-6 py-2 bg-green-500 text-white font-pokemon text-sm rounded pixel-border hover:bg-green-400"
                whileHover={{ scale: 1.05, boxShadow: '0 0 10px rgba(34, 197, 94, 0.7)' }}
                whileTap={{ scale: 0.95 }}
              >
                View Pets
              </motion.button>
            </Link>
          </div>
        </motion.div>
      </div>
    </div>
  );
};

export default ProfilePage;