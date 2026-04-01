import { useEffect, useState } from 'react';

export function BattleIntroOverlay({
    p1Avatar, p2Avatar, p2Name, onDone,
}: {
    p1Avatar?: string;
    p2Avatar?: string;
    p2Name?: string;
    onDone: () => void;
}) {
    const [phase, setPhase] = useState<'vs' | 'out'>('vs');

    useEffect(() => {
        const t1 = setTimeout(() => setPhase('out'), 2000);
        const t2 = setTimeout(onDone, 2600);
        return () => { clearTimeout(t1); clearTimeout(t2); };
    }, [onDone]);

    return (
        <div
            className={`
        fixed inset-0 z-50 flex items-center justify-center
        transition-opacity duration-500
        ${phase === 'out' ? 'opacity-0' : 'opacity-100'}
      `}
            style={{ background: 'rgb(2,6,23)' }}
        >
            {/* Left trainer */}
            <div className="flex flex-col items-center gap-2 animate-[slideFromLeft_0.6s_ease-out]">
                {p1Avatar
                    ? <img src={p1Avatar} alt="you" className="w-20 h-24 object-contain" style={{ imageRendering: 'pixelated' }} />
                    : <div className="text-6xl">🧑</div>}
                <span className="font-press text-xs text-blue-300">YOU</span>
            </div>

            {/* VS */}
            <div className="mx-8 sm:mx-16 text-center">
                <div
                    className="font-press text-4xl sm:text-6xl text-yellow-400 animate-[vsFlash_0.8s_ease-out]"
                    style={{ textShadow: '0 0 20px #facc15, 0 0 40px #facc1588' }}
                >
                    VS
                </div>
            </div>

            {/* Right trainer */}
            <div className="flex flex-col items-center gap-2 animate-[slideFromRight_0.6s_ease-out]">
                {p2Avatar
                    ? <img src={p2Avatar} alt="gym" className="w-20 h-24 object-contain scale-x-[-1]" style={{ imageRendering: 'pixelated' }} />
                    : <div className="text-6xl">🤖</div>}
                <span className="font-press text-xs text-red-300">{p2Name ?? 'GYM LEADER'}</span>
            </div>
        </div>
    );
}
