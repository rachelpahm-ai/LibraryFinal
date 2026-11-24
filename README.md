# BookNest – Hệ thống quản lý thư viện

**BookNest** là đồ án môn **Lập trình Web** tại **Trường Đại học Ngoại ngữ – Tin học TP.HCM (HUFLIT)**. Hệ thống hỗ trợ tra cứu sách, quản lý danh mục và thành viên, lập phiếu mượn – trả, xử lý tiền phạt quá hạn và quản lý yêu cầu đặt trước sách. Độc giả có thể theo dõi lịch sử mượn cá nhân sau khi đăng nhập.

> **Phạm vi:** Đồ án phục vụ mục đích học tập và trình diễn trên môi trường phát triển (local). Tài khoản, số liệu và giao dịch trong bộ dữ liệu Seed là dữ liệu giả lập.

## 1. Công nghệ sử dụng

| Thành phần | Công nghệ |
|---|---|
| Ngôn ngữ | C# |
| Backend | ASP.NET MVC 5 trên .NET Framework 4.8 |
| Giao diện | Razor Views, HTML5, CSS3, JavaScript, Bootstrap 5.3.3 |
| Cơ sở dữ liệu | Microsoft SQL Server / SQL Server LocalDB |
| ORM | Entity Framework 6 – Database First (`Model1.edmx`) |
| Xác thực & phân quyền | Forms Authentication; vai trò `Admin`, `Librarian`, `Reader` |
| Công cụ phát triển | Visual Studio 2022, SQL Server Management Studio (SSMS), Git/GitHub |

## 2. Chức năng chính

### Khách truy cập
- Xem trang chủ, danh mục sách, chi tiết sách, tác giả và thể loại.
- Tra cứu sách và xem trạng thái còn/hết sách.
- Không được thực hiện các thao tác CRUD quản trị hoặc xem lịch sử mượn cá nhân.

### Độc giả (`Reader`)
- Sử dụng các chức năng tra cứu công khai.
- Đăng nhập và chọn **Lịch sử mượn** trên thanh điều hướng để xem các phiếu của chính mình.
- Xem ngày mượn, hạn trả, ngày trả thực tế, danh sách sách, trạng thái và tiền phạt đã ghi nhận.
- Không được xem lịch sử của thành viên khác hoặc thực hiện CRUD quản trị.

### Thủ thư (`Librarian`)
- Quản lý sách, thể loại, tác giả và thành viên.
- Quản lý phiếu mượn: lập phiếu, tìm kiếm, xem chi tiết, xác nhận trả và hủy phiếu chưa trả.
- Quản lý các yêu cầu đặt trước theo chức năng đã triển khai.
- Xem lịch sử mượn của từng thành viên tại **Thành viên → Lịch sử mượn**.
- Không có quyền quản lý tài khoản tại `/Users`.

### Quản trị viên (`Admin`)
- Thực hiện các chức năng quản lý của thủ thư.
- Quản lý tài khoản và vai trò người dùng.

### Lịch sử mượn cá nhân và bảo vệ dữ liệu

| Vai trò | Quyền xem lịch sử |
|---|---|
| Khách | Không được truy cập |
| Reader | Chỉ xem lịch sử của chính mình |
| Librarian | Xem lịch sử của từng thành viên |
| Admin | Xem lịch sử của từng thành viên |

- `LoanHistoryController` xác thực người dùng và xác định quyền truy cập từ `Account.Role` và `Account.MemberID`.
- Reader truy cập `/LoanHistory`; nếu truyền `memberId` của người khác, Controller từ chối với HTTP 403.
- Admin và Librarian truy cập từ nút **Lịch sử mượn** tại trang Thành viên (`/LoanHistory?memberId=...`).
- Lịch sử được sắp xếp theo ngày mượn mới nhất; trạng thái gồm Đang mượn, Quá hạn và Đã trả. Tiền phạt hiển thị là giá trị đã lưu trong `Loan.TotalFine`, không phải số tiền dự tính cho phiếu chưa trả.
- Chức năng chỉ đọc dữ liệu hiện có, không yêu cầu thay đổi schema hoặc EDMX. Các phiếu đã hủy và bị xóa không còn trong lịch sử.

### Giao diện và trải nghiệm người dùng

- Đồng bộ bảng dữ liệu, nút CRUD, bộ lọc và phân trang theo phong cách BookNest.
- Nút thêm mới và tìm kiếm có nền tím; nút chi tiết có viền tím, nút sửa có viền xám và nút xóa có viền đỏ.
- Phân biệt trạng thái phiếu mượn bằng màu sắc và nhãn văn bản; hiển thị vai trò tài khoản rõ ràng.
- Thanh điều hướng hiển thị theo vai trò; Reader có mục Lịch sử mượn riêng, Admin và Librarian xem lịch sử qua trang Thành viên.
- Navbar có menu thu gọn trên màn hình nhỏ, sử dụng Bootstrap Collapse.
- CSS dùng chung tại `Library/Content/library.css`, layout tại `Library/Views/Shared/LibraryLayout.cshtml`.

### Quy tắc nghiệp vụ mượn – trả
- Thành viên phải tồn tại, không bị khóa và có thẻ còn hạn.
- Không cho mượn khi thành viên có sách quá hạn chưa trả hoặc vượt quá **5 quyển đang mượn**.
- Chỉ cho mượn sách đang hoạt động và có `StockQuantity > 0`.
- Khi tạo phiếu, hệ thống trừ tồn kho; khi trả hoặc hủy phiếu chưa trả, hệ thống hoàn lại tồn kho.
- Tiền phạt quá hạn: **5.000 VNĐ × số ngày quá hạn × số cuốn**.
- Các thao tác tạo/trả/hủy phiếu sử dụng giao dịch cơ sở dữ liệu (*transaction*) để hạn chế dữ liệu không nhất quán.

## 3. Thành viên thực hiện

| Thành viên | Vai trò |
|---|---|
| **Phạm Ngọc Phương Trinh** | Nhóm trưởng |
| **Nguyễn Thị Cẩm Tiên** | Thành viên |

## 4. Yêu cầu môi trường

- **Windows** với **Visual Studio 2022**, đã cài workload **ASP.NET and web development** và bộ công cụ cho **.NET Framework 4.8**.
- **SQL Server LocalDB** (khuyến nghị instance `(localdb)\MSSQLLocalDB`) hoặc SQL Server tương thích.
- **SQL Server Management Studio (SSMS)** để chạy script SQL.
- **Git** hoặc **GitHub Desktop** để clone repository.

## 5. Hướng dẫn cài đặt và chạy trên máy mới

### Bước 1 – Clone repository

```bash
git clone https://github.com/rachelpahm-ai/LibraryFinal.git
cd LibraryFinal
```

Có thể dùng **GitHub Desktop → File → Clone repository** thay cho lệnh Git.

### Bước 2 – Tạo cơ sở dữ liệu

1. Mở **SSMS** và kết nối vào instance SQL Server dự định chạy project, ví dụ:
   - **Server name:** `(localdb)\MSSQLLocalDB`
   - **Authentication:** Windows Authentication
2. Mở và chạy **`Database/01_LibraryManagement_Schema.sql`** bằng **Execute (F5)** để tạo database **`LibraryManagement`** và 9 bảng.
3. Sau khi Schema thành công, mở và chạy **`Database/02_LibraryManagement_Seed.sql`** để thêm dữ liệu demo.
4. Kiểm tra database **`LibraryManagement` → Tables** đã có 9 bảng và tab **Messages** không có lỗi.

**Thứ tự bắt buộc: `01_LibraryManagement_Schema.sql` → `02_LibraryManagement_Seed.sql`.** Chỉ chạy Schema trên môi trường chưa có các bảng tương ứng; **không chạy lại Schema trên database đang chứa dữ liệu**. Nên sao lưu trước khi thao tác trên CSDL đã sử dụng.

Trên **database mới**, Seed cung cấp dữ liệu ban đầu:

| Đối tượng | Số lượng |
|---|---:|
| Thể loại (`Category`) | 12 |
| Tác giả (`Author`) | 28 |
| Sách (`Book`) | 34 |
| Thành viên (`Member`) | 8 |
| Tài khoản (`Account`) | 4 |
| Phiếu mượn (`Loan`) | 4 |
| Chi tiết phiếu (`LoanDetail`) | 7 |
| Đặt trước (`Reservation`) | 3 |

> Các mốc ngày trong dữ liệu mượn – trả mẫu được tính từ ngày chạy Seed, vì vậy trạng thái quá hạn phụ thuộc vào thời điểm kiểm thử. Số lượng dữ liệu và tồn kho có thể thay đổi sau khi thao tác trên website.

### Bước 3 – Kiểm tra kết nối trong `Web.config`

Tìm phần `<connectionStrings>` trong file **`Web.config`** của ứng dụng web. Tên connection string phải là **`LibraryManagementEntities`** và cấu hình SQL Server phải trỏ đúng database vừa tạo:

```xml
<connectionStrings>
  <add name="LibraryManagementEntities"
       connectionString="metadata=res://*/Models.Model1.csdl|res://*/Models.Model1.ssdl|res://*/Models.Model1.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=(localdb)\MSSQLLocalDB;initial catalog=LibraryManagement;integrated security=True;MultipleActiveResultSets=True;App=EntityFramework&quot;"
       providerName="System.Data.EntityClient" />
</connectionStrings>
```

Nếu máy dùng SQL Server instance khác, chỉ điều chỉnh `data source` cho phù hợp. **Không đổi tên `LibraryManagementEntities` hoặc các đường dẫn metadata EDMX** khi chỉ chuyển máy.

### Bước 4 – Mở và chạy ứng dụng

1. Mở file **`.sln`** ở thư mục gốc repository bằng Visual Studio 2022.
2. Nếu Visual Studio yêu cầu, thực hiện **Restore NuGet Packages**.
3. Chọn **Build → Rebuild Solution**.
4. Nhấn **`Ctrl + F5`** để chạy website trên localhost.
5. Mở trang **Sách** để kiểm tra dữ liệu và đăng nhập bằng tài khoản demo bên dưới.

Cổng localhost do Visual Studio/IIS Express cấp trên từng máy có thể khác nhau.

## 6. Tài khoản dùng thử

| Username | Mật khẩu | Vai trò |
|---|---|---|
| `demo_admin` | `BookNestDemo@2026!` | Admin |
| `demo_librarian` | `BookNestDemo@2026!` | Librarian |
| `demo_reader01` | `BookNestDemo@2026!` | Reader |
| `demo_reader02` | `BookNestDemo@2026!` | Reader |

Hai tài khoản Reader trong bộ Seed đã được liên kết với bản ghi `Member` tương ứng. Mật khẩu demo được lưu trong SQL dưới dạng hash **PBKDF2-HMAC-SHA256 (600.000 vòng lặp)**, không lưu dạng plaintext.

**Lưu ý bảo mật:** Những tài khoản và mật khẩu này chỉ dùng để demo trên máy local; không dùng cho môi trường triển khai thực tế.

## 7. Cấu trúc repository

```text
LibraryFinal/
├── Database/
│   ├── 01_LibraryManagement_Schema.sql
│   └── 02_LibraryManagement_Seed.sql
├── Library/
│   ├── Controllers/
│   │   └── LoanHistoryController.cs
│   ├── Models/              # Entity Framework và các model
│   ├── Views/               # Giao diện Razor
│   │   ├── LoanHistory/
│   │   └── Shared/LibraryLayout.cshtml
│   ├── Filters/             # Kiểm soát quyền truy cập
│   ├── Content/
│   │   ├── library.css      # CSS dùng chung
│   │   └── images/          # Ảnh bìa sách
│   └── Web.config
├── *.sln                    # File solution Visual Studio
└── README.md
```

## 8. Lưu ý về CSDL và ảnh sách

- SQL Server có **9 bảng vật lý**: `Account`, `Author`, `Book`, `Book_Author`, `Category`, `Loan`, `LoanDetail`, `Member`, `Reservation`.
- Trong sơ đồ **EDMX**, `Book_Author` được Entity Framework ánh xạ thành quan hệ nhiều–nhiều giữa `Book` và `Author`, nên có thể chỉ thấy **8 Entity**; đây không phải lỗi thiếu bảng SQL.
- Repository có **26 ảnh bìa demo**: `seed-book-01.jpg` đến `seed-book-26.jpg`, nằm trong `Library/Content/images/`.
- **8 ảnh bìa từ dữ liệu gốc** (`book-01.jpg` đến `book-08.jpg`) hiện chưa được bổ sung đầy đủ; một số sách có thể dùng hình thay thế/không hiển thị ảnh. Việc này không ảnh hưởng đến dữ liệu SQL hay các chức năng CRUD.
- Bộ Seed có các tình huống mẫu: thành viên bị khóa, thẻ hết hạn, sách hết tồn, phiếu đang mượn, đã trả và quá hạn.

## 9. Một số lỗi thường gặp

| Hiện tượng | Cách kiểm tra |
|---|---|
| `Cannot open database "LibraryManagement"` | Kiểm tra đã chạy Schema và đúng SQL Server instance trong `Web.config`. |
| `Invalid column name 'MemberID'` | Kiểm tra đang dùng Schema mới trong `Database/`, không phải script gốc cũ. |
| Lỗi metadata `Model1.csdl/.ssdl/.msl` | Kiểm tra phần metadata của connection string và các file `.edmx` trong project. |
| Không hiển thị ảnh bìa | Kiểm tra file ảnh trong `Library/Content/images/` và tên file ở `Book.ImageUrl`. |
| Không đăng nhập được tài khoản demo | Kiểm tra đã chạy Seed trên đúng database mà website kết nối. |
| `CREATE TABLE` báo đối tượng đã tồn tại | Không chạy lại Schema trên database đã được khởi tạo. |

## 10. Kiểm thử nhanh sau khi cài đặt

| Tình huống | Kết quả mong đợi |
|---|---|
| Đăng nhập với tài khoản demo | Hiển thị menu đúng vai trò |
| Admin/Thủ thư chọn lịch sử tại trang Thành viên | Hiển thị phiếu của thành viên được chọn |
| Reader mở `/LoanHistory` | Chỉ hiển thị phiếu của bản thân (có thể chưa có phiếu) |
| Reader truyền `memberId` của thành viên khác | HTTP 403 |
| Khách mở `/LoanHistory` | Yêu cầu đăng nhập |
| Librarian truy cập `/Users` | Bị từ chối |
| Tạo phiếu hợp lệ | Tồn kho giảm theo số lượng mượn |
| Trả đúng hạn | Tồn kho được hoàn lại, tiền phạt bằng 0 |
| Trả quá hạn | Tính tiền phạt 5.000 VNĐ/ngày/cuốn |
| Thu nhỏ cửa sổ trình duyệt | Menu có thể thu gọn và mở lại |

Đây là checklist kiểm thử kỳ vọng cho bản cài đặt mới, không phải cam kết rằng mọi trường hợp đã được kiểm thử tự động. Nên dùng database riêng để thử các thao tác thay đổi dữ liệu.

## 11. Quy trình quản lý mã nguồn

- Phát triển tính năng trên các nhánh `feature/*`.
- Tích hợp và kiểm thử trên `test/merge-all` trước khi đưa phiên bản ổn định vào `main`.
- Không commit mật khẩu thật, file backup `.bak`, dữ liệu cá nhân thật hoặc các file cục bộ như `.mdf`/`.ldf`.

---

**BookNest – Đồ án Lập trình Web | HUFLIT | 2026**

Cập nhật tài liệu: 24/11/2026 – bổ sung lịch sử mượn cá nhân và đồng bộ giao diện.
