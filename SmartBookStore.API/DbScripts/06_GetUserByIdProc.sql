CREATE OR ALTER PROCEDURE usp_GetUserById
    @Id INT
AS
BEGIN
    SELECT Id, Username, Email, PasswordHash, Salt, Role, CreatedAt, LastLoginAt, IsActive
    FROM Users
    WHERE Id = @Id;
END