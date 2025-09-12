import { useEffect, useRef } from "react";

interface BackgroundMusicProps {
  isPlaying: boolean;
  volume: number;
  showRewardAnimation: boolean;
  toggleMusic: () => void;
}

const BackgroundMusic: React.FC<BackgroundMusicProps> = ({
  isPlaying,
  volume,
  showRewardAnimation,
  toggleMusic,
}) => {
  const audioRef = useRef<HTMLAudioElement | null>(null);

  useEffect(() => {
    if (audioRef.current) {
      if (isPlaying && !showRewardAnimation) {
        audioRef.current.play().catch((err) => console.error("Error playing background music:", err));
      } else {
        audioRef.current.pause();
      }
      audioRef.current.volume = volume;
    }
  }, [isPlaying, volume, showRewardAnimation]);

  return (
    <>
      <button
        onClick={toggleMusic}
        className="absolute top-15 right-4 bg-gray-800 p-2 rounded-full text-white font-pixel border-2 border-white hover:bg-gray-700"
      >
        {isPlaying ? "🔇 Tắt nhạc" : "🔊 Bật nhạc"}
      </button>
      <audio
        ref={audioRef}
        loop
        src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1757509871/battle-theme-2-looping-tune-225562_x6ci21.mp3"
      />
    </>
  );
};

export default BackgroundMusic;