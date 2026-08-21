# Chuẩn hóa tính khoảng ôn và tham số M04

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M04-SRS-INTERVAL-CALCULATION-1.0` |
| Task | M04-T016 |
| Đầu vào | M04-MEMORY-STATES-DEFINITIONS-1.0 (M04-T015), SM-2 Algorithm Standard |
| Phạm vi | Thuật toán toán học cập nhật $Interval$ và $EaseFactor$ dựa trên thuật toán SM-2 biến thể WordSoul |
| Tự kiểm | B-G02 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả công thức toán học tính toán khoảng cách ngày ôn tập tiếp theo ($Interval$) và điều chỉnh hệ số dễ nhớ ($EaseFactor$).

- **Công thức Thuật toán SRS (SM-2 Variant Formula)**:
  - Khi gợi nhớ ĐÚNG ($IsCorrect = true$):
    - Lần 1: $I_1 = 1$ ngày
    - Lần 2: $I_2 = 6$ ngày
    - Lần $n \ge 3$: $I_n = \text{Round}(I_{n-1} \times EF)$
    - $EF_{new} = EF_{old} + (0.1 - (5 - q) \times (0.08 + (5 - q) \times 0.02))$, với $q \in [3, 5]$ tùy theo tốc độ phản hồi.
  - Khi gợi nhớ SAI ($IsCorrect = false$):
    - $I_{new} = 1$ ngày, $RepetitionCount = 0$.
    - $EF_{new} = \max(1.30, EF_{old} - 0.20)$.
- **Giới hạn An toàn Tham số (`Parameter Bounds Invariant`)**:
  - $1.30 \le EaseFactor \le 2.50$
  - $1 \le Interval \le 365$ ngày.

## 2. Dynamic SRS Calculation Engine Code

```csharp
public SrsCalculationResult CalculateNextInterval(int currentRepetition, double currentEf, int currentInterval, bool isCorrect, long responseTimeMs)
{
    if (!isCorrect)
    {
        return new SrsCalculationResult {
            NextRepetition = 0,
            NextIntervalDays = 1,
            NextEaseFactor = Math.Max(1.30, currentEf - 0.20),
            NextState = MemoryState.RELEARNING
        };
    }
    
    // Đánh giá chất lượng q (3 = Chậm/Khó, 4 = Bình thường, 5 = Nhanh/Dễ)
    int q = responseTimeMs < 3000 ? 5 : (responseTimeMs < 8000 ? 4 : 3);
    
    double nextEf = currentEf + (0.1 - (5 - q) * (0.08 + (5 - q) * 0.02));
    nextEf = Math.Clamp(nextEf, 1.30, 2.50);
    
    int nextInterval;
    if (currentRepetition == 0) nextInterval = 1;
    else if (currentRepetition == 1) nextInterval = 6;
    else nextInterval = (int)Math.Min(365, Math.Round(currentInterval * nextEf));
    
    int nextRep = currentRepetition + 1;
    MemoryState nextState = nextInterval >= 21 ? MemoryState.MASTERED : (nextInterval >= 7 ? MemoryState.REVIEWING : MemoryState.LEARNING);
    
    return new SrsCalculationResult {
        NextRepetition = nextRep,
        NextIntervalDays = nextInterval,
        NextEaseFactor = nextEf,
        NextState = nextState
    };
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `IC-G01`: $EaseFactor$ không bao giờ giảm xuống dưới $1.30$ dù trả lời sai liên tiếp nhiều lần.
- `IC-G02`: $Interval$ không bao giờ vượt quá $365$ ngày.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `IC16-01` | Trả lời sai khi đang có $EF = 1.40$ | $EF$ mới bị kẹp kịch sàn ở $1.30$, $Interval = 1$ ngày. |
| `IC16-02` | Trả lời đúng nhanh ($q = 5$) lần thứ 3 khi $Interval = 6, EF = 2.50$ | $Interval$ mới $= \text{Round}(6 \times 2.50) = 15$ ngày, $EF = 2.50$. |
| `IC16-03` | Kiểm thử hoàn tất luồng M04-SRS-INTERVAL-CALCULATION-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M04-IC-F01` | Cần unit test kẹp biên cho $EF$ và $Interval$ | Đảm bảo không lỗi toán học SRS | M04-T017 |

## 5. Tự kiểm M04-T016
- Đã đặc tả chuẩn hóa tính khoảng ôn và tham số M04-T016.
- Ghi nhận 2 Regression Gates (`IC-G01`–`IC-G02`) và 3 Test Cases (`IC16-01`–`IC16-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa tính khoảng ôn và tham số M04-T016 | WSA-7K2 |
