import { useState, useRef, useCallback } from 'react';

export type RecorderState = 'idle' | 'requesting' | 'recording' | 'processing' | 'done' | 'error';

interface UseAudioRecorderReturn {
  state: RecorderState;
  audioBlob: Blob | null;
  durationMs: number;
  error: string | null;
  startRecording: () => Promise<void>;
  stopRecording: () => void;
  reset: () => void;
}

/**
 * Custom hook bọc MediaRecorder API.
 * Thu âm WebM/Opus (Chrome) hoặc WAV (Safari), trả về Blob để gửi lên backend.
 */
export function useAudioRecorder(maxDurationMs = 10000): UseAudioRecorderReturn {
  const [state, setState] = useState<RecorderState>('idle');
  const [audioBlob, setAudioBlob] = useState<Blob | null>(null);
  const [durationMs, setDurationMs] = useState(0);
  const [error, setError] = useState<string | null>(null);

  const mediaRecorderRef = useRef<MediaRecorder | null>(null);
  const chunksRef = useRef<Blob[]>([]);
  const streamRef = useRef<MediaStream | null>(null);
  const startTimeRef = useRef<number>(0);
  const autoStopTimerRef = useRef<ReturnType<typeof setTimeout> | null>(null);

  const stopRecording = useCallback(() => {
    if (autoStopTimerRef.current) {
      clearTimeout(autoStopTimerRef.current);
      autoStopTimerRef.current = null;
    }
    if (
      mediaRecorderRef.current &&
      mediaRecorderRef.current.state !== 'inactive'
    ) {
      mediaRecorderRef.current.stop();
    }
    // Stream tracks sẽ được dừng trong onstop
  }, []);

  const startRecording = useCallback(async () => {
    setError(null);
    setAudioBlob(null);
    setDurationMs(0);
    chunksRef.current = [];
    setState('requesting');

    try {
      const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
      streamRef.current = stream;

      // Ưu tiên định dạng theo thứ tự hỗ trợ trình duyệt
      const mimeType = [
        'audio/webm;codecs=opus',
        'audio/webm',
        'audio/ogg;codecs=opus',
        'audio/wav',
      ].find((t) => MediaRecorder.isTypeSupported(t)) ?? '';

      const recorder = new MediaRecorder(stream, mimeType ? { mimeType } : {});
      mediaRecorderRef.current = recorder;

      recorder.ondataavailable = (e) => {
        if (e.data.size > 0) chunksRef.current.push(e.data);
      };

      recorder.onstop = () => {
        const elapsed = Date.now() - startTimeRef.current;
        setDurationMs(elapsed);

        const blob = new Blob(chunksRef.current, {
          type: mimeType || 'audio/webm',
        });
        setAudioBlob(blob);
        setState('done');

        // Giải phóng microphone
        streamRef.current?.getTracks().forEach((t) => t.stop());
        streamRef.current = null;
      };

      recorder.onerror = () => {
        setError('Lỗi khi thu âm, vui lòng thử lại.');
        setState('error');
        stream.getTracks().forEach((t) => t.stop());
      };

      recorder.start(100); // Lấy chunk mỗi 100ms
      startTimeRef.current = Date.now();
      setState('recording');

      // Tự dừng sau maxDurationMs
      autoStopTimerRef.current = setTimeout(() => {
        stopRecording();
      }, maxDurationMs);
    } catch (err: unknown) {
      const msg =
        err instanceof DOMException && err.name === 'NotAllowedError'
          ? 'Cần quyền truy cập microphone. Vui lòng cho phép và thử lại.'
          : 'Không thể mở microphone. Vui lòng kiểm tra thiết bị.';
      setError(msg);
      setState('error');
    }
  }, [maxDurationMs, stopRecording]);

  const reset = useCallback(() => {
    stopRecording();
    setAudioBlob(null);
    setDurationMs(0);
    setError(null);
    setState('idle');
    chunksRef.current = [];
  }, [stopRecording]);

  return { state, audioBlob, durationMs, error, startRecording, stopRecording, reset };
}
