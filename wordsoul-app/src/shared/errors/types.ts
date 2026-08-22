export type AppErrorKind =
  | 'api'
  | 'network'
  | 'timeout'
  | 'cancelled'
  | 'unknown';

export type ValidationErrors = Record<string, string[]>;

export interface ProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  instance?: string;
  code?: string;
  traceId?: string;
  errors?: ValidationErrors;
  retryAfter?: number;
  message?: string;
  Message?: string;
}

export interface AppErrorOptions {
  kind: AppErrorKind;
  code: string;
  message: string;
  status?: number;
  traceId?: string;
  fieldErrors?: ValidationErrors;
  retryAfterSeconds?: number;
  isRetryable?: boolean;
  cause?: unknown;
}

export class AppError extends Error {
  readonly kind: AppErrorKind;
  readonly code: string;
  readonly status?: number;
  readonly traceId?: string;
  readonly fieldErrors?: ValidationErrors;
  readonly retryAfterSeconds?: number;
  readonly isRetryable: boolean;
  readonly cause?: unknown;

  constructor(options: AppErrorOptions) {
    super(options.message);
    this.name = 'AppError';
    this.kind = options.kind;
    this.code = options.code;
    this.status = options.status;
    this.traceId = options.traceId;
    this.fieldErrors = options.fieldErrors;
    this.retryAfterSeconds = options.retryAfterSeconds;
    this.isRetryable = options.isRetryable ?? false;
    this.cause = options.cause;
  }
}
