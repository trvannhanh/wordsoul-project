export function TrainerSprite({
    avatarUrl,
    flipped = false,
    hidden = false,
}: {
    avatarUrl?: string;
    flipped?: boolean;
    hidden?: boolean;
}) {
    return (
        <div
            className={`
        flex items-end justify-center
        transition-all duration-700
        ${hidden ? 'opacity-0 translate-y-8' : 'opacity-100 translate-y-0'}
        ${flipped ? 'scale-x-[-1]' : ''}
      `}
        >
            {avatarUrl ? (
                <img
                    src={avatarUrl}
                    alt="trainer"
                    className="w-16 h-20 sm:w-20 sm:h-24 object-contain"
                    style={{ imageRendering: 'pixelated' }}
                />
            ) : (
                <div className="w-16 h-20 sm:w-20 sm:h-24 flex items-end justify-center text-5xl pb-1">
                    🧑
                </div>
            )}
        </div>
    );
}
