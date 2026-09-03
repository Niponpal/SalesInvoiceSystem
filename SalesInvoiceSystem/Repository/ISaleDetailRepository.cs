using Dapper;
using SalesInvoiceSystem.Data;
using SalesInvoiceSystem.Models;
using System.Data;

namespace SalesInvoiceSystem.Repository
{
    public interface ISaleDetailRepository
    {
        Task<IEnumerable<SaleDetail>> GetBySaleIdAsync(
            int saleId,
            CancellationToken cancellationToken);

        Task<SaleDetail> AddSaleDetailAsync(
            SaleDetail saleDetail,
            CancellationToken cancellationToken);

        Task<SaleDetail> GetSaleDetailByIdAsync(
            int id,
            CancellationToken cancellationToken);

        Task<SaleDetail> DeleteSaleDetailAsync(
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


        // =====================================================
        // GET SALE DETAILS BY SALE ID
        // =====================================================

        public async Task<IEnumerable<SaleDetail>> GetBySaleIdAsync(
            int saleId,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.CreateDbConnection();

            var parameters = new DynamicParameters();

            parameters.Add("@SaleId", saleId);

            var command = new CommandDefinition(
                "sp_SaleDetail_GetBySaleId",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            return await conn.QueryAsync<SaleDetail>(command);
        }


        // =====================================================
        // GET SALE DETAIL BY ID
        // =====================================================

        public async Task<SaleDetail> GetSaleDetailByIdAsync(
            int id,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.CreateDbConnection();

            var parameters = new DynamicParameters();

            parameters.Add("@Id", id);

            var command = new CommandDefinition(
                "sp_SaleDetail_GetById",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var saleDetail =
                await conn.QuerySingleOrDefaultAsync<SaleDetail>(
                    command);

            if (saleDetail == null)
            {
                throw new KeyNotFoundException(
                    $"Sale detail with Id {id} not found.");
            }

            return saleDetail;
        }


        // =====================================================
        // CREATE SALE DETAIL
        // =====================================================

        public async Task<SaleDetail> AddSaleDetailAsync(
            SaleDetail saleDetail,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.CreateDbConnection();

            var parameters = new DynamicParameters();

            parameters.Add("@SaleId", saleDetail.SaleId);
            parameters.Add("@ProductId", saleDetail.ProductId);
            parameters.Add("@Quantity", saleDetail.Quantity);
            parameters.Add("@UnitPrice", saleDetail.UnitPrice);
            parameters.Add("@TotalPrice", saleDetail.TotalPrice);

            var command = new CommandDefinition(
                "sp_SaleDetail_Create",
                parameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            return await conn.QuerySingleAsync<SaleDetail>(
                command);
        }


        // =====================================================
        // DELETE SALE DETAIL
        // =====================================================

        public async Task<SaleDetail> DeleteSaleDetailAsync(
            int id,
            CancellationToken cancellationToken)
        {
            using var conn = _factory.CreateDbConnection();

            // First get the record
            var getParameters = new DynamicParameters();

            getParameters.Add("@Id", id);

            var getCommand = new CommandDefinition(
                "sp_SaleDetail_GetById",
                getParameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var saleDetail =
                await conn.QuerySingleOrDefaultAsync<SaleDetail>(
                    getCommand);

            if (saleDetail == null)
            {
                throw new KeyNotFoundException(
                    $"Sale detail with Id {id} not found.");
            }


            // Delete
            var deleteParameters = new DynamicParameters();

            deleteParameters.Add("@Id", id);

            var deleteCommand = new CommandDefinition(
                "sp_SaleDetail_Delete",
                deleteParameters,
                commandType: CommandType.StoredProcedure,
                cancellationToken: cancellationToken);

            var affectedRows =
                await conn.ExecuteAsync(deleteCommand);

            if (affectedRows == 0)
            {
                throw new Exception(
                    $"Sale detail with Id {id} could not be deleted.");
            }

            return saleDetail;
        }
    }
}