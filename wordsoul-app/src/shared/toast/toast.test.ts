import { describe, expect, it, vi } from 'vitest';
import { subscribeToToasts, toast } from './toast';

describe('toast event store', () => {
  it('publishes a toast with a stable caller-provided id', () => {
    const listener = vi.fn();
    const unsubscribe = subscribeToToasts(listener);

    toast.success('Đã lưu thay đổi.', { id: 'profile-saved' });

    expect(listener).toHaveBeenCalledWith(
      expect.objectContaining({
        id: 'profile-saved',
        variant: 'success',
        message: 'Đã lưu thay đổi.',
      }),
    );
    unsubscribe();
  });
});
