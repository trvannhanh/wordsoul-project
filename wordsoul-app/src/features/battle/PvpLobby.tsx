import { useCallback, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    getMyPvpRating,
    getWaitingPvpRooms,
    getPvpLeaderboard,
    getBattleHistory,
    getBattleHistoryDetail,
} from '../../services/pvp';
import type {
    PvpRatingDto,
    PvpLobbyRoomDto,
    PvpLeaderboardEntryDto,
    BattleHistoryEntryDto,
    BattleHistoryDetailDto,
} from '../../services/pvp';

const TIER_COLORS: Record<string, { text: string; glow: string; bg: string; border: string }> = {
    Bronze: { text: 'text-orange-400', glow: 'rgba(251,146,60,0.6)', bg: 'bg-orange-900/20', border: 'border-orange-500/40' },
    Silver: { text: 'text-gray-300', glow: 'rgba(156,163,175,0.6)', bg: 'bg-gray-700/30', border: 'border-gray-400/40' },
    Gold: { text: 'text-yellow-400', glow: 'rgba(250,204,21,0.6)', bg: 'bg-yellow-900/20', border: 'border-yellow-500/40' },
    Platinum: { text: 'text-blue-300', glow: 'rgba(147,197,253,0.6)', bg: 'bg-blue-900/20', border: 'border-blue-400/40' },
    Diamond: { text: 'text-cyan-300', glow: 'rgba(103,232,249,0.6)', bg: 'bg-cyan-900/20', border: 'border-cyan-400/40' },
};

const TIER_ICONS: Record<string, string> = {
    Bronze: '🥉', Silver: '🥈', Gold: '🥇', Platinum: '💎', Diamond: '👑',
};

export default function PvpLobby() {
    const navigate = useNavigate();

    const [joinCode, setJoinCode] = useState('');
    const [ratingInfo, setRatingInfo] = useState<PvpRatingDto | null>(null);
    const [ratingLoading, setRatingLoading] = useState(true);

    // Lobbies state
    const [lobbies, setLobbies] = useState<PvpLobbyRoomDto[]>([]);
    const [lobbiesLoading, setLobbiesLoading] = useState(false);

    // Leaderboard state
    const [leaderboard, setLeaderboard] = useState<PvpLeaderboardEntryDto[]>([]);
    const [leaderboardLoading, setLeaderboardLoading] = useState(false);

    // History state
    const [historyList, setHistoryList] = useState<BattleHistoryEntryDto[]>([]);
    const [historyLoading, setHistoryLoading] = useState(false);
    const [historyPage, setHistoryPage] = useState(1);
    const [historyTotalCount, setHistoryTotalCount] = useState(0);
    const historyPageSize = 5; // Reduced page size to fit cleanly on the single-page dashboard

    // Detail Modal state
    const [selectedSessionId, setSelectedSessionId] = useState<number | null>(null);
    const [detailData, setDetailData] = useState<BattleHistoryDetailDto | null>(null);
    const [detailLoading, setDetailLoading] = useState(false);

    const fetchLobbies = useCallback(() => {
        setLobbiesLoading(true);
        getWaitingPvpRooms()
            .then(data => setLobbies(data))
            .catch(err => console.error('Failed to load waiting rooms', err))
            .finally(() => setLobbiesLoading(false));
    }, []);

    const fetchLeaderboard = useCallback(() => {
        setLeaderboardLoading(true);
        getPvpLeaderboard(50)
            .then(data => setLeaderboard(data))
            .catch(err => console.error('Failed to load leaderboard', err))
            .finally(() => setLeaderboardLoading(false));
    }, []);

    const fetchHistory = useCallback(() => {
        setHistoryLoading(true);
        getBattleHistory('PvP', historyPage, historyPageSize)
            .then(data => {
                setHistoryList(data.items);
                setHistoryTotalCount(data.totalCount);
            })
            .catch(err => console.error('Failed to load battle history', err))
            .finally(() => setHistoryLoading(false));
    }, [historyPage, historyPageSize]);

    // Fetch all dashboard data on mount
    useEffect(() => {
        getMyPvpRating()
            .then(data => setRatingInfo(data))
            .catch(err => console.error('Failed to load rating', err))
            .finally(() => setRatingLoading(false));

        fetchLobbies();
        fetchLeaderboard();
    }, [fetchLeaderboard, fetchLobbies]);

    // Reload history when page changes
    useEffect(() => {
        fetchHistory();
    }, [fetchHistory]);
    const handleOpenDetail = (sessionId: number) => {
        setSelectedSessionId(sessionId);
        setDetailLoading(true);
        setDetailData(null);
        getBattleHistoryDetail(sessionId)
            .then(data => setDetailData(data))
            .catch(err => console.error('Failed to load detail', err))
            .finally(() => setDetailLoading(false));
    };

    const handleCloseDetail = () => {
        setSelectedSessionId(null);
        setDetailData(null);
    };

    const tierStyle = ratingInfo ? (TIER_COLORS[ratingInfo.tier] ?? TIER_COLORS.Bronze) : null;
    const winRate = ratingInfo && (ratingInfo.pvpWins + ratingInfo.pvpLosses) > 0
        ? Math.round((ratingInfo.pvpWins / (ratingInfo.pvpWins + ratingInfo.pvpLosses)) * 100)
        : null;
    const totalPages = Math.ceil(historyTotalCount / historyPageSize);

    // Simulate pet hp states round by round for detail view
    const renderRoundSummaryList = (detail: BattleHistoryDetailDto) => {
        const p1Pets = detail.p1.selectedPets.map(p => ({ ...p, currentHp: p.maxHp }));
        const p2Pets = detail.p2.selectedPets.map(p => ({ ...p, currentHp: p.maxHp }));

        return detail.rounds.map((round) => {
            const activeP1Idx = p1Pets.findIndex(p => p.currentHp > 0);
            const activeP2Idx = p2Pets.findIndex(p => p.currentHp > 0);

            const activeP1 = activeP1Idx !== -1 ? p1Pets[activeP1Idx] : null;
            const activeP2 = activeP2Idx !== -1 ? p2Pets[activeP2Idx] : null;

            let dmgText = '';
            let faintText = '';

            if (round.damageDealt > 0 && round.damagedPlayer > 0) {
                const targetPlayer = round.damagedPlayer;
                const activePet = targetPlayer === 1 ? activeP1 : activeP2;
                const attackerName = targetPlayer === 1 ? detail.p2.name : detail.p1.name;
                const defenderName = targetPlayer === 1 ? detail.p1.name : detail.p2.name;

                if (activePet) {
                    activePet.currentHp = Math.max(0, activePet.currentHp - round.damageDealt);
                    const isFainted = activePet.currentHp === 0;

                    let multText = '';
                    if (round.typeMultiplier > 1) {
                        multText = ' (Siêu hiệu quả! 🔥)';
                    } else if (round.typeMultiplier < 1 && round.typeMultiplier > 0) {
                        multText = ' (Không hiệu quả lắm... 💧)';
                    }

                    dmgText = `${attackerName} gây ${round.damageDealt} sát thương lên ${activePet.displayName}${multText}. HP còn: ${activePet.currentHp}/${activePet.maxHp}`;

                    if (isFainted) {
                        faintText = `💀 ${activePet.displayName} của ${defenderName} đã bị hạ gục!`;
                    }
                }
            } else {
                dmgText = 'Không ai chịu sát thương.';
            }

            const p1AnsColor = round.p1Correct ? 'text-green-400' : 'text-red-400';
            const p2AnsColor = round.p2Correct ? 'text-green-400' : 'text-red-400';

            return (
                <div key={round.roundIndex} className="bg-black/40 border border-gray-800 rounded-xl p-4 space-y-3 font-noto">
                    <div className="flex justify-between items-center border-b border-gray-800 pb-2">
                        <span className="font-press text-[10px] text-purple-400">ROUND {round.roundIndex + 1}</span>
                        <div className="text-right">
                            <span className="font-press text-[11px] text-yellow-400 tracking-wider uppercase">{round.word}</span>
                            <span className="text-xs text-gray-400 ml-2">({round.meaning})</span>
                        </div>
                    </div>

                    <div className="grid grid-cols-2 gap-4 text-xs">
                        <div className="bg-slate-900/50 p-2.5 rounded-lg border border-slate-800">
                            <div className="font-pixel text-[9px] text-gray-500 mb-1">{detail.p1.name.toUpperCase()}</div>
                            <div className="flex flex-col">
                                <span className={`font-semibold ${p1AnsColor}`}>
                                    {round.p1Correct ? '✓' : '✗'} {round.p1Answer || '[Không trả lời]'}
                                </span>
                                <span className="text-[10px] text-gray-500 mt-0.5">Thời gian: {round.p1AnswerMs ? `${round.p1AnswerMs}ms` : '-'}</span>
                                <span className="text-[10px] text-gray-400">Điểm round: +{round.p1Score ?? 0}</span>
                            </div>
                        </div>

                        <div className="bg-slate-900/50 p-2.5 rounded-lg border border-slate-800">
                            <div className="font-pixel text-[9px] text-gray-500 mb-1">{detail.p2.name.toUpperCase()}</div>
                            <div className="flex flex-col">
                                <span className={`font-semibold ${p2AnsColor}`}>
                                    {round.p2Correct ? '✓' : '✗'} {round.p2Answer || '[Không trả lời]'}
                                </span>
                                <span className="text-[10px] text-gray-500 mt-0.5">Thời gian: {round.p2AnswerMs ? `${round.p2AnswerMs}ms` : '-'}</span>
                                <span className="text-[10px] text-gray-400">Điểm round: +{round.p2Score ?? 0}</span>
                            </div>
                        </div>
                    </div>

                    <div className="bg-purple-950/15 border border-purple-900/30 p-2.5 rounded-lg text-xs space-y-1">
                        <div className="text-purple-300 flex items-center gap-1.5">
                            <span>⚔️</span>
                            <span>{dmgText}</span>
                        </div>
                        {faintText && (
                            <div className="text-red-400 font-bold flex items-center gap-1.5 animate-pulse mt-1">
                                <span>{faintText}</span>
                            </div>
                        )}
                    </div>
                </div>
            );
        });
    };

    return (
        <div className="min-h-screen text-white flex flex-col items-center py-8 px-4 relative overflow-hidden"
            style={{ background: 'linear-gradient(160deg, rgb(2,6,23) 0%, rgba(88,28,135,0.1) 50%, rgb(2,6,23) 100%)' }}>

            {/* Background glow */}
            <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[800px] h-[400px] rounded-full opacity-10 blur-3xl pointer-events-none"
                style={{ background: 'radial-gradient(ellipse, #7c3aed, transparent)' }} />

            {/* Top Bar Navigation */}
            <div className="w-full max-w-7xl flex justify-between items-center mb-6 z-10">
                <button onClick={() => navigate('/home')}
                    className="text-gray-400 hover:text-white font-pixel text-lg flex items-center gap-1 transition-colors custom-cursor">
                    ← Home
                </button>
                <div className="text-right">
                    <h1 className="font-pixel text-2xl md:text-3xl text-purple-400 tracking-wider"
                        style={{ textShadow: '0 0 15px rgba(168,85,247,0.5)' }}>
                        ĐẤU TRƯỜNG XẾP HẠNG
                    </h1>
                </div>
            </div>

            {/* MAIN DASHBOARD GRID */}
            <div className="w-full max-w-7xl grid grid-cols-1 lg:grid-cols-12 gap-6 z-10 px-2 items-start">

                {/* ═══════════════════════════════════════════════════════
                    LEFT COLUMN: PERSONAL STATS & PLAY ACTIONS (Span 3)
                    ═══════════════════════════════════════════════════════ */}
                <div className="lg:col-span-3 space-y-4">
                    {/* Rating Card */}
                    {!ratingLoading && ratingInfo && tierStyle && (
                        <div className={`w-full rounded-2xl ${tierStyle.bg} border ${tierStyle.border} p-5`}
                            style={{ boxShadow: `0 0 15px ${tierStyle.glow}20` }}>
                            <div className="flex items-center gap-3 mb-4">
                                <div className={`w-12 h-12 rounded-full flex items-center justify-center text-2xl border ${tierStyle.border}`}
                                    style={{ background: 'rgba(0,0,0,0.4)' }}>
                                    {TIER_ICONS[ratingInfo.tier] ?? '🏆'}
                                </div>
                                <div>
                                    <div className="font-pixel text-[8px] text-gray-500">RANK</div>
                                    <div className={`font-press text-sm ${tierStyle.text}`}>
                                        {ratingInfo.tier.toUpperCase()}
                                    </div>
                                </div>
                                <div className="ml-auto text-right">
                                    <div className="font-pixel text-[8px] text-gray-500">ĐIỂM XẾP HẠNG</div>
                                    <div className="font-press text-base text-white">{ratingInfo.pvpRating}</div>
                                </div>
                            </div>

                            <div className="grid grid-cols-3 gap-1.5">
                                <div className="text-center bg-black/30 rounded-lg py-1.5 border border-gray-800">
                                    <div className="font-pixel text-[8px] text-green-400">THẮNG</div>
                                    <div className="font-press text-xs text-white">{ratingInfo.pvpWins}</div>
                                </div>
                                <div className="text-center bg-black/30 rounded-lg py-1.5 border border-gray-800">
                                    <div className="font-pixel text-[8px] text-red-400">THUA</div>
                                    <div className="font-press text-xs text-white">{ratingInfo.pvpLosses}</div>
                                </div>
                                <div className="text-center bg-black/30 rounded-lg py-1.5 border border-gray-800">
                                    <div className="font-pixel text-[8px] text-blue-400">TỈ LỆ THẮNG</div>
                                    <div className="font-press text-xs text-white">
                                        {winRate !== null ? `${winRate}%` : '-'}
                                    </div>
                                </div>
                            </div>
                        </div>
                    )}
                    {ratingLoading && (
                        <div className="w-full h-32 rounded-2xl bg-gray-800/30 border border-gray-700/30 animate-pulse" />
                    )}

                    {/* Find Match Card */}
                    <div className="relative overflow-hidden rounded-2xl border border-purple-500/40 bg-purple-950/20 p-5"
                        style={{ boxShadow: '0 0 20px rgba(168,85,247,0.1)' }}>
                        <div className="absolute top-0 left-0 right-0 h-0.5"
                            style={{ background: 'linear-gradient(90deg, transparent, #a855f7, #3b82f6, transparent)' }} />
                        <div className="flex items-center gap-2 mb-3">
                            <span className="text-xl">🔍</span>
                            <div>
                                <h2 className="font-pixel text-xs text-purple-300">TÌM TRẬN ĐẤU</h2>
                                <p className="font-noto text-gray-500 text-[10px] mt-0.5">ELO Matchmaking tự động</p>
                            </div>
                        </div>
                        <button
                            id="btn-find-match"
                            onClick={() => navigate('/pvp/pets?mode=matchmaking')}
                            className="w-full py-3 rounded-xl font-pixel text-lg custom-cursor text-white transition-all duration-200
                                bg-gradient-to-r from-purple-600 to-indigo-600 hover:from-purple-500 hover:to-indigo-500 hover:scale-[1.01]
                                shadow-[0_4px_15px_rgba(168,85,247,0.3)]">
                            Bắt Đầu
                        </button>
                    </div>

                    {/* Create Room Card */}
                    <div className="rounded-2xl border border-gray-800 bg-gray-900/20 p-4 flex flex-col gap-2.5">
                        <div className="flex items-center gap-2">
                            <span className="text-lg">🏠</span>
                            <div>
                                <h2 className="font-pixel text-[10px] text-gray-300">TẠO PHÒNG CHỜ</h2>
                                <p className="font-noto text-gray-500 text-[9px] mt-0.5">Nhận code mời bạn bè</p>
                            </div>
                        </div>
                        <button
                            id="btn-create-room"
                            onClick={() => navigate('/pvp/pets?mode=create')}
                            className="w-full py-2.5 rounded-xl font-pixel text-lg bg-gray-800 hover:bg-gray-700 text-white transition-all custom-cursor">
                            TẠO PHÒNG
                        </button>
                    </div>

                    {/* Join Room Card */}
                    <div className="rounded-2xl border border-gray-800 bg-gray-900/20 p-4 flex flex-col gap-2.5">
                        <div className="flex items-center gap-2">
                            <span className="text-lg">🎮</span>
                            <div>
                                <h2 className="font-pixel text-[10px] text-gray-300">VÀO PHÒNG CODE</h2>
                                <p className="font-noto text-gray-500 text-[9px] mt-0.5">Nhập mã phòng của đối thủ</p>
                            </div>
                        </div>
                        <input
                            id="input-room-code"
                            type="text"
                            value={joinCode}
                            onChange={e => setJoinCode(e.target.value.toUpperCase())}
                            placeholder="CODE PHÒNG"
                            maxLength={6}
                            className="w-full bg-gray-950 border border-gray-700 rounded-xl px-3 py-2
                                font-press text-center text-sm text-yellow-400 uppercase tracking-widest
                                focus:border-purple-500 focus:outline-none focus:shadow-[0_0_8px_rgba(168,85,247,0.2)]
                                transition-all placeholder:text-gray-800"
                        />
                        <button
                            id="btn-join-room"
                            onClick={() => {
                                if (joinCode.trim().length === 6)
                                    navigate(`/pvp/pets?mode=join&code=${joinCode.trim()}`);
                            }}
                            disabled={joinCode.trim().length !== 6}
                            className={`w-full py-2.5 rounded-xl font-press text-[10px] transition-all
                                ${joinCode.trim().length === 6
                                    ? 'bg-blue-600 hover:bg-blue-500 custom-cursor text-white shadow-[0_0_10px_rgba(37,99,235,0.2)]'
                                    : 'bg-gray-800/40 text-gray-600 cursor-not-allowed border border-gray-850'}`}>
                            THAM GIA PHÒNG
                        </button>
                    </div>
                </div>

                {/* ═══════════════════════════════════════════════════════
                    MIDDLE COLUMN: LOBBIES & MATCH HISTORY (Span 6)
                    ═══════════════════════════════════════════════════════ */}
                <div className="lg:col-span-6 space-y-6">

                    {/* Waiting Lobbies List */}
                    <div className="bg-black/20 border border-gray-800 rounded-2xl p-5 space-y-4">
                        <div className="flex justify-between items-center">
                            <div className="flex items-center gap-2">
                                <span className="text-lg">🏠</span>
                                <h2 className="font-pixel text-lg text-purple-300">PHÒNG CHỜ CÔNG KHAI</h2>
                            </div>
                            <button
                                onClick={fetchLobbies}
                                disabled={lobbiesLoading}
                                className="bg-purple-950/40 hover:bg-purple-900/40 border border-purple-500/20 text-purple-300 font-pixel text-sm custom-cursor py-1 px-2.5 rounded-lg flex items-center gap-1 transition-all">
                                <span>{lobbiesLoading ? '🔄 Đang tải...' : '🔄 Tải lại'}</span>
                            </button>
                        </div>

                        {lobbiesLoading && (
                            <div className="space-y-2">
                                <div className="h-16 bg-gray-800/20 border border-gray-850 rounded-xl animate-pulse" />
                            </div>
                        )}

                        {!lobbiesLoading && lobbies.length === 0 && (
                            <div className="text-center py-6 bg-gray-900/10 border border-gray-850 rounded-xl p-4">
                                <p className="font-pixel text-[9px] text-gray-500">Chưa có phòng chờ nào đang trực tuyến.</p>
                            </div>
                        )}

                        {!lobbiesLoading && lobbies.length > 0 && (
                            <div className="grid gap-2 max-h-[190px] overflow-y-auto pr-1">
                                {lobbies.map((room) => (
                                    <div key={room.sessionId} className="bg-gray-950/30 border border-gray-800 hover:border-purple-500/20 rounded-xl p-3 flex items-center justify-between transition-all">
                                        <div>
                                            <div className="flex items-center gap-2">
                                                <span className="font-press text-sm text-yellow-400 tracking-wider">{room.roomCode}</span>
                                                <span className="text-[8px] font-pixel text-purple-400">Host: {room.hostUsername}</span>
                                            </div>
                                            <div className="text-[9px] text-gray-500 font-noto mt-0.5">
                                                Rating: {room.hostRating} · Tạo lúc: {new Date(room.createdAt).toLocaleTimeString()}
                                            </div>
                                        </div>
                                        <button
                                            onClick={() => navigate(`/pvp/pets?mode=join&code=${room.roomCode}`)}
                                            className="bg-blue-600 hover:bg-blue-500 text-white font-press text-[9px] py-2 px-3.5 rounded-lg transition-all">
                                            THAM GIA ⚔️
                                        </button>
                                    </div>
                                ))}
                            </div>
                        )}
                    </div>

                    {/* Battle History List */}
                    <div className="bg-black/20 border border-gray-800 rounded-2xl p-5 space-y-4">
                        <div className="flex flex-col md:flex-row md:justify-between md:items-center gap-2">
                            <div className="flex items-center gap-2">
                                <span className="text-lg">📜</span>
                                <h2 className="font-pixel text-lg text-purple-300">LỊCH SỬ ĐẤU PVP</h2>
                            </div>
                        </div>

                        {historyLoading && (
                            <div className="space-y-2">
                                {[1, 2].map(n => (
                                    <div key={n} className="h-16 bg-gray-800/10 border border-gray-850 rounded-xl animate-pulse" />
                                ))}
                            </div>
                        )}

                        {!historyLoading && historyList.length === 0 && (
                            <div className="text-center py-10 bg-gray-900/10 border border-gray-850 rounded-xl p-4">
                                <p className="font-pixel text-[9px] text-gray-500">Chưa có dữ liệu trận đấu.</p>
                            </div>
                        )}

                        {!historyLoading && historyList.length > 0 && (
                            <div className="space-y-4">
                                <div className="grid gap-2">
                                    {historyList.map((match) => {
                                        const isWin = match.currentUserWon;
                                        const isAbandoned = match.status === 'Abandoned';

                                        let resultText = 'THẮNG';
                                        let resultColor = 'text-green-400 bg-green-950/20 border-green-500/20';
                                        if (isAbandoned) {
                                            resultText = 'BỎ CUỘC';
                                            resultColor = 'text-gray-400 bg-gray-900/30 border-gray-800';
                                        } else if (!isWin) {
                                            resultText = 'THUA';
                                            resultColor = 'text-red-400 bg-red-950/20 border-red-500/20';
                                        }

                                        return (
                                            <div key={match.sessionId} className="bg-gray-950/30 border border-gray-800 rounded-xl p-3 flex flex-col md:flex-row md:items-center justify-between gap-2 font-noto">
                                                <div className="space-y-1">
                                                    <div className="flex items-center gap-1.5">
                                                        <span className={`text-[8px] font-pixel px-1 py-0.2 rounded border ${resultColor}`}>
                                                            {resultText}
                                                        </span>
                                                        <span className="font-pixel text-[8px] text-purple-400">
                                                            RANKED PVP
                                                        </span>
                                                    </div>

                                                    <div className="text-xs font-semibold">
                                                        {match.isCurrentUserP1 ? match.challengerUsername : match.opponentName}
                                                        <span className="text-gray-500 font-normal mx-1">vs</span>
                                                        <span className="text-purple-300">{match.isCurrentUserP1 ? match.opponentName : match.challengerUsername}</span>
                                                    </div>

                                                    <div className="text-[10px] text-gray-400">
                                                        Round: <span className="font-mono text-white">{match.isCurrentUserP1 ? match.challengerCorrect : match.opponentCorrect}-{match.isCurrentUserP1 ? match.opponentCorrect : match.challengerCorrect}</span>
                                                        <span className="text-gray-600 mx-1.5">|</span>
                                                        Điểm: <span className="font-mono text-white">{match.isCurrentUserP1 ? match.challengerTotalScore : match.opponentTotalScore}-{match.isCurrentUserP1 ? match.opponentTotalScore : match.challengerTotalScore}</span>
                                                        <span className="text-gray-600 mx-1.5">|</span>
                                                        <span className="text-[9px] text-gray-500 font-pixel">{new Date(match.startedAt).toLocaleString('vi-VN')}</span>
                                                    </div>
                                                </div>

                                                <button
                                                    onClick={() => handleOpenDetail(match.sessionId)}
                                                    className="custom-cursor bg-slate-800 hover:bg-slate-700 border border-gray-700 text-gray-300 font-pixel text-[8px] py-1.5 px-2.5 rounded-lg transition-all self-end md:self-center">
                                                    XEM CHI TIẾT
                                                </button>
                                            </div>
                                        );
                                    })}
                                </div>

                                {/* Pagination Controls */}
                                {totalPages > 1 && (
                                    <div className="flex justify-between items-center bg-black/40 border border-gray-800 rounded-xl p-2.5 font-pixel text-[8px]">
                                        <button
                                            onClick={() => setHistoryPage(prev => Math.max(1, prev - 1))}
                                            disabled={historyPage === 1}
                                            className={`custom-cursor px-2.5 py-1.5 rounded-lg transition-all border ${historyPage === 1 ? 'border-gray-800/30 text-gray-600 cursor-not-allowed' : 'border-purple-500/30 text-purple-300 hover:bg-purple-950/20'}`}>
                                            ◀ PREV
                                        </button>
                                        <span className="text-gray-400">
                                            PAGE {historyPage} / {totalPages}
                                        </span>
                                        <button
                                            onClick={() => setHistoryPage(prev => Math.min(totalPages, prev + 1))}
                                            disabled={historyPage >= totalPages}
                                            className={`custom-cursor px-2.5 py-1.5 rounded-lg transition-all border ${historyPage >= totalPages ? 'border-gray-800/30 text-gray-600 cursor-not-allowed' : 'border-purple-500/30 text-purple-300 hover:bg-purple-950/20'}`}>
                                            NEXT ▶
                                        </button>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>
                </div>

                {/* ═══════════════════════════════════════════════════════
                    RIGHT COLUMN: PVP LEADERBOARD (Span 3)
                    ═══════════════════════════════════════════════════════ */}
                <div className="lg:col-span-3">
                    <div className="bg-black/20 border border-gray-800 rounded-2xl p-5 space-y-4">
                        <div className="flex items-center gap-2">
                            <span className="text-lg">🏆</span>
                            <h2 className="font-pixel text-lg text-purple-300">BẢNG XẾP HẠNG</h2>
                        </div>

                        {leaderboardLoading && (
                            <div className="space-y-2">
                                {[1, 2, 3, 4].map(n => (
                                    <div key={n} className="h-10 bg-gray-800/25 border border-gray-850 rounded-xl animate-pulse" />
                                ))}
                            </div>
                        )}

                        {!leaderboardLoading && leaderboard.length === 0 && (
                            <div className="text-center py-6 bg-gray-900/10 border border-gray-850 rounded-xl">
                                <p className="font-pixel text-[8px] text-gray-500">Chưa có người chơi.</p>
                            </div>
                        )}

                        {!leaderboardLoading && leaderboard.length > 0 && (
                            <div className="border border-gray-850 rounded-xl overflow-hidden font-noto">
                                <div className="grid grid-cols-12 gap-1 bg-purple-950/15 border-b border-gray-850 px-3 py-2 font-press text-[7px] text-gray-400">
                                    <div className="col-span-2">RK</div>
                                    <div className="col-span-6">PLAYER</div>
                                    <div className="col-span-4 text-right">ELO</div>
                                </div>

                                <div className="divide-y divide-gray-900 max-h-[350px] overflow-y-auto">
                                    {leaderboard.slice(0, 20).map((player) => {
                                        let rankBg = '';
                                        let rankEmoji = '';
                                        let rankColor = 'text-white';

                                        if (player.rank === 1) {
                                            rankBg = 'bg-yellow-950/10 border-l-2 border-yellow-500';
                                            rankEmoji = '👑';
                                            rankColor = 'text-yellow-400 font-bold';
                                        } else if (player.rank === 2) {
                                            rankBg = 'bg-slate-800/10 border-l-2 border-gray-400';
                                            rankEmoji = '🥈';
                                            rankColor = 'text-gray-300 font-bold';
                                        } else if (player.rank === 3) {
                                            rankBg = 'bg-amber-900/10 border-l-2 border-amber-600';
                                            rankEmoji = '🥉';
                                            rankColor = 'text-amber-500 font-bold';
                                        }

                                        return (
                                            <div key={player.userId} className={`grid grid-cols-12 gap-1 items-center px-3 py-2.5 text-[11px] transition-all hover:bg-white/5 ${rankBg}`}>
                                                <div className="col-span-2 font-press text-[8px] text-gray-500 flex items-center">
                                                    {rankEmoji ? <span>{rankEmoji}</span> : <span className="pl-1">{player.rank}</span>}
                                                </div>
                                                <div className="col-span-6 flex items-center gap-1.5">
                                                    {player.avatarUrl ? (
                                                        <img src={player.avatarUrl} alt={player.userName} className="w-4 h-4 rounded-full border border-gray-800 bg-gray-900" />
                                                    ) : (
                                                        <div className="w-4 h-4 rounded-full bg-purple-900/40 flex items-center justify-center text-[7px] font-pixel border border-purple-500/20 text-purple-400">👤</div>
                                                    )}
                                                    <span className={`font-semibold truncate ${rankColor}`}>{player.userName}</span>
                                                </div>
                                                <div className="col-span-4 text-right font-press text-[8px] text-purple-300">{player.pvpRating}</div>
                                            </div>
                                        );
                                    })}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            </div>

            {/* DETAIL REPLAY MODAL */}
            {selectedSessionId !== null && (
                <div className="fixed inset-0 bg-black/85 backdrop-blur-sm z-50 flex items-center justify-center p-4">
                    <div className="bg-slate-900 border border-purple-500/30 rounded-3xl p-6 max-w-3xl w-full max-h-[90vh] overflow-y-auto relative text-white flex flex-col">

                        <button
                            onClick={handleCloseDetail}
                            className="absolute top-4 right-4 text-gray-400 hover:text-white text-xl font-press p-1 hover:bg-white/5 rounded-lg">
                            ✕
                        </button>

                        {detailLoading && (
                            <div className="py-20 text-center flex flex-col items-center justify-center gap-4">
                                <div className="w-10 h-10 border-4 border-purple-500 border-t-transparent rounded-full animate-spin" />
                                <p className="font-pixel text-lg text-purple-400">ĐANG TẢI CHI TIẾT TRẬN ĐẤU...</p>
                            </div>
                        )}

                        {!detailLoading && detailData && (
                            <div className="space-y-6">
                                <div className="text-center border-b border-gray-800 pb-4">
                                    <span className="font-pixel text-xs text-purple-400 uppercase tracking-widest block mb-1">
                                        {detailData.type === 'GymBattle' ? 'PVE GYM BATTLE REPLAY' : 'RANKED PVP BATTLE REPLAY'}
                                    </span>
                                    <h3 className="font-pixel text-xl text-white mb-2">CHI TIẾT TRẬN ĐẤU</h3>

                                    <div className="my-3 flex justify-center">
                                        {detailData.status === 'Abandoned' ? (
                                            <span className="px-6 py-1.5 rounded-xl font-pixel text-lg text-gray-400 bg-gray-850 border border-gray-700 tracking-widest shadow-[0_0_15px_rgba(156,163,175,0.1)]">
                                                BỎ CUỘC
                                            </span>
                                        ) : detailData.currentUserWon ? (
                                            <span className="px-6 py-1.5 rounded-xl font-pixel text-lg text-green-400 bg-green-950/20 border border-green-500/40 tracking-widest shadow-[0_0_20px_rgba(74,222,128,0.3)] animate-pulse">
                                                CHIẾN THẮNG
                                            </span>
                                        ) : (
                                            <span className="px-6 py-1.5 rounded-xl font-pixel text-lg text-red-400 bg-red-950/20 border border-red-500/40 tracking-widest shadow-[0_0_20px_rgba(248,113,113,0.3)]">
                                                THẤT BẠI
                                            </span>
                                        )}
                                    </div>

                                    <p className="text-xs text-gray-500 font-pixel mt-1">
                                        Bắt đầu: {new Date(detailData.startedAt).toLocaleString('vi-VN')}
                                        {detailData.completedAt && ` · Thời gian: ${Math.round((new Date(detailData.completedAt).getTime() - new Date(detailData.startedAt).getTime()) / 1000)} giây`}
                                    </p>
                                </div>

                                <div className="grid grid-cols-2 gap-4">
                                    {/* Player 1 */}
                                    <div className="bg-slate-950/40 border border-gray-800 rounded-2xl p-4 flex flex-col items-center text-center">
                                        <div className="w-10 h-10 bg-purple-900/30 border border-purple-500/20 rounded-full flex items-center justify-center text-lg font-pixel mb-1.5 text-purple-300">👤</div>
                                        <span className="font-bold text-sm text-white truncate max-w-full">{detailData.p1.name}</span>
                                        <span className="text-[10px] text-gray-500 font-pixel mt-0.5">PLAYER 1</span>

                                        <div className="mt-3 grid grid-cols-2 gap-2.5 w-full text-center">
                                            <div className="bg-black/30 rounded-lg py-1 px-1 border border-gray-900">
                                                <div className="text-[8px] text-gray-500 font-pixel">CÂU ĐÚNG</div>
                                                <div className="font-press text-xs text-green-400">{detailData.p1.correctCount} / {detailData.rounds.length}</div>
                                            </div>
                                            <div className="bg-black/30 rounded-lg py-1 px-1 border border-gray-900">
                                                <div className="text-[8px] text-gray-500 font-pixel">TỔNG ĐIỂM</div>
                                                <div className="font-press text-xs text-yellow-400">{detailData.p1.totalScore}</div>
                                            </div>
                                        </div>

                                        <div className="mt-4 w-full">
                                            <div className="text-[8px] text-gray-500 font-pixel mb-1.5 text-left uppercase">Đội hình Pet:</div>
                                            <div className="grid grid-cols-3 gap-1">
                                                {detailData.p1.selectedPets.map((pet, idx) => (
                                                    <div key={idx} className={`relative p-1 bg-black/40 border border-gray-800 rounded-lg text-center ${pet.isFainted ? 'opacity-40 grayscale' : ''}`}>
                                                        {pet.imageUrl ? (
                                                            <img src={pet.imageUrl} alt={pet.displayName} className="w-8 h-8 mx-auto object-contain" />
                                                        ) : (
                                                            <div className="w-8 h-8 mx-auto flex items-center justify-center text-base">👾</div>
                                                        )}
                                                        <div className="text-[8px] font-bold truncate mt-1 text-white">{pet.displayName}</div>
                                                        <div className="w-full bg-red-950 rounded-full h-1 mt-1 overflow-hidden">
                                                            <div className="bg-green-500 h-full transition-all" style={{ width: `${(pet.currentHp / pet.maxHp) * 100}%` }} />
                                                        </div>
                                                        <div className="text-[7px] text-gray-400 mt-0.5">{pet.currentHp}/{pet.maxHp}</div>
                                                        {pet.isFainted && (
                                                            <span className="absolute inset-0 flex items-center justify-center text-[8px] font-bold text-red-500 bg-black/60 rounded-lg">KO</span>
                                                        )}
                                                    </div>
                                                ))}
                                            </div>
                                        </div>
                                    </div>

                                    {/* Player 2 */}
                                    <div className="bg-slate-950/40 border border-gray-800 rounded-2xl p-4 flex flex-col items-center text-center">
                                        <div className="w-10 h-10 bg-purple-900/30 border border-purple-500/20 rounded-full flex items-center justify-center text-lg font-pixel mb-1.5 text-purple-300">🤖</div>
                                        <span className="font-bold text-sm text-white truncate max-w-full">{detailData.p2.name}</span>
                                        <span className="text-[10px] text-gray-500 font-pixel mt-0.5">{detailData.type === 'GymBattle' ? 'BOT GYM' : 'PLAYER 2'}</span>

                                        <div className="mt-3 grid grid-cols-2 gap-2.5 w-full text-center">
                                            <div className="bg-black/30 rounded-lg py-1 px-1 border border-gray-900">
                                                <div className="text-[8px] text-gray-500 font-pixel">CÂU ĐÚNG</div>
                                                <div className="font-press text-xs text-green-400">{detailData.p2.correctCount} / {detailData.rounds.length}</div>
                                            </div>
                                            <div className="bg-black/30 rounded-lg py-1 px-1 border border-gray-900">
                                                <div className="text-[8px] text-gray-500 font-pixel">TỔNG ĐIỂM</div>
                                                <div className="font-press text-xs text-yellow-400">{detailData.p2.totalScore}</div>
                                            </div>
                                        </div>

                                        <div className="mt-4 w-full">
                                            <div className="text-[8px] text-gray-500 font-pixel mb-1.5 text-left uppercase">Đội hình Pet:</div>
                                            <div className="grid grid-cols-3 gap-1">
                                                {detailData.p2.selectedPets.map((pet, idx) => (
                                                    <div key={idx} className={`relative p-1 bg-black/40 border border-gray-800 rounded-lg text-center ${pet.isFainted ? 'opacity-40 grayscale' : ''}`}>
                                                        {pet.imageUrl ? (
                                                            <img src={pet.imageUrl} alt={pet.displayName} className="w-8 h-8 mx-auto object-contain" />
                                                        ) : (
                                                            <div className="w-8 h-8 mx-auto flex items-center justify-center text-base">👾</div>
                                                        )}
                                                        <div className="text-[8px] font-bold truncate mt-1 text-white">{pet.displayName}</div>
                                                        <div className="w-full bg-red-950 rounded-full h-1 mt-1 overflow-hidden">
                                                            <div className="bg-green-500 h-full transition-all" style={{ width: `${(pet.currentHp / pet.maxHp) * 100}%` }} />
                                                        </div>
                                                        <div className="text-[7px] text-gray-400 mt-0.5">{pet.currentHp}/{pet.maxHp}</div>
                                                        {pet.isFainted && (
                                                            <span className="absolute inset-0 flex items-center justify-center text-[8px] font-bold text-red-500 bg-black/60 rounded-lg">KO</span>
                                                        )}
                                                    </div>
                                                ))}
                                            </div>
                                        </div>
                                    </div>
                                </div>

                                <div className="space-y-4">
                                    <div className="font-pixel text-lg text-purple-300 border-b border-gray-800 pb-2">NHẬT KÝ ĐẤU (COMBAT LOGS)</div>
                                    <div className="space-y-3">
                                        {renderRoundSummaryList(detailData)}
                                    </div>
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            )}
        </div>
    );
}
