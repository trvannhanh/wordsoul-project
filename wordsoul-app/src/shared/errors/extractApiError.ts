import axios, { AxiosError } from 'axios';
import { AppError, type ProblemDetails } from './types';

const FALLBACK_MESSAGES: Record<number, string> = {
  400: 'Yêu cầu không hợp lệ.',
  401: 'Bạn cần đăng nhập để tiếp tục.',
  403: 'Bạn không có quyền thực hiện thao tác này.',
  404: 'Không tìm thấy dữ liệu được yêu cầu.',
  409: 'Dữ liệu đã thay đổi hoặc bị xung đột.',
  422: 'Dữ liệu nhập vào chưa hợp lệ.',
  429: 'Bạn thao tác quá nhanh. Vui lòng thử lại sau.',
  500: 'Hệ thống đang gặp sự cố. Vui lòng thử lại sau.',
};

const CODE_BY_STATUS: Record<number, string> = {
  400: 'BAD_REQUEST',
  401: 'UNAUTHORIZED',
  403: 'FORBIDDEN',
  404: 'RESOURCE_NOT_FOUND',
  409: 'CONFLICT',
  422: 'VALIDATION_ERROR',
  429: 'RATE_LIMIT_EXCEEDED',
  500: 'INTERNAL_SERVER_ERROR',
};

const asProblemDetails = (value: unknown): ProblemDetails | undefined => {
  if (!value || typeof value !== 'object' || Array.isArray(value)) {
    return undefined;
  }

  return value as ProblemDetails;
};

const readRetryAfter = (
  error: AxiosError,
  problem?: ProblemDetails,
): number | undefined => {
  if (typeof problem?.retryAfter === 'number') {
    return problem.retryAfter;
  }

  const header = error.response?.headers['retry-after'];
  if (typeof header === 'number') {
    return header;
  }

  if (typeof header !== 'string') {
    return undefined;
  }

  const seconds = Number(header);
  if (Number.isFinite(seconds)) {
    return seconds;
  }

  const retryAt = Date.parse(header);
  return Number.isNaN(retryAt)
    ? undefined
    : Math.max(0, Math.ceil((retryAt - Date.now()) / 1000));
};

const fallbackMessage = (status?: number) =>
  (status && FALLBACK_MESSAGES[status]) ||
  (status && status >= 500
    ? FALLBACK_MESSAGES[500]
    : 'Đã xảy ra lỗi. Vui lòng thử lại.');

export const extractApiError = (error: unknown): AppError => {
  if (error instanceof AppError) {
    return error;
  }

  if (axios.isCancel(error)) {
    return new AppError({
      kind: 'cancelled',
      code: 'REQUEST_CANCELLED',
      message: 'Yêu cầu đã bị huỷ.',
      cause: error,
    });
  }

  if (axios.isAxiosError(error)) {
    const axiosError = error as AxiosError;

    if (
      axiosError.code === AxiosError.ECONNABORTED ||
      axiosError.code === 'ETIMEDOUT'
    ) {
      return new AppError({
        kind: 'timeout',
        code: 'REQUEST_TIMEOUT',
        message: 'Yêu cầu mất quá nhiều thời gian. Vui lòng thử lại.',
        isRetryable: true,
        cause: error,
      });
    }

    if (!axiosError.response) {
      return new AppError({
        kind: 'network',
        code: 'NETWORK_ERROR',
        message: 'Không thể kết nối tới máy chủ. Vui lòng kiểm tra kết nối mạng.',
        isRetryable: true,
        cause: error,
      });
    }

    const status = axiosError.response.status;
    const problem = asProblemDetails(axiosError.response.data);
    const message =
      problem?.detail ||
      problem?.message ||
      problem?.Message ||
      fallbackMessage(status);

    return new AppError({
      kind: 'api',
      code:
        problem?.code ||
        CODE_BY_STATUS[status] ||
        (status >= 500 ? 'INTERNAL_SERVER_ERROR' : 'API_ERROR'),
      message,
      status,
      traceId: problem?.traceId,
      fieldErrors: problem?.errors,
      retryAfterSeconds: readRetryAfter(axiosError, problem),
      isRetryable: status === 408 || status === 429 || status >= 500,
      cause: error,
    });
  }

  if (error instanceof Error) {
    return new AppError({
      kind: 'unknown',
      code: 'UNKNOWN_ERROR',
      message: error.message || 'Đã xảy ra lỗi. Vui lòng thử lại.',
      cause: error,
    });
  }

  return new AppError({
    kind: 'unknown',
    code: 'UNKNOWN_ERROR',
    message: 'Đã xảy ra lỗi. Vui lòng thử lại.',
    cause: error,
  });
};
