import { useEffect, useRef, useState } from 'react';

export function FillInBlankInput({
    disabled, onSubmit,
}: {
    disabled: boolean;
    onSubmit: (ans: string, ms: number) => void;
}) {
    const [val, setVal] = useState('');
    const startRef = useRef(Date.now());

    useEffect(() => {
        startRef.current = Date.now();
        setVal('');
    }, [disabled]);

    const submit = () => {
        if (disabled || !val.trim()) return;
        onSubmit(val.trim(), Date.now() - startRef.current);
    };

    return (
        <div className="flex gap-2">
            <input
                type="text"
                value={val}
                onChange={e => setVal(e.target.value)}
                onKeyDown={e => e.key === 'Enter' && submit()}
                disabled={disabled}
                placeholder="Type the missing word..."
                autoFocus
                className="
          flex-1 px-4 py-3 rounded border-2 border-gray-600
          bg-[#0a0a1a] text-white font-pixel text-xs
          focus:outline-none focus:border-yellow-400
          placeholder-gray-600 disabled:opacity-40
        "
            />
            <button
                onClick={submit}
                disabled={disabled || !val.trim()}
                className="
          px-5 py-3 rounded border-2 border-yellow-400
          font-press text-xs bg-yellow-400 text-black
          hover:bg-yellow-300 active:scale-95
          disabled:bg-gray-700 disabled:border-gray-600 disabled:text-gray-500
          transition-all
        "
            >
                GO
            </button>
        </div>
    );
}
