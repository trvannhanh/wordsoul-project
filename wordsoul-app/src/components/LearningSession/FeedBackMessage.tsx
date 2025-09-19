// components/LearningSession/FeedbackMessage.tsx
import { motion, AnimatePresence } from "framer-motion";

interface FeedbackMessageProps {
  message: string | null;
}

const FeedbackMessage: React.FC<FeedbackMessageProps> = ({ message }) => (
  <AnimatePresence>
    {message && (
      <motion.div
        className="absolute top-10 left-1/2 transform -translate-x-1/2 bg-gray-800 text-white font-pixel text-lg p-4 rounded-lg border-2 border-yellow-300"
        initial={{ opacity: 0, y: -20 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -20 }}
        transition={{ duration: 0.5 }}
      >
        {message}
      </motion.div>
    )}
  </AnimatePresence>
);

export default FeedbackMessage;