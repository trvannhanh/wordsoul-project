import { useNavigate } from "react-router-dom";
import type { PetDto } from "../../types/PetDto";


interface PetCardProps {
  pet: PetDto;
}

const PetCard: React.FC<PetCardProps> = ({ pet }) => {
  const navigate = useNavigate();

  const handleClick = () => {
    navigate(`/pets/${pet.id}`);
  };

  if (!pet.isOwned) {
    return (
      <div className="pet-background pixel-border p-4 rounded-md shadow-md text-black h-64 relative" onClick={handleClick}>

        <img
          src={pet.imageUrl}
          alt={pet.name}
          className="w-full h-32 object-contain transform hover:scale-105 transition-transform duration-300 rounded-md"
          style={{ filter: 'brightness(0)' }} // Tạo hiệu ứng bóng đen
        />

        <h3 className="text-lg font-bold">???</h3>
        <p className="text-sm">Rarity: ???</p>
        <p className="text-sm">Type: ???</p>

        <span className="text-xl font-pixel absolute top-0">#{pet.order}</span>
      </div>

    );
  }

  return (
    <div className="pet-background pixel-border p-4 rounded-md shadow-md text-black h-64 relative" onClick={handleClick}>
      <img src={pet.imageUrl} alt={pet.name} className="w-full h-32 object-contain rounded-md mb-2 transform hover:scale-105 transition-transform duration-300" />
      <h3 className="text-lg font-bold">{pet.name.trim()}</h3>
      <p className="text-sm">Rarity: {pet.rarity}</p>
      <p className="text-sm">Type: {pet.type}</p>
      <span className="text-xl font-pixel absolute top-0">#{pet.order}</span>
    </div>
  );
};

export default PetCard;