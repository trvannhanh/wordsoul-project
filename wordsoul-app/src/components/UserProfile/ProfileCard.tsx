// import { useEffect } from 'react';
import { Link } from 'react-router-dom';
import { motion } from 'framer-motion';
import StatCard from './StatCard';
import { useAuth } from '../../hooks/Auth/useAuth';


const ProfileCard: React.FC = () => {
  const { user } = useAuth();

  // // Debug user changes
  // useEffect(() => {
  //   console.log('ProfileCard user updated:', user);
  //   console.log('Total AP:', user?.totalAP);
  // }, [user]);

  return (
    <motion.div
      className="background-color pixel-border rounded-xl p-6"
      initial={{ opacity: 0, x: 20 }}
      animate={{ opacity: 1, x: 0 }}
      transition={{ duration: 0.5 }}
    >
      <div className="flex gap-4 mb-4">
        <div className="w-12 h-12">
          <img
            src={
              user?.avatarUrl ??
              'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756453095/boy_c1k3lt.gif'
            }
            alt="avatar"
            className="w-full h-full object-cover pixelated rounded"
          />
        </div>
        <div>
          <div className="font-pokemon text-lg text-color">{user?.username ?? 'Guest'}</div>
          <div className="text-xs text-color">Level {user?.level ?? 0}</div>
        </div>
      </div>
      <div className="grid grid-cols-2 gap-4 mb-4">
        <StatCard label="Total XP" value={user?.totalXP ?? 0} />
        <StatCard label="Total AP" value={user?.totalAP ?? 0} />
        <StatCard label="Pets" value={user?.petCount ?? 0} />
        <StatCard label="Streak" value={user?.streakDays ?? 0} />
      </div>
      <Link to="/profile" className="no-underline">
        <motion.button
          className="w-full px-4 py-2 background-color text-color font-pokemon text-sm rounded pixel-border hover:bg-blue-400 custom-cursor"
          whileHover={{ scale: 1.05, boxShadow: '0 0 10px rgba(59, 130, 246, 0.7)' }}
          whileTap={{ scale: 0.95 }}
        >
          View Profile
        </motion.button>
      </Link>
    </motion.div>
  );
};

export default ProfileCard;