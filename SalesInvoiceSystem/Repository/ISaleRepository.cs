using SalesInvoiceSystem.Models;

namespace SalesInvoiceSystem.Repository;

public interface ISaleRepository
{
    Task<IEnumerable<Sale>> GetAllSalesAsync(CancellationToken cancellationToken);

    Task<Sale> GetSaleByIdAsync(long id, CancellationToken cancellationToken);

    Task<Sale> AddSaleAsync(Sale sale, CancellationToken cancellationToken);

    Task<Sale> UpdateSaleAsync(Sale sale, CancellationToken cancellationToken);

    Task<Sale> DeleteSaleAsync(long id, CancellationToken cancellationToken);
}
