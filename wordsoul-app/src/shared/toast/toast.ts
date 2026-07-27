export type ToastVariant = 'success' | 'error' | 'warning' | 'info';

export interface ToastOptions {
  id?: string;
  duration?: number;
  description?: string;
}

export interface ToastItem extends ToastOptions {
  id: string;
  message: string;
  variant: ToastVariant;
}

type ToastListener = (toast: ToastItem) => void;

const listeners = new Set<ToastListener>();
const QUEUED_TOAST_KEY = 'wordsoul:queued-toast';

const publish = (
  variant: ToastVariant,
  message: string,
  options: ToastOptions = {},
) => {
  const item: ToastItem = {
    ...options,
    id: options.id ?? crypto.randomUUID(),
    message,
    variant,
  };

  listeners.forEach((listener) => listener(item));
  return item.id;
};

export const toast = {
  success: (message: string, options?: ToastOptions) =>
    publish('success', message, options),
  error: (message: string, options?: ToastOptions) =>
    publish('error', message, options),
  warning: (message: string, options?: ToastOptions) =>
    publish('warning', message, options),
  info: (message: string, options?: ToastOptions) =>
    publish('info', message, options),
};

export const queueToastAfterNavigation = (
  variant: ToastVariant,
  message: string,
  options: ToastOptions = {},
) => {
  const item: ToastItem = {
    ...options,
    id: options.id ?? crypto.randomUUID(),
    message,
    variant,
  };

  sessionStorage.setItem(QUEUED_TOAST_KEY, JSON.stringify(item));
};

export const consumeQueuedToast = (): ToastItem | undefined => {
  const serialized = sessionStorage.getItem(QUEUED_TOAST_KEY);
  if (!serialized) {
    return undefined;
  }

  sessionStorage.removeItem(QUEUED_TOAST_KEY);

  try {
    return JSON.parse(serialized) as ToastItem;
  } catch {
    return undefined;
  }
};

export const subscribeToToasts = (listener: ToastListener) => {
  listeners.add(listener);
  return () => {
    listeners.delete(listener);
  };
};
