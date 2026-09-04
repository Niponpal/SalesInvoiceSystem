using Microsoft.AspNetCore.Mvc;

using SalesInvoiceSystem.Models;
using SalesInvoiceSystem.Repository;
using System.Data;

namespace SalesInvoiceSystem.Controllers
{
    public class SaleController : Controller
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;

        private readonly IWebHostEnvironment _environment;

        public SaleController(
            ISaleRepository saleRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository,
            IWebHostEnvironment environment)
        {
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Index( CancellationToken cancellationToken)
        {
            var sales = await _saleRepository.GetAllSalesAsync(cancellationToken);

            return View(sales);
        }

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(int? id, CancellationToken cancellationToken)
        {
            await LoadDropdowns(cancellationToken);

            if (id.HasValue && id.Value > 0)
            {
                var sale = await _saleRepository.GetSaleByIdAsync(id.Value, cancellationToken);

                if (sale == null)
                {
                    return NotFound();
                }

                return View(sale);
            }

            var newSale = new Sale
            {
                InvoiceNo = $"INV-{DateTime.Now:yyyyMMddHHmmss}",
                SaleDate = DateTime.Now,
                CustomerId = 0,
                TotalAmount = 0,
                SaleDetails = new List<SaleDetail>()
            };

            return View(newSale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(
            Sale sale,
            CancellationToken cancellationToken)
        {
            await LoadDropdowns(cancellationToken);

            if (string.IsNullOrWhiteSpace(sale.InvoiceNo))
            {
                ModelState.AddModelError(nameof(sale.InvoiceNo), "Invoice number is required.");
            }

            if (sale.CustomerId <= 0)
            {
                ModelState.AddModelError(nameof(sale.CustomerId), "Please select a customer.");
            }

            if (sale.SaleDetails == null || sale.SaleDetails.Count == 0)
            {
                ModelState.AddModelError("", "Please add at least one product.");
            }

            if (sale.SaleDetails != null)
            {
                foreach (var detail in sale.SaleDetails)
                {
                    if (detail.ProductId <= 0)
                    {
                        ModelState.AddModelError("", "Please select a valid product.");
                    }

                    if (detail.Quantity <= 0)
                    {
                        ModelState.AddModelError("", "Quantity must be greater than 0.");
                    }

                    if (detail.UnitPrice <= 0)
                    {
                        ModelState.AddModelError("", "Invalid product price.");
                    }
                }
            }

            foreach (var detail in sale.SaleDetails)
            {
                detail.TotalPrice = detail.Quantity * detail.UnitPrice;
            }

            sale.TotalAmount = sale.SaleDetails.Sum(x => x.TotalPrice);

            if (sale.Id == 0)
            {
                try
                {
                    var createdSale = await _saleRepository.AddSaleAsync(sale, cancellationToken);

                    TempData["SuccessMessage"] = "Invoice created successfully.";

                    return RedirectToAction(nameof(Details), new { id = createdSale.Id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);

                    return View(sale);
                }
            }

            try
            {
                var updatedSale = await _saleRepository.UpdateSaleAsync(sale, cancellationToken);

                TempData["SuccessMessage"] = "Invoice updated successfully.";

                return RedirectToAction(nameof(Details), new { id = updatedSale.Id });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);

                return View(sale);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var sale = await _saleRepository.GetSaleByIdAsync(id, cancellationToken);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var sale = await _saleRepository.GetSaleByIdAsync(id, cancellationToken);

            if (sale == null)
            {
                return NotFound();
            }

            return View(sale);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                await _saleRepository.DeleteSaleAsync(id, cancellationToken);

                TempData["SuccessMessage"] = "Invoice deleted successfully.";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;

                return RedirectToAction(nameof(Index));
            }
        }

        public async Task<IActionResult> Print(
    long id,
    CancellationToken cancellationToken)
        {
            var reportData =
                await _saleRepository.GetSaleInvoiceReportAsync(
                    id,
                    cancellationToken);

            if (reportData == null || reportData.Count == 0)
            {
                return NotFound("Sale invoice not found.");
            }


            // RDLC path
            var reportPath = Path.Combine(
                _environment.ContentRootPath,
                "Reports",
                "SaleInvoice.rdlc"
            );

            if (!System.IO.File.Exists(reportPath))
            {
                return NotFound("SaleInvoice.rdlc not found.");
            }

            // RDLC
            using var report = new Microsoft.Reporting.WebForms.LocalReport();

            report.ReportPath = reportPath;

            // Bind Dataset
            report.DataSources.Clear();

            report.DataSources.Add(
                new ReportDataSource(
                    "SaleInvoiceDataSet",
                    reportData
                )
            );

            // Generate PDF
            var pdf = report.Render("PDF");

            var invoiceNo = reportData
                .First()
                .InvoiceNo;

            return File(
                pdf,
                "application/pdf",
                $"{invoiceNo}.pdf"
            );
        }

        private async Task LoadDropdowns(CancellationToken cancellationToken)
        {
            var products = await _productRepository.GetAllProductAsync(cancellationToken);

            var customers = await _customerRepository.GetAllCustomerAsync(cancellationToken);

            ViewBag.Products = products;

            ViewBag.Customers = customers;
        }
    }
}