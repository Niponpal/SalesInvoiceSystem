using Dapper;
using SalesInvoiceSystem.Data;
using SalesInvoiceSystem.DTOs;
using SalesInvoiceSystem.Models;
using System.Data;

namespace SalesInvoiceSystem.Repository;

public interface ISaleRepository
{
    Task<IEnumerable<Sale>> GetAllSalesAsync(CancellationToken cancellationToken);

    Task<Sale> GetSaleByIdAsync(long id, CancellationToken cancellationToken);

    Task<Sale> AddSaleAsync(Sale sale, CancellationToken cancellationToken);

    Task<Sale> UpdateSaleAsync(Sale sale, CancellationToken cancellationToken);

    Task<Sale> DeleteSaleAsync(long id, CancellationToken cancellationToken);

    Task<List<SaleInvoiceReportDto>> GetSaleInvoiceReportAsync(
    long id,
    CancellationToken cancellationToken);
}

public class SaleRepository : ISaleRepository
{
    private readonly DbConnectionFactory _factory;

    public SaleRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }
    public async Task<List<SaleInvoiceReportDto>> GetSaleInvoiceReportAsync(
    long id,
    CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);

        var command = new CommandDefinition(
            "dbo.sp_Sale_GetById",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken
        );

        using var multi = await conn.QueryMultipleAsync(command);

        // Result 1: Sale
        var sale = await multi.ReadSingleOrDefaultAsync<Sale>();

        if (sale == null)
        {
            throw new KeyNotFoundException(
                $"Sale with Id {id} not found.");
        }

        // Result 2: Customer
        sale.Customer =
            await multi.ReadSingleOrDefaultAsync<Customer>();

        // Result 3: SaleDetails + Product
        sale.SaleDetails = multi
            .Read<SaleDetail, Product, SaleDetail>(
                (detail, product) =>
                {
                    detail.Product = product;
                    return detail;
                },
                splitOn: "ProductId"
            )
            .ToList();

        // Convert to Report DTO
        var reportData = sale.SaleDetails
            .Select(detail => new SaleInvoiceReportDto
            {
                SaleId = sale.Id,
                InvoiceNo = sale.InvoiceNo,
                SaleDate = sale.SaleDate,

                CustomerId = sale.CustomerId,
                CustomerName = sale.Customer?.CustomerName ?? "",
                CustomerPhone = sale.Customer?.Phone ?? "",
                CustomerAddress = sale.Customer?.Address ?? "",

                ProductId = detail.ProductId,
                ProductName = detail.Product?.ProductName ?? "",

                Quantity = detail.Quantity,
                UnitPrice = detail.UnitPrice,
                TotalPrice = detail.TotalPrice,

                InvoiceTotal = sale.TotalAmount
            })
            .ToList();

        return reportData;
    }
    public async Task<Sale> AddSaleAsync(Sale sale, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@InvoiceNo", sale.InvoiceNo.Trim());
        parameters.Add("@SaleDate", sale.SaleDate);
        parameters.Add("@CustomerId", sale.CustomerId);
        parameters.Add("@TotalAmount", sale.TotalAmount);

        var saleDetailsTable = new DataTable();
        saleDetailsTable.Columns.Add("ProductId", typeof(int));
        saleDetailsTable.Columns.Add("Quantity", typeof(int));
        saleDetailsTable.Columns.Add("UnitPrice", typeof(decimal));
        saleDetailsTable.Columns.Add("TotalPrice", typeof(decimal));

        foreach (var detail in sale.SaleDetails)
        {
            saleDetailsTable.Rows.Add(
                detail.ProductId,
                detail.Quantity,
                detail.UnitPrice,
                detail.TotalPrice);
        }

        parameters.Add("@SaleDetails", saleDetailsTable.AsTableValuedParameter("dbo.SaleDetailType"));

        var command = new CommandDefinition("sp_Sale_Create", parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);

        var result = await conn.QuerySingleAsync<Sale>(command);

        return result;
    }

    public async Task<IEnumerable<Sale>> GetAllSalesAsync(
       CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var command = new CommandDefinition(
            "dbo.sp_Sale_GetAll",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var sales = await conn.QueryAsync<Sale, Customer, Sale>(
            command,
            (sale, customer) =>
            {
                sale.Customer = customer;
                return sale;
            },
            splitOn: "Customer_Id");

        return sales;
    }

    public async Task<Sale> GetSaleByIdAsync(long id, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@Id", id);

        var command = new CommandDefinition(
            "dbo.sp_Sale_GetById",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        using var multi = await conn.QueryMultipleAsync(command);

        var sale = await multi.ReadSingleOrDefaultAsync<Sale>();

        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with Id {id} not found.");
        }

        sale.Customer = await multi.ReadSingleOrDefaultAsync<Customer>();

        sale.SaleDetails = multi.Read<SaleDetail, Product, SaleDetail>(
            (detail, product) =>
            {
                detail.Product = product;
                return detail;
            },
            splitOn: "Id"
        ).ToList();

        return sale;
    }

    public async Task<Sale> UpdateSaleAsync(Sale sale, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();
        parameters.Add("@Id", sale.Id);
        parameters.Add("@InvoiceNo", sale.InvoiceNo.Trim());
        parameters.Add("@CustomerId", sale.CustomerId);
        parameters.Add("@SaleDate", sale.SaleDate);
        parameters.Add("@TotalAmount", sale.TotalAmount);

        var command = new CommandDefinition(
            "dbo.sp_Sale_Update",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var result = await conn.QuerySingleOrDefaultAsync<Sale>(command);

        if (result == null)
        {
            throw new KeyNotFoundException($"Sale with Id {sale.Id} not found.");
        }

        return result;
    }

    public async Task<Sale> DeleteSaleAsync(long id, CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var saleParameters = new DynamicParameters();
        saleParameters.Add("@Id", id);

        var getCommand = new CommandDefinition(
            "sp_Sale_GetById",
            saleParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var sale = await conn.QuerySingleOrDefaultAsync<Sale>(getCommand);

        if (sale == null)
        {
            throw new KeyNotFoundException($"Sale with Id {id} not found.");
        }

        var deleteParameters = new DynamicParameters();
        deleteParameters.Add("@Id", id);

        var deleteCommand = new CommandDefinition(
            "sp_Sale_Delete",
            deleteParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var affectedRows = await conn.ExecuteAsync(deleteCommand);

        if (affectedRows == 0)
        {
            throw new Exception($"Sale with Id {id} could not be deleted.");
        }

        return sale;
    }
}