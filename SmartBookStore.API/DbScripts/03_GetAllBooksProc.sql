CREATE OR ALTER PROCEDURE usp_GetAllBooks
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        SELECT Id, Title, Author, Isbn, Price, PublishedDate, Genre, InStock
        FROM Books
        ORDER BY Title;
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END