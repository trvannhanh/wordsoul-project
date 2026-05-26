import { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { fetchGymDetail } from '../../services/gym';
import type { GymLeaderDto } from '../../types/GymTypes';
import { GymStatus } from '../../types/GymTypes';
import { getBattleHistory, getBattleHistoryDetail } from '../../services/pvp';
import type { BattleHistoryEntryDto, BattleHistoryDetailDto } from '../../services/pvp';


const THEME_COLORS: Record<string, string> = {
  DailyLife: '#a8a878', Nature: '#78c850', Food: '#f08030', Weather: '#a0c8f0',
  Technology: '#f8d030', Travel: '#e0c068', Health: '#ee99ac', Sports: '#c03028',
  Business: '#b8b8d0', Science: '#f85888', Art: '#a8b820', Communication: '#6890f0',
  Mystery: '#705898', Dark: '#705848', Academic: '#98d8d8',
  Challenge: '#b8a038', TrapWords: '#a040a0', System: '#7038f8', Custom: '#a8a8a8',
};
const THEME_EMOJI: Record<string, string> = {
  DailyLife: '⭐', Nature: '🌿', Food: '🔥', Weather: '🦅',
  Technology: '⚡', Travel: '🏜️', Health: '✨', Sports: '🥋',
  Business: '⚙️', Science: '🔮', Art: '🐛', Communication: '💧',
  Mystery: '👻', Dark: '🌙', Academic: '❄️',
  Challenge: '🪨', TrapWords: '☠️', System: '🐉', Custom: '👑',
};

function useCooldownTimer(cooldownEndsAt?: string) {
  const [remaining, setRemaining] = useState('');
  useEffect(() => {
    if (!cooldownEndsAt) return;
    const tick = () => {
      const diff = new Date(cooldownEndsAt).getTime() - Date.now();
      if (diff <= 0) { setRemaining(''); return; }
      const h = Math.floor(diff / 3600000);
      const m = Math.floor((diff % 3600000) / 60000);
      const s = Math.floor((diff % 60000) / 1000);
      setRemaining(`${h}h ${m}m ${s}s`);
    };
    tick();
    const id = setInterval(tick, 1000);
    return () => clearInterval(id);
  }, [cooldownEndsAt]);
  return remaining;
}

export default function GymDetail() {
  const { gymId } = useParams<{ gymId: string }>();
  const navigate = useNavigate();
  const [gym, setGym] = useState<GymLeaderDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // History state
  const [historyList, setHistoryList] = useState<BattleHistoryEntryDto[]>([]);
  const [historyLoading, setHistoryLoading] = useState(false);
  const [historyPage, setHistoryPage] = useState(1);
  const [historyTotalCount, setHistoryTotalCount] = useState(0);
  const historyPageSize = 5;

  // Detail Modal state
  const [selectedSessionId, setSelectedSessionId] = useState<number | null>(null);
  const [detailData, setDetailData] = useState<BattleHistoryDetailDto | null>(null);
  const [detailLoading, setDetailLoading] = useState(false);

  const cooldownRemaining = useCooldownTimer(gym?.cooldownEndsAt);
  const color = gym ? (THEME_COLORS[gym.theme] ?? '#a8a8a8') : '#a8a8a8';

  const load = useCallback(async () => {
    if (!gymId) return;
    try {
      const data = await fetchGymDetail(Number(gymId));
      setGym(data);
    } catch {
      setError('Không thể tải thông tin Gym.');
    } finally {
      setLoading(false);
    }
  }, [gymId]);

  const loadHistory = useCallback(async () => {
    if (!gymId) return;
    setHistoryLoading(true);
    try {
      const data = await getBattleHistory('GymBattle', historyPage, historyPageSize, Number(gymId));
      setHistoryList(data.items);
      setHistoryTotalCount(data.totalCount);
    } catch (err) {
      console.error('Failed to load gym battle history', err);
    } finally {
      setHistoryLoading(false);
    }
  }, [gymId, historyPage]);

  useEffect(() => { load(); }, [load]);
  useEffect(() => { loadHistory(); }, [loadHistory]);

  const handleStart = () => {
    if (!gym) return;
    navigate(`/gym/${gym.id}/pets`);
  };

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


  if (loading) return <DetailSkeleton />;
  if (!gym) return (
    <div className="min-h-screen flex items-center justify-center" style={{ background: 'rgb(2,6,23)' }}>
      <p className="text-red-400 font-pixel text-sm">Gym not found.</p>
    </div>
  );

  const xpPct = Math.min(100, Math.round((gym.currentXp / gym.xpThreshold) * 100));
  const vocabPct = Math.min(100, Math.round((gym.currentVocabCount / gym.vocabThreshold) * 100));
  const canBattle = gym.status === GymStatus.Unlocked && !gym.isOnCooldown;

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
            <span className="font-press text-[10px]" style={{ color }}>ROUND {round.roundIndex + 1}</span>
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

          <div className="bg-purple-950/15 border border-purple-900/30 p-2.5 rounded-lg text-xs space-y-1" style={{ borderColor: `${color}22`, background: `${color}08` }}>
            <div className="text-purple-300 flex items-center gap-1.5" style={{ color }}>
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
    <div className="min-h-screen text-white" style={{ background: 'rgb(2,6,23)' }}>
      {/* Banner */}
      <div className="relative overflow-hidden py-16 px-6 text-center"
        style={{ background: `linear-gradient(135deg, rgb(2,6,23) 0%, ${color}22 50%, rgb(2,6,23) 100%)` }}>
        <button onClick={() => navigate('/gym')}
          className="absolute top-4 left-4 text-gray-400 hover:text-white font-pixel text-xs flex items-center gap-1">
          ← Back
        </button>

        {/* Avatar */}
        <div className="mx-auto mb-4 w-28 h-28 rounded-full flex items-center justify-center text-6xl"
          style={{ border: `3px solid ${color}`, boxShadow: `0 0 30px ${color}66`, background: `${color}22` }}>
          {gym.avatarUrl
            ? <img src={gym.avatarUrl} alt={gym.name} className="w-full h-full rounded-full object-cover pixel-art" />
            : THEME_EMOJI[gym.theme] ?? '🏅'}
        </div>

        <h1 className="font-press text-2xl mb-1" style={{ color, textShadow: `0 0 20px ${color}88` }}>
          {gym.name}
        </h1>
        <p className="text-gray-300 font-noto text-base mb-2">{gym.title}</p>
        <span className={`inline-block px-3 py-1 rounded-full text-xs font-pixel ${gym.status === GymStatus.Defeated ? 'bg-green-500/20 text-green-400 border border-green-500/40'
          : gym.status === GymStatus.Unlocked ? 'bg-yellow-500/20 text-yellow-400 border border-yellow-500/40'
            : 'bg-gray-700/50 text-gray-400 border border-gray-600/40'}`}>
          {gym.status === GymStatus.Defeated ? 'DEFEATED' : gym.status === GymStatus.Unlocked ? 'UNLOCKED' : 'LOCKED'}
        </span>
      </div>

      <div className="max-w-2xl mx-auto px-4 py-8 space-y-6">
        {/* Description */}
        <div className="rounded-xl p-5 border"
          style={{ background: `${color}0d`, borderColor: `${color}33` }}>
          <p className="text-gray-300 font-noto text-sm leading-relaxed">{gym.description}</p>
        </div>

        {/* Stats row */}
        <div className="grid grid-cols-3 gap-3">
          {[
            { label: 'Theme', value: gym.theme },
            { label: 'CEFR', value: gym.requiredCefrLevel },
            { label: 'Questions', value: gym.questionCount.toString() },
          ].map(({ label, value }) => (
            <div key={label} className="rounded-lg p-3 text-center border border-white/10"
              style={{ background: 'rgba(255,255,255,0.03)' }}>
              <p className="text-gray-500 font-noto text-xs mb-1">{label}</p>
              <p className="font-pixel text-xs" style={{ color }}>{value}</p>
            </div>
          ))}
        </div>

        {/* Unlock conditions */}
        <div className="rounded-xl p-5 border border-white/10" style={{ background: 'rgba(255,255,255,0.03)' }}>
          <h2 className="font-press text-xs mb-4" style={{ color }}>UNLOCK CONDITIONS</h2>
          <div className="space-y-4">
            <Condition
              label={`XP — ${gym.currentXp.toLocaleString()} / ${gym.xpThreshold.toLocaleString()}`}
              pct={xpPct} color={color} met={gym.currentXp >= gym.xpThreshold} />
            <Condition
              label={`${gym.requiredCefrLevel} Words (≥${gym.requiredMemoryState}) — ${gym.currentVocabCount} / ${gym.vocabThreshold}`}
              pct={vocabPct} color={color} met={gym.currentVocabCount >= gym.vocabThreshold} />
            {gym.gymOrder > 1 && (
              <div className="flex items-center gap-2 text-xs font-noto text-gray-400">
                <span className={gym.status !== GymStatus.Locked ? 'text-green-400' : 'text-red-400'}>
                  {gym.status !== GymStatus.Locked ? '✓' : '✗'}
                </span>
                Previous Gym defeated
              </div>
            )}
          </div>
        </div>

        {/* Battle record */}
        {gym.totalAttempts > 0 && (
          <div className="grid grid-cols-2 gap-3">
            <Stat label="Attempts" value={gym.totalAttempts.toString()} color={color} />
            <Stat label="Best Score" value={`${gym.bestScore}%`} color={color} />
            {gym.defeatedAt && (
              <Stat label="Defeated On" value={new Date(gym.defeatedAt).toLocaleDateString()} color={color} />
            )}
            <Stat label="Pass Rate" value={`${gym.passRatePercent}%`} color={color} />
          </div>
        )}

        {/* Reward preview */}
        <div className="rounded-xl p-4 border border-yellow-500/20 bg-yellow-500/5">
          <p className="font-pixel text-xs text-yellow-400 mb-2">🏆 VICTORY REWARD</p>
          <div className="flex items-center gap-4 text-sm font-noto text-gray-300">
            <span>+{gym.xpReward} XP</span>
            <span className="flex items-center gap-2">
              •
              {gym.badgeImageUrl && <img src={gym.badgeImageUrl} alt={gym.badgeName} className="w-6 h-6 object-contain pixel-art" />}
              {gym.badgeName}
            </span>
          </div>
        </div>

        {/* Cooldown warning */}
        {gym.isOnCooldown && cooldownRemaining && (
          <div className="rounded-xl p-4 border border-red-500/30 bg-red-500/10 text-center">
            <p className="font-pixel text-red-400 text-xs">⏳ COOLDOWN</p>
            <p className="font-press text-xl text-red-300 mt-1">{cooldownRemaining}</p>
            <p className="text-gray-400 text-xs font-noto mt-1">Come back after the cooldown to challenge again</p>
          </div>
        )}

        {/* Lịch sử khiêu chiến Gym */}
        <div className="rounded-xl p-5 border border-white/10" style={{ background: 'rgba(255,255,255,0.02)' }}>
          <h2 className="font-press text-xs mb-4" style={{ color }}>LỊCH SỬ KHIÊU CHIẾN</h2>

          {historyLoading && (
            <div className="space-y-2 py-4">
              {[1, 2].map(n => (
                <div key={n} className="h-16 bg-gray-800/10 border border-gray-800 rounded-xl animate-pulse" />
              ))}
            </div>
          )}

          {!historyLoading && historyList.length === 0 && (
            <div className="text-center py-6 bg-gray-900/10 border border-white/5 rounded-xl p-4">
              <p className="font-pixel text-[9px] text-gray-500">Bạn chưa khiêu chiến Gym này lần nào.</p>
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
                    resultColor = 'text-gray-400 bg-gray-900/30 border-gray-850';
                  } else if (!isWin) {
                    resultText = 'THUA';
                    resultColor = 'text-red-400 bg-red-950/20 border-red-500/20';
                  }

                  return (
                    <div key={match.sessionId} className="bg-slate-950/40 border border-white/5 hover:border-white/10 rounded-xl p-3 flex flex-col sm:flex-row sm:items-center justify-between gap-2 font-noto transition-all">
                      <div className="space-y-1 text-left">
                        <div className="flex items-center gap-1.5">
                          <span className={`text-[8px] font-pixel px-1 py-0.2 rounded border ${resultColor}`}>
                            {resultText}
                          </span>
                          <span className="font-pixel text-[8px]" style={{ color }}>
                            #{gym.gymOrder} {gym.name.toUpperCase()}
                          </span>
                        </div>

                        <div className="text-xs font-semibold">
                          {match.challengerUsername}
                          <span className="text-gray-500 font-normal mx-1">vs</span>
                          <span className="text-gray-300">{match.opponentName}</span>
                        </div>

                        <div className="text-[10px] text-gray-400">
                          Round: <span className="font-mono text-white">{match.challengerCorrect}-{match.opponentCorrect}</span>
                          <span className="text-gray-600 mx-1.5">|</span>
                          Điểm: <span className="font-mono text-white">{match.challengerTotalScore}-{match.opponentTotalScore}</span>
                          <span className="text-gray-600 mx-1.5">|</span>
                          <span className="text-[9px] text-gray-500 font-pixel">{new Date(match.startedAt).toLocaleString('vi-VN')}</span>
                        </div>
                      </div>

                      <button
                        onClick={() => handleOpenDetail(match.sessionId)}
                        className="bg-slate-800 hover:bg-slate-700 border border-gray-700 text-gray-300 font-pixel text-[8px] py-1.5 px-2.5 rounded-lg transition-all self-end sm:self-center">
                        XEM CHI TIẾT
                      </button>
                    </div>
                  );
                })}
              </div>

              {/* Pagination Controls */}
              {historyTotalCount > historyPageSize && (
                <div className="flex justify-between items-center bg-black/40 border border-white/5 rounded-xl p-2.5 font-pixel text-[8px]">
                  <button
                    onClick={() => setHistoryPage(prev => Math.max(1, prev - 1))}
                    disabled={historyPage === 1}
                    className={`px-2.5 py-1.5 rounded-lg transition-all border ${historyPage === 1 ? 'border-gray-800/30 text-gray-600 cursor-not-allowed' : 'border-white/10 text-gray-300 hover:bg-white/5'}`}
                    style={historyPage !== 1 ? { borderColor: `${color}33`, color } : undefined}>
                    ◀ PREV
                  </button>
                  <span className="text-gray-400">
                    PAGE {historyPage} / {Math.ceil(historyTotalCount / historyPageSize)}
                  </span>
                  <button
                    onClick={() => setHistoryPage(prev => Math.min(Math.ceil(historyTotalCount / historyPageSize), prev + 1))}
                    disabled={historyPage >= Math.ceil(historyTotalCount / historyPageSize)}
                    className={`px-2.5 py-1.5 rounded-lg transition-all border ${historyPage >= Math.ceil(historyTotalCount / historyPageSize) ? 'border-gray-800/30 text-gray-600 cursor-not-allowed' : 'border-white/10 text-gray-300 hover:bg-white/5'}`}
                    style={historyPage < Math.ceil(historyTotalCount / historyPageSize) ? { borderColor: `${color}33`, color } : undefined}>
                    NEXT ▶
                  </button>
                </div>
              )}
            </div>
          )}
        </div>


        {/* Error */}
        {error && (
          <div className="rounded-lg p-3 border border-red-500/30 bg-red-500/10">
            <p className="text-red-400 font-noto text-sm text-center">{error}</p>
          </div>
        )}

        {/* Action button */}
        {gym.status === GymStatus.Locked && (
          <button disabled className="w-full py-4 rounded-xl font-pixel text-sm text-gray-500 bg-gray-800 cursor-not-allowed border border-gray-700">
            🔒 LOCKED — Meet the conditions first
          </button>
        )}
        {gym.status === GymStatus.Defeated && (
          <button onClick={handleStart}
            className="w-full py-4 rounded-xl font-pixel text-sm text-white bg-green-700 hover:bg-green-600 transition-colors border border-green-500/30">
            🔁 REMATCH
          </button>
        )}
        {canBattle && (
          <button onClick={handleStart}
            className="w-full py-4 rounded-xl font-pixel text-sm transition-all hover:scale-105 hover:shadow-lg custom-cursor"
            style={{ background: color, color: '#000', boxShadow: `0 0 20px ${color}66` }}>
            ⚔️ CHALLENGE {gym.name.toUpperCase()}!
          </button>
        )}
        {gym.status === GymStatus.Unlocked && gym.isOnCooldown && (
          <button disabled className="w-full py-4 rounded-xl font-pixel text-sm text-gray-500 bg-gray-800 cursor-not-allowed border border-gray-700">
            ⏳ On Cooldown
          </button>
        )}
      </div>

      {/* DETAIL REPLAY MODAL */}
      {selectedSessionId !== null && (
        <div className="fixed inset-0 bg-black/85 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-slate-900 border rounded-3xl p-6 max-w-3xl w-full max-h-[90vh] overflow-y-auto relative text-white flex flex-col"
            style={{ borderColor: `${color}55`, boxShadow: `0 0 25px ${color}33` }}>
            
            <button
              onClick={handleCloseDetail}
              className="absolute top-4 right-4 text-gray-400 hover:text-white text-xl font-press p-1 hover:bg-white/5 rounded-lg">
              ✕
            </button>

            {detailLoading && (
              <div className="py-20 text-center flex flex-col items-center justify-center gap-4">
                <div className="w-10 h-10 border-4 border-t-transparent rounded-full animate-spin" style={{ borderColor: color, borderTopColor: 'transparent' }} />
                <p className="font-press text-[10px]" style={{ color }}>ĐANG TẢI CHI TIẾT TRẬN ĐẤU...</p>
              </div>
            )}

            {!detailLoading && detailData && (
              <div className="space-y-6">
                <div className="text-center border-b border-gray-800 pb-4">
                  <span className="font-pixel text-xs uppercase tracking-widest block mb-1" style={{ color }}>
                    PVE GYM BATTLE REPLAY
                  </span>
                  <h3 className="font-press text-lg text-white mb-2">CHI TIẾT TRẬN ĐẤU</h3>
                  
                  <div className="my-3 flex justify-center">
                    {detailData.status === 'Abandoned' ? (
                      <span className="px-6 py-1.5 rounded-xl font-press text-sm text-gray-400 bg-gray-850 border border-gray-700 tracking-widest shadow-[0_0_15px_rgba(156,163,175,0.1)]">
                        BỎ CUỘC (ABANDONED)
                      </span>
                    ) : detailData.currentUserWon ? (
                      <span className="px-6 py-1.5 rounded-xl font-press text-sm text-green-400 bg-green-950/20 border border-green-500/40 tracking-widest shadow-[0_0_20px_rgba(74,222,128,0.3)] animate-pulse">
                        CHIẾN THẮNG (VICTORY) 🎉
                      </span>
                    ) : (
                      <span className="px-6 py-1.5 rounded-xl font-press text-sm text-red-400 bg-red-950/20 border border-red-500/40 tracking-widest shadow-[0_0_20px_rgba(248,113,113,0.3)]">
                        THẤT BẠI (DEFEAT) 💀
                      </span>
                    )}
                  </div>
                  
                  <p className="text-xs text-gray-500 font-pixel mt-1 text-center">
                    Bắt đầu: {new Date(detailData.startedAt).toLocaleString('vi-VN')} 
                    {detailData.completedAt && ` · Thời gian: ${Math.round((new Date(detailData.completedAt).getTime() - new Date(detailData.startedAt).getTime()) / 1000)} giây`}
                  </p>
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* Player 1 */}
                  <div className="bg-slate-950/40 border border-gray-800 rounded-2xl p-4 flex flex-col items-center text-center">
                    <div className="w-10 h-10 bg-purple-900/30 border border-purple-500/20 rounded-full flex items-center justify-center text-lg font-pixel mb-1.5 text-purple-300"
                      style={{ borderColor: `${color}33`, color }}>👤</div>
                    <span className="font-bold text-sm text-white truncate max-w-full">{detailData.p1.name}</span>
                    <span className="text-[10px] text-gray-500 font-pixel mt-0.5">PLAYER</span>
                    
                    <div className="mt-3 grid grid-cols-2 gap-2.5 w-full text-center">
                      <div className="bg-black/30 rounded-lg py-1 px-1 border border-gray-950">
                        <div className="text-[8px] text-gray-500 font-pixel">CÂU ĐÚNG</div>
                        <div className="font-press text-xs text-green-400">{detailData.p1.correctCount} / {detailData.rounds.length}</div>
                      </div>
                      <div className="bg-black/30 rounded-lg py-1 px-1 border border-gray-950">
                        <div className="text-[8px] text-gray-500 font-pixel">TỔNG ĐIỂM</div>
                        <div className="font-press text-xs text-yellow-400">{detailData.p1.totalScore}</div>
                      </div>
                    </div>

                    <div className="mt-4 w-full">
                      <div className="text-[8px] text-gray-500 font-pixel mb-1.5 text-left uppercase">Đội hình Pet:</div>
                      <div className="grid grid-cols-3 gap-1">
                        {detailData.p1.selectedPets.map((pet, idx) => (
                          <div key={idx} className={`relative p-1 bg-black/40 border border-gray-850 rounded-lg text-center ${pet.isFainted ? 'opacity-40 grayscale' : ''}`}>
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

                  {/* Player 2 (Bot Gym) */}
                  <div className="bg-slate-950/40 border border-gray-800 rounded-2xl p-4 flex flex-col items-center text-center">
                    <div className="w-10 h-10 rounded-full flex items-center justify-center text-lg font-pixel mb-1.5"
                      style={{ background: `${color}22`, border: `2px solid ${color}66`, color }}>🤖</div>
                    <span className="font-bold text-sm text-white truncate max-w-full">{detailData.p2.name}</span>
                    <span className="text-[10px] text-gray-500 font-pixel mt-0.5">GYM LEADER</span>
                    
                    <div className="mt-3 grid grid-cols-2 gap-2.5 w-full text-center">
                      <div className="bg-black/30 rounded-lg py-1 px-1 border border-gray-950">
                        <div className="text-[8px] text-gray-500 font-pixel">CÂU ĐÚNG</div>
                        <div className="font-press text-xs text-green-400">{detailData.p2.correctCount} / {detailData.rounds.length}</div>
                      </div>
                      <div className="bg-black/30 rounded-lg py-1 px-1 border border-gray-950">
                        <div className="text-[8px] text-gray-500 font-pixel">TỔNG ĐIỂM</div>
                        <div className="font-press text-xs text-yellow-400">{detailData.p2.totalScore}</div>
                      </div>
                    </div>

                    <div className="mt-4 w-full">
                      <div className="text-[8px] text-gray-500 font-pixel mb-1.5 text-left uppercase">Đội hình Pet:</div>
                      <div className="grid grid-cols-3 gap-1">
                        {detailData.p2.selectedPets.map((pet, idx) => (
                          <div key={idx} className={`relative p-1 bg-black/40 border border-gray-850 rounded-lg text-center ${pet.isFainted ? 'opacity-40 grayscale' : ''}`}>
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
                  <div className="font-press text-xs border-b border-gray-800 pb-2 text-left" style={{ color }}>NHẬT KÝ ĐẤU (COMBAT LOGS)</div>
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

function Condition({ label, pct, color, met }: { label: string; pct: number; color: string; met: boolean }) {
  return (
    <div>
      <div className="flex justify-between text-xs font-noto text-gray-400 mb-1">
        <span className="flex items-center gap-1">
          <span className={met ? 'text-green-400' : 'text-gray-500'}>{met ? '✓' : '○'}</span>
          {label}
        </span>
        <span>{pct}%</span>
      </div>
      <div className="h-2 bg-gray-800 rounded-full overflow-hidden">
        <div className="h-full rounded-full transition-all duration-700"
          style={{ width: `${pct}%`, background: met ? '#48bb78' : color, boxShadow: `0 0 8px ${met ? '#48bb7888' : color + '88'}` }} />
      </div>
    </div>
  );
}

function Stat({ label, value, color }: { label: string; value: string; color: string }) {
  return (
    <div className="rounded-lg p-3 border border-white/10 text-center" style={{ background: 'rgba(255,255,255,0.03)' }}>
      <p className="text-gray-500 font-noto text-xs mb-1">{label}</p>
      <p className="font-pixel text-sm" style={{ color }}>{value}</p>
    </div>
  );
}

function DetailSkeleton() {
  return (
    <div className="min-h-screen" style={{ background: 'rgb(2,6,23)' }}>
      <div className="h-64 animate-pulse" style={{ background: 'rgba(255,255,255,0.05)' }} />
      <div className="max-w-2xl mx-auto px-4 py-8 space-y-4">
        {[...Array(4)].map((_, i) => (
          <div key={i} className="h-20 rounded-xl animate-pulse" style={{ background: 'rgba(255,255,255,0.04)' }} />
        ))}
      </div>
    </div>
  );
}
