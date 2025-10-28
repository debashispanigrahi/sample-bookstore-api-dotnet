-- Create default admin user if not exists
IF NOT EXISTS (SELECT 1 FROM Users WHERE Username = 'admin')
BEGIN
    DECLARE @Salt NVARCHAR(500) = '$(DefaultAdminSalt)';
    DECLARE @PasswordHash NVARCHAR(500) = '$(DefaultAdminPasswordHash)';
    
    INSERT INTO Users (Username, Email, PasswordHash, Salt, Role, CreatedAt, IsActive)
    VALUES (
        'admin',
        'admin@smartbookstore.com',
        @PasswordHash,
        @Salt,
        'Admin',
        GETUTCDATE(),
        1
    );
    PRINT 'Default admin user created';
END
ELSE
BEGIN
    PRINT 'Admin user already exists';
END