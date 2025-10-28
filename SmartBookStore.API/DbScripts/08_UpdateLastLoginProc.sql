CREATE OR ALTER PROCEDURE usp_UpdateLastLogin
    @Id INT,
    @LastLoginAt DATETIME2
AS
BEGIN
    UPDATE Users
    SET LastLoginAt = @LastLoginAt
    WHERE Id = @Id;
END