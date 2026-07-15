import type { UserVocabularySetDto } from '../../../types/UserDto';
import type { VocabularySetProgressDto } from '../../../types/VocabularySetDto';

interface VocabSetHeroSectionProps {
  title: string;
  description: string | null;
  isPublic: boolean;
  isLoggedIn: boolean;
  // Primary actions
  userSetInfo: UserVocabularySetDto | null;
  isLoading: boolean;
  onLearnOrRegister: () => void;
  onNavigateToLogin: () => void;
  petsLoading: boolean;
  onShowPets: () => void;
  // Management (top-right)
  isOwner: boolean;
  unregistering: boolean;
  onUnregister: () => void;
  publishing: boolean;
  onPublish: () => void;
  onEdit: () => void;
  // Progress toggle
  progress: VocabularySetProgressDto | null;
  showProgress: boolean;
  onToggleProgress: () => void;
}

const SESSION_MILESTONES = [5, 10, 25, 50];

const VocabSetHeroSection: React.FC<VocabSetHeroSectionProps> = ({
  title, description, isPublic,
  isLoggedIn, userSetInfo, isLoading, onLearnOrRegister, onNavigateToLogin,
  petsLoading, onShowPets,
  isOwner, unregistering, onUnregister, publishing, onPublish, onEdit,
  progress, showProgress, onToggleProgress,
}) => {
  const n = progress?.totalCompletedSession ?? 0;
  const maxMilestone = SESSION_MILESTONES[SESSION_MILESTONES.length - 1];
  const barPct = Math.min((n / maxMilestone) * 100, 100);
  const nextMilestone = SESSION_MILESTONES.find(m => m > n);

  return (
    <div
      className="relative min-h-[15rem] flex flex-col items-center justify-center bg-fixed bg-cover bg-center top-[2.5rem]"
      style={{ backgroundImage: "url(https://res.cloudinary.com/dqpkxxzaf/image/upload/v1759257741/zl6vjhfc09aa1_leyauw.gif)", minHeight: '16rem' }}
    >
      {/* Dark overlay */}
      <div className="absolute inset-0 bg-black/55" />

      {/* Top-right management buttons */}
      <div className="absolute top-6 right-3 flex items-center gap-2 z-20">
        {!isPublic && (
          <span className="text-xs text-gray-300 px-2 py-0.5 rounded-full border border-gray-500 bg-black/40">🔒 Private</span>
        )}
        {/* Progress toggle */}
        {isLoggedIn && userSetInfo && progress && (
          <button
            title={showProgress ? 'Ẩn tiến trình' : 'Tiến trình'}
            onClick={onToggleProgress}
            className="w-8 h-8 flex items-center justify-center rounded-full bg-indigo-700/80 hover:bg-indigo-600 text-white shadow transition custom-cursor"
          >
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z" />
            </svg>
          </button>
        )}
        {/* Unregister */}
        {isLoggedIn && userSetInfo && !isOwner && (
          <button
            title={unregistering ? 'Đang hủy...' : 'Hủy đăng ký'}
            disabled={unregistering}
            onClick={onUnregister}
            className="w-8 h-8 flex items-center justify-center rounded-full bg-red-700/80 hover:bg-red-600 text-white shadow transition disabled:opacity-50 custom-cursor"
          >
            {unregistering
              ? <svg className="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
              : <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" /></svg>
            }
          </button>
        )}
        {/* Edit */}
        {isOwner && (
          <button
            title="Chỉnh sửa"
            onClick={onEdit}
            className="w-8 h-8 flex items-center justify-center rounded-full bg-gray-600/80 hover:bg-gray-500 text-white shadow transition custom-cursor"
          >
            <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.232 5.232l3.536 3.536M9 11l-6 6v3h3l6-6-3-3zm6-6l3 3" />
            </svg>
          </button>
        )}
        {/* Publish */}
        {isOwner && !isPublic && (
          <button
            title={publishing ? 'Đang publish...' : 'Publish'}
            disabled={publishing}
            onClick={onPublish}
            className="w-8 h-8 flex items-center justify-center rounded-full bg-green-700/80 hover:bg-green-600 text-white shadow transition disabled:opacity-50 custom-cursor"
          >
            {publishing
              ? <svg className="w-3.5 h-3.5 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
              : <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M3.055 11H5a2 2 0 012 2v1a2 2 0 002 2 2 2 0 012 2v2.945M8 3.935V5.5A2.5 2.5 0 0010.5 8h.5a2 2 0 012 2 2 2 0 104 0 2 2 0 012-2h1.064M15 20.488V18a2 2 0 012-2h3.064" /></svg>
            }
          </button>
        )}
      </div>

      {/* Center content: title + description + primary buttons */}
      <div className="relative z-10 flex flex-col items-center text-center px-6 py-8 gap-3">
        <h1 className="text-2xl font-pixel text-white drop-shadow">{title}</h1>
        {description && (
          <p className="text-sm text-gray-200 font-extralight max-w-md">{description}</p>
        )}
        {/* Primary action buttons */}
        <div className="flex items-center gap-3 mt-2 flex-wrap justify-center font-pixel">
          {/* Learn / Register */}
          {isLoggedIn ? (
            <button
              disabled={isLoading || (userSetInfo?.isCompleted ?? false)}
              onClick={onLearnOrRegister}
              className="flex items-center gap-2 px-6 py-2 rounded-full bg-yellow-400 hover:bg-yellow-300 text-black text-sm shadow-lg transition disabled:opacity-60 custom-cursor"
            >
              {isLoading
                ? <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
                : <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832l3-2a1 1 0 000-1.664l-3-2z" clipRule="evenodd" /></svg>
              }
              {userSetInfo
                ? (userSetInfo.isCompleted ? 'Đã hoàn thành' : 'Học')
                : 'Đăng ký & Học'}
            </button>
          ) : (
            <button
              onClick={onNavigateToLogin}
              className="flex items-center gap-2 px-6 py-2 rounded-full bg-yellow-400 hover:bg-yellow-300 text-black text-sm shadow-lg transition custom-cursor"
            >
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20"><path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM9.555 7.168A1 1 0 008 8v4a1 1 0 001.555.832l3-2a1 1 0 000-1.664l-3-2z" clipRule="evenodd" /></svg>
              Đăng nhập để học
            </button>
          )}
          {/* Vocamon list */}
          {isLoggedIn && (
            <button
              disabled={petsLoading}
              onClick={onShowPets}
              className="flex items-center gap-2 px-4 py-2 rounded-full bg-blue-500 hover:bg-blue-400 text-white text-sm shadow-lg transition disabled:opacity-60 custom-cursor"
            >
              {petsLoading
                ? <svg className="w-4 h-4 animate-spin" fill="none" viewBox="0 0 24 24"><circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" /><path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" /></svg>
                : <span></span>
              }
              Danh sách Vocamon
            </button>
          )}
        </div>
      </div>

      {/* Bottom: session progress bar */}
      {isLoggedIn && progress && (
        <div className="absolute bottom-0 left-100 right-100 z-20 px-4 py-2 bg-black/50 backdrop-blur-sm">
          <div className="flex items-center gap-3">
            <span className="text-[11px] text-gray-300 flex-shrink-0 whitespace-nowrap">
              {n} phiên
              {nextMilestone && <span className="text-yellow-300"> / {nextMilestone}</span>}
            </span>
            <div className="relative flex-1 h-1.5 bg-gray-700 rounded-full overflow-visible">
              <div
                className="h-1.5 rounded-full bg-gradient-to-r from-yellow-600 to-yellow-300 transition-all"
                style={{ width: `${barPct}%` }}
              />
              {SESSION_MILESTONES.map(m => {
                const pos = (m / maxMilestone) * 100;
                const done = n >= m;
                return (
                  <div
                    key={m}
                    title={`${m} phiên`}
                    className={`absolute top-1/2 -translate-y-1/2 w-2 h-2 rounded-full border ${done ? 'bg-yellow-400 border-yellow-200' : 'bg-gray-600 border-gray-500'}`}
                    style={{ left: `calc(${pos}% - 4px)` }}
                  />
                );
              })}
            </div>
            <span className="text-[10px] text-gray-500 flex-shrink-0">50</span>
          </div>
        </div>
      )}
    </div>
  );
};

export default VocabSetHeroSection;
