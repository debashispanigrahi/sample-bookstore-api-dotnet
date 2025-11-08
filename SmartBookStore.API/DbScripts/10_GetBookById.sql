CREATE OR ALTER PROCEDURE usp_GetBookById
    @Id INT
AS
BEGIN
    SELECT Id, Title, Author, Isbn, Price, PublishedDate, Genre, InStock
    FROM Books WHERE Id = @Id;
END