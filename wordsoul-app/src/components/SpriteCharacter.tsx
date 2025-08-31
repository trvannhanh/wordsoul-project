import React from "react";
import charBlue from "../assets/char_blue.png"; // đi từ components ra ngoài, vào assets

const SpriteCharacter: React.FC = () => {
  return (
    <div
      className="w-10 h-16 bg-no-repeat"
      style={{
        backgroundImage: `url(${charBlue})`,
        animation: "play 1s steps(8) infinite",
      }}
    ></div>
  );
};

export default SpriteCharacter;