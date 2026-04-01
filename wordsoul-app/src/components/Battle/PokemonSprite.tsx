import type { PetStateDto } from '../../types/BattleArenaTypes';

export type SpriteState = 'idle' | 'attack' | 'hit' | 'faint' | 'enter';

export function PokemonSprite({
    pet,
    back = false,
    state: spriteState = 'idle',
}: {
    pet: PetStateDto;
    back?: boolean;
    state?: SpriteState;
}) {
    const animClass = {
        idle: '',
        attack: back
            ? 'animate-[slideRight_0.3s_ease-out]'
            : 'animate-[slideLeft_0.3s_ease-out]',
        hit: 'animate-[flashRed_0.4s_ease-in-out]',
        faint: 'animate-[faintDown_0.6s_ease-in_forwards]',
        enter: back
            ? 'animate-[slideFromLeft_0.5s_ease-out]'
            : 'animate-[slideFromRight_0.5s_ease-out]',
    }[spriteState];

    const pokemonName = pet.displayName ? pet.displayName.toLowerCase() : 'bulbasaur';
    const imgUrl = back
        ? `https://img.pokemondb.net/sprites/black-white/anim/back-normal/${pokemonName}.gif`
        : `https://img.pokemondb.net/sprites/black-white/anim/normal/${pokemonName}.gif`;

    return (
        <div
            className={`relative flex items-end justify-center ${animClass}`}
        >
            {/* Ground shadow */}
            <div
                className="absolute bottom-0 left-1/2 -translate-x-1/2 w-16 h-3 rounded-full opacity-30"
                style={{ background: 'radial-gradient(ellipse, #000 0%, transparent 70%)' }}
            />
            <img
                src={imgUrl}
                alt={pet.displayName}
                className={`
          w-32 h-32 sm:w-64 sm:h-64 object-contain
          image-rendering-pixelated
          ${pet.isFainted ? 'grayscale opacity-20' : ''}
        `}
                style={{ imageRendering: 'pixelated' }}
                onError={(e) => {
                    // fallback to pet.imageUrl if the exact name isn't found on pokemondb
                    if (pet.imageUrl) {
                        (e.target as HTMLImageElement).src = pet.imageUrl;
                    }
                }}
            />
        </div>
    );
}
