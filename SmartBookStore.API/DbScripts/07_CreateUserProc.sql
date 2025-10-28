CREATE OR ALTER PROCEDURE usp_CreateUser
    @Username NVARCHAR(100),
    @Email NVARCHAR(255),
    @PasswordHash NVARCHAR(500),
    @Salt NVARCHAR(500),
    @Role NVARCHAR(50),
    @CreatedAt DATETIME2,
    @NewId INT OUTPUT
AS
BEGIN
    -- Check if username already exists
    IF EXISTS (SELECT 1 FROM Users WHERE Username = @Username)
    BEGIN
        RAISERROR ('Username already exists', 16, 1);
        RETURN;
    END

    -- Check if email already exists
    IF EXISTS (SELECT 1 FROM Users WHERE Email = @Email)
    BEGIN
        RAISERROR ('Email already exists', 16, 1);
        RETURN;
    END

    INSERT INTO Users (Username, Email, PasswordHash, Salt, Role, CreatedAt, IsActive)
    VALUES (@Username, @Email, @PasswordHash, @Salt, @Role, @CreatedAt, 1);
    
    SET @NewId = SCOPE_IDENTITY();
    PRINT 'User created successfully';
END