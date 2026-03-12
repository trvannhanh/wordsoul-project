import { AnimatePresence, motion } from 'framer-motion';
import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import PrologueIntro from './PrologueIntro';
import TrialQuiz from './TrialQuiz';
import StarterPicker from './StarterPicker';

type Step = 'intro' | 'quiz' | 'outro' | 'pick';

// ── Dialogs ────────────────────────────────────────────────────────────────
const INTRO_DIALOG = [
    "Chào mừng đến với WordSoul...",
    "Nơi ngôn ngữ và các sinh vật huyền bí hòa làm một.",
    "Ta là Giáo sư Oak, người đã dành cả đời nghiên cứu các từ ngữ kỳ diệu.",
    "Trước khi bắt đầu hành trình, hãy để ta kiểm tra khả năng của bạn!",
];

const OUTRO_DIALOG = [
    "Khá lắm! Bạn vừa vượt qua bài kiểm tra tân thủ một cách xuất sắc.",
    "Ta thấy ở bạn tiềm năng của một nhà thám hiểm từ vựng thực thụ...Nhưng hành trình thực sự chỉ mới bắt đầu.",
    "Pokemon, sinh vật kì diệu sẽ là người bạn đồng hành trên con đường chinh phục từ vựng của bạn. Mỗi người bạn đồng hành sẽ mang lại một buff đặc biệt trong mỗi phiên học của bạn:",
    "Hãy chọn cho mình một người bạn đồng hành đầu tiên nhé!",
];

interface OnboardingFlowProps {
    onClose?: () => void;
}

const OnboardingFlow: React.FC<OnboardingFlowProps> = ({ onClose }) => {
    const navigate = useNavigate();
    const [step, setStep] = useState<Step>('intro');

    const handlePickComplete = (petId: number) => {
        localStorage.setItem('onboarding_starter_pet_id', String(petId));
        setTimeout(() => navigate('/register'), 1200);
    };

    return (
        <AnimatePresence mode="wait">
            <motion.div
                key={step}                          // re-animate on step change
                className="fixed inset-0 z-[9999]"
                initial={{ opacity: 0 }}
                animate={{ opacity: 1 }}
                exit={{ opacity: 0 }}
                transition={{ duration: 0.3 }}
            >
                {step === 'intro' && (
                    <PrologueIntro
                        dialog={INTRO_DIALOG}
                        onStart={() => setStep('quiz')}
                        onStartLabel="⚡ Bắt đầu kiểm tra!"
                        onExit={onClose}
                    />
                )}

                {step === 'quiz' && (
                    <TrialQuiz
                        onComplete={() => setStep('outro')}   // ← outro sau quiz
                        onExit={onClose}
                    />
                )}

                {step === 'outro' && (
                    <PrologueIntro
                        dialog={OUTRO_DIALOG}
                        onStart={() => setStep('pick')}
                        onStartLabel="🎒 Chọn người bạn đồng hành!"
                        onExit={onClose}
                    />
                )}

                {step === 'pick' && (
                    <StarterPicker onPicked={handlePickComplete} />
                )}
            </motion.div>
        </AnimatePresence>
    );
};

export default OnboardingFlow;