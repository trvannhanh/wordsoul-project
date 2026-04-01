import { useEffect, useRef, useState } from 'react';
import { HP_COLOR } from './HpBar';

export function ScoreTimer({
    timeLimitMs, onElapsed, answered,
}: {
    timeLimitMs: number;
    onElapsed: (ms: number) => void;
    answered: boolean;
}) {
    const [elapsed, setElapsed] = useState(0);
    const startRef = useRef(Date.now());
    const calledRef = useRef(false);

    useEffect(() => {
        startRef.current = Date.now();
        setElapsed(0);
        calledRef.current = false;
    }, [timeLimitMs]);

    useEffect(() => {
        if (answered) return;
        const id = setInterval(() => {
            const e = Date.now() - startRef.current;
            if (e >= timeLimitMs) {
                clearInterval(id);
                setElapsed(timeLimitMs);
                if (!calledRef.current) { calledRef.current = true; onElapsed(timeLimitMs); }
            } else setElapsed(e);
        }, 50);
        return () => clearInterval(id);
    }, [timeLimitMs, onElapsed, answered]);

    const pct = Math.max(0, 100 - (elapsed / timeLimitMs) * 100);
    const color = HP_COLOR(pct);
    const score = Math.max(0, Math.round(1000 - (elapsed / timeLimitMs) * 900));

    return (
        <div className="flex items-center gap-3">
            <span className="font-pixel text-[9px] text-gray-400 shrink-0">TIME</span>
            <div className="flex-1 h-[6px] rounded-sm border border-gray-600 bg-gray-900 overflow-hidden">
                <div
                    className="h-full rounded-sm"
                    style={{
                        width: `${pct}%`,
                        background: color,
                        boxShadow: `0 0 6px ${color}88`,
                        transition: 'width 0.05s linear',
                    }}
                />
            </div>
            <span className="font-pixel text-[10px] shrink-0" style={{ color }}>
                {score}
            </span>
        </div>
    );
}
