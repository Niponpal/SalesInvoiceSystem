using Dapper;
using SalesInvoiceSystem.Data;
using SalesInvoiceSystem.Models;
using System.Data;

namespace SalesInvoiceSystem.Repository;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetAllProductAsync(CancellationToken cancellationToken);

    Task<Product> GetProductByIdAsync( long id, CancellationToken cancellationToken);

    Task<Product> AddProductAsync( Product product,  CancellationToken cancellationToken);

    Task<Product> UpdateProductAsync( Product product, CancellationToken cancellationToken);

    Task<Product> DeleteProductAsync( long id, CancellationToken cancellationToken);
}

public class ProductRepository : IProductRepository
{
    private readonly DbConnectionFactory _factory;

    public ProductRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    // =========================================================
    // CREATE PRODUCT
    // =========================================================

    public async Task<Product> AddProductAsync(  Product product,  CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@ProductName", product.ProductName.Trim());
        parameters.Add("@Price", product.Price);
        parameters.Add("@Stock", product.Stock);

        var command = new CommandDefinition(
            "sp_Product_Create",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await conn.QuerySingleAsync<Product>(command);
    }

    // =========================================================
    // GET ALL PRODUCTS
    // =========================================================

    public async Task<IEnumerable<Product>> GetAllProductAsync( CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var command = new CommandDefinition(
            "sp_Product_GetAll",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await conn.QueryAsync<Product>(command);
    }

    // =========================================================
    // GET PRODUCT BY ID
    // =========================================================

    public async Task<Product> GetProductByIdAsync(long id, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);

        var command = new CommandDefinition(
            "sp_Product_GetById",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var product =
            await conn.QuerySingleOrDefaultAsync<Product>(command);

        if (product == null)
        {
            throw new KeyNotFoundException(
                $"Product with Id {id} not found.");
        }

        return product;
    }

    // =========================================================
    // UPDATE PRODUCT
    // =========================================================

    public async Task<Product> UpdateProductAsync( Product product, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@Id", product.Id);
        parameters.Add("@ProductName", product.ProductName.Trim());
        parameters.Add("@Price", product.Price);
        parameters.Add("@Stock", product.Stock);

        var command = new CommandDefinition(
            "sp_Product_Update",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var result =
            await conn.QuerySingleOrDefaultAsync<Product>(command);

        if (result == null)
        {
            throw new KeyNotFoundException(
                $"Product with Id {product.Id} not found.");
        }

        return result;
    }

    // =========================================================
    // DELETE PRODUCT
    // =========================================================

    public async Task<Product> DeleteProductAsync( long id, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        // First get product
        var productParameters = new DynamicParameters();
        productParameters.Add("@Id", id);

        var getCommand = new CommandDefinition(
            "sp_Product_GetById",
            productParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var product =
            await conn.QuerySingleOrDefaultAsync<Product>(getCommand);

        if (product == null)
        {
            throw new KeyNotFoundException(
                $"Product with Id {id} not found.");
        }

        // Delete product
        var deleteParameters = new DynamicParameters();
        deleteParameters.Add("@Id", id);

        var deleteCommand = new CommandDefinition(
            "sp_Product_Delete",
            deleteParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var affectedRows =
            await conn.ExecuteAsync(deleteCommand);

        if (affectedRows == 0)
        {
            throw new Exception(
                $"Product with Id {id} could not be deleted.");
        }

        return product;
    }
}