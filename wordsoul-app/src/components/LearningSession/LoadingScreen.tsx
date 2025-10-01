import { useEffect, useRef } from "react";

const LoadingScreen = () => {
  const audioRef = useRef<HTMLAudioElement>(null);

  useEffect(() => {
    if (audioRef.current) {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      audioRef.current.play().catch((error: any) => {
        console.error("Audio playback failed:", error);
      });
    }
  }, []);

  return (
    <div className="fixed inset-0 z-50 flex flex-col items-center justify-center background-color pixelated font-mono">
      {/* Audio element for battle intro sound */}
      <audio
        ref={audioRef}
        src="https://res.cloudinary.com/dqpkxxzaf/video/upload/v1759229095/tmpq91k5v_6_cz3e3l.mp3"
      />

      {/* Logo / Hình ảnh */}
      <img
        src="https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759229729/wooper-pokemon_xn87cu.gif"
        alt="loading"
        className="w-52 h-52 mb-8"
      />

      {/* Spinner */}
      <div className="w-16 h-16 border-4 border-pink-500 border-dashed rounded-full animate-spin mb-4" />

      {/* Loading text */}
      <p className="text-pink-400 text-xl animate-pulse tracking-widest">
        Loading...
      </p>
    </div>
  );
};

export default LoadingScreen;
