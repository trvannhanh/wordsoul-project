# Xây dựng sổ quyền tài sản — Lát A M12

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M12-ASSET-RIGHTS-LEDGER-1.0` |
| Task | M12-T044-A |
| Đầu vào | M12-ASSET-CATALOG-1.0 (D-067), A0-T004 (REL-04), CT-01 |
| Phạm vi | Sổ đăng ký quyền sở hữu và giấy phép tài sản số (`Asset Rights Ledger`), giao thức tự động xác minh bản quyền và quy trình gỡ bỏ khi hết hạn |
| Tự kiểm | A-G03, A-G05; REL-04 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Tài liệu này đặc tả Sổ Quyền Tài sản Số Bất biến (`Asset Rights Ledger & Registry System`) thuộc M12 cho Lát A, phục vụ lưu vết bằng chứng sở hữu tác quyền, điều khoản giấy phép và trạng thái xác minh bản quyền đối với 100% tài sản số trong hệ thống WordSoul.

- **Bất biến Append-Only Sổ Quyền (`Rights Ledger Append-Only Invariant`)**: Mọi thao tác xác minh, cấp phép, cập nhật hoặc thu hồi bản quyền tài sản số được ghi vết bất biến vào Sổ Quyền `AssetRightsLedger`. CẤM chỉnh sửa hoặc xóa các bản ghi lịch sử cấp phép cũ trong CSDL.
- **Ràng buộc Phê duyệt Xuất bản Cứng (`CT-01 & REL-04 Compliance Gate`)**:
  Dịch vụ Xác minh Bản quyền (`AssetRightsService.VerifyRights(assetId)`) trả về trạng thái `rightsCleared`. Nếu `rightsCleared == false` (chưa clear bản quyền hoặc giấy phép đã hết hạn), hệ thống LẬP TỨC KHÓA thao tác phê duyệt/xuất bản bất kỳ mục từ/bộ từ vựng nào chứa tài sản đó (CT-01).
- **Tự động Cảnh báo Hết hạn Giấy phép (`License Expiration Alerting`)**: Cron Job quét hàng ngày tự động phát cảnh báo cho Quản trị viên An ninh/Biên tập trước 30 ngày khi một giấy phép tài sản số sắp hết hạn.
- **Giao thức Thu hồi Tự động khi Vi phạm (`Automated Takedown Protocol`)**: Khi một giấy phép bị thu hồi hoặc hết hạn mà không được gia hạn, hệ thống tự động đổi `rightsCleared = false` và kích hoạt Lệnh Thu hồi Khẩn cấp ($\le 60\text{s}$ Emergency Recall M11-T019) gỡ học liệu chứa tài sản đó khỏi ứng dụng.

## 2. Mô hình Thực thể Sổ Quyền Tài sản Số (AssetRightsLedger Schema)

```json
{
  "ledgerId": "01J5XB00000000000000000001",
  "assetId": "01J5XA00000000000000000001",
  "licenseId": "LIC-2026-CC-BY-4.0-001",
  "licenseType": "CC-BY-4.0",
  "rightsOwner": "Oxford Learner Data / WordSoul Publishing",
  "commercialAllowed": true,
  "attributionRequired": true,
  "attributionText": "Audio generated via Azure TTS / Oxford Dictionary License",
  "expirationDateUtc": "2028-12-31T23:59:59Z",
  "rightsCleared": true,
  "verificationStatus": "VERIFIED_ACTIVE",
  "verifiedByActorId": "USR-ADMIN-007",
  "verifiedAtUtc": "2026-08-20T10:00:00Z"
}
```

## 3. Danh mục Phân loại Giấy phép Bản quyền (License Types Enum)

| Mã LicenseType | Tên loại giấy phép | Thương mại? | Cần ghi nhận tác giả? | Thời hạn mặc định | Áp dụng chính |
|---|---|---|---|---|---|
| `CC-BY-4.0` | Creative Commons Attribution | Có | Có (`attributionRequired = true`) | Vĩnh viễn | Âm thanh/Hình ảnh học liệu mở |
| `RoyaltyFree` | Mua bản quyền thương mại | Có | Không | Theo hợp đồng | Hình ảnh/Âm thanh mua từ kho |
| `InternalGenerated` | Nội bộ WordSoul tự sản xuất | Có | Không | Vĩnh viễn | Nội dung biên tập viên tự làm |
| `UserUploaded` | Người dùng tự tải lên | Không (Nội bộ) | Không | 14 ngày (Xóa tự động) | Ghi âm phát âm người học M05 |
| `PublicDomain` | Miễn phí cộng đồng | Có | Không | Vĩnh viễn | Học liệu tác quyền tự do |

## 4. Dịch vụ Tự động Xác minh Bản quyền (Asset Rights Verification Service)

API kiểm tra bản quyền dành cho các Module M02, M06, M07 và M11 gọi trước khi Approve/Publish:

```csharp
public async Task<AssetVerificationResult> VerifyAssetRightsAsync(string assetId)
{
    var latestRights = await _db.AssetRightsLedgers
        .Where(l => l.AssetId == assetId)
        .OrderByDescending(l => l.VerifiedAtUtc)
        .FirstOrDefaultAsync();

    if (latestRights == null)
    {
        return AssetVerificationResult.Blocked("UNREGISTERED_ASSET_RIGHTS", "Tài sản chưa được đăng ký sổ quyền.");
    }

    if (!latestRights.RightsCleared || latestRights.VerificationStatus != "VERIFIED_ACTIVE")
    {
        return AssetVerificationResult.Blocked("UNCLEARED_ASSET_RIGHTS", "Tài sản chưa được xác minh bản quyền (REL-04).");
    }

    if (latestRights.ExpirationDateUtc.HasValue && latestRights.ExpirationDateUtc.Value <= DateTime.UtcNow)
    {
        return AssetVerificationResult.Blocked("EXPIRED_ASSET_LICENSE", "Giấy phép bản quyền tài sản đã hết hạn.");
    }

    return AssetVerificationResult.Cleared(latestRights.LicenseId, latestRights.AttributionText);
}
```

## 5. Quy trình Cảnh báo Hết hạn và Thu hồi Tự động (Takedown Protocol)

```
[Cron Daily Job: AssetLicenseExpirationScanner]
                      |
                      v
     (Scan ExpirationDateUtc <= UtcNow + 30 Days)
                      |
         +------------+------------+
         | (Expires in <= 30 Days) | (Expired NOW)
         v                         v
  [Send Alert to Admin]   [Auto-Revoke Rights & Takedown]
  - Severity: WARN        - Set rightsCleared = false
  - Require Renewal       - Trigger Emergency Recall (M11-T019)
                          - Remove from Public CDN Cache
```

## 6. Regression Gate và Case tự kiểm

### 6.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `AR-G01` | Mọi thay đổi trạng thái bản quyền tài sản ghi vết bất biến vào `AssetRightsLedger`. |
| `AR-G02` | 100% tài sản số xuất bản có `rightsCleared == true` và giấy phép còn hiệu lực. |
| `AR-G03` | API `VerifyAssetRights` phản hồi chính xác trạng thái bản quyền cho M02/M11 gọi kiểm tra. |
| `AR-G04` | Cấm xuất bản mục từ/bộ từ nếu API `VerifyAssetRights` trả về `Blocked` (CT-01). |
| `AR-G05` | Cron Job tự động phát cảnh báo cho Admin trước 30 ngày khi giấy phép tài sản sắp hết hạn. |
| `AR-G06` | Giấy phép hết hạn tự động chuyển `rightsCleared = false` và kích hoạt Thu hồi Khẩn cấp $\le 60\text{s}$. |
| `AR-G07` | Giấy phép `CC-BY-4.0` tự động đính kèm chuỗi ghi nhận tác giả `attributionText` vào DTO trả về. |
| `AR-G08` | Cấm sửa đổi hoặc xóa các bản ghi `AssetRightsLedger` lịch sử trong CSDL. |
| `AR-G09` | Phân quyền xác minh/cấp phép bản quyền tuân thủ ma trận vai trò M11 (`R12 Security Admin`). |
| `AR-G10` | 100% các test case tự kiểm AR44-01–20 đạt thành công trong bộ suite kiểm thử. |

### 6.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `AR44-01` | Đăng ký sổ quyền mới cho tài sản âm thanh với giấy phép `CC-BY-4.0` | Tạo `AssetRightsLedger` thành công, `rightsCleared = true` |
| `AR44-02` | Gọi API `VerifyAssetRights` cho tài sản đã clear bản quyền | Trả về `Cleared` kèm `licenseId` và `attributionText` |
| `AR44-03` | Gọi API `VerifyAssetRights` cho tài sản chưa có trong sổ quyền | Trả về `Blocked` lỗi `UNREGISTERED_ASSET_RIGHTS` |
| `AR44-04` | Thử xuất bản mục từ chứa tài sản bị `Blocked` bản quyền | System reject xuất bản mục từ theo CT-01 |
| `AR44-05` | Giấy phép tài sản còn hạn 20 ngày ($< 30$ ngày) | Cron Scanner phát cảnh báo `WARN_LICENSE_EXPIRING_SOON` |
| `AR44-06` | Giấy phép tài sản hết hạn vào ngày hôm nay | Cron Scanner chuyển `rightsCleared = false`, kích hoạt takedown |
| `AR44-07` | Lệnh Takedown tự động gỡ tài sản khỏi cache public | Ẩn tài sản khỏi API công khai trong vòng $\le 60$ giây |
| `AR44-08` | Gia hạn giấy phép đã hết hạn bằng bản ghi ledger mới | `rightsCleared` chuyển lại `true`, khôi phục hoạt động |
| `AR44-09` | Thử dùng lệnh SQL `UPDATE AssetRightsLedgers SET rightsCleared = false` | DB deny operation, bảng lưu lịch sử bất biến |
| `AR44-10` | Đăng ký tài sản nội bộ tự làm (`InternalGenerated`) | Đặt `rightsCleared = true`, `expirationDateUtc = null` |
| `AR44-11` | Đăng ký tài sản người học tải lên (`UserUploaded`) | Đặt `commercialAllowed = false`, `expirationDate = 14 ngày` |
| `AR44-12` | Tra cứu lịch sử cấp phép của 1 tài sản qua 3 lần gia hạn | Trả về đủ 3 bản ghi `AssetRightsLedger` theo thời gian |
| `AR44-13` | Admin không phải Security Admin thực hiện xác minh sổ quyền | Deny 403 Forbidden |
| `AR44-14` | Tải đồng thời 100 request xác minh bản quyền tài sản | Response p95 $< 15\text{ms}$ từ Redis cache |
| `AR44-15` | Thu hồi bản quyền đối với một tập hợp 10 tài sản | Tự động phát Thu hồi Khẩn cấp cho 10 tài sản đó |
| `AR44-16` | Kiểm tra chuỗi ghi nhận tác giả cho tài sản `CC-BY-4.0` | DTO công khai hiển thị đúng `attributionText` |
| `AR44-17` | Thao tác đăng ký sổ quyền bị lỗi DB giữa chừng | Rollback transaction toàn bộ, không tạo ledger dở dang |
| `AR44-18` | Xem vết Audit Log M11 sau khi cấp phép bản quyền | Ghi nhận Audit Event với mã action M11 phù hợp |
| `AR44-19` | Phân tích tham chiếu trước khi thu hồi giấy phép tài sản | Quét các bộ từ vựng M02 bị ảnh hưởng bởi thu hồi (T020) |
| `AR44-20` | Kiểm thử hoàn tất luồng sổ quyền tài sản M12-ASSET-RIGHTS-LEDGER-1.0 | Đạt 100% tiêu chí nghiệm thu |

## 7. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M12-AR-I01` | Trong `WordSoulApi`, chưa có bảng `AssetRightsLedgers` lưu vết bản quyền bất biến | Chưa có hạ tầng theo dõi giấy phép và tác quyền tài sản | M12-T049 (Source task) |
| `M12-AR-I02` | Chưa có Dịch vụ Tự động Xác minh Bản quyền (`AssetRightsService`) | Module M02/M11 duyệt bài chưa thể kiểm tra tự động bản quyền | M12-T049; REL-04 |
| `M12-AR-I03` | Thiếu Cron Job phát cảnh báo trước 30 ngày khi giấy phép hết hạn | Rủi ro để lọt giấy phép hết hạn gây vi phạm pháp lý | M12-T049 |
| `M12-AR-I04` | Thiếu luồng Takedown tự động kích hoạt Emergency Recall $\le 60\text{s}$ | Không thể thu hồi nhanh học liệu khi giấy phép bị hủy | M12-T049 |
| `M12-AR-I05` | DTO công khai chưa đính kèm `attributionText` cho tài sản `CC-BY-4.0` | Rủi ro vi phạm điều khoản ghi nhận tác giả Creative Commons | M12-T049 |

- `M12-AR-F01`: Tạo bảng CSDL `AssetRightsLedgers` và DTO quản lý sổ quyền (tiếp nhận: M12-T049).
- `M12-AR-F02`: Triển khai `AssetRightsService` với API `VerifyAssetRights` (tiếp nhận: M12-T049; REL-04).
- `M12-AR-F03`: Xây dựng `AssetLicenseExpirationScannerJob` quét hết hạn hàng ngày (tiếp nhận: M12-T049).
- `M12-AR-F04`: Thiết lập bộ kiểm thử tự động AR-G01–G10 và AR44-01–20 (tiếp nhận: M12 tasks).
- `M12-AR-F05`: Thu thập bằng chứng runtime cho luồng sổ quyền tài sản M12 (tiếp nhận: M12 tasks; A-G03/A-G05).

## 8. Tự kiểm M12-T044-A

- Đã thiết kế hoàn chỉnh `M12-ASSET-RIGHTS-LEDGER-1.0` với Schema `AssetRightsLedger` bất biến append-only.
- Đã chốt Dịch vụ Tự động Xác minh Bản quyền `VerifyAssetRights` cho M02/M11 kiểm tra trước khi Approve/Publish.
- Đã lồng ghép chốt CT-01 & REL-04: Cấm xuất bản nếu `rightsCleared == false`.
- Đã xây dựng Giao thức Cảnh báo Hết hạn trước 30 ngày và Thu hồi Tự động SLA $\le 60\text{s}$ khi vi phạm.
- Đã xác lập 10 Regression Gates (`AR-G01`–`AR-G10`) và 20 Test Cases tự kiểm (`AR44-01`–`AR44-20`).
- Đã ghi nhận 5 sai lệch tĩnh từ codebase hiện tại và 5 finding tiếp nhận cho các task triển khai sau.

## 9. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả sổ quyền tài sản M12-T044-A | WSA-7K2 |
