import { useNavigate } from "react-router-dom";
import { rarityBorders, typeBackgrounds, type PetDto } from "../../types/PetDto";

interface PetCardProps {
  pet: PetDto;
}



const PetCard: React.FC<PetCardProps> = ({ pet }) => {
  const navigate = useNavigate();

  // Chọn background dựa trên pet.type, mặc định là pet-background
  const backgroundClass = typeBackgrounds[pet.type] || "pet-background";
  // Chọn border dựa trên pet.rarity, mặc định là border-gray-500
  const borderClass = rarityBorders[pet.rarity] || "border-gray-500";

  const handleClick = () => {
    navigate(`/pets/${pet.id}`);
  };
  if (!pet.isOwned) {
    return (
      <div
        className={`${backgroundClass} ${borderClass} border-8 border-solid bg-no-repeat bg-cover bg-center p-4 rounded-md shadow-md text-black h-50 relative`}
        onClick={handleClick}
      >
        <img
          src={pet.imageUrl}
          alt={pet.name}
          className="w-full h-32 object-contain transform hover:scale-105 transition-transform duration-300 rounded-md"
          style={{ filter: "brightness(0)" }} // Tạo hiệu ứng bóng đen
        />
        <h3 className="text-lg font-bold text-center">???</h3>
        <span className="text-xl font-pixel absolute top-0">#{pet.order}</span>
      </div>
    );
  }

  return (
    <div
      className={`${backgroundClass} ${borderClass} border-8 border-solid bg-no-repeat bg-cover bg-center p-4 rounded-md shadow-md text-black h-50 relative`}
      onClick={handleClick}
    >
      <img
        src={pet.imageUrl}
        alt={pet.name}
        className="w-full h-32 object-contain rounded-md mb-2 transform hover:scale-105 transition-transform duration-300"
      />
      <h3 className="text-lg font-bold text-center text-yellow-400">{pet.name.trim()}</h3>
      <span className="text-xl font-pixel absolute top-0">#{pet.order}</span>
    </div>
  );
};

export default PetCard;