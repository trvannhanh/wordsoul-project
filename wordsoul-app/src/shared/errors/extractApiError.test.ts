import {
  AxiosError,
  AxiosHeaders,
  CanceledError,
  type AxiosResponse,
  type InternalAxiosRequestConfig,
} from 'axios';
import { describe, expect, it } from 'vitest';
import { extractApiError } from './extractApiError';
import { AppError } from './types';

const axiosErrorWithResponse = (
  status: number,
  data: unknown,
  headers: Record<string, string> = {},
) => {
  const config = {
    headers: new AxiosHeaders(),
  } as InternalAxiosRequestConfig;
  const response: AxiosResponse = {
    data,
    status,
    statusText: '',
    headers: new AxiosHeaders(headers),
    config,
  };

  return new AxiosError(
    'Request failed',
    AxiosError.ERR_BAD_RESPONSE,
    config,
    undefined,
    response,
  );
};

describe('extractApiError', () => {
  it('extracts the canonical ProblemDetails contract', () => {
    const result = extractApiError(
      axiosErrorWithResponse(404, {
        type: 'https://wordsoul.app/errors/not-found',
        title: 'Resource Not Found',
        status: 404,
        detail: 'Không tìm thấy thú cưng.',
        code: 'PET_NOT_FOUND',
        traceId: 'trace-123',
      }),
    );

    expect(result).toMatchObject({
      kind: 'api',
      status: 404,
      code: 'PET_NOT_FOUND',
      message: 'Không tìm thấy thú cưng.',
      traceId: 'trace-123',
      isRetryable: false,
    });
  });

  it('keeps validation errors for inline form display', () => {
    const result = extractApiError(
      axiosErrorWithResponse(422, {
        detail: 'Dữ liệu chưa hợp lệ.',
        code: 'VALIDATION_ERROR',
        errors: {
          username: ['Tên đăng nhập là bắt buộc.'],
        },
      }),
    );

    expect(result.fieldErrors).toEqual({
      username: ['Tên đăng nhập là bắt buộc.'],
    });
  });

  it('supports a legacy message during migration', () => {
    const result = extractApiError(
      axiosErrorWithResponse(409, {
        message: 'Tên đăng nhập đã tồn tại.',
      }),
    );

    expect(result.message).toBe('Tên đăng nhập đã tồn tại.');
    expect(result.code).toBe('CONFLICT');
  });

  it('extracts retry-after from response headers', () => {
    const result = extractApiError(
      axiosErrorWithResponse(429, {}, { 'retry-after': '30' }),
    );

    expect(result.retryAfterSeconds).toBe(30);
    expect(result.isRetryable).toBe(true);
  });

  it('classifies timeout, network, and cancelled requests', () => {
    const timeout = extractApiError(
      new AxiosError('timeout', AxiosError.ECONNABORTED),
    );
    const network = extractApiError(
      new AxiosError('network', AxiosError.ERR_NETWORK),
    );
    const cancelled = extractApiError(new CanceledError());

    expect(timeout).toMatchObject({ kind: 'timeout', isRetryable: true });
    expect(network).toMatchObject({ kind: 'network', isRetryable: true });
    expect(cancelled).toMatchObject({
      kind: 'cancelled',
      isRetryable: false,
    });
  });

  it('returns an existing AppError without wrapping it again', () => {
    const original = new AppError({
      kind: 'api',
      code: 'CUSTOM_ERROR',
      message: 'Custom error',
    });

    expect(extractApiError(original)).toBe(original);
  });

  it('uses a safe fallback for unknown values', () => {
    const result = extractApiError(null);

    expect(result).toMatchObject({
      kind: 'unknown',
      code: 'UNKNOWN_ERROR',
      message: 'Đã xảy ra lỗi. Vui lòng thử lại.',
    });
  });
});
