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
      <div className="bg-black text-white border-white border-2  p-4 rounded-md shadow-md h-64 flex items-center justify-center" onClick={handleClick}>
        <span className="text-4xl font-pixel">?</span>
      </div>
    );
  }

  return (
    <div className="pet-background pixel-border p-4 rounded-md shadow-md text-black h-64" onClick={handleClick}>
      <img src={pet.imageUrl} alt={pet.name} className="w-full h-32 object-contain rounded-md mb-2" />
      <h3 className="text-lg font-bold">{pet.name.trim()}</h3>
      <p className="text-sm">Rarity: {pet.rarity}</p>
      <p className="text-sm">Type: {pet.type}</p>
    </div>
  ); 
};

export default PetCard;