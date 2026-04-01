import type { PetStateDto } from '../../types/BattleArenaTypes';

export function PartyBalls({
    pets,
    flipped = false,
}: {
    pets: PetStateDto[];
    flipped?: boolean;
}) {
    const pokeballImg = 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1757509182/pokeball-sprite-png_4945371_nm8b89.png';
    const faintPokeballImg = 'https://res.cloudinary.com/dqpkxxzaf/image/upload/v1774967062/faint_pokeball_hl51cz.png';

    return (
        <div className={`flex gap-1 ${flipped ? 'flex-row-reverse' : ''}`}>
            {pets.map((p, i) => (
                <img
                    key={i}
                    title={p.displayName}
                    src={p.isFainted ? faintPokeballImg : pokeballImg}
                    alt={p.isFainted ? 'Fainted' : 'Ready'}
                    className="w-4 h-4 sm:w-5 sm:h-5 object-contain"
                    style={{ imageRendering: 'pixelated' }}
                />
            ))}
        </div>
    );
}
