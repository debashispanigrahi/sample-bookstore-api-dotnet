CREATE OR ALTER PROCEDURE usp_CreateBook
    @Title NVARCHAR(500),
    @Author NVARCHAR(300),
    @Isbn NVARCHAR(50),
    @Price DECIMAL(10,2),
    @PublishedDate DATE,
    @Genre NVARCHAR(100),
    @InStock BIT,
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    BEGIN TRY
        -- Validate required fields
        IF NULLIF(LTRIM(RTRIM(@Title)), '') IS NULL
            THROW 50001, 'Title is required', 1;
        IF NULLIF(LTRIM(RTRIM(@Author)), '') IS NULL
            THROW 50002, 'Author is required', 1;
        IF NULLIF(LTRIM(RTRIM(@Isbn)), '') IS NULL
            THROW 50003, 'ISBN is required', 1;
        IF @Price <= 0
            THROW 50004, 'Price must be greater than zero', 1;

        -- Check for duplicate ISBN
        IF EXISTS (SELECT 1 FROM Books WHERE Isbn = @Isbn)
        BEGIN
            THROW 50005, 'A book with this ISBN already exists', 1;
        END

        INSERT INTO Books (Title, Author, Isbn, Price, PublishedDate, Genre, InStock)
        VALUES (@Title, @Author, @Isbn, @Price, @PublishedDate, @Genre, @InStock);
        
        SET @NewId = SCOPE_IDENTITY();
        PRINT 'Book created successfully';
    END TRY
    BEGIN CATCH
        THROW;
    END CATCH
END