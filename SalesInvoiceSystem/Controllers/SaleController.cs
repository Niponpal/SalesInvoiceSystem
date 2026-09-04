using Microsoft.AspNetCore.Mvc;
using SalesInvoiceSystem.Models;
using SalesInvoiceSystem.Repository;

namespace SalesInvoiceSystem.Controllers
{
    public class SaleController : Controller
    {
        private readonly ISaleRepository _saleRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;

        public SaleController(
            ISaleRepository saleRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository)
        {
            _saleRepository = saleRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
        }


        // ==========================================
        // INDEX
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index(
            CancellationToken cancellationToken)
        {
            var sales =
                await _saleRepository.GetAllSalesAsync(
                    cancellationToken);

            return View(sales);
        }


        // ==========================================
        // CREATE / EDIT GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> CreateOrEdit(
            int? id,
            CancellationToken cancellationToken)
        {
            await LoadDropdowns(cancellationToken);


            // ==========================================
            // EDIT
            // ==========================================

            if (id.HasValue && id.Value > 0)
            {
                var sale =
                    await _saleRepository.GetSaleByIdAsync(
                        id.Value,
                        cancellationToken);

                if (sale == null)
                {
                    return NotFound();
                }

                return View(sale);
            }


            // ==========================================
            // CREATE
            // ==========================================

            var newSale = new Sale
            {
                InvoiceNo =
                    $"INV-{DateTime.Now:yyyyMMddHHmmss}",

                SaleDate = DateTime.Now,

                CustomerId = 0,

                TotalAmount = 0,

                SaleDetails =
                    new List<SaleDetail>()
            };

            return View(newSale);
        }


        // ==========================================
        // CREATE / EDIT POST
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateOrEdit(
            Sale sale,
            CancellationToken cancellationToken)
        {
            await LoadDropdowns(cancellationToken);


            // ==========================================
            // INVOICE VALIDATION
            // ==========================================

            if (string.IsNullOrWhiteSpace(sale.InvoiceNo))
            {
                ModelState.AddModelError(
                    nameof(sale.InvoiceNo),
                    "Invoice number is required.");
            }


            // ==========================================
            // CUSTOMER VALIDATION
            // ==========================================

            if (sale.CustomerId <= 0)
            {
                ModelState.AddModelError(
                    nameof(sale.CustomerId),
                    "Please select a customer.");
            }


            // ==========================================
            // SALE DETAILS VALIDATION
            // ==========================================

            if (sale.SaleDetails == null ||
                sale.SaleDetails.Count == 0)
            {
                ModelState.AddModelError(
                    "",
                    "Please add at least one product.");
            }


            // ==========================================
            // PRODUCT VALIDATION
            // ==========================================

            if (sale.SaleDetails != null)
            {
                foreach (var detail in sale.SaleDetails)
                {
                    if (detail.ProductId <= 0)
                    {
                        ModelState.AddModelError(
                            "",
                            "Please select a valid product.");
                    }

                    if (detail.Quantity <= 0)
                    {
                        ModelState.AddModelError(
                            "",
                            "Quantity must be greater than 0.");
                    }

                    if (detail.UnitPrice <= 0)
                    {
                        ModelState.AddModelError(
                            "",
                            "Invalid product price.");
                    }
                }
            }


          
          

            // ==========================================
            // CALCULATE DETAIL TOTAL
            // ==========================================

            foreach (var detail in sale.SaleDetails)
            {
                detail.TotalPrice =
                    detail.Quantity *
                    detail.UnitPrice;
            }


            // ==========================================
            // CALCULATE GRAND TOTAL
            // ==========================================

            sale.TotalAmount =
                sale.SaleDetails.Sum(
                    x => x.TotalPrice);


            // ==========================================
            // CREATE
            // ==========================================

            if (sale.Id == 0)
            {
                try
                {
                    var createdSale =
                        await _saleRepository.AddSaleAsync(
                            sale,
                            cancellationToken);


                    TempData["SuccessMessage"] =
                        "Invoice created successfully.";


                    return RedirectToAction(
                        nameof(Details),
                        new
                        {
                            id = createdSale.Id
                        });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError(
                        "",
                        ex.Message);

                    return View(sale);
                }
            }


            // ==========================================
            // EDIT
            // ==========================================

            try
            {
                var updatedSale =
                    await _saleRepository.UpdateSaleAsync(
                        sale,
                        cancellationToken);


                TempData["SuccessMessage"] =
                    "Invoice updated successfully.";


                return RedirectToAction(
                    nameof(Details),
                    new
                    {
                        id = updatedSale.Id
                    });
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(
                    "",
                    ex.Message);

                return View(sale);
            }
        }


        // ==========================================
        // DETAILS
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest();
            }


            var sale =
                await _saleRepository.GetSaleByIdAsync(
                    id,
                    cancellationToken);


            if (sale == null)
            {
                return NotFound();
            }


            return View(sale);
        }


        // ==========================================
        // DELETE GET
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest();
            }


            var sale =
                await _saleRepository.GetSaleByIdAsync(
                    id,
                    cancellationToken);


            if (sale == null)
            {
                return NotFound();
            }


            return View(sale);
        }


        // ==========================================
        // DELETE POST
        // ==========================================

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
                await _saleRepository.DeleteSaleAsync(
                    id,
                    cancellationToken);


                TempData["SuccessMessage"] =
                    "Invoice deleted successfully.";


                return RedirectToAction(
                    nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] =
                    ex.Message;


                return RedirectToAction(
                    nameof(Index));
            }
        }


        // ==========================================
        // LOAD DROPDOWNS
        // ==========================================

        private async Task LoadDropdowns(
            CancellationToken cancellationToken)
        {
            var products =
                await _productRepository.GetAllProductAsync(
                    cancellationToken);


            var customers =
                await _customerRepository.GetAllCustomerAsync(
                    cancellationToken);


            ViewBag.Products = products;

            ViewBag.Customers = customers;
        }
    }
}