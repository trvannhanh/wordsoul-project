# Đặc tả bàn giao phần thưởng cho M06 M07

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M07-REWARD-HANDOFF-M06-1.0` |
| Task | M07-T032 |
| Đầu vào | M07-REWARD-PACKAGE-SNAPSHOT-1.0 (M07-T031), M06-MULTI-COMPONENT-REWARD-1.0 (M06-T015) |
| Phạm vi | Đặc tả hợp đồng sự kiện phát phần thưởng `GrantQuestRewardCommand` từ M07 gửi sang M06 Engine |
| Tự kiểm | B-G04 |
| Phiên bản | 1.0 — 2026-08-22 |

## 1. Mục tiêu và invariant

Tài liệu này chuẩn hóa hợp đồng bàn giao quyền nhận thưởng (`Quest Reward Handoff Protocol`) từ M07 sang M06.

- **Ranh giới Quyền Sở hữu Dữ liệu Tài sản (`M06 Ownership Invariant`)**:
  - M07 CHỈ LÀ MODULE XÁC NHẬN ĐỦ ĐIỀU KIỆN NHẬN THƯỞNG. M07 CẤM trực tiếp cập nhật số dư Gold, Gems hay Exp trong cơ sở dữ liệu.
  - 100% việc tăng số dư BẮT BUỘC gửi `GrantQuestRewardCommand` sang M06.
- **Tính Duy nhất của Mã Lệnh Bàn giao (`Idempotent Claim Token Rule`)**: Command gửi sang M06 BẮT BUỘC chứa `ClaimToken = claim_{userQuestId}`. M06 từ chối $100\%$ các lệnh trùng mã `ClaimToken`.

## 2. Cấu trúc Lệnh Bàn giao Phần thưởng (GrantQuestRewardCommand Payload)

```json
{
  "claimToken": "claim_uq_88123",
  "userId": "usr_99123",
  "sourceModule": "M07_QUEST",
  "userQuestId": "uq_88123",
  "gold": 100,
  "exp": 50,
  "items": []
}
```

## 3. Regression Gates và Test Cases

### 3.1. Regression Gates
- `RH-G01`: 100% lệnh phát thưởng từ M07 sang M06 chứa thuộc tính `ClaimToken` duy nhất.
- `RH-G02`: M07 không trực tiếp sửa bất kỳ bảng tài sản nào trong cơ sở dữ liệu.

### 3.2. Test Cases tự kiểm
| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RH32-01` | Learner bấm "Nhận thưởng" cho `UserQuestId = uq_88123` | M07 phát `GrantQuestRewardCommand` với `ClaimToken = claim_uq_88123` sang M06. |
| `RH32-02` | Do chập chờn mạng, M07 gửi lại `GrantQuestRewardCommand` 2 lần | M06 kiểm tra `ClaimToken`, bỏ qua lượt gửi thứ 2, trả về kết quả giao dịch thành công. |
| `RH32-03` | Kiểm thử hoàn tất luồng M07-REWARD-HANDOFF-M06-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 4. Đối chiếu Hiện trạng và Finding

| Finding ID | Quan sát | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M07-RH-F01` | Sử dụng MassTransit Command Bus gửi `GrantQuestRewardCommand` | Bảo đảm tính nhất quán giao dịch giữa M07 và M06 | M06-T015 |

## 5. Tự kiểm M07-T032
- Đã hoàn thành đặc tả `M07-REWARD-HANDOFF-M06-1.0`.
- Chốt cấu trúc `GrantQuestRewardCommand` và ranh giới M06 Single Source of Truth.
- Ghi nhận 2 Regression Gates (`RH-G01`–`RH-G02`) và 3 Test Cases (`RH32-01`–`RH32-03`).

## 6. Lịch sử
| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-22 | 1.0 | Tạo mới đặc tả đặc tả bàn giao phần thưởng cho M06 M07-T032 | WSA-7K2 |
