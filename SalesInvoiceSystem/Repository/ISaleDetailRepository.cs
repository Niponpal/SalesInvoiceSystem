using Dapper;
using SalesInvoiceSystem.Data;
using SalesInvoiceSystem.Models;
using System.Data;

namespace SalesInvoiceSystem.Repository
{
    public interface ISaleDetailRepository
    {
        Task<IEnumerable<Sale>> GetAllSaleAsync(
            CancellationToken cancellationToken);

        Task<Sale?> GetSaleByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<int> AddSaleAsync(
            Sale sale,
            CancellationToken cancellationToken);

        Task DeleteSaleAsync(
            int id,
            CancellationToken cancellationToken);
    }


    public class SaleDetailRepository : ISaleDetailRepository
    {
        private readonly DbConnectionFactory _factory;

        public SaleDetailRepository(DbConnectionFactory factory)
        {
            _factory = factory;
        }


        // ==========================================
        // GET ALL SALES
        // ==========================================
        public async Task<IEnumerable<Sale>> GetAllSaleAsync(
            CancellationToken cancellationToken)
        {
            using var conn = _factory.CreateDbConnection();

            var command = new CommandDefinition(
                "sp_Sale_GetAll",
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            return await conn.QueryAsync<Sale>(command);
        }


        // ==========================================
        // GET SALE BY ID
        // ==========================================
        public async Task<Sale?> GetSaleByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.CreateDbConnection();

            var parameters = new DynamicParameters();

            parameters.Add("@Id", id);

            var command = new CommandDefinition(
                "sp_Sale_GetById",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );

            using var multi = await conn.QueryMultipleAsync(command);


            // First result = Sale
            var sale =
                await multi.ReadSingleOrDefaultAsync<Sale>();


            // Second result = Sale Details
            if (sale != null)
            {
                var details =
                    await multi.ReadAsync<SaleDetail>();

                sale.SaleDetails = details.ToList();
            }

            return sale;
        }


        // ==========================================
        // ADD SALE / CREATE INVOICE
        // ==========================================
        public async Task<int> AddSaleAsync(
            Sale sale,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.CreateDbConnection();


            // ==========================================
            // Create DataTable for TVP
            // ==========================================

            var table = new DataTable();

            table.Columns.Add(
                "ProductId",
                typeof(int)
            );

            table.Columns.Add(
                "Quantity",
                typeof(int)
            );

            table.Columns.Add(
                "UnitPrice",
                typeof(decimal)
            );

            table.Columns.Add(
                "TotalPrice",
                typeof(decimal)
            );


            // ==========================================
            // Add Sale Details into DataTable
            // ==========================================

            foreach (var detail in sale.SaleDetails)
            {
                table.Rows.Add(
                    detail.ProductId,
                    detail.Quantity,
                    detail.UnitPrice,
                    detail.Quantity * detail.UnitPrice
                );
            }


            // ==========================================
            // Parameters
            // ==========================================

            var parameters = new DynamicParameters();

            parameters.Add(
                "@InvoiceNo",
                sale.InvoiceNo
            );

            parameters.Add(
                "@CustomerId",
                sale.CustomerId
            );

            parameters.Add(
                "@SaleDate",
                sale.SaleDate
            );

            parameters.Add(
                "@TotalAmount",
                sale.TotalAmount
            );


            // ==========================================
            // TVP Parameter
            // ==========================================

            parameters.Add(
                "@SaleDetails",
                table.AsTableValuedParameter(
                    "dbo.SaleDetailType"
                )
            );


            // ==========================================
            // Execute Stored Procedure
            // ==========================================

            var command = new CommandDefinition(
                "sp_Sale_Create",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );


            return await conn.QuerySingleAsync<int>(command);
        }


        // ==========================================
        // DELETE SALE
        // ==========================================
        public async Task DeleteSaleAsync(
            int id,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.CreateDbConnection();

            var parameters = new DynamicParameters();

            parameters.Add(
                "@Id",
                id
            );


            var command = new CommandDefinition(
                "sp_Sale_Delete",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken
            );


            await conn.ExecuteAsync(command);
        }
    }
}