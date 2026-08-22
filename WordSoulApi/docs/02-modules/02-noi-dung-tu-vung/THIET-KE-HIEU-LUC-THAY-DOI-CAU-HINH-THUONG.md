# Thiết kế hiệu lực thay đổi cấu hình thưởng M02

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M02-REWARD-CONFIG-EFFECTIVITY-1.0` |
| Task | M02-T043 |
| Đầu vào | M02-SET-REWARD-LINK-1.0 (M02-T042), M06-VERSIONED-REWARD-CALCULATION-1.0 (M06-T014) |
| Phạm vi | Quy tắc quản lý phiên bản cấu hình thưởng (`RewardConfigVersion`), thời điểm có hiệu lực và bảo đảm không hồi tố các phiên đã hoàn thành |
| Tự kiểm | B-G01, B-G03 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này quy định hiệu lực thời gian và cơ chế phiên bản hóa cấu hình thưởng (`RewardConfig`) liên kết giữa M02 và M06.

- **Không Áp dụng Cấu hình Thưởng Hồi tố (`No Retroactive Reward Change Invariant`)**:
  - Khi Admin thay đổi cấu hình thưởng của bộ từ vựng (ví dụ tăng thưởng từ 50 Gold lên 100 Gold), cấu hình mới CHỈ CÓ HIỆU LỰC đối với các phiên học khởi tạo TỪ THỜI ĐIỂM SỬA VỀ SAU.
  - Các phiên học đã chốt `COMPLETED` trước thời điểm sửa BẮT BUỘC giữ nguyên mức thưởng cũ.
- **Ảnh chụp Mã Cấu hình Thưởng theo Phiên (`Session Reward Config Pinning Invariant`)**:
  - Khi tạo phiên học M03, hệ thống đóng băng `RewardConfigCode` và `RewardConfigVersion` hiện hành vào `SessionSnapshotJson`.

## 2. Cấu trúc Cấu hình Thưởng có Phiên bản (Versioned Reward Config Schema)

```csharp
public class SetRewardConfig
{
    public Guid SetRewardConfigId { get; set; }
    public Guid VocabularySetId { get; set; }
    public string RewardConfigCode { get; set; }
    public int Version { get; set; } = 1;
    
    public DateTime EffectiveFromUtc { get; set; }
    public DateTime? EffectiveToUtc { get; set; }
    
    public bool IsActive { get; set; } = true;
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RC-G01`: 100% phiên học đã hoàn thành trước thời điểm đổi cấu hình thưởng duy trì mức thưởng nguyên bản.
- `RC-G02`: `SessionSnapshotJson` lưu giữ chính xác `RewardConfigCode` và `RewardConfigVersion` tại thời điểm tạo phiên.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RC43-01` | Learner A tạo phiên lúc 10:00 (RewardConfig v1 = 50 Gold). Admin cập nhật Config v2 = 100 Gold lúc 10:05. Learner A hoàn thành phiên lúc 10:10 | Learner A nhận thưởng 50 Gold (theo v1 đã snapshot). |
| `RC43-02` | Learner B tạo phiên mới lúc 10:15 | Learner B snapshot Config v2 và nhận thưởng 100 Gold khi hoàn thành. |
| `RC43-03` | Kiểm thử hoàn tất luồng M02-REWARD-CONFIG-EFFECTIVITY-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M02-RC-F01` | Cần bổ sung cột `RewardConfigVersion` vào DB M02 và Event payload | Đảm bảo M06 tính toán đúng gói thưởng | M06-T014 |

## 5. Tự kiểm M02-T043
- Đã hoàn thành đặc tả `M02-REWARD-CONFIG-EFFECTIVITY-1.0`.
- Chốt nguyên tắc pinning cấu hình thưởng và cấm thay đổi hồi tố.
- Ghi nhận 2 Regression Gates (`RC-G01`–`RC-G02`) và 3 Test Cases (`RC43-01`–`RC43-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả thiết kế hiệu lực thay đổi cấu hình thưởng M02-T043 | WSA-7K2 |
