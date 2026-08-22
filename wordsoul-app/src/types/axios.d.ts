import 'axios';

declare module 'axios' {
  export interface AxiosRequestConfig {
    errorHandling?: {
      suppressToast?: boolean;
      toastMessage?: string;
    };
  }

  export interface InternalAxiosRequestConfig {
    errorHandling?: {
      suppressToast?: boolean;
      toastMessage?: string;
    };
    _retry?: boolean;
  }
}
