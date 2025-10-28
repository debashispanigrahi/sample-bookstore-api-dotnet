-- Create Books table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Books]') AND type in (N'U'))
BEGIN
    CREATE TABLE [Books] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Title] NVARCHAR(500) NOT NULL,
        [Author] NVARCHAR(300) NOT NULL,
        [Isbn] NVARCHAR(50) NOT NULL,
        [Price] DECIMAL(10,2) NOT NULL,
        [PublishedDate] DATE NOT NULL,
        [Genre] NVARCHAR(100),
        [InStock] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Created Books table';
END
ELSE
BEGIN
    PRINT 'Books table already exists';
END
GO

-- Create Users table if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[Users]') AND type in (N'U'))
BEGIN
    CREATE TABLE [Users] (
        [Id] INT PRIMARY KEY IDENTITY(1,1),
        [Username] NVARCHAR(100) NOT NULL UNIQUE,
        [Email] NVARCHAR(255) NOT NULL UNIQUE,
        [PasswordHash] NVARCHAR(500) NOT NULL,
        [Salt] NVARCHAR(500) NOT NULL,
        [Role] NVARCHAR(50) NOT NULL DEFAULT 'User',
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LastLoginAt] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1
    );
    PRINT 'Created Users table';
END
ELSE
BEGIN
    PRINT 'Users table already exists';
END