import { useEffect, useState } from 'react';

export const HP_COLOR = (pct: number) =>
    pct > 50 ? '#58d858' : pct > 25 ? '#f8d030' : '#f83030';

export function HpBar({
    current, max,
}: {
    current: number; max: number;
}) {
    const pct = Math.max(0, Math.min(100, (current / max) * 100));
    const color = HP_COLOR(pct);
    const [display, setDisplay] = useState(pct);

    useEffect(() => {
        // Smooth drain animation
        const start = display;
        const end = pct;
        if (start === end) return;
        const steps = 30;
        let step = 0;
        const id = setInterval(() => {
            step++;
            setDisplay(start + ((end - start) * step) / steps);
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
