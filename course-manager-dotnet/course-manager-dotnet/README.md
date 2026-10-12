# Quản lý khóa học — C#/.NET Solution

Bản này dùng C# ASP.NET Core MVC để mở bằng file `.sln` trong Visual Studio. Nó là project riêng với bản Java Spring Boot.

## Mở và chạy bằng Visual Studio

1. Cài Visual Studio 2022 phiên bản 17.8 trở lên, chọn workload **ASP.NET and web development**. Cài PostgreSQL Server và pgAdmin.
2. Mở pgAdmin, kết nối tới PostgreSQL Server. Nhấp phải **Databases → Create → Database...**, nhập tên `course_manager_sln`, bấm **Save**. Hoặc mở Query Tool của database `postgres` và chạy `database/create_database.sql`.
3. Mở `CourseManager.sln` bằng Visual Studio.
4. Mở `CourseManager.Web/appsettings.json`; thay `your_postgres_password` bằng mật khẩu PostgreSQL bạn đã đặt khi cài PostgreSQL.
5. Trong Solution Explorer, nhấp phải `CourseManager.Web` → **Set as Startup Project** nếu project chưa được chọn.
6. Nhấn **F5** hoặc nút chạy màu xanh. Trang web tự mở tại `http://localhost:5080`.

Lần đầu Visual Studio tải package NuGet nên cần Internet. Project dùng .NET 8 và EF Core 8; Npgsql Entity Framework provider 8.0.11 hỗ trợ .NET 8. Nếu mở bằng terminal thay vì Visual Studio:

Nếu chương trình dừng ở `EnsureCreatedAsync()` và báo lỗi kết nối PostgreSQL, kiểm tra theo thứ tự: (1) mở Windows Services (`Win + R` → `services.msc`) và khởi động dịch vụ PostgreSQL (tên thường có dạng `postgresql-x64-xx`); (2) kiểm tra database `course_manager_sln` đã được tạo trong pgAdmin; (3) kiểm tra `Host=localhost;Port=5432`, username `postgres` và mật khẩu trong `appsettings.json`. `EnableRetryOnFailure()` sẽ thử lại lỗi kết nối tạm thời, nhưng không thể sửa sai mật khẩu hoặc PostgreSQL đang tắt. pgAdmin là công cụ quản lý; PostgreSQL Server phải được cài và đang chạy để ứng dụng kết nối.

```powershell
cd CourseManager.Web
dotnet restore
dotnet run
```

## Database và dữ liệu mẫu

Kết nối mặc định nằm tại `CourseManager.Web/appsettings.json`. Sau khi database `course_manager_sln` có sẵn, `Program.cs` gọi `EnsureCreatedAsync()` để tạo ba bảng. `DemoData` thêm 3 loại, 5 khóa học và ảnh mẫu nếu database chưa có dữ liệu. Một khóa học có thể không có ảnh; trang chi tiết sẽ thông báo rõ.

Các bảng: `course_categories`, `courses`, `course_images`. Quan hệ được cấu hình trong `Data/CourseDbContext.cs`: loại có nhiều khóa học; khóa học có nhiều ảnh. Khóa ngoại loại dùng `Restrict`, khóa ngoại ảnh dùng `Cascade`. Tên khóa học có unique index ghép với `CourseCategoryId`.

## Cấu trúc chính

```text
CourseManager.sln
CourseManager.Web/
├── CourseManager.Web.csproj
├── Program.cs
├── appsettings.json
├── Controllers/       # MVC endpoints
├── Data/              # EF Core DbContext và dữ liệu demo
├── Models/            # CourseCategory, Course, CourseImage
├── Services/          # Truy vấn và quy tắc nghiệp vụ
├── ViewModels/        # Dữ liệu form/danh sách
├── Views/             # Razor pages (Index, Create, Edit, Details)
└── wwwroot/css/
```

## Những chức năng có trong project

- Tạo, sửa, xem chi tiết, xóa khóa học; tạo/sửa/xóa loại khóa học.
- Tìm theo một phần tên, lọc đồng thời theo loại, trạng thái và cận học phí; sắp xếp học phí tăng/giảm hoặc thời lượng tăng dần. `CourseService.SearchAsync()` xây dựng `IQueryable` và để EF Core chuyển điều kiện thành SQL; không lọc toàn bộ dữ liệu bằng vòng lặp C#.
- Có phân trang và giữ lại tham số lọc giữa các trang.
- `DurationLabel` tính khi hiển thị: tối đa 30 giờ là Ngắn hạn; trên 30 giờ là Dài hạn.
- Thêm ảnh qua URL, chọn/đổi ảnh đại diện, xóa ảnh; nếu xóa ảnh đại diện thì ảnh còn lại đầu tiên được chọn làm đại diện.
- Từ chối xóa loại đang được khóa học dùng. Khóa học ngừng tuyển sinh vẫn xuất hiện trong danh sách quản trị.
- Data Annotations kiểm tra tên, học phí, thời lượng, loại khóa học và URL ảnh. ID không tồn tại, bộ lọc không có kết quả, khoảng phí sai và khóa học chưa có ảnh đều có trang/thông báo phù hợp.

## Giải thích để trình bày

- **Model** là lớp dữ liệu; thuộc tính điều hướng thể hiện quan hệ giữa các bảng.
- **DbContext** định nghĩa bảng, unique index và cách xử lý khóa ngoại.
- **Service** giữ logic lọc, kiểm tra khóa học trùng tên, quản lý ảnh đại diện và chặn xóa loại đang được dùng.
- **Controller** nhận HTTP request, gọi service rồi trả về View hoặc redirect.
- **ViewModel** làm dữ liệu truyền qua form, tránh bind trực tiếp toàn bộ entity khi thêm/sửa.
- **Razor View** tạo giao diện HTML; Bootstrap tải từ CDN để bố cục responsive.

## Giả định

- Học phí tính VNĐ; thời lượng là số giờ nguyên.
- Ảnh được nhập bằng URL công khai, không tải file lên server.
- Bài tập chạy cục bộ, chưa yêu cầu đăng nhập.
- Project dùng `EnsureCreated` cho bài tập; khi cần thay đổi schema nhiều lần, nên chuyển sang EF Core Migrations.
