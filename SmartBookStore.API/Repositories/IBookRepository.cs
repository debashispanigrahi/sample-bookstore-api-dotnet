using System;
using Dapper;
using FluentResults;
using Microsoft.Data.SqlClient;
using SmartBookStore.API.Models;

namespace SmartBookStore.API.Repositories;

public interface IBookRepository
{
    Task<Result<IEnumerable<Book>>> GetAllAsync();
    Task<Result<int>> CreateAsync(Book book);
}

public class BookRepository(IConfiguration config) : IBookRepository
{
    private SqlConnection CreateConnection()
    {
        var conn = new SqlConnection(config.GetConnectionString("DefaultConnection"));
        return conn; // caller uses 'using var' (dispose) and opens implicitly on first use
    }

    public async Task<Result<IEnumerable<Book>>> GetAllAsync()
    {
        try
        {
            using var connection = CreateConnection();
            var sql = "SELECT * FROM Books";
            var books = await connection.QueryAsync<Book>(sql);
            return Result.Ok(books);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to retrieve books: {ex.Message}");
        }
    }

    public async Task<Result<int>> CreateAsync(Book book)
    {
        try
        {
            if (book == null)
                return Result.Fail("Book cannot be null");

            if (string.IsNullOrWhiteSpace(book.Title))
                return Result.Fail("Book title is required");

            if (string.IsNullOrWhiteSpace(book.Author))
                return Result.Fail("Book author is required");

            using var connection = CreateConnection();
            var sql = "INSERT INTO Books (Title, Author, Genre, Price) OUTPUT INSERTED.Id VALUES (@Title, @Author, @Genre, @Price);";
            var bookId = await connection.ExecuteScalarAsync<int>(sql, book);
            return Result.Ok(bookId);
        }
        catch (Exception ex)
        {
            return Result.Fail($"Failed to create book: {ex.Message}");
        }
    }
}