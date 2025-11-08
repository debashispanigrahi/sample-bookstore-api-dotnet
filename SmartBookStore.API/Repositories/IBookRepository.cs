using Dapper;
using System.Linq;
using SmartBookStore.API.Models;

namespace SmartBookStore.API.Repositories;

public interface IBookRepository
{
    Task<IEnumerable<Book?>> GetAllAsync();
    Task<Book?> GetByIdAsync(int Id);
    Task<int> CreateAsync(Book book);
}

public class BookRepository : IBookRepository
{
    public async Task<IEnumerable<Book?>> GetAllAsync()
    {
        using var connection = DbConnectionFactory.Create();
        return await connection.QueryAsync<Book>("usp_GetAllBooks", commandType: System.Data.CommandType.StoredProcedure);
    }

    public async Task<int> CreateAsync(Book book)
    {
        using var connection = DbConnectionFactory.Create();
        var parameters = new DynamicParameters();
        parameters.Add("@Title", book.Title);
        parameters.Add("@Author", book.Author);
        parameters.Add("@Isbn", book.Isbn);
        parameters.Add("@Price", book.Price);
        parameters.Add("@PublishedDate", DateTime.Now);
        parameters.Add("@Genre", book.Genre);
        parameters.Add("@InStock", book.InStock);
        parameters.Add("@NewId", dbType: System.Data.DbType.Int32, direction: System.Data.ParameterDirection.Output);

        await connection.ExecuteAsync("usp_CreateBook", parameters, commandType: System.Data.CommandType.StoredProcedure);

        return parameters.Get<int>("@NewId");
    }

    public async Task<Book?> GetByIdAsync(int Id)
    {
        using var connection = DbConnectionFactory.Create();
        return await connection.QuerySingleOrDefaultAsync<Book>("usp_GetBookById", new { Id }, commandType: System.Data.CommandType.StoredProcedure);
    }
}