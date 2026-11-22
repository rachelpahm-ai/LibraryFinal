/*
 BookNest | 02_LibraryManagement_Seed.sql
 SEED: 8 cuốn gốc + 26 cuốn bổ sung, 28 tác giả, 12 thể loại, 8 thành viên,
 4 tài khoản demo, phiếu mượn/trả và đặt trước.
 Yêu cầu: đã chạy 01_LibraryManagement_Schema.sql trên database mới LibraryManagement.
 KHÔNG DROP/DELETE; có TRANSACTION + TRY/CATCH; chạy lại không chèn trùng.
 Tài khoản demo dùng PBKDF2-HMAC-SHA256 (600000 rounds), chỉ dùng ở LOCAL DEV.
 Ảnh bổ sung: Content/images/seed-book-01.jpg ... seed-book-26.jpg.
 Ảnh 8 sách gốc: book-01.jpg ... book-08.jpg (phải có trong project).
*/
USE [LibraryManagement];
GO
SET NOCOUNT ON;
SET XACT_ABORT ON;

-- Dừng an toàn nếu schema DB chưa phù hợp với phiên bản BookNest hiện tại.
IF OBJECT_ID(N'dbo.Account',N'U') IS NULL OR
   OBJECT_ID(N'dbo.Member',N'U') IS NULL OR
   OBJECT_ID(N'dbo.Book',N'U') IS NULL OR
   OBJECT_ID(N'dbo.Book_Author',N'U') IS NULL OR
   OBJECT_ID(N'dbo.Loan',N'U') IS NULL OR
   OBJECT_ID(N'dbo.LoanDetail',N'U') IS NULL OR
   OBJECT_ID(N'dbo.Reservation',N'U') IS NULL OR
   OBJECT_ID(N'dbo.Author',N'U') IS NULL OR
   OBJECT_ID(N'dbo.Category',N'U') IS NULL
    THROW 51000, N'Thieu bang: can khoi tao du 9 bang cua LibraryManagement truoc khi seed.', 1;
IF COL_LENGTH(N'dbo.Account',N'MemberID') IS NULL
    THROW 51001, N'Account chua co MemberID. Hay chay schema moi nhat 01_LibraryManagement_Schema.sql.', 1;
GO

BEGIN TRY
    BEGIN TRANSACTION;

    -- 1. THE LOAI - noi voi ten, khong dua vao identity ID co dinh
    DECLARE @Categories TABLE (CategoryName NVARCHAR(100) PRIMARY KEY);
    INSERT INTO @Categories VALUES
(N'Lập trình'),
(N'Kinh tế'),
(N'Văn học'),
(N'Kỹ năng sống'),
(N'Khoa học'),
(N'Thiếu nhi'),
(N'Tiểu thuyết'),
(N'Lịch sử'),
(N'Kỹ năng học tập'),
(N'Tâm lý học'),
(N'Khoa học dữ liệu'),
(N'Công nghệ thông tin');

    INSERT INTO dbo.Category (CategoryName)
    SELECT s.CategoryName FROM @Categories s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Category c WHERE c.CategoryName = s.CategoryName);

    -- 2. TAC GIA
    DECLARE @Authors TABLE (AuthorName NVARCHAR(100) PRIMARY KEY);
    INSERT INTO @Authors VALUES
(N'Nguyễn Nhật Ánh'),
(N'Robert C. Martin'),
(N'Dale Carnegie'),
(N'Stephen Hawking'),
(N'Adam Smith'),
(N'Tô Hoài'),
(N'Kim Lân'),
(N'Nam Cao'),
(N'Ngô Tất Tố'),
(N'Nguyễn Ngọc Tư'),
(N'J. K. Rowling'),
(N'George Orwell'),
(N'Paulo Coelho'),
(N'Haruki Murakami'),
(N'Yuval Noah Harari'),
(N'James Clear'),
(N'Cal Newport'),
(N'Daniel Kahneman'),
(N'Donella Meadows'),
(N'Eric Matthes'),
(N'Luciano Ramalho'),
(N'Martin Kleppmann'),
(N'Joe Reis'),
(N'Abraham Silberschatz'),
(N'Andrew Hunt'),
(N'Martin Fowler'),
(N'Peter Thiel'),
(N'Eric Ries');

    INSERT INTO dbo.Author (AuthorName)
    SELECT s.AuthorName FROM @Authors s
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Author a WHERE a.AuthorName = s.AuthorName);

    -- 3. SACH: 8 sách gốc và 26 sách bổ sung (ghi tên tệp ảnh, không ghi absolute path)
    DECLARE @Books TABLE (
        Title NVARCHAR(200) PRIMARY KEY,
        CategoryName NVARCHAR(100), AuthorName NVARCHAR(100),
        Price DECIMAL(18,2), StockQuantity INT,
        ImageUrl VARCHAR(500), Description NVARCHAR(MAX), IsActive BIT
    );
    INSERT INTO @Books (Title,CategoryName,AuthorName,Price,StockQuantity,ImageUrl,Description,IsActive) VALUES
-- 8 cuốn sách từ LibraryManagement.sql ban đầu (không phụ thuộc CategoryID/AuthorID cố định)
(N'Clean Code',N'Lập trình',N'Robert C. Martin',450000,10,'book-01.jpg',N'Handbook of Agile Software Craftsmanship',1),
(N'Cho Tôi Xin Một Vé Đi Tuổi Thơ',N'Văn học',N'Nguyễn Nhật Ánh',85000,15,'book-02.jpg',N'Tác phẩm nổi tiếng viết về tuổi thơ của Nguyễn Nhật Ánh',1),
(N'Đắc Nhân Tâm',N'Kỹ năng sống',N'Dale Carnegie',98000,20,'book-03.jpg',N'Sách kỹ năng sống và nghệ thuật thu phục lòng người',1),
(N'Lược Sử Thời Gian',N'Khoa học',N'Stephen Hawking',150000,8,'book-04.jpg',N'Cuốn sách kinh điển về vũ trụ học của Stephen Hawking',1),
(N'Của Cải Của Các Dân Tộc',N'Kinh tế',N'Adam Smith',220000,5,'book-05.jpg',N'Nền tảng kinh tế học cổ điển của Adam Smith',1),
(N'Mắt Biếc',N'Văn học',N'Nguyễn Nhật Ánh',110000,12,'book-06.jpg',N'Câu chuyện tình yêu đơn phương đầy day dứt',1),
(N'The Clean Coder',N'Lập trình',N'Robert C. Martin',480000,7,'book-07.jpg',N'A Code of Conduct for Professional Programmers',1),
(N'Quẳng Gánh Lo Đi Mà Vui Sống',N'Kỹ năng sống',N'Dale Carnegie',89000,18,'book-08.jpg',N'Bí quyết để vượt qua lo lắng và tận hưởng cuộc sống',1),
(N'Dế Mèn Phiêu Lưu Ký',N'Thiếu nhi',N'Tô Hoài',92000,8,'seed-book-01.jpg',N'Cuộc phiêu lưu và trưởng thành của Dế Mèn qua những miền đất mới, với nhiều bài học về tình bạn và trách nhiệm.',1),
(N'Vợ Nhặt',N'Văn học',N'Kim Lân',79000,5,'seed-book-02.jpg',N'Truyện ngắn khắc họa tình người, lòng nhân ái và khát vọng sống trong nạn đói năm 1945.',1),
(N'Chí Phèo',N'Văn học',N'Nam Cao',88000,6,'seed-book-03.jpg',N'Bi kịch của một con người bị tha hóa và mong muốn được sống lương thiện giữa xã hội cũ.',1),
(N'Tắt Đèn',N'Văn học',N'Ngô Tất Tố',85000,7,'seed-book-04.jpg',N'Phản ánh cuộc sống cơ cực của người nông dân và sức phản kháng của chị Dậu trước sưu thuế.',1),
(N'Cánh Đồng Bất Tận',N'Văn học',N'Nguyễn Ngọc Tư',125000,4,'seed-book-05.jpg',N'Những phận người trên miền sông nước Nam Bộ qua câu chuyện về gia đình, cô đơn và sự cảm thông.',1),
(N'Tôi Thấy Hoa Vàng Trên Cỏ Xanh',N'Văn học',N'Nguyễn Nhật Ánh',115000,11,'seed-book-06.jpg',N'Câu chuyện tuổi thơ ở làng quê, khơi gợi tình anh em, tình bạn và những rung động đầu đời.',1),
(N'Harry Potter và Hòn Đá Phù Thủy',N'Thiếu nhi',N'J. K. Rowling',185000,6,'seed-book-07.jpg',N'Harry khám phá thế giới phù thủy, tình bạn và những bí ẩn trong năm đầu tiên tại Hogwarts.',1),
(N'1984',N'Tiểu thuyết',N'George Orwell',139000,3,'seed-book-08.jpg',N'Tiểu thuyết phản địa đàng về giám sát, kiểm soát tư tưởng và quyền tự do cá nhân.',1),
(N'Nhà Giả Kim',N'Tiểu thuyết',N'Paulo Coelho',99000,0,'seed-book-09.jpg',N'Hành trình theo đuổi ước mơ và khám phá ý nghĩa cuộc sống của chàng chăn cừu Santiago.',1),
(N'Rừng Na Uy',N'Tiểu thuyết',N'Haruki Murakami',165000,5,'seed-book-10.jpg',N'Câu chuyện về tuổi trẻ, ký ức, tình yêu và mất mát trong bối cảnh Nhật Bản những năm 1960.',1),
(N'Sapiens: Lược Sử Loài Người',N'Lịch sử',N'Yuval Noah Harari',249000,8,'seed-book-11.jpg',N'Khái quát lịch sử loài người qua các cuộc cách mạng nhận thức, nông nghiệp và khoa học.',1),
(N'Homo Deus',N'Lịch sử',N'Yuval Noah Harari',265000,4,'seed-book-12.jpg',N'Bàn về các khả năng và thách thức của nhân loại trước công nghệ, dữ liệu và trí tuệ nhân tạo.',1),
(N'Atomic Habits',N'Kỹ năng sống',N'James Clear',175000,9,'seed-book-13.jpg',N'Giới thiệu cách xây dựng thói quen tốt qua những thay đổi nhỏ, bền vững và có hệ thống.',1),
(N'Deep Work',N'Kỹ năng học tập',N'Cal Newport',189000,7,'seed-book-14.jpg',N'Trình bày phương pháp tập trung sâu để học và làm việc hiệu quả trong môi trường nhiều xao nhãng.',1),
(N'Tư Duy Nhanh Và Chậm',N'Tâm lý học',N'Daniel Kahneman',235000,6,'seed-book-15.jpg',N'Khám phá hai hệ thống tư duy và những thiên lệch thường gặp trong phán đoán, ra quyết định.',1),
(N'Thinking in Systems',N'Khoa học',N'Donella Meadows',215000,3,'seed-book-16.jpg',N'Giới thiệu tư duy hệ thống, vòng phản hồi và cách nhận diện những điểm đòn bẩy trong hệ phức tạp.',1),
(N'Python Crash Course',N'Lập trình',N'Eric Matthes',420000,10,'seed-book-17.jpg',N'Hướng dẫn thực hành Python từ cú pháp cơ bản đến các dự án lập trình ứng dụng.',1),
(N'Fluent Python',N'Lập trình',N'Luciano Ramalho',580000,5,'seed-book-18.jpg',N'Phân tích cách sử dụng hiệu quả các đặc trưng ngôn ngữ Python và cấu trúc dữ liệu.',1),
(N'Designing Data-Intensive Applications',N'Khoa học dữ liệu',N'Martin Kleppmann',650000,8,'seed-book-19.jpg',N'Tìm hiểu nền tảng thiết kế hệ thống dữ liệu tin cậy, mở rộng được và dễ bảo trì.',1),
(N'Fundamentals of Data Engineering',N'Khoa học dữ liệu',N'Joe Reis',560000,6,'seed-book-20.jpg',N'Tổng quan vòng đời kỹ thuật dữ liệu từ thu thập, lưu trữ, xử lý đến vận hành.',1),
(N'Database System Concepts',N'Công nghệ thông tin',N'Abraham Silberschatz',540000,3,'seed-book-21.jpg',N'Giới thiệu nguyên lý cơ sở dữ liệu, mô hình quan hệ, SQL, giao dịch và quản trị lưu trữ.',1),
(N'Operating System Concepts',N'Công nghệ thông tin',N'Abraham Silberschatz',590000,6,'seed-book-22.jpg',N'Giải thích tiến trình, luồng, đồng bộ, bộ nhớ và hệ thống tệp trong hệ điều hành.',1),
(N'The Pragmatic Programmer',N'Lập trình',N'Andrew Hunt',475000,4,'seed-book-23.jpg',N'Những nguyên tắc và thực hành giúp lập trình viên cải thiện chất lượng phần mềm và nghề nghiệp.',0),
(N'Refactoring',N'Lập trình',N'Martin Fowler',495000,5,'seed-book-24.jpg',N'Trình bày kỹ thuật tái cấu trúc mã nguồn nhằm cải thiện thiết kế mà không đổi hành vi hệ thống.',1),
(N'Zero to One',N'Kinh tế',N'Peter Thiel',169000,3,'seed-book-25.jpg',N'Những góc nhìn về khởi nghiệp, sáng tạo giá trị và xây dựng lợi thế cạnh tranh.',1),
(N'The Lean Startup',N'Kinh tế',N'Eric Ries',185000,7,'seed-book-26.jpg',N'Giới thiệu phương pháp thử nghiệm giả thuyết và học hỏi nhanh khi phát triển sản phẩm mới.',1);

    INSERT INTO dbo.Book (Title, CategoryID, AuthorID, Price, StockQuantity, ImageUrl, Description, IsActive)
    SELECT s.Title, c.CategoryID, a.AuthorID,
           s.Price, s.StockQuantity, s.ImageUrl, s.Description, s.IsActive
    FROM @Books s
    JOIN dbo.Category c ON c.CategoryName = s.CategoryName
    JOIN dbo.Author a ON a.AuthorName = s.AuthorName
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Book b WHERE b.Title = s.Title);

    -- Trên DB mới, mọi sách trong bộ seed phải tra được ID tác giả và thể loại.
    -- Nếu sách trùng tên đang tồn tại trên DB cũ, giữ nguyên dữ liệu cũ.
    IF EXISTS (
        SELECT 1 FROM @Books s
        LEFT JOIN dbo.Category c ON c.CategoryName = s.CategoryName
        LEFT JOIN dbo.Author a ON a.AuthorName = s.AuthorName
        WHERE c.CategoryID IS NULL OR a.AuthorID IS NULL
    ) THROW 51007, N'Ten the loai hoac tac gia cua sach seed khong ton tai.', 1;

    -- Duy tri Book_Author (nhieu-nhieu) tu Book.AuthorID, khong trung khoa chinh
    INSERT INTO dbo.Book_Author (BookID, AuthorID)
    SELECT b.BookID, b.AuthorID
    FROM dbo.Book b
    JOIN @Books s ON s.Title = b.Title
    WHERE NOT EXISTS (
        SELECT 1 FROM dbo.Book_Author ba
        WHERE ba.BookID = b.BookID AND ba.AuthorID = b.AuthorID
    );

    -- 4. THANH VIEN: 2 Reader + 6 tinh huong demo
    DECLARE @Members TABLE (
        FullName NVARCHAR(100), Email VARCHAR(100) PRIMARY KEY,
        Phone VARCHAR(20), Address NVARCHAR(255),
        ExpiryOffsetDays INT, IsLocked BIT
    );
    INSERT INTO @Members VALUES
(N'Độc giả An (Demo)','reader01.booknest@example.com','0900001101',N'TP.HCM',365,0),
(N'Độc giả Bình (Demo)','reader02.booknest@example.com','0900001102',N'TP.HCM',365,0),
(N'Nguyễn Minh Thư (Demo)','member.free.booknest@example.com','0900001103',N'TP.HCM',180,0),
(N'Trần Gia Huy (Demo)','member.locked.booknest@example.com','0900001104',N'TP.HCM',365,1),
(N'Lê Phương Linh (Demo)','member.expired.booknest@example.com','0900001105',N'TP.HCM',-30,0),
(N'Phạm Nhật Minh (Demo)','member.returned.booknest@example.com','0900001106',N'TP.HCM',180,0),
(N'Võ Ngọc Hà (Demo)','member.reserve.booknest@example.com','0900001107',N'TP.HCM',270,0),
(N'Hoàng Tú Anh (Demo)','member.ontime.booknest@example.com','0900001108',N'TP.HCM',270,0);

    INSERT INTO dbo.Member (FullName,Email,Phone,Address,CardExpiryDate,IsLocked)
    SELECT m.FullName,m.Email,m.Phone,m.Address,
           DATEADD(DAY,m.ExpiryOffsetDays,CONVERT(date,GETDATE())),m.IsLocked
    FROM @Members m
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Member x WHERE x.Email = m.Email);

    -- 5. TAI KHOAN (KHONG GHI DE MAT KHAU CU)
    -- PasswordHelper.VerifyPassword: PBKDF2-HMAC-SHA256, 600000 vong, salt16, hash32
    -- Password demo: xem README_DEMO.txt (khong phai plaintext trong SQL).
    DECLARE @Accounts TABLE (
        Username VARCHAR(50) PRIMARY KEY,
        PasswordHash VARCHAR(255), Role NVARCHAR(50), MemberEmail VARCHAR(100) NULL
    );
    INSERT INTO @Accounts VALUES
('demo_admin','PBKDF2-SHA256$600000$usPE1qHLpym4qSEihN0Wig==$aWxRh+MXniSHPIc92LiSL4L0YB1CXICnGk/G/zaBG+I=',N'Admin',NULL),
('demo_librarian','PBKDF2-SHA256$600000$rWwiWcojRWOOAaX9yAO0AA==$6AvvOB6pTb5RA10fTHC7SoQk5so1Qu48jUIQ/nW0HRg=',N'Librarian',NULL),
('demo_reader01','PBKDF2-SHA256$600000$hhXlglENnjF0E5g5Uyy0+Q==$cB4CZJYKxzoEWotZp8O9pxxw9fJEZNwaoU771nGpolg=',N'Reader','reader01.booknest@example.com'),
('demo_reader02','PBKDF2-SHA256$600000$ol6QOJ62JflYLXH5BTy/tA==$Rm38oOMeXoL5xkCRAzlA/dPWzilbFs1spuS3Ad9jDwU=',N'Reader','reader02.booknest@example.com');

    IF EXISTS (
        SELECT 1 FROM @Accounts a
        LEFT JOIN dbo.Member m ON m.Email = a.MemberEmail
        WHERE a.Role = N'Reader' AND m.MemberID IS NULL
    ) THROW 51002, N'Khong tim thay Member lien ket cho tai khoan Reader.', 1;

    INSERT INTO dbo.Account (Username,Password,Role,MemberID)
    SELECT a.Username,a.PasswordHash,a.Role,m.MemberID
    FROM @Accounts a
    LEFT JOIN dbo.Member m ON m.Email = a.MemberEmail
    WHERE NOT EXISTS (SELECT 1 FROM dbo.Account x WHERE x.Username = a.Username);

    -- 6. PHIEU MUON - tao theo MemberEmail, khong hardcode ID
    DECLARE @memberId INT, @bookId INT, @loanId INT;
    DECLARE @today DATE = CONVERT(date,GETDATE());

    -- 6A: Reader01 dang muon 4 quyen, chua qua han (test gioi han 5 quyen)
    SELECT @memberId = MemberID FROM dbo.Member WHERE Email='reader01.booknest@example.com';
    IF NOT EXISTS (SELECT 1 FROM dbo.Loan WHERE MemberID=@memberId)
    BEGIN
        INSERT dbo.Loan (MemberID,LoanDate,DueDate,ReturnDate,TotalFine)
        VALUES (@memberId,DATEADD(DAY,-4,@today),DATEADD(DAY,10,@today),NULL,0);
        SET @loanId = CONVERT(int,SCOPE_IDENTITY());

        DECLARE @BorrowBooks TABLE (Title NVARCHAR(200) PRIMARY KEY);
        INSERT @BorrowBooks VALUES
        (N'Python Crash Course'),(N'Atomic Habits'),(N'Dế Mèn Phiêu Lưu Ký'),(N'1984');

        IF (SELECT COUNT(*) FROM dbo.Book b JOIN @BorrowBooks x ON x.Title=b.Title WHERE b.StockQuantity >=1) <> 4
            THROW 51003, N'Khong du 4 sach de tao phieu Reader01.', 1;

        INSERT dbo.LoanDetail (LoanID,BookID,Quantity,FineAmount)
        SELECT @loanId,b.BookID,1,0 FROM dbo.Book b JOIN @BorrowBooks x ON x.Title=b.Title;

        UPDATE b SET b.StockQuantity=b.StockQuantity-1
        FROM dbo.Book b JOIN @BorrowBooks x ON x.Title=b.Title;
    END

    -- 6B: Reader02 co phieu qua han 4 ngay (test CHAN MUON MOI)
    SELECT @memberId = MemberID FROM dbo.Member WHERE Email='reader02.booknest@example.com';
    IF NOT EXISTS (SELECT 1 FROM dbo.Loan WHERE MemberID=@memberId)
    BEGIN
        SET @bookId = NULL;
        SELECT @bookId=BookID FROM dbo.Book WHERE Title=N'Sapiens: Lược Sử Loài Người';
        IF @bookId IS NULL OR (SELECT StockQuantity FROM dbo.Book WHERE BookID=@bookId) < 1
            THROW 51004, N'Khong du sach cho phieu Reader02.',1;

        INSERT dbo.Loan (MemberID,LoanDate,DueDate,ReturnDate,TotalFine)
        VALUES (@memberId,DATEADD(DAY,-18,@today),DATEADD(DAY,-4,@today),NULL,0);
        SET @loanId = CONVERT(int,SCOPE_IDENTITY());
        INSERT dbo.LoanDetail (LoanID,BookID,Quantity,FineAmount)
        VALUES (@loanId,@bookId,1,0);
        UPDATE dbo.Book SET StockQuantity=StockQuantity-1 WHERE BookID=@bookId;
    END

    -- 6C: Phiếu đã trả trễ 3 ngày -> 1 sach x 3 ngay x 5000 = 15000
    SELECT @memberId = MemberID FROM dbo.Member WHERE Email='member.returned.booknest@example.com';
    IF NOT EXISTS (SELECT 1 FROM dbo.Loan WHERE MemberID=@memberId)
    BEGIN
        SET @bookId = NULL;
        SELECT @bookId=BookID FROM dbo.Book WHERE Title=N'Tư Duy Nhanh Và Chậm';
        IF @bookId IS NULL THROW 51005, N'Khong tim thay sach tra tre.',1;
        INSERT dbo.Loan (MemberID,LoanDate,DueDate,ReturnDate,TotalFine)
        VALUES (@memberId,DATEADD(DAY,-26,@today),DATEADD(DAY,-12,@today),DATEADD(DAY,-9,@today),15000);
        SET @loanId=CONVERT(int,SCOPE_IDENTITY());
        INSERT dbo.LoanDetail (LoanID,BookID,Quantity,FineAmount)
        VALUES (@loanId,@bookId,1,15000);
        -- Phieu da tra: ton kho HIEN TAI khong duoc tru them.
    END

    -- 6D: Phiếu đã trả đúng hạn
    SELECT @memberId=MemberID FROM dbo.Member WHERE Email='member.ontime.booknest@example.com';
    IF NOT EXISTS (SELECT 1 FROM dbo.Loan WHERE MemberID=@memberId)
    BEGIN
        SET @bookId = NULL;
        SELECT @bookId=BookID FROM dbo.Book WHERE Title=N'Nhà Giả Kim';
        IF @bookId IS NULL THROW 51006, N'Khong tim thay sach tra dung han.',1;
        INSERT dbo.Loan (MemberID,LoanDate,DueDate,ReturnDate,TotalFine)
        VALUES (@memberId,DATEADD(DAY,-20,@today),DATEADD(DAY,-6,@today),DATEADD(DAY,-10,@today),0);
        SET @loanId=CONVERT(int,SCOPE_IDENTITY());
        INSERT dbo.LoanDetail (LoanID,BookID,Quantity,FineAmount)
        VALUES (@loanId,@bookId,1,0);
    END

    -- 7. DAT TRUOC: dung Status tieng Viet dung voi controller
    DECLARE @Reservations TABLE (MemberEmail VARCHAR(100),BookTitle NVARCHAR(200),Status NVARCHAR(50),DaysAgo INT);
    INSERT @Reservations VALUES
    ('member.reserve.booknest@example.com',N'Nhà Giả Kim',N'Chờ',2),
    ('member.free.booknest@example.com',N'Atomic Habits',N'Đã nhận',8),
    ('member.expired.booknest@example.com',N'1984',N'Hủy',5);

    INSERT INTO dbo.Reservation (MemberID,BookID,ReserveDate,Status)
    SELECT m.MemberID,b.BookID,DATEADD(DAY,-r.DaysAgo,GETDATE()),r.Status
    FROM @Reservations r
    JOIN dbo.Member m ON m.Email=r.MemberEmail
    JOIN dbo.Book b ON b.Title=r.BookTitle
    WHERE NOT EXISTS (
       SELECT 1 FROM dbo.Reservation x
       WHERE x.MemberID=m.MemberID AND x.BookID=b.BookID
    );

    COMMIT TRANSACTION;
    PRINT N'[BookNest SEED] Thanh cong. Da them du lieu mau (chi chen neu chua co).';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION;
    PRINT N'[BookNest SEED] That bai. Tat ca thay doi trong lan chay nay da rollback.';
    THROW;
END CATCH;
GO

-- 8. KIEM TRA NHANH
SELECT N'Category' AS TableName, COUNT(*) AS Total FROM dbo.Category
UNION ALL SELECT N'Author',COUNT(*) FROM dbo.Author
UNION ALL SELECT N'Book',COUNT(*) FROM dbo.Book
UNION ALL SELECT N'Member',COUNT(*) FROM dbo.Member
UNION ALL SELECT N'Account',COUNT(*) FROM dbo.Account
UNION ALL SELECT N'Loan',COUNT(*) FROM dbo.Loan
UNION ALL SELECT N'LoanDetail',COUNT(*) FROM dbo.LoanDetail
UNION ALL SELECT N'Reservation',COUNT(*) FROM dbo.Reservation;

SELECT Username, Role, MemberID FROM dbo.Account
WHERE Username IN ('demo_admin','demo_librarian','demo_reader01','demo_reader02')
ORDER BY Username;

SELECT m.FullName, l.LoanID, l.LoanDate,l.DueDate,l.ReturnDate,l.TotalFine,
       CASE WHEN l.ReturnDate IS NULL AND l.DueDate < CONVERT(date,GETDATE())
         THEN N'Quá hạn' WHEN l.ReturnDate IS NULL THEN N'Đang mượn'
         ELSE N'Đã trả' END AS LoanState
FROM dbo.Loan l JOIN dbo.Member m ON m.MemberID=l.MemberID
WHERE m.Email LIKE '%.booknest@example.com'
ORDER BY l.LoanID;
