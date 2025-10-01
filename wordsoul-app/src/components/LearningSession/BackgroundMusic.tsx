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
        className="absolute top-2 sm:top-2 lg:top-10 right-6 bg-gray-800 p-2 rounded-full text-white font-pixel border-2 border-white hover:bg-gray-700"
      >
        {isPlaying ? "🔇 Tắt nhạc" : "🔊 Bật nhạc"}
      </button>
      <audio
        ref={audioRef}
        loop
        src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1758098297/wild-pokemon-battle_oj4ltv.mp3"
      />
    </>
  );
};

export default BackgroundMusic;