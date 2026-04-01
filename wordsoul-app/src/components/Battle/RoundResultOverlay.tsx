import { useEffect } from 'react';
import type { RoundResultDto } from '../../types/BattleArenaTypes';

export function RoundResultOverlay({
    result, onDone, opponentLabel = 'GYM',
}: {
    result: RoundResultDto; onDone: () => void; opponentLabel?: string;
}) {
    useEffect(() => {
        const t = setTimeout(onDone, 2800);
        return () => clearTimeout(t);
    }, [onDone]);

    const youHit = result.damagedPlayer === 2;
    const draw = result.damagedPlayer === 0;

    return (
        <div className="absolute inset-0 z-50 flex flex-col items-center justify-center gap-3"
            style={{ background: 'rgba(0,0,0,0.85)', backdropFilter: 'blur(4px)' }}>

            {/* Result banner */}
            <div
                className={`
          font-press text-xl sm:text-2xl px-6 py-2 rounded border-2
          ${youHit ? 'text-yellow-300 border-yellow-400 bg-yellow-900/40' :
                        draw ? 'text-gray-300 border-gray-500 bg-gray-800/40' :
                            'text-red-400 border-red-500 bg-red-900/40'}\n        `}
                style={{ textShadow: `0 0 12px ${youHit ? '#facc15' : draw ? '#888' : '#f87171'}` }}
            >
                {youHit ? '⚔️ HIT!' : draw ? '🤝 DRAW!' : '💥 OUCH!'}
            </div>

            {/* Damage */}
            {result.damageDealt > 0 && (
                <div className={`font-press text-sm ${youHit ? 'text-green-400' : 'text-red-400'}`}>
                    {result.damageDealt} DMG
                    {result.typeEffectivenessText && (
                        <span className="font-pixel text-[10px] ml-2 text-yellow-300">
                            {result.typeEffectivenessText}
                        </span>
                    )}
                </div>
            )}

            {/* Scores comparison */}
            <div className="grid grid-cols-2 gap-6 text-center mt-1">
                <div>
                    <div className="font-pixel text-[9px] text-blue-300 mb-1">YOU</div>
                    <div className={`font-press text-lg ${result.p1Correct ? 'text-green-400' : 'text-red-400'}`}>
                        {result.p1Score}
                    </div>
                    <div className="font-pixel text-[8px] text-gray-500 mt-0.5">{result.p1AnswerMs}ms</div>
                </div>
                <div>
                    <div className="font-pixel text-[9px] text-red-300 mb-1">{opponentLabel}</div>
                    <div className={`font-press text-lg ${result.p2Correct ? 'text-green-400' : 'text-red-400'}`}>
                        {result.p2Score}
                    </div>
                    <div className="font-pixel text-[8px] text-gray-500 mt-0.5">{result.p2AnswerMs}ms</div>
                </div>
            </div>

            {/* Correct answer */}
            <div className="mt-2 px-4 py-1.5 rounded border border-green-500/40 bg-green-900/20">
                <span className="font-pixel text-[9px] text-gray-400">CORRECT: </span>
                <span className="font-noto text-green-300 text-xs">{result.correctAnswer}</span>
            </div>
        </div>
    );
}
