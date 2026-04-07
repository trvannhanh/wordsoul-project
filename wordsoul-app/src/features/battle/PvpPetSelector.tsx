import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { useAuth } from '../../hooks/Auth/useAuth';
import { fetchPets } from '../../services/pet';
import { createPvpSession, joinPvpSession } from '../../services/pvp';

interface OwnedPet {
    id: number;
    petId: number;
    petName: string;
    petImageUrl?: string;
    level: number;
}

export default function PvpPetSelector() {
    const navigate = useNavigate();
    const [searchParams] = useSearchParams();
    const mode = searchParams.get('mode') || 'create';
    const roomCode = searchParams.get('code') || '';

    const { user } = useAuth();
    const [pets, setPets] = useState<OwnedPet[]>([]);
    const [selected, setSelected] = useState<number[]>([]);
    const [loading, setLoading] = useState(true);
    const [starting, setStarting] = useState(false);
    const [error, setError] = useState('');

    useEffect(() => {
        if (!user) return;
        fetchPets({ isOwned: true })
            .then(data => {
                const mapped: OwnedPet[] = data.map(p => ({
                    id: p.id,
                    petId: p.id,
                    petName: p.name,
                    petImageUrl: p.imageUrl,
                    level: 1, // You could pull level from API if implemented
                }));
                setPets(mapped);
            })
            .catch(err => console.error(err))
            .finally(() => setLoading(false));
    }, [user]);

    const toggleSelect = (petId: number) => {
        setSelected(prev =>
            prev.includes(petId)
                ? prev.filter(id => id !== petId)
                : prev.length < 3
                    ? [...prev, petId]
                    : prev
        );
    };

    const handleStart = async () => {
        if (selected.length !== 3) return;
        setStarting(true);
        setError('');
        try {
            if (mode === 'create') {
                const { sessionId, roomCode } = await createPvpSession(selected);
                navigate(`/pvp/arena/${sessionId}?code=${roomCode}`);
            } else if (mode === 'join') {
                const { sessionId } = await joinPvpSession(roomCode, selected);
                navigate(`/pvp/arena/${sessionId}`);
            } else {
                // mode === 'matchmaking': go to matchmaking screen
                navigate('/pvp/matchmaking', { state: { selectedPetIds: selected } });
            }
        } catch (err: unknown) {
            const msg = (err as { response?: { data?: { error?: string } } })?.response?.data?.error
                ?? 'Could not start.';
            setError(msg);
            setStarting(false);
        }
    };

    if (loading) return (
        <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
            <div className="text-white font-pixel text-sm animate-pulse">Loading Pets...</div>
        </div>
    );

    return (
        <div className="min-h-screen text-white flex flex-col items-center py-12 px-4"
            style={{ background: 'rgb(2,6,23)' }}>
            <button onClick={() => navigate('/pvp')}
                className="self-start text-gray-400 hover:text-white font-pixel text-xs mb-6 flex items-center gap-1">
                ← Back
            </button>

            <h1 className="font-press text-xl text-purple-400 mb-1 drop-shadow-[0_0_12px_rgba(168,85,247,0.6)]">
                {mode === 'create' ? 'CREATE PvP ROOM' : mode === 'matchmaking' ? 'SELECT YOUR TEAM' : `JOIN ROOM: ${roomCode}`}
            </h1>
            <p className="font-noto text-gray-400 text-sm mb-8 text-center max-w-sm">
                {mode === 'matchmaking'
                    ? 'Choose 3 Pokémon for matchmaking — you\'ll be matched with a player of similar rating'
                    : 'Select exactly 3 Pokémon for this PvP match'}
            </p>

            {pets.length < 3 && (
                <div className="mb-6 px-4 py-3 rounded-xl bg-red-900/30 border border-red-500/40 text-red-400 font-pixel text-xs text-center">
                    ⚠️ You need at least 3 Pokémon to participate in PvP!
                </div>
            )}

            {error && (
                <div className="mb-4 px-4 py-3 rounded-xl bg-red-900/30 border border-red-500/40 text-red-400 font-pixel text-xs">
                    {error}
                </div>
            )}

            <div className="grid grid-cols-2 sm:grid-cols-3 gap-4 w-full max-w-lg mb-8">
                {pets.map(pet => {
                    const isSelected = selected.includes(pet.id);
                    const slotNum = selected.indexOf(pet.id) + 1;
                    return (
                        <button key={pet.id} onClick={() => toggleSelect(pet.id)}
                            className={`relative rounded-2xl p-4 transition-all duration-200 border-2 flex flex-col items-center gap-2
                ${isSelected
                                    ? 'border-purple-400 bg-purple-400/20 scale-105 shadow-[0_0_20px_rgba(168,85,247,0.5)]'
                                    : 'border-gray-700/60 bg-gray-800/50 hover:border-gray-500 hover:scale-102'}`}>
                            {isSelected && (
                                <div className="absolute top-2 right-2 w-5 h-5 rounded-full bg-purple-400 text-black text-xs font-press flex items-center justify-center">
                                    {slotNum}
                                </div>
                            )}
                            {pet.petImageUrl ? (
                                <img src={pet.petImageUrl} alt={pet.petName}
                                    className="w-16 h-16 object-contain pixel-art" />
                            ) : (
                                <div className="w-16 h-16 rounded-full bg-gray-700 flex items-center justify-center text-3xl">🎯</div>
                            )}
                            <span className="font-pixel text-xs text-white text-center">{pet.petName}</span>
                            <span className="font-noto text-gray-400 text-xs text-center border bg-gray-900/50 border-gray-600 rounded px-1 mt-1">
                                Lv. {pet.level}
                            </span>
                        </button>
                    );
                })}
            </div>

            <div className="flex gap-3 mb-6">
                {[0, 1, 2].map(i => (
                    <div key={i} className={`w-10 h-10 rounded-full border-2 flex items-center justify-center transition-all
            ${i < selected.length
                            ? 'border-purple-400 bg-purple-400/30'
                            : 'border-gray-700 bg-gray-800/50'}`}>
                        {i < selected.length
                            ? <span className="text-purple-400 text-lg shadow-[0_0_5px_rgba(168,85,247,0.8)]">✓</span>
                            : <span className="text-gray-600 text-lg">?</span>}
                    </div>
                ))}
            </div>

            <button onClick={handleStart} disabled={selected.length !== 3 || starting || pets.length < 3}
                className={`w-full max-w-xs py-4 rounded-xl font-press text-sm transition-all duration-200
          ${selected.length === 3 && !starting
                        ? 'bg-purple-500 text-white hover:bg-purple-400 hover:scale-105 shadow-[0_0_25px_rgba(168,85,247,0.5)]'
                        : 'bg-gray-800 text-gray-600 cursor-not-allowed border border-gray-700'}`}>
                {starting ? 'PREPARING...' :
                    selected.length === 3
                        ? mode === 'create' ? 'CREATE ROOM'
                            : mode === 'matchmaking' ? '🔍 FIND MATCH'
                                : 'JOIN ROOM'
                        : `SELECT ${3 - selected.length} MORE`}
            </button>
        </div>
    );
}
