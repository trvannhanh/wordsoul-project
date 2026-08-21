# Chuẩn hóa liên kết bộ với danh mục thưởng M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-SET-REWARD-LINK-1.0` |
| Task | M02-T042 |
| Đầu vào | M02-SET-LIFECYCLE-1.0 (M02-T017), M06-ASSET-ITEM-DICT-1.0 (M06-T001) |
| Phạm vi | Ranh giới trách nhiệm liên kết cấu hình thưởng của bộ từ vựng giữa M02 và M06 |
| Tự kiểm | B-G03 |
| Phiên bản | 1.0 — 2026-08-21 |

## 1. Mục tiêu và invariant

Tài liệu này quy định mối quan hệ liên kết giữa Bộ từ vựng (M02) và Cấu hình phần thưởng (M06).

- **Phân tách Ranh giới Sở hữu (`Ownership Boundary Invariant`)**:
  - *M02 (Sở hữu Cấu hình Liên kết)*: M02 chỉ chịu trách nhiệm lưu giữ mã gói thưởng tương ứng (`RewardConfigId`) và các tham số điều kiện học liệu (ví dụ: số từ hoàn thành tối thiểu).
  - *M06 (Sở hữu Cấp thưởng)*: M06 sở hữu toàn bộ logic tính toán, cấp Gold/Exp/Items thực tế và sổ cái biến động tài sản. M02 CẤM tự ý cấp tài sản trực tiếp.
- **Chặn Bộ từ Ngừng hoạt động (`Inactive Set Block Invariant`)**: Khi bộ từ vựng chuyển sang trạng thái `ARCHIVED` hoặc `RECALLED`, liên kết thưởng lập tức bị vô hiệu hóa (`IsRewardEnabled = false`). CẤM phát sự kiện cấp thưởng cho các bộ từ không còn hoạt động.

## 2. Cấu trúc Liên kết Cấu hình Thưởng (Reward Link Envelope)

```csharp
public class SetRewardLink
{
    public Guid VocabularySetId { get; set; }
    public string RewardConfigCode { get; set; } // e.g. "REWARD_SET_STANDARD_A1"
    public int MinimumWordsRequired { get; set; }
    public bool IsRewardEnabled { get; set; } = true;
    public string RewardSnapshotDigest { get; set; }
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RL-G01`: 100% bộ từ ở trạng thái `ARCHIVED` hoặc `RECALLED` bị chặn không phát lệnh thưởng sang M06.
- `RL-G02`: M02 không lưu trữ số lượng Gold/Exp trực tiếp trong DB mà chỉ lưu `RewardConfigCode` trỏ tới M06.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RL42-01` | Người học hoàn thành bộ từ `PUBLISHED` có `RewardConfigCode = "REWARD_SET_BASIC"` | M02 phát event kèm `RewardConfigCode` sang M06 để cấp thưởng. |
| `RL42-02` | Người học hoàn thành bộ từ bị `RECALLED` | System không phát lệnh cấp thưởng sang M06. |
| `RL42-03` | Kiểm thử hoàn tất luồng M02-SET-REWARD-LINK-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-RL-F01` | Bổ sung trường `RewardConfigCode` vào entity `VocabularySet` | Đảm bảo liên kết sạch sang M06 | M02-T049 |

## 5. Tự kiểm M02-T042
- Đã hoàn thành đặc tả `M02-SET-REWARD-LINK-1.0`.
- Chốt ranh giới sở hữu M02-M06 và 2 Regression Gates (`RL-G01`–`RL-G02`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-21 | 1.0 | Tạo mới đặc tả chuẩn hóa liên kết bộ với danh mục thưởng M02-T042 | WSA-7K2 |
