using System;
using Dapper;
using Microsoft.Data.SqlClient;
using SmartBookStore.API.Models;

namespace SmartBookStore.API.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book>> GetAllAsync();
    Task<int> CreateAsync(Book book);
}

public class BookRepository : IBookRepository
{
    public BookRepository()
    {
    }

    public async Task<IEnumerable<Book>> GetAllAsync()
    {
        using var connection = DbConnectionFactory.Create();
        return await connection.QueryAsync<Book>("usp_GetAllBooks", commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(Book book)
    {
        if (book == null) throw new ArgumentNullException(nameof(book));
        if (string.IsNullOrWhiteSpace(book.Title)) throw new ArgumentException("Book title is required", nameof(book.Title));
        if (string.IsNullOrWhiteSpace(book.Author)) throw new ArgumentException("Book author is required", nameof(book.Author));

        using var connection = DbConnectionFactory.Create();
        var parameters = new DynamicParameters();
        parameters.Add("@Title", book.Title);
        parameters.Add("@Author", book.Author);
        parameters.Add("@Genre", book.Genre);
        parameters.Add("@Price", book.Price);
        parameters.Add("@NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_CreateBook", parameters, commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }
}