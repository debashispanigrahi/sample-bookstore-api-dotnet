CREATE OR ALTER PROCEDURE usp_GetUserByEmail
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NULLIF(LTRIM(RTRIM(@Email)), '') IS NULL
            THROW 50001, 'Email is required', 1;

        SELECT Id, Username, Email, PasswordHash, Salt, Role, CreatedAt, LastLoginAt, IsActive
        FROM Users
        WHERE Email = @Email;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END