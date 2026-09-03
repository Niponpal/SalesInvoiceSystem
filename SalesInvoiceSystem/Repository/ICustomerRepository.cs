using Dapper;
using SalesInvoiceSystem.Data;
using SalesInvoiceSystem.Models;
using System.Data;

namespace SalesInvoiceSystem.Repository;

public interface ICustomerRepository
{
    Task<IEnumerable<Customer>> GetAllCustomerAsync( CancellationToken cancellationToken);

    Task<Customer> GetCustomerByIdAsync( long id,  CancellationToken cancellationToken);

    Task<Customer> AddCustomerAsync(Customer customer,  CancellationToken cancellationToken);

    Task<Customer> UpdateCustomerAsync( Customer customer, CancellationToken cancellationToken);

    Task<Customer> DeleteCustomerAsync(long id, CancellationToken cancellationToken);
}

public class CustomerRepository : ICustomerRepository
{
    private readonly DbConnectionFactory _factory;

    public CustomerRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    // =========================================================
    // CREATE CUSTOMER
    // =========================================================

    public async Task<Customer> AddCustomerAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@CustomerName", customer.CustomerName.Trim());
        parameters.Add("@Phone", customer.Phone.Trim());
        parameters.Add("@Email", customer.Email?.Trim());
        parameters.Add("@Address", customer.Address?.Trim());

        var command = new CommandDefinition(
            "sp_Customer_Create",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await conn.QuerySingleAsync<Customer>(command);
    }

    // =========================================================
    // GET ALL CUSTOMERS
    // =========================================================

    public async Task<IEnumerable<Customer>> GetAllCustomerAsync(
        CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var command = new CommandDefinition(
            "sp_Customer_GetAll",
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        return await conn.QueryAsync<Customer>(command);
    }

    // =========================================================
    // GET CUSTOMER BY ID
    // =========================================================

    public async Task<Customer> GetCustomerByIdAsync(
        long id,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@Id", id);

        var command = new CommandDefinition(
            "sp_Customer_GetById",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var customer =
            await conn.QuerySingleOrDefaultAsync<Customer>(command);

        if (customer == null)
        {
            throw new KeyNotFoundException(
                $"Customer with Id {id} not found.");
        }

        return customer;
    }

    // =========================================================
    // UPDATE CUSTOMER
    // =========================================================

    public async Task<Customer> UpdateCustomerAsync(
        Customer customer,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        var parameters = new DynamicParameters();

        parameters.Add("@Id", customer.Id);
        parameters.Add("@CustomerName", customer.CustomerName.Trim());
        parameters.Add("@Phone", customer.Phone.Trim());
        parameters.Add("@Email", customer.Email?.Trim());
        parameters.Add("@Address", customer.Address?.Trim());

        var command = new CommandDefinition(
            "sp_Customer_Update",
            parameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var result =
            await conn.QuerySingleOrDefaultAsync<Customer>(command);

        if (result == null)
        {
            throw new KeyNotFoundException(
                $"Customer with Id {customer.Id} not found.");
        }

        return result;
    }

    // =========================================================
    // DELETE CUSTOMER
    // =========================================================

    public async Task<Customer> DeleteCustomerAsync(
        long id,
        CancellationToken cancellationToken)
    {
        using var conn = _factory.CreateDbConnection();

        // First get customer
        var customerParameters = new DynamicParameters();

        customerParameters.Add("@Id", id);

        var getCommand = new CommandDefinition(
            "sp_Customer_GetById",
            customerParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var customer =
            await conn.QuerySingleOrDefaultAsync<Customer>(getCommand);

        if (customer == null)
        {
            throw new KeyNotFoundException(
                $"Customer with Id {id} not found.");
        }

        // Delete customer
        var deleteParameters = new DynamicParameters();

        deleteParameters.Add("@Id", id);

        var deleteCommand = new CommandDefinition(
            "sp_Customer_Delete",
            deleteParameters,
            commandType: CommandType.StoredProcedure,
            cancellationToken: cancellationToken);

        var affectedRows =
            await conn.ExecuteAsync(deleteCommand);

        if (affectedRows == 0)
        {
            throw new Exception(
                $"Customer with Id {id} could not be deleted.");
        }

        return customer;
    }
}