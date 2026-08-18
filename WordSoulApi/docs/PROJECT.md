# WordSoul Project

## Mục tiêu

WordSoul là hệ sinh thái học từ vựng tiếng Anh có ôn tập lặp lại, tiến độ, gamification, cộng đồng, quản trị và nhiều client. Repository gồm:

- `WordSoulApi/`: .NET 9 API, domain, application, infrastructure, workers và tests.
- `wordsoul-app/`: learner web bằng React/Vite.
- `wordsoul-admin/`: admin web bằng Next.js.
- `wordsoul-mobile/`: mobile bằng Expo/React Native.

## Ranh giới kỹ thuật

- Luồng API: Domain → Application → Infrastructure → API composition.
- SQL Server giữ dữ liệu quan hệ bền vững; Redis dùng cho cache/coordination; tài sản lớn dùng media/blob storage.
- API là nguồn sự thật cho danh tính, quyền, ownership, tiến độ, phần thưởng và trạng thái nghiệp vụ.
- Mọi mutation nhạy cảm phải xét authentication, authorization, account state, ownership, idempotency/concurrency và audit phù hợp.
- Không để secret, token, PII thật, payload nhà cung cấp hoặc generated output vào Git hay tài liệu.

## Cách làm việc

- WSA-7K2 là người thực hiện và tự nghiệm thu duy nhất.
- Trạng thái và thứ tự công việc nằm trong [TASKS.md](./TASKS.md).
- Quyết định đang có hiệu lực nằm trong [DECISIONS.md](./DECISIONS.md).
- Chỉ đọc tài liệu module, REL/CT hoặc gate khi task đang làm liên kết tới phạm vi đó.
- Tài liệu trong các thư mục đánh số là nguồn tham khảo chi tiết, không phải tracker hoạt động.

## Definition of Done chung

Một task được hoàn thành khi:

1. Đầu ra được mô tả trong task đã tồn tại.
2. Dependency và phạm vi parent `-A` được xử lý đúng.
3. Kiểm tra phù hợp đã chạy và kết quả được ghi trong `TASKS.md`.
4. Không có thay đổi ngoài phạm vi, secret, PII hoặc generated output ngoài ý muốn.
5. Thay đổi được commit; Git commit là nhật ký công việc.

## Lệnh kiểm tra thường dùng

- API: `dotnet test WordSoulApi/WordSoulApi.sln` hoặc `dotnet build WordSoulApi/WordSoulApi.sln`.
- Learner web: chạy lint, test và build liên quan trong `wordsoul-app`.
- Admin: chạy lint và build trong `wordsoul-admin`.
- Mobile: chạy `npx tsc --noEmit` và kiểm tra hành trình Expo bị tác động.
- Task tài liệu: kiểm tra liên kết Markdown, bảng, trạng thái và phạm vi diff; không cần build mã nguồn.

## Tài liệu chi tiết khi cần

- [Tổng quan cũ](./01-tong-quan/TONG-QUAN-HE-THONG.md)
- [Danh mục module](./02-modules/README.md)
- [Kế hoạch Giai đoạn A](./03-ke-hoach-giai-doan-a/KE-HOACH-TRIEN-KHAI-GIAI-DOAN-A-B.md)
- [REL/CT Lát 0](./04-thuc-thi/lat-0/README.md)
- [Bộ kiểm tra Cổng A](./05-bang-chung/cong-a/README.md)
