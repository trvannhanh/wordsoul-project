import type { PetStateDto } from '../../types/BattleArenaTypes';
import { HpBar } from './HpBar';

export function Nameplate({
    pet, flipped = false, shake = false,
}: {
    pet: PetStateDto; flipped?: boolean; shake?: boolean;
}) {
    return (
        <div
            className={`
        relative px-3 py-2 rounded border-2
        bg-[#1a1a2e] border-gray-600
        w-44 sm:w-80
        ${shake ? 'animate-[shake_0.4s_ease-in-out]' : ''}
        ${flipped ? 'text-right' : 'text-left'}
      `}
            style={{ boxShadow: '2px 2px 0 #00000088' }}
        >
            {/* Pixel corner accent */}
            <div className="absolute top-0 left-0 w-2 h-2 bg-gray-600" />
            <div className="absolute top-0 right-0 w-2 h-2 bg-gray-600" />

            <div className="font-pixel text-[11px] text-white truncate mb-1">
                {pet.displayName}
            </div>
            <div className={`flex items-center gap-1 ${flipped ? 'flex-row-reverse' : ''}`}>
                <span className="font-pixel text-[8px] text-gray-400">Lv</span>
                <span className="font-pixel text-[10px] text-yellow-300">?</span>
            </div>
            <div className="mt-1.5">
                <HpBar current={pet.currentHp} max={pet.maxHp} />
            </div>
        </div>
    );
}
