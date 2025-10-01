import { motion } from 'framer-motion';
import type { UserProgressDto } from '../../types/UserDto';

interface ReviewBoxProps {
  progress: UserProgressDto | null;
  loading: boolean;
  onCreateReviewSession: () => void;
}

const ReviewBox: React.FC<ReviewBoxProps> = ({ progress, loading, onCreateReviewSession }) => {
  return (
    <motion.div
      className="review-box-background pixel-border rounded-xl p-6 text-center"
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.5 }}
    >
      <div className="flex justify-center mb-4">
        <img
          src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1756453095/boy_c1k3lt.gif"
          alt="minh hoa"
          className="w-16 h-16 object-cover pixelated"
        />
      </div>
      <h2 className="fon-pixel text-3xl mb-2 text-yellow-300">Welcome to WordSoul</h2>
      {progress && progress.reviewWordCount > 0 ? (
        <>
          <p className="text-sm text-yellow-300 mb-4">
            Bạn có {progress.reviewWordCount} từ cần ôn tập{' '}
            {progress.nextReviewTime &&
              `sau ${new Date(progress.nextReviewTime).toLocaleTimeString()}`}
          </p>
          <div className="flex justify-center"> {/* Container Flexbox để căn giữa nút */}
            <motion.button
              className="relative flex items-center justify-center w-32 px-4 py-2 bg-yellow-400 text-black font-pixel text-sm rounded pixel-border-dark hover:bg-yellow-300 custom-cursor"
              onClick={onCreateReviewSession}
              disabled={loading}
              whileHover={{ scale: 1.05, boxShadow: '0 0 10px rgba(255, 204, 0, 0.7)' }}
              whileTap={{ scale: 0.95 }}
            >
              <span className="absolute left-0 w-1 h-full bg-yellow-600" />
              <span>Ôn tập</span>
              <span className="absolute right-0 w-1 h-full bg-yellow-600" />
            </motion.button>
          </div>
        </>
      ) : (
        <>
          <p className="text-sm text-gray-200 mb-4">
            Hành trình của bạn chỉ mới bắt đầu, cùng khám phá nào!!
          </p>
          <div className="flex justify-center"> {/* Container Flexbox để căn giữa nút */}
            <motion.button
              className="relative flex items-center justify-center w-32 px-4 py-2 bg-yellow-400 text-black font-pokemon text-sm rounded pixel-border-dark hover:bg-yellow-300 custom-cursor"
              whileHover={{ scale: 1.05, boxShadow: '0 0 10px rgba(255, 204, 0, 0.7)' }}
              whileTap={{ scale: 0.95 }}
            >
              <span className="absolute left-0 w-1 h-full bg-yellow-600" />
              <span>Học</span>
              <span className="absolute right-0 w-1 h-full bg-yellow-600" />
            </motion.button>
          </div>
        </>
      )}
    </motion.div>
  );
};

export default ReviewBox;