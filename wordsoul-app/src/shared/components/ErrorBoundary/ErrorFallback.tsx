interface ErrorFallbackProps {
  onRetry: () => void;
}

export const ErrorFallback = ({ onRetry }: ErrorFallbackProps) => (
  <main className="flex min-h-screen items-center justify-center bg-slate-950 px-4 text-slate-100">
    <section
      role="alert"
      className="w-full max-w-lg rounded-2xl border border-red-400/60 bg-slate-900 p-8 text-center shadow-2xl"
    >
      <p className="font-pixel text-sm text-red-300">SYSTEM ERROR</p>
      <h1 className="mt-4 text-2xl font-bold">Ứng dụng gặp sự cố</h1>
      <p className="mt-3 text-sm text-slate-300">
        Đã xảy ra lỗi ngoài dự kiến. Bạn có thể thử tải lại màn hình hoặc quay về
        trang chủ.
      </p>
      <div className="mt-6 flex flex-wrap justify-center gap-3">
        <button
          type="button"
          onClick={onRetry}
          className="rounded-lg bg-blue-600 px-4 py-2 font-semibold hover:bg-blue-500"
        >
          Thử lại
        </button>
        <a
          href="/home"
          className="rounded-lg border border-slate-500 px-4 py-2 font-semibold hover:bg-slate-800"
        >
          Về trang chủ
        </a>
        <button
          type="button"
          onClick={() => window.location.reload()}
          className="rounded-lg border border-slate-500 px-4 py-2 font-semibold hover:bg-slate-800"
        >
          Tải lại trang
        </button>
      </div>
    </section>
  </main>
);
