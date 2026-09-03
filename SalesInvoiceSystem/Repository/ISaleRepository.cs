using Dapper;
using SalesInvoiceSystem.Data;
using SalesInvoiceSystem.Models;
using System.Data;

namespace SalesInvoiceSystem.Repository;

public interface ISaleRepository
{
    Task<IEnumerable<Sale>> GetAllSalesAsync(CancellationToken cancellationToken);

    Task<Sale> GetSaleByIdAsync( long id,CancellationToken cancellationToken);

    Task<Sale> AddSaleAsync( Sale sale, CancellationToken cancellationToken);

    Task<Sale> UpdateSaleAsync( Sale sale, CancellationToken cancellationToken);

    Task<Sale> DeleteSaleAsync( long id, CancellationToken cancellationToken);
}


public class SaleRepository : ISaleRepository
{
    private readonly DbConnectionFactory _factory;

    public SaleRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }


    public async Task<Sale> AddSaleAsync(Sale sale,CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@InvoiceNo", sale.InvoiceNo.Trim());
        parameters.Add("@SaleDate", sale.SaleDate);
        parameters.Add("@CustomerId", sale.CustomerId);
        parameters.Add("@TotalAmount", sale.TotalAmount);

        var command = new CommandDefinition(
            "sp_Sale_Create",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await conn.QuerySingleAsync<Sale>(command);
    }


  
    public async Task<IEnumerable<Sale>> GetAllSalesAsync( CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var command = new CommandDefinition(
            "sp_Sale_GetAll",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await conn.QueryAsync<Sale>(command);
    }


    public async Task<Sale> GetSaleByIdAsync( long id, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@Id", id);

        var command = new CommandDefinition(
            "sp_Sale_GetById",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var sale =  await conn.QuerySingleOrDefaultAsync<Sale>(command);

        if (sale == null)
        {
            throw new KeyNotFoundException(
                $"Sale with Id {id} not found.");
        }

        return sale;
    }


    public async Task<Sale> UpdateSaleAsync(  Sale sale, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@Id", sale.Id);
        parameters.Add("@InvoiceNo", sale.InvoiceNo.Trim());
        parameters.Add("@CustomerId", sale.CustomerId);
        parameters.Add("@SaleDate", sale.SaleDate);
        parameters.Add("@TotalAmount", sale.TotalAmount);

        var command = new CommandDefinition(
            "sp_Sale_Update",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var result =
            await conn.QuerySingleOrDefaultAsync<Sale>(command);

        if (result == null)
        {
            throw new KeyNotFoundException(
                $"Sale with Id {sale.Id} not found.");
        }

        return result;
    }


    public async Task<Sale> DeleteSaleAsync(  long id,CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var saleParameters = new DynamicParameters();

        saleParameters.Add("@Id", id);

        var getCommand = new CommandDefinition(
            "sp_Sale_GetById",
            saleParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var sale =
            await conn.QuerySingleOrDefaultAsync<Sale>(getCommand);

        if (sale == null)
        {
            throw new KeyNotFoundException(
                $"Sale with Id {id} not found.");
        }


        var deleteParameters = new DynamicParameters();

        deleteParameters.Add("@Id", id);

        var deleteCommand = new CommandDefinition(
            "sp_Sale_Delete",
            deleteParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var affectedRows =
            await conn.ExecuteAsync(deleteCommand);

        if (affectedRows == 0)
        {
            throw new Exception(
                $"Sale with Id {id} could not be deleted.");
        }

        return sale;
    }
}