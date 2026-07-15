import { useEffect, useRef, useState } from 'react';
import { getHpColor } from './battleColors';


export function HpBar({
    current, max,
}: {
    current: number; max: number;
}) {
    const pct = Math.max(0, Math.min(100, (current / max) * 100));
    const color = getHpColor(pct);
    const [display, setDisplay] = useState(pct);
    const displayRef = useRef(pct);

    useEffect(() => {
        const start = displayRef.current;
        const end = pct;
        if (start === end) return;
        const steps = 30;
        let step = 0;
        const id = setInterval(() => {
            step++;
            const nextDisplay = start + ((end - start) * step) / steps;
            displayRef.current = nextDisplay;
            setDisplay(nextDisplay);
            if (step >= steps) clearInterval(id);
        }, 16);
        return () => clearInterval(id);
    }, [pct]);

    return (
        <div className="w-full select-none">
            {/* HG/SS style nameplate */}
            <div className="flex justify-between items-baseline mb-0.5">
                <span className="font-pixel text-[8px] tracking-widest text-gray-300 uppercase">HP</span>
                <span className="font-pixel text-[9px]" style={{ color }}>
                    {current}<span className="text-gray-600">/{max}</span>
                </span>
            </div>
            {/* Outer border (pixel-style) */}
            <div className="h-[6px] rounded-sm border border-gray-600 bg-gray-900 overflow-hidden">
                <div
                    className="h-full rounded-sm transition-none"
                    style={{
                        width: `${display}%`,
                        background: `linear-gradient(180deg, ${color}dd 0%, ${color} 60%, ${color}99 100%)`,
                        boxShadow: `0 0 4px ${color}88`,
                    }}
                />
            </div>
        </div>
    );
}
