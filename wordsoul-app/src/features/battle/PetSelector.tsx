import { useState, useEffect } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { setupArenaBattle } from '../../services/gym';
import { useAuth } from '../../hooks/Auth/useAuth';
import { fetchPets } from '../../services/pet';

interface OwnedPet {
    id: number;
    petId: number;
    petName: string;
    petImageUrl?: string;
    level: number;
}

export default function PetSelector() {
    const { gymId } = useParams<{ gymId: string }>();
    const navigate = useNavigate();
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
                    level: 1,
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
            const sessionId = await setupArenaBattle(Number(gymId), selected);
            navigate(`/arena/${sessionId}`);
        } catch (err: any) {
            setError(err?.response?.data?.error ?? 'Không thể bắt đầu trận đấu.');
            setStarting(false);
        }
    };

    if (loading) return (
        <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
            <div className="text-white font-pixel text-sm animate-pulse">Loading...</div>
        </div>
    );

    return (
        <div className="min-h-screen text-white flex flex-col items-center py-12 px-4"
            style={{ background: 'rgb(2,6,23)' }}>
            <button onClick={() => navigate(`/gym/${gymId}`)}
                className="self-start text-gray-400 hover:text-white font-pixel text-xs mb-6 flex items-center gap-1">
                ← Back
            </button>

            <h1 className="font-press text-xl text-yellow-400 mb-1 drop-shadow-[0_0_12px_rgba(250,204,21,0.6)]">
                CHOOSE YOUR TEAM
            </h1>
            <p className="font-noto text-gray-400 text-sm mb-8">
                Select exactly 3 Pokémon to bring into battle
            </p>

            {pets.length < 3 && (
                <div className="mb-6 px-4 py-3 rounded-xl bg-red-900/30 border border-red-500/40 text-red-400 font-pixel text-xs text-center">
                    ⚠️ You need at least 3 Pokémon to start a battle!
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
                                    ? 'border-yellow-400 bg-yellow-400/10 scale-105 shadow-[0_0_20px_rgba(250,204,21,0.5)]'
                                    : 'border-gray-700/60 bg-gray-800/50 hover:border-gray-500 hover:scale-102'}`}>
                            {isSelected && (
                                <div className="absolute top-2 right-2 w-5 h-5 rounded-full bg-yellow-400 text-black text-xs font-press flex items-center justify-center">
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
                            <span className="font-noto text-gray-400 text-xs">Lv. {pet.level}</span>
                        </button>
                    );
                })}
            </div>

            {/* Progress indicator */}
            <div className="flex gap-3 mb-6">
                {[0, 1, 2].map(i => (
                    <div key={i} className={`w-10 h-10 rounded-full border-2 flex items-center justify-center transition-all
            ${i < selected.length
                            ? 'border-yellow-400 bg-yellow-400/20'
                            : 'border-gray-700 bg-gray-800/50'}`}>
                        {i < selected.length
                            ? <span className="text-yellow-400 text-lg">✓</span>
                            : <span className="text-gray-600 text-lg">?</span>}
                    </div>
                ))}
            </div>

            <button onClick={handleStart} disabled={selected.length !== 3 || starting || pets.length < 3}
                className={`w-full max-w-xs py-4 rounded-2xl font-press text-sm transition-all duration-200
          ${selected.length === 3 && !starting
                        ? 'bg-yellow-400 text-black hover:bg-yellow-300 hover:scale-105 shadow-[0_0_25px_rgba(250,204,21,0.5)]'
                        : 'bg-gray-700 text-gray-500 cursor-not-allowed'}`}>
                {starting ? 'PREPARING BATTLE...' : selected.length === 3 ? '⚔️ START BATTLE!' : `SELECT ${3 - selected.length} MORE`}
            </button>
        </div>
    );
}
