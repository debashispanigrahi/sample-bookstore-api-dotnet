CREATE OR ALTER PROCEDURE usp_GetUserByUsername
    @Username NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        IF NULLIF(LTRIM(RTRIM(@Username)), '') IS NULL
            THROW 50001, 'Username is required', 1;

        SELECT Id, Username, Email, PasswordHash, Salt, Role, CreatedAt, LastLoginAt, IsActive
        FROM Users
        WHERE Username = @Username;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END