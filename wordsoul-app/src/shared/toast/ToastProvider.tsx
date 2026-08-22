import {
  useCallback,
  useEffect,
  useMemo,
  useState,
  type PropsWithChildren,
} from 'react';
import {
  consumeQueuedToast,
  subscribeToToasts,
  toast,
  type ToastItem,
  type ToastVariant,
} from './toast';
import { ToastContext } from './ToastContext';

const MAX_VISIBLE_TOASTS = 4;
const DEFAULT_DURATION = 4500;

const variantStyles: Record<ToastVariant, string> = {
  success: 'border-emerald-400 bg-emerald-950 text-emerald-50',
  error: 'border-red-400 bg-red-950 text-red-50',
  warning: 'border-amber-400 bg-amber-950 text-amber-50',
  info: 'border-sky-400 bg-sky-950 text-sky-50',
};

const variantLabels: Record<ToastVariant, string> = {
  success: 'Thành công',
  error: 'Có lỗi xảy ra',
  warning: 'Cảnh báo',
  info: 'Thông tin',
};

export const ToastProvider = ({ children }: PropsWithChildren) => {
  const [items, setItems] = useState<ToastItem[]>([]);

  const dismiss = useCallback((id: string) => {
    setItems((current) => current.filter((item) => item.id !== id));
  }, []);

  useEffect(() => {
    const unsubscribe = subscribeToToasts((nextToast) => {
        setItems((current) => {
          const withoutDuplicate = current.filter(
            (item) =>
              item.id !== nextToast.id &&
              !(
                item.variant === nextToast.variant &&
                item.message === nextToast.message
              ),
          );
          return [...withoutDuplicate, nextToast].slice(-MAX_VISIBLE_TOASTS);
        });

        window.setTimeout(
          () => dismiss(nextToast.id),
          nextToast.duration ?? DEFAULT_DURATION,
        );
      });

    const queuedToast = consumeQueuedToast();
    if (queuedToast) {
      toast[queuedToast.variant](queuedToast.message, queuedToast);
    }

    return unsubscribe;
  }, [dismiss]);

  const value = useMemo(() => toast, []);

  return (
    <ToastContext.Provider value={value}>
      {children}
      <div
        className="pointer-events-none fixed right-4 top-4 z-[9999] flex w-[min(24rem,calc(100vw-2rem))] flex-col gap-3"
        aria-live="polite"
        aria-label="Thông báo ứng dụng"
      >
        {items.map((item) => (
          <div
            key={item.id}
            role={item.variant === 'error' ? 'alert' : 'status'}
            className={`pointer-events-auto rounded-lg border px-4 py-3 shadow-2xl ${variantStyles[item.variant]}`}
          >
            <div className="flex items-start gap-3">
              <div className="min-w-0 flex-1">
                <p className="text-sm font-semibold">
                  {variantLabels[item.variant]}
                </p>
                <p className="mt-1 break-words text-sm">{item.message}</p>
                {item.description && (
                  <p className="mt-1 break-all text-xs opacity-75">
                    {item.description}
                  </p>
                )}
              </div>
              <button
                type="button"
                onClick={() => dismiss(item.id)}
                className="rounded px-1 text-lg leading-none opacity-70 hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-white"
                aria-label="Đóng thông báo"
              >
                ×
              </button>
            </div>
          </div>
        ))}
      </div>
    </ToastContext.Provider>
  );
};
