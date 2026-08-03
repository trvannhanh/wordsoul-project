import i18n from '../i18n';

interface ApiErrorResponse {
  code?: string;
  message?: string;
  response?: {
    data?: {
      code?: string;
      message?: string;
    };
    status?: number;
  };
}

/**
 * Maps backend API errors to localized translation strings
 */
export function getLocalizedErrorMessage(error: ApiErrorResponse | unknown): string {
  if (!error) {
    return i18n.t('common:errors.general');
  }

  const errObj = error as ApiErrorResponse;
  const errorCode = errObj.response?.data?.code || errObj.code;
  const status = errObj.response?.status;

  if (errorCode) {
    const translationKey = `common:errors.${errorCode.toLowerCase()}` as any;
    if (i18n.exists(translationKey)) {
      return i18n.t(translationKey);
    }
  }

  if (status === 401) {
    return i18n.t('common:errors.unauthorized');
  }
  if (status === 403) {
    return i18n.t('common:errors.forbidden');
  }
  if (status === 404) {
    return i18n.t('common:errors.not_found');
  }

  const rawMessage = errObj.response?.data?.message || errObj.message;
  if (rawMessage && typeof rawMessage === 'string') {
    return rawMessage;
  }

  return i18n.t('common:errors.general');
}
