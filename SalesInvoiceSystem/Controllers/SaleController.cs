using Microsoft.AspNetCore.Mvc;
using SalesInvoiceSystem.Models;
using SalesInvoiceSystem.Repository;

namespace SalesInvoiceSystem.Controllers;

public class SaleController : Controller
{
    private readonly ISaleRepository _saleRepository;
    private readonly IProductRepository _productRepository;
    private readonly ICustomerRepository _customerRepository;

    // ==========================================
    // CONSTRUCTOR
    // ==========================================
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
    // DETAILS
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> Details(
        long id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid Sale Id.");
        }

        var sale =
            await _saleRepository.GetSaleByIdAsync(
                id,
                cancellationToken);

        return View(sale);
    }

    // ==========================================
    // CREATE / EDIT - GET
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> CreateOrEdit(
        long? id,
        CancellationToken cancellationToken)
    {
        // ------------------------------------------
        // Load Products
        // ------------------------------------------
        var products =
            await _productRepository.GetAllProductAsync(
                cancellationToken);

        ViewBag.Products = products;


        // ------------------------------------------
        // Load Customers
        // ------------------------------------------
        var customers =
            await _customerRepository.GetAllCustomerAsync(
                cancellationToken);

        ViewBag.Customers = customers;


        // ------------------------------------------
        // CREATE
        // ------------------------------------------
        if (id == null || id == 0)
        {
            var sale = new Sale
            {
                SaleDate = DateTime.Now,

                InvoiceNo =
                    "INV-" +
                    DateTime.Now.ToString("yyyyMMddHHmmss")
            };

            return View(sale);
        }


        // ------------------------------------------
        // EDIT
        // ------------------------------------------
        var existingSale =
            await _saleRepository.GetSaleByIdAsync(
                id.Value,
                cancellationToken);

        return View(existingSale);
    }

    // ==========================================
    // CREATE / EDIT - POST
    // ==========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit(
        Sale sale,
        CancellationToken cancellationToken)
    {
        // ------------------------------------------
        // Validation
        // ------------------------------------------
        if (!ModelState.IsValid)
        {
            // Reload Products
            ViewBag.Products =
                await _productRepository.GetAllProductAsync(
                    cancellationToken);

            // Reload Customers
            ViewBag.Customers =
                await _customerRepository.GetAllCustomerAsync(
                    cancellationToken);

            return View(sale);
        }


        // ------------------------------------------
        // CREATE
        // ------------------------------------------
        if (sale.Id == 0)
        {
            sale.SaleDate = DateTime.Now;

            await _saleRepository.AddSaleAsync(
                sale,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Sale created successfully.";
        }


        // ------------------------------------------
        // UPDATE
        // ------------------------------------------
        else
        {
            await _saleRepository.UpdateSaleAsync(
                sale,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Sale updated successfully.";
        }


        // ------------------------------------------
        // Redirect
        // ------------------------------------------
        return RedirectToAction(nameof(Index));
    }

    // ==========================================
    // DELETE - GET
    // ==========================================
    [HttpGet]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid Sale Id.");
        }

        var sale =
            await _saleRepository.GetSaleByIdAsync(
                id,
                cancellationToken);

        return View(sale);
    }

    // ==========================================
    // DELETE - POST
    // ==========================================
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        long id,
        CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid Sale Id.");
        }

        await _saleRepository.DeleteSaleAsync(
            id,
            cancellationToken);

        TempData["SuccessMessage"] =
            "Sale deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}