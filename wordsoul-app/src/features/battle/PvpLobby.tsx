import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { getMyPvpRating } from '../../services/pvp';
import type { PvpRatingDto } from '../../services/pvp';

export default function PvpLobby() {
    const navigate = useNavigate();
    const [joinCode, setJoinCode] = useState('');
    const [ratingInfo, setRatingInfo] = useState<PvpRatingDto | null>(null);
    const [loading, setLoading] = useState(true);

    useEffect(() => {
        getMyPvpRating()
            .then(data => setRatingInfo(data))
            .catch(err => console.error('Failed to load rating', err))
            .finally(() => setLoading(false));
    }, []);

    const handleCreate = () => {
        navigate('/pvp/pets?mode=create');
    };

    const handleJoin = () => {
        if (joinCode.trim().length === 6) {
            navigate(`/pvp/pets?mode=join&code=${joinCode.trim().toUpperCase()}`);
        }
    };

    return (
        <div className="min-h-screen text-white flex flex-col items-center py-12 px-4"
            style={{ background: 'rgb(2,6,23)' }}>
            
            <button onClick={() => navigate('/home')}
                className="self-start text-gray-400 hover:text-white font-pixel text-xs mb-6 flex items-center gap-1">
                ← Back to Home
            </button>

            <h1 className="font-press text-3xl text-purple-400 mb-2 drop-shadow-[0_0_15px_rgba(168,85,247,0.6)]">
                PvP ARENA
            </h1>
            <p className="font-noto text-gray-400 text-sm mb-8">
                Battle against other players in real-time
            </p>

            {/* User Rating Card */}
            {!loading && ratingInfo && (
                <div className="mb-10 w-full max-w-sm rounded-2xl bg-purple-900/20 border border-purple-500/40 p-6 flex flex-col items-center">
                    <div className="w-16 h-16 rounded-full bg-purple-800/50 flex items-center justify-center text-3xl mb-3 shadow-[0_0_15px_rgba(168,85,247,0.5)]">
                        🏆
                    </div>
                    <div className="font-pixel text-xs text-purple-300 mb-1">YOUR RATING</div>
                    <div className="font-press text-4xl text-yellow-400 mb-4">{ratingInfo.pvpRating}</div>
                    
                    <div className="grid grid-cols-2 gap-4 w-full">
                        <div className="text-center bg-gray-900/50 rounded-lg py-2 border border-gray-700">
                            <div className="font-pixel text-[10px] text-green-400">WINS</div>
                            <div className="font-press text-lg text-white">{ratingInfo.pvpWins}</div>
                        </div>
                        <div className="text-center bg-gray-900/50 rounded-lg py-2 border border-gray-700">
                            <div className="font-pixel text-[10px] text-red-400">LOSSES</div>
                            <div className="font-press text-lg text-white">{ratingInfo.pvpLosses}</div>
                        </div>
                    </div>
                </div>
            )}

            {/* Action Buttons */}
            <div className="w-full max-w-sm space-y-6">
                <div className="bg-gray-800/40 p-6 rounded-2xl border border-gray-700 flex flex-col gap-4">
                    <h2 className="font-pixel text-sm text-center text-gray-300">Host a Match</h2>
                    <button onClick={handleCreate}
                        className="w-full py-4 rounded-xl font-press text-sm bg-purple-600 hover:bg-purple-500 text-white transition-all shadow-[0_0_20px_rgba(168,85,247,0.4)]">
                        CREATE ROOM
                    </button>
                </div>

                <div className="bg-gray-800/40 p-6 rounded-2xl border border-gray-700 flex flex-col gap-4 relative">
                    <h2 className="font-pixel text-sm text-center text-gray-300">Join a Match</h2>
                    <input 
                        type="text" 
                        value={joinCode}
                        onChange={e => setJoinCode(e.target.value)}
                        placeholder="ENTER 6-CHAR CODE"
                        maxLength={6}
                        className="w-full bg-gray-900 border-2 border-gray-600 rounded-xl px-4 py-3 font-press text-center text-xl text-yellow-400 uppercase tracking-widest focus:border-purple-500 focus:outline-none"
                    />
                    <button 
                        onClick={handleJoin}
                        disabled={joinCode.trim().length !== 6}
                        className={`w-full py-4 rounded-xl font-press text-sm transition-all
                            ${joinCode.trim().length === 6 
                                ? 'bg-blue-600 hover:bg-blue-500 text-white shadow-[0_0_20px_rgba(37,99,235,0.4)]' 
                                : 'bg-gray-700 text-gray-500 cursor-not-allowed'
                            }`}>
                        JOIN ROOM
                    </button>
                </div>
            </div>

        </div>
    );
}
