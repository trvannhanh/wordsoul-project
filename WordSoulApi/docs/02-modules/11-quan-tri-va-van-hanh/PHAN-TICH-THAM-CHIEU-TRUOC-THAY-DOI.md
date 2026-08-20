# Thiết kế phân tích tham chiếu trước thay đổi M11

| Thuộc tính | Giá trị |
|---|---|
| Contract ID | `M11-REFERENCE-IMPACT-1.0` |
| Task | M11-T020 |
| Đầu vào | M11-CROSS-CONTENT-MATRIX-1.0, M11-CONTENT-LIFECYCLE-1.0, M11-PREVIEW-1.0, REL-04 |
| Phạm vi | Động cơ phân tích đồ thị phụ thuộc và đánh giá tác động tham chiếu trước khi chỉnh sửa, ngừng dùng hoặc xuất bản nội dung |
| Tự kiểm | A-G03, A-G06 |
| Phiên bản | 1.0 — 2026-08-20 |

## 1. Mục tiêu và invariant

Động cơ phân tích tham chiếu trước thay đổi (Pre-Change Reference & Impact Analysis Engine) cung cấp cơ chế tự động quét đồ thị phụ thuộc giữa các thực thể nội dung quản trị (Bộ từ M02, Loại phiên học M03, Nhiệm vụ/Thành tựu M07, Phòng đấu M08, Mẫu thông báo M10, Tài sản phương tiện M12) nhằm phát hiện sớm rủi ro gãy liên kết, vi phạm bản quyền hoặc phá vỡ tính toàn vẹn của ứng dụng trước khi quyết định phê duyệt và xuất bản.

- **Bắt buộc phân tích trước mọi Mutation**: Mọi Yêu cầu thay đổi (`Change Request`) hoặc thao tác duyệt nội dung (`Approve/Publish/Deprecate/Recall`) BẮT BUỘC phải thực thi phân tích tham chiếu và lưu kết quả `ReferenceImpactReport` kèm `changeRevisionId`.
- **Ranh giới tác động 3 cấp độ (`Impact Severity Levels`)**:
  - `BLOCKING_HARD_DEPENDENCY`: Tác động làm gãy phụ thuộc cứng (vd: ẩn bộ từ M02 đang được dùng làm điều kiện cho Nhiệm vụ M07 active). HỆ THỐNG BẮT BUỘC TỪ CHỐI THAO TÁC (`HARD BLOCK`).
  - `WARN_CASCADE_UPDATE`: Tác động có thể xử lý thông qua cập nhật chuỗi (`Cascade Change Set`). Yêu cầu xác nhận tạo Change Set đồng bộ.
  - `SAFE_NO_ACTIVE_REF`: Không có phụ thuộc active, thao tác an toàn.
- **Rà soát bản quyền tài sản liên đới (REL-04 / CT-01 / A-G03)**: Đồ thị tham chiếu tự động duyệt qua toàn bộ cây tài sản phương tiện con (âm thanh phát âm, hình ảnh minh họa). Nếu phát hiện ít nhất 1 tài sản con chưa xác minh bản quyền (`rightsCleared == false`), hệ thống lập tức chốt trạng thái `BLOCKING_UNCLEARED_ASSET` (CT-01).

## 2. Ephemeral & Durable Reference Model

| Model / Record | Identity | Nội dung chính | Tính chất |
|---|---|---|---|
| `ReferenceImpactReport` | `reportId` | `changeRevisionId`, `targetEntityId`, `targetEntityType`, `overallImpactLevel`, `blockingReason`, `scannedAtUtc` | Bất biến |
| `EntityDependencyNode` | `nodeId` | `entityId`, `entityType`, `entityVersion`, `rightsCleared`, `status` | Snapshot node |
| `EntityDependencyEdge` | `edgeId` | `sourceNodeId`, `targetNodeId`, `dependencyType` (`HARD_BINDING` / `SOFT_ALIAS` / `MEDIA_ASSET`), `isStrict` | Snapshot edge |

Mô hình Đồ thị Tham chiếu (`Dependency Graph Topology`):
```
[Target Entity: Vocab Set M02] (Version v1.2)
       |
       +---> (HARD_BINDING) ---> [Quest Requirement M07] (Active) ==> BLOCK IF DEPRECATED
       |
       +---> (SOFT_ALIAS) ---> [Battle Room Config M08] (Inactive) ==> WARN CASCADE
       |
       +---> (MEDIA_ASSET) ---> [Audio Asset M05/M12] (rightsCleared = false) ==> BLOCK (CT-01 / REL-04)
```

## 3. Phân loại Cấp độ Tác động Tham chiếu

| Cấp độ tác động | Ý nghĩa nghiệp vụ | Hành vi hệ thống | Thẩm quyền xử lý |
|---|---|---|---|
| `SAFE_NO_ACTIVE_REF` | Không có thực thể con/cha nào phụ thuộc active. | Cho phép đi tiếp bước Phê duyệt. | System Auto |
| `WARN_CASCADE_UPDATE` | Phụ thuộc mềm có thể tự động cập nhật phiên bản mới. | Yêu cầu xác nhận tạo Cascade Change Set. | Content Admin (R03) |
| `BLOCKING_HARD_DEPENDENCY` | Làm gãy giao dịch hoặc điều kiện active của module khác. | Từ chối thao tác (`Hard Block`). | Cấm bypass |
| `BLOCKING_UNCLEARED_ASSET` | Tồn tại tài sản phương tiện con chưa duyệt bản quyền REL-04. | Từ chối thao tác theo CT-01 (`Hard Block`). | Cấm bypass |
| `UNCERTAIN_GRAPH_STALE` | Dữ liệu đồ thị phụ thuộc bị rỗng hoặc stale $> 1$ giờ. | Tạm dừng phê duyệt, yêu cầu rebuild graph. | Ops Admin (R02) |

## 4. Giao thức Phân tích Đồ thị Phụ thuộc

1. **Khởi chạy Quét (Graph Traversal)**: Truy vấn đệ quy từ `targetEntityId` đến độ sâu tối đa $D = 5$ tầng liên kết.
2. **Kiểm tra Trạng thái Runtime**:
   - Đếm số lượng phiên học (`Active Learning Sessions M03`) đang nạp thực thể.
   - Đếm số lượng trận đấu (`Active Battle Rooms M08`) đang sử dụng thực thể.
   - Đếm số lượng nhiệm vụ (`Active Quests M07`) đang lấy thực thể làm điều kiện.
3. **Tổng hợp Báo cáo `ReferenceImpactReport`**: Tính toán `overallImpactLevel` theo nguyên tắc Max-Severity (nếu có 1 node `BLOCKING` thì toàn bộ báo cáo thành `BLOCKING`).

## 5. Tích hợp với Vòng đời Nội dung (T019) và Change Decision (D-040)

- Khi Admin chuyển trạng thái nội dung từ `in_review` $\to$ `approved` trong `M11-CONTENT-LIFECYCLE-1.0`:
  1. Hệ thống tự động kích hoạt `Pre-Change Reference Analysis`.
  2. Báo cáo `ReferenceImpactReport` phải đạt `overallImpactLevel == SAFE_NO_ACTIVE_REF` hoặc `WARN_CASCADE_UPDATE` (đã xác nhận Cascade).
  3. `reportId` được khóa cố định vào bản ghi `ContentQualityReview` và `ChangeDecisionRecord`.
  4. Nếu báo cáo là `BLOCKING_HARD_DEPENDENCY` hoặc `BLOCKING_UNCLEARED_ASSET`, nút "Phê duyệt" trên giao diện Admin bị khóa và thông báo lý do chi tiết.

## 6. Phân tích Rủi ro Bản quyền và Quyền Tài sản (REL-04 / A-G03 / A-G05)

- **Nguyên tắc Truy vết Con (Child Asset Rights Verification)**:
  Nội dung bộ từ M02 gồm 50 mục từ. Mỗi mục từ chứa 1 hình ảnh và 1 âm thanh. Động cơ phân tích tham chiếu rà soát đủ 100 tài sản con. Nếu 99 tài sản đã clear nhưng 1 file âm thanh có `rightsCleared == false`:
  - `overallImpactLevel` chuyển thành `BLOCKING_UNCLEARED_ASSET`.
  - Hệ thống chỉ rõ ID của file âm thanh vi phạm và ngăn chặn xuất bản bộ từ cho đến khi file đó được thay thế hoặc xác minh bản quyền theo REL-04.

## 7. Regression Gate và Case tự kiểm

### 7.1. Danh mục Regression Gate

| Gate ID | Điều kiện đạt |
|---|---|
| `RI-G01` | 100% thao tác chuyển trạng thái `approved/published/deprecated` bắt buộc có `ReferenceImpactReport`. |
| `RI-G02` | Đồ thị tham chiếu quét đệ quy tối thiểu 5 tầng phụ thuộc chéo module M01–M12. |
| `RI-G03` | Cấp độ `BLOCKING_HARD_DEPENDENCY` lập tức từ chối thao tác phê duyệt, cấm can thiệp tay bypass. |
| `RI-G04` | Cấp độ `BLOCKING_UNCLEARED_ASSET` chặn 100% các hành vi công khai tài sản chưa clear bản quyền (CT-01). |
| `RI-G05` | Rà soát chính xác các phiên học active M03, trận đấu active M08 và nhiệm vụ active M07. |
| `RI-G06` | Cấp độ `WARN_CASCADE_UPDATE` bắt buộc tạo Cascade Change Set đồng bộ trước khi cho phép duyệt. |
| `RI-G07` | Dữ liệu đồ thị phụ thuộc bị stale $> 1$h tự động chuyển thành `UNCERTAIN_GRAPH_STALE`, tạm dừng duyệt. |
| `RI-G08` | Mọi báo cáo `ReferenceImpactReport` được lưu vết bất biến trỏ tới `changeRevisionId`. |
| `RI-G09` | Phân quyền và truy soát an toàn tuân thủ nghiêm ngặt ma trận vai trò `M11-PERM-1.0`. |
| `RI-G10` | 100% các test case tự kiểm RI20-01–20 đạt thành công trong bộ suite kiểm thử. |

### 7.2. Danh mục Test Case tự kiểm

| Case ID | Tình huống | Kết quả bắt buộc |
|---|---|---|
| `RI20-01` | Phân tích tham chiếu bộ từ M02 không có phụ thuộc active | Trả về `SAFE_NO_ACTIVE_REF`, cho phép phê duyệt |
| `RI20-02` | Ẩn bộ từ M02 đang được dùng làm điều kiện Nhiệm vụ M07 active | Trả về `BLOCKING_HARD_DEPENDENCY`, từ chối phê duyệt |
| `RI20-03` | Xuất bản bộ từ M02 chứa 1 file âm thanh `rightsCleared == false` | Trả về `BLOCKING_UNCLEARED_ASSET`, chặn theo CT-01 |
| `RI20-04` | Sửa phiên bản bài học M03 có 5 phòng đấu M08 đang chờ | Trả về `WARN_CASCADE_UPDATE`, yêu cầu tạo Cascade Change Set |
| `RI20-05` | Đồ thị phụ thuộc Redis cache bị mất kết nối | Tự động rebuild graph từ SQL và gắn warning độ trễ |
| `RI20-06` | Dữ liệu đồ thị phụ thuộc chưa được cập nhật $> 2$ giờ | Trả về `UNCERTAIN_GRAPH_STALE`, tạm dừng bước duyệt |
| `RI20-07` | Thử phê duyệt nội dung khi báo cáo tham chiếu đang ở `BLOCKING` | System deny request với lỗi `APPROVAL_BLOCKED_BY_IMPACT_REPORT` |
| `RI20-08` | Phân tích tham chiếu nội dung có độ sâu đồ thị 4 tầng | Quét thành công đầy đủ 4 tầng phụ thuộc |
| `RI20-09` | Thay đổi mẫu thông báo M10 đang gắn vào 1 chiến dịch active | Trả về `BLOCKING_HARD_DEPENDENCY`, yêu cầu dừng chiến dịch trước |
| `RI20-10` | Kiểm tra tính bất biến của bản ghi `ReferenceImpactReport` | API deny mọi thao tác Update/Delete trên Report |
| `RI20-11` | Thay thế 1 file hình ảnh bị lỗi trong bộ từ M02 | Phân tích tham chiếu xác nhận file mới đã clear REL-04, duyệt an toàn |
| `RI20-12` | Quét tham chiếu bộ từ M02 có 1000 người học đang trong phiên học active | Đếm đúng 1000 active sessions, gắn `BLOCKING_HARD_DEPENDENCY` |
| `RI20-13` | Admin xác nhận tạo Cascade Change Set cho cấp độ `WARN_CASCADE` | Tạo Change Set đồng bộ thành công, cho phép duyệt |
| `RI20-14` | Tra cứu báo cáo tham chiếu theo `changeRevisionId` | Trả về báo cáo chính xác kèm danh sách các node/edge |
| `RI20-15` | Thử bypass bước phân tích tham chiếu để duyệt trực tiếp | System reject với lỗi `MISSING_REFERENCE_IMPACT_REPORT` |
| `RI20-16` | Phân tích tham chiếu cho thao tác Ngừng dùng (`Deprecate`) khóa cấu hình | Quét đủ các consumer active trước khi cho phép Deprecate |
| `RI20-17` | Tải đồng thời 40 yêu cầu phân tích tham chiếu đồ thị | Động cơ xử lý p95 $< 300\text{ms}$, không gây treo DB |
| `RI20-18` | Rà soát bản quyền tài sản phương tiện cho 500 mục từ M02 | Quét 1000 tài sản con thành công, trả kết quả chính xác |
| `RI20-19` | User không có quyền `R03 Content Admin` gọi API phân tích tham chiếu | Deny 403 Forbidden |
| `RI20-20` | Kiểm thử hoàn tất luồng: Pre-Change Analysis $\to$ Report Safe $\to$ Content Approved | Toàn bộ quy trình chạy mượt mà, lưu audit log đủ |

## 8. Đối chiếu hiện trạng và finding

| Finding ID | Quan sát tĩnh | Sai lệch | Task tiếp nhận |
|---|---|---|---|
| `M11-RI-I01` | Trong source `WordSoulApi`, chưa có engine quét đồ thị phụ thuộc (`Dependency Graph Engine`) | Không thể phát hiện rủi ro gãy liên kết trước khi sửa/xóa | M11-T049 |
| `M11-RI-I02` | Chưa có cơ chế chặn phê duyệt tự động khi báo cáo tác động thuộc mức `BLOCKING` | Quản trị viên có thể vô tình duyệt nội dung gây crash app | M11-T049 |
| `M11-RI-I03` | Thiếu sự kết nối rà soát bản quyền tài sản con (REL-04) trong cây phụ thuộc | Rủi ro lọt tài sản chưa duyệt bản quyền ra ứng dụng | M02 tasks; M11-T049 |
| `M11-RI-I04` | Thiếu bảng lưu trữ bất biến `ReferenceImpactReport` | Không thể kiểm toán lý do phê duyệt/từ chối của từng revision | M11-T049 |
| `M11-RI-I05` | Chưa có cơ chế đếm active sessions/battles/quests thời gian thực trong graph analyzer | Kết quả đánh giá tác động không phản ánh đúng tải lượng runtime | M11-T049 |

- `M11-RI-F01`: Triển khai `DependencyGraphAnalyzer` và API phân tích tham chiếu (tiếp nhận: M11-T049).
- `M11-RI-F02`: Tích hợp bộ chặn phê duyệt tự động khi `impactLevel == BLOCKING` (tiếp nhận: M11-T049).
- `M11-RI-F03`: Xây dựng dịch vụ rà soát bản quyền tài sản con theo hợp đồng REL-04 (tiếp nhận: M02 tasks; M11-T049).
- `M11-RI-F04`: Thiết lập bộ kiểm thử tự động RI-G01–G10 và RI20-01–20 (tiếp nhận: M11-T049).
- `M11-RI-F05`: Thu thập bằng chứng runtime cho luồng phân tích tham chiếu trước thay đổi (tiếp nhận: M11-T049; A-G03/A-G06).

## 9. Tự kiểm M11-T020

- Đã thiết kế hoàn chỉnh `M11-REFERENCE-IMPACT-1.0` với động cơ quét đồ thị phụ thuộc đệ quy 5 tầng.
- Đã chốt 3 cấp độ tác động tham chiếu (`SAFE_NO_ACTIVE_REF`, `WARN_CASCADE_UPDATE`, `BLOCKING_HARD_DEPENDENCY`).
- Đã lồng ghép cơ chế rà soát bản quyền tài sản con `BLOCKING_UNCLEARED_ASSET` tuân thủ REL-04 và CT-01.
- Đã xác lập quy trình khóa cứng nút Phê duyệt khi báo cáo thuộc cấp độ `BLOCKING`.
- Đã xác lập 10 Regression Gates (`RI-G01`–`RI-G10`) và 20 Test Cases tự kiểm (`RI20-01`–`RI20-20`).
- Đã ghi nhận 5 sai lệch tĩnh và 5 finding tiếp nhận cho các task triển khai sau.

## 10. Lịch sử

| Ngày | Phiên bản | Thay đổi | Người tự kiểm |
|---|---|---|---|
| 2026-08-20 | 1.0 | Khởi tạo đặc tả thiết kế phân tích tham chiếu trước thay đổi M11-T020 | WSA-7K2 |
