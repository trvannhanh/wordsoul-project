/**
 * ============================================================
 *  WordSoul – Performance Test: SubmitAnswer (LearningSession)
 *  Công cụ: k6 (https://k6.io)
 *
 *  Kịch bản:
 *    - setup(): login + tạo session cho 100 accounts (tuần tự, 1 lần)
 *    - default(): 100 VU chỉ gọi SubmitAnswer liên tục trong 5 phút
 *    - Không tạo session mới trong quá trình test
 *
 *  Chạy:
 *    k6 run --insecure-skip-tls-verify -e BASE_URL=https://localhost:7272 wordsoul_submit_answer_test.js
 * ============================================================
 */

import http from "k6/http";
import { check, sleep } from "k6";
import { Rate, Trend, Counter } from "k6/metrics";

// ─────────────────────────────────────────────────────────────
//  Cấu hình môi trường
// ─────────────────────────────────────────────────────────────
const BASE_URL    = __ENV.BASE_URL    || "https://localhost:7272";
const VOCAB_SET_ID = parseInt(__ENV.VOCAB_SET_ID) || 11;
const PASSWORD    = "Test@123456";
const VU_COUNT    = 100;

// ─────────────────────────────────────────────────────────────
//  Custom Metrics
// ─────────────────────────────────────────────────────────────
const setupErrorRate  = new Rate("setup_error_rate");
const submitErrorRate = new Rate("submit_answer_error_rate");
const submitAnswerTime = new Trend("submit_answer_response_time", true);
const throughputCounter = new Counter("submit_answer_requests_total");

// ─────────────────────────────────────────────────────────────
//  Cấu hình tải – 100 VU × 5 phút
// ─────────────────────────────────────────────────────────────
export const options = {
  insecureSkipTLSVerify: true,

  scenarios: {
    submit_answer_load: {
      executor: "ramping-vus",
      startVUs: 0,
      stages: [
        { duration: "30s", target: 100 }, // Ramp-up
        { duration: "4m",  target: 100 }, // Steady state
        { duration: "30s", target: 0   }, // Ramp-down
      ],
      gracefulStop: "10s",
    },
  },

  thresholds: {
    submit_answer_response_time: ["p(95)<2000"],
    submit_answer_error_rate:    ["rate<0.01"],
    setup_error_rate:            ["rate<0.05"],
    http_req_failed:             ["rate<0.05"],
  },
};

// ─────────────────────────────────────────────────────────────
//  setup() – chạy 1 lần DUY NHẤT trước khi test bắt đầu
//  1. Login 100 accounts tuần tự → lấy token
//  2. Tạo LearningSession cho mỗi account → lấy sessionId + vocabularyIds
//  Trả về mảng vuData[100] để các VU dùng trong default()
// ─────────────────────────────────────────────────────────────
export function setup() {
  console.log(`[setup] Bắt đầu: login + tạo session cho ${VU_COUNT} accounts...`);

  const vuData = []; // vuData[i] = { token, sessionId, vocabularyIds }
  let loginOk = 0;
  let sessionOk = 0;

  for (let i = 0; i < VU_COUNT; i++) {
    const username = `testuser${i + 1}`;

    // ── 1. Login ──────────────────────────────────────────────
    const loginRes = http.post(
      `${BASE_URL}/api/auth/login`,
      JSON.stringify({ username, password: PASSWORD }),
      { headers: { "Content-Type": "application/json" }, timeout: "30s" }
    );

    if (loginRes.status !== 200) {
      console.warn(`[setup] Login FAIL ${username}: HTTP ${loginRes.status}`);
      setupErrorRate.add(1);
      vuData.push(null);
      sleep(0.1);
      continue;
    }

    let token;
    try { token = JSON.parse(loginRes.body).accessToken; } catch { token = null; }

    if (!token) {
      console.warn(`[setup] Không lấy được token cho ${username}`);
      setupErrorRate.add(1);
      vuData.push(null);
      sleep(0.1);
      continue;
    }
    loginOk++;
    setupErrorRate.add(0);

    // ── 2. Tạo LearningSession ────────────────────────────────
    const sessionRes = http.post(
      `${BASE_URL}/api/learning-sessions/${VOCAB_SET_ID}`,
      null,
      {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
        timeout: "30s",
      }
    );

    if (sessionRes.status !== 200) {
      console.warn(`[setup] CreateSession FAIL ${username}: HTTP ${sessionRes.status} – ${sessionRes.body?.substring(0, 80)}`);
      vuData.push(null);
      sleep(0.1);
      continue;
    }

    let sessionData;
    try { sessionData = JSON.parse(sessionRes.body); } catch { sessionData = null; }

    if (!sessionData?.id) {
      console.warn(`[setup] Session thiếu id cho ${username}: ${sessionRes.body?.substring(0, 80)}`);
      vuData.push(null);
      sleep(0.1);
      continue;
    }

    // ── Fallback: vocabularyIds rỗng → lấy từ GET /questions ──
    // Xảy ra khi API trả existing session nhưng không eager-load SessionVocabularies
    let vocabularyIds = sessionData.vocabularyIds ?? [];
    if (vocabularyIds.length === 0) {
      const qRes = http.get(
        `${BASE_URL}/api/learning-sessions/${sessionData.id}/questions`,
        {
          headers: { Authorization: `Bearer ${token}` },
          timeout: "15s",
        }
      );
      if (qRes.status === 200) {
        try {
          const questions = JSON.parse(qRes.body);
          vocabularyIds = questions.map((q) => q.vocabularyId).filter(Boolean);
        } catch { /* bỏ qua */ }
      }
      if (vocabularyIds.length === 0) {
        console.warn(`[setup] Không lấy được vocabularyIds cho ${username} (sessionId=${sessionData.id}). Bỏ qua VU này.`);
        vuData.push(null);
        sleep(0.1);
        continue;
      }
      console.log(`[setup] Fallback questions OK cho ${username}: ${vocabularyIds.length} từ`);
    }

    sessionOk++;
    vuData.push({
      username,
      token,
      sessionId:    sessionData.id,
      vocabularyIds: vocabularyIds, // đã được resolve từ session hoặc /questions
    });

    sleep(0.1); // nghỉ nhỏ giữa mỗi account
  }

  console.log(`[setup] Xong: ${loginOk} login OK, ${sessionOk} session OK / ${VU_COUNT} accounts.`);
  if (sessionOk === 0) {
    console.error("[setup] KHÔNG có session nào – kiểm tra server, DB và VOCAB_SET_ID!");
  }

  return { vuData };
}

// ─────────────────────────────────────────────────────────────
//  VU state
// ─────────────────────────────────────────────────────────────
let answerIdx = 0; // xoay vòng qua vocabularyIds

// ─────────────────────────────────────────────────────────────
//  Main VU Function – CHỈ gọi SubmitAnswer, không tạo session
//  data.vuData[tokenIdx] = { token, sessionId, vocabularyIds }
// ─────────────────────────────────────────────────────────────
export default function (data) {
  const tokenIdx = (__VU - 1) % VU_COUNT;
  const vu = data.vuData[tokenIdx];

  // VU không có session hợp lệ (login/session thất bại trong setup) → bỏ qua
  if (!vu) {
    setupErrorRate.add(1);
    sleep(2);
    return;
  }

  // Xoay vòng qua các từ vựng trong session (không bao giờ tạo session mới)
  const vocabId = vu.vocabularyIds[answerIdx % vu.vocabularyIds.length];
  answerIdx++;

  // QuestionType: 0=Flashcard (luôn pass), dùng để tập trung đo throughput
  const questionType = answerIdx % 4; // xoay 0,1,2,3 để test đủ loại

  const payload = JSON.stringify({
    vocabularyId:        vocabId,
    questionType:        questionType,
    answer:              questionType === 0 ? "seen" : "test_answer",
    responseTimeSeconds: Math.random() * 8 + 1,
    hintCount:           Math.random() < 0.2 ? 1 : 0,
  });

  const start = Date.now();
  const res = http.post(
    `${BASE_URL}/api/learning-sessions/${vu.sessionId}/answers`,
    payload,
    {
      headers: {
        "Content-Type": "application/json",
        Authorization:  `Bearer ${vu.token}`,
      },
      timeout: "15s",
      tags: { endpoint: "submit_answer" },
    }
  );
  const elapsed = Date.now() - start;

  submitAnswerTime.add(elapsed);
  throughputCounter.add(1);

  const ok = check(res, {
    "submit_answer: status 200":          (r) => r.status === 200,
    "submit_answer: has isCorrect field": (r) => {
      try { return "isCorrect" in JSON.parse(r.body); } catch { return false; }
    },
    "submit_answer: p95 < 2000ms":        ()  => elapsed < 2000,
  });

  submitErrorRate.add(!ok);

  if (!ok && res.status !== 200) {
    console.warn(
      `SubmitAnswer FAIL (session=${vu.sessionId}, vocab=${vocabId}): ` +
      `HTTP ${res.status} in ${elapsed}ms – ${res.body?.substring(0, 80)}`
    );
  }

  // Think time ~0.5–2s (simulate real user)
  sleep(Math.random() * 1.5 + 0.5);
}

// ─────────────────────────────────────────────────────────────
//  Summary
// ─────────────────────────────────────────────────────────────
export function handleSummary(data) {
  const submit    = data.metrics["submit_answer_response_time"];
  const errRate   = data.metrics["submit_answer_error_rate"];
  const totalReqs = data.metrics["submit_answer_requests_total"];
  const durationMs = data.state?.testRunDurationMs || 300000;

  const rps = totalReqs
    ? (totalReqs.values.count / (durationMs / 1000)).toFixed(2)
    : "N/A";

  console.log(`
╔══════════════════════════════════════════════════════════════╗
║          WORDSOUL – PERFORMANCE TEST SUMMARY                 ║
╠══════════════════════════════════════════════════════════════╣
║  [SubmitAnswer] Response Time (ms)                           ║
║    Median (p50) : ${String(submit?.values?.["p(50)"]?.toFixed(0) ?? "N/A").padEnd(10)} ms                              ║
║    p90          : ${String(submit?.values?.["p(90)"]?.toFixed(0) ?? "N/A").padEnd(10)} ms                              ║
║    p95          : ${String(submit?.values?.["p(95)"]?.toFixed(0) ?? "N/A").padEnd(10)} ms                              ║
║    p99          : ${String(submit?.values?.["p(99)"]?.toFixed(0) ?? "N/A").padEnd(10)} ms                              ║
║    Max          : ${String(submit?.values?.max?.toFixed(0)        ?? "N/A").padEnd(10)} ms                              ║
╠══════════════════════════════════════════════════════════════╣
║  Throughput     : ${String(rps).padEnd(10)} req/s                              ║
║  Total Requests : ${String(totalReqs?.values?.count ?? "N/A").padEnd(10)}                                   ║
╠══════════════════════════════════════════════════════════════╣
║  Error Rate     : ${String(((errRate?.values?.rate ?? 0) * 100).toFixed(2)).padEnd(10)} %                              ║
╚══════════════════════════════════════════════════════════════╝
`);

  return {
    "result_summary.json": JSON.stringify(data, null, 2),
    stdout: "",
  };
}
