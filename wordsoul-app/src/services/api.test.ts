import {
  AxiosError,
  AxiosHeaders,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';
import { afterEach, describe, expect, it, vi } from 'vitest';
import { AppError } from '../shared/errors';
import { subscribeToToasts, type ToastItem } from '../shared/toast/toast';
import api from './api';

const failingAdapter = (status: number, data: unknown) =>
  (config: InternalAxiosRequestConfig) => {
    const response: AxiosResponse = {
      data,
      status,
      statusText: '',
      headers: new AxiosHeaders(),
      config,
    };

    return Promise.reject(
      new AxiosError(
        'Request failed',
        AxiosError.ERR_BAD_RESPONSE,
        config,
        undefined,
        response,
      ),
    );
  };

describe('API response error policy', () => {
  afterEach(() => {
    vi.restoreAllMocks();
  });

  it('normalizes ProblemDetails responses into AppError', async () => {
    await expect(
      api.get('/test', {
        adapter: failingAdapter(404, {
          detail: 'Không tìm thấy dữ liệu.',
          code: 'RESOURCE_NOT_FOUND',
          traceId: 'trace-404',
        }),
      }),
    ).rejects.toMatchObject({
      name: 'AppError',
      status: 404,
      code: 'RESOURCE_NOT_FOUND',
      message: 'Không tìm thấy dữ liệu.',
      traceId: 'trace-404',
    });
  });

  it('publishes a deduplicated global toast for server errors', async () => {
    const listener = vi.fn<(item: ToastItem) => void>();
    const unsubscribe = subscribeToToasts(listener);

    await expect(
      api.get('/test', {
        adapter: failingAdapter(500, {
          detail: 'Máy chủ gặp sự cố.',
          code: 'INTERNAL_SERVER_ERROR',
          traceId: 'trace-500',
        }),
      }),
    ).rejects.toBeInstanceOf(AppError);

    expect(listener).toHaveBeenCalledWith(
      expect.objectContaining({
        id: 'api-error:INTERNAL_SERVER_ERROR',
        variant: 'error',
        message: 'Máy chủ gặp sự cố.',
        description: 'Mã đối soát: trace-500',
      }),
    );
    unsubscribe();
  });

  it('does not publish a toast when a request owns its inline error UI', async () => {
    const listener = vi.fn<(item: ToastItem) => void>();
    const unsubscribe = subscribeToToasts(listener);

    await expect(
      api.get('/test', {
        adapter: failingAdapter(500, {
          detail: 'Máy chủ gặp sự cố.',
          code: 'INTERNAL_SERVER_ERROR',
        }),
        errorHandling: { suppressToast: true },
      }),
    ).rejects.toBeInstanceOf(AppError);

    expect(listener).not.toHaveBeenCalled();
    unsubscribe();
  });
});
