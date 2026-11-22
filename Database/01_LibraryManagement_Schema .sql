-- ============================================================================
-- BOOKNEST - LIBRARY MANAGEMENT
-- 01_LibraryManagement_Schema.sql
--
-- Cấu trúc từ database LibraryManagement (SSMS export: 22/11/2026).
-- Portable schema cho SQL Server / SQL Server LocalDB.
-- Chạy trong SSMS trên DATABASE MỚI. Không chạy lên DB đang có bảng/dữ liệu.
-- Schema-only: không bao gồm INSERT dữ liệu mẫu, mật khẩu hay ảnh bìa.
-- ============================================================================

USE [master];
GO

-- Để SQL Server tự chọn thư mục MDF/LDF mặc định của từng máy.
IF DB_ID(N'LibraryManagement') IS NULL
    EXEC(N'CREATE DATABASE [LibraryManagement]');
GO

USE [LibraryManagement];
GO

-- Table: [dbo].[Account]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Account](
	[AccountID] [int] IDENTITY(1,1) NOT NULL,
	[Username] [varchar](50) NOT NULL,
	[Password] [varchar](255) NOT NULL,
	[Role] [nvarchar](50) NOT NULL,
	[MemberID] [int] NULL,
PRIMARY KEY CLUSTERED 
(
	[AccountID] ASC
) ON [PRIMARY],
UNIQUE NONCLUSTERED 
(
	[Username] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Table: [dbo].[Author]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Author](
	[AuthorID] [int] IDENTITY(1,1) NOT NULL,
	[AuthorName] [nvarchar](100) NOT NULL,
	[ImageUrl] [nvarchar](255) NULL,
PRIMARY KEY CLUSTERED 
(
	[AuthorID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Table: [dbo].[Book]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Book](
	[BookID] [int] IDENTITY(1,1) NOT NULL,
	[Title] [nvarchar](200) NOT NULL,
	[CategoryID] [int] NOT NULL,
	[AuthorID] [int] NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[StockQuantity] [int] NOT NULL,
	[ImageUrl] [varchar](500) NULL,
	[Description] [nvarchar](max) NULL,
	[IsActive] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[BookID] ASC
) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
-- Table: [dbo].[Book_Author]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Book_Author](
	[BookID] [int] NOT NULL,
	[AuthorID] [int] NOT NULL,
 CONSTRAINT [PK_Book_Author] PRIMARY KEY CLUSTERED 
(
	[BookID] ASC,
	[AuthorID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Table: [dbo].[Category]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Category](
	[CategoryID] [int] IDENTITY(1,1) NOT NULL,
	[CategoryName] [nvarchar](100) NOT NULL,
PRIMARY KEY CLUSTERED 
(
	[CategoryID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Table: [dbo].[Loan]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Loan](
	[LoanID] [int] IDENTITY(1,1) NOT NULL,
	[MemberID] [int] NOT NULL,
	[LoanDate] [date] NOT NULL,
	[DueDate] [date] NOT NULL,
	[ReturnDate] [date] NULL,
	[TotalFine] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[LoanID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Table: [dbo].[LoanDetail]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoanDetail](
	[LoanDetailID] [int] IDENTITY(1,1) NOT NULL,
	[LoanID] [int] NOT NULL,
	[BookID] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[FineAmount] [decimal](18, 2) NULL,
PRIMARY KEY CLUSTERED 
(
	[LoanDetailID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Table: [dbo].[Member]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Member](
	[MemberID] [int] IDENTITY(1,1) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Email] [varchar](100) NULL,
	[Phone] [varchar](20) NULL,
	[Address] [nvarchar](255) NULL,
	[CardExpiryDate] [date] NULL,
	[IsLocked] [bit] NULL,
PRIMARY KEY CLUSTERED 
(
	[MemberID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Table: [dbo].[Reservation]
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Reservation](
	[ReserveID] [int] IDENTITY(1,1) NOT NULL,
	[MemberID] [int] NOT NULL,
	[BookID] [int] NOT NULL,
	[ReserveDate] [datetime] NULL,
	[Status] [nvarchar](50) NULL,
PRIMARY KEY CLUSTERED 
(
	[ReserveID] ASC
) ON [PRIMARY]
) ON [PRIMARY]
GO
-- Index: [UX_Account_MemberID]
CREATE UNIQUE NONCLUSTERED INDEX [UX_Account_MemberID] ON [dbo].[Account]
(
	[MemberID] ASC
)
WHERE ([MemberID] IS NOT NULL)
ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
-- Index: [UX_Member_Email]
CREATE UNIQUE NONCLUSTERED INDEX [UX_Member_Email] ON [dbo].[Member]
(
	[Email] ASC
)
WHERE ([Email] IS NOT NULL)
ON [PRIMARY]
GO
ALTER TABLE [dbo].[Book] ADD  DEFAULT ((1)) FOR [IsActive]
GO
ALTER TABLE [dbo].[Loan] ADD  DEFAULT (getdate()) FOR [LoanDate]
GO
ALTER TABLE [dbo].[Loan] ADD  DEFAULT ((0)) FOR [TotalFine]
GO
ALTER TABLE [dbo].[LoanDetail] ADD  DEFAULT ((0)) FOR [FineAmount]
GO
ALTER TABLE [dbo].[Member] ADD  DEFAULT ((0)) FOR [IsLocked]
GO
ALTER TABLE [dbo].[Reservation] ADD  DEFAULT (getdate()) FOR [ReserveDate]
GO
ALTER TABLE [dbo].[Reservation] ADD  DEFAULT (N'Chờ xử lý') FOR [Status]
GO
ALTER TABLE [dbo].[Account]  WITH CHECK ADD  CONSTRAINT [FK_Account_Member] FOREIGN KEY([MemberID])
REFERENCES [dbo].[Member] ([MemberID])
GO
ALTER TABLE [dbo].[Account] CHECK CONSTRAINT [FK_Account_Member]
GO
ALTER TABLE [dbo].[Book]  WITH CHECK ADD  CONSTRAINT [FK_Book_Author] FOREIGN KEY([AuthorID])
REFERENCES [dbo].[Author] ([AuthorID])
GO
ALTER TABLE [dbo].[Book] CHECK CONSTRAINT [FK_Book_Author]
GO
ALTER TABLE [dbo].[Book]  WITH CHECK ADD  CONSTRAINT [FK_Book_Category] FOREIGN KEY([CategoryID])
REFERENCES [dbo].[Category] ([CategoryID])
GO
ALTER TABLE [dbo].[Book] CHECK CONSTRAINT [FK_Book_Category]
GO
ALTER TABLE [dbo].[Book_Author]  WITH CHECK ADD  CONSTRAINT [FK_BookAuthor_Author] FOREIGN KEY([AuthorID])
REFERENCES [dbo].[Author] ([AuthorID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Book_Author] CHECK CONSTRAINT [FK_BookAuthor_Author]
GO
ALTER TABLE [dbo].[Book_Author]  WITH CHECK ADD  CONSTRAINT [FK_BookAuthor_Book] FOREIGN KEY([BookID])
REFERENCES [dbo].[Book] ([BookID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Book_Author] CHECK CONSTRAINT [FK_BookAuthor_Book]
GO
ALTER TABLE [dbo].[Loan]  WITH CHECK ADD  CONSTRAINT [FK_Loan_Member] FOREIGN KEY([MemberID])
REFERENCES [dbo].[Member] ([MemberID])
GO
ALTER TABLE [dbo].[Loan] CHECK CONSTRAINT [FK_Loan_Member]
GO
ALTER TABLE [dbo].[LoanDetail]  WITH CHECK ADD  CONSTRAINT [FK_LoanDetail_Book] FOREIGN KEY([BookID])
REFERENCES [dbo].[Book] ([BookID])
GO
ALTER TABLE [dbo].[LoanDetail] CHECK CONSTRAINT [FK_LoanDetail_Book]
GO
ALTER TABLE [dbo].[LoanDetail]  WITH CHECK ADD  CONSTRAINT [FK_LoanDetail_Loan] FOREIGN KEY([LoanID])
REFERENCES [dbo].[Loan] ([LoanID])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[LoanDetail] CHECK CONSTRAINT [FK_LoanDetail_Loan]
GO
ALTER TABLE [dbo].[Reservation]  WITH CHECK ADD  CONSTRAINT [FK_Reservation_Book] FOREIGN KEY([BookID])
REFERENCES [dbo].[Book] ([BookID])
GO
ALTER TABLE [dbo].[Reservation] CHECK CONSTRAINT [FK_Reservation_Book]
GO
ALTER TABLE [dbo].[Reservation]  WITH CHECK ADD  CONSTRAINT [FK_Reservation_Member] FOREIGN KEY([MemberID])
REFERENCES [dbo].[Member] ([MemberID])
GO
ALTER TABLE [dbo].[Reservation] CHECK CONSTRAINT [FK_Reservation_Member]
GO
ALTER TABLE [dbo].[Book]  WITH CHECK ADD CHECK  (([Price]>=(0)))
GO
ALTER TABLE [dbo].[Book]  WITH CHECK ADD CHECK  (([StockQuantity]>=(0)))
GO
ALTER TABLE [dbo].[Loan]  WITH CHECK ADD  CONSTRAINT [CHK_DueDate] CHECK  (([DueDate]>=[LoanDate]))
GO
ALTER TABLE [dbo].[Loan] CHECK CONSTRAINT [CHK_DueDate]
GO
ALTER TABLE [dbo].[Loan]  WITH CHECK ADD  CONSTRAINT [CHK_ReturnDate] CHECK  (([ReturnDate] IS NULL OR [ReturnDate]>=[LoanDate]))
GO
ALTER TABLE [dbo].[Loan] CHECK CONSTRAINT [CHK_ReturnDate]
GO
ALTER TABLE [dbo].[Loan]  WITH CHECK ADD CHECK  (([TotalFine]>=(0)))
GO
ALTER TABLE [dbo].[LoanDetail]  WITH CHECK ADD CHECK  (([FineAmount]>=(0)))
GO
ALTER TABLE [dbo].[LoanDetail]  WITH CHECK ADD CHECK  (([Quantity]>(0)))
GO


-- Kiểm tra nhanh số bảng sau khi script chạy xong (mong đợi: 9).
SELECT COUNT(*) AS TotalTables
FROM sys.tables
WHERE schema_id = SCHEMA_ID(N'dbo')
  AND name IN (N'Account', N'Author', N'Book', N'Book_Author',
               N'Category', N'Loan', N'LoanDetail', N'Member', N'Reservation');
GO
