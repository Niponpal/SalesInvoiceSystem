using Microsoft.AspNetCore.Mvc;
using SalesInvoiceSystem.Models;
using SalesInvoiceSystem.Repository;

namespace SalesInvoiceSystem.Controllers;

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

    [HttpGet]
    public async Task<IActionResult> Index( CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetAllSalesAsync(cancellationToken);

        return View(sales);
    }

    [HttpGet]
    public async Task<IActionResult> Details( long id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid Sale Id.");
        }

        var sale = await _saleRepository.GetSaleByIdAsync( id, cancellationToken);

        return View(sale);
    }

    
    [HttpGet]
    public async Task<IActionResult> CreateOrEdit(
        long? id,
        CancellationToken cancellationToken)
    {
        
        var products = await _productRepository.GetAllProductAsync(cancellationToken);

        ViewBag.Products = products;


       var customers = await _customerRepository.GetAllCustomerAsync(cancellationToken);

        ViewBag.Customers = customers;

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


        var existingSale =
            await _saleRepository.GetSaleByIdAsync(
                id.Value,
                cancellationToken);

        return View(existingSale);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit( Sale sale, CancellationToken cancellationToken)
    {
       
        if (!ModelState.IsValid)
        {
            ViewBag.Products =  await _productRepository.GetAllProductAsync( cancellationToken);

            ViewBag.Customers =  await _customerRepository.GetAllCustomerAsync( cancellationToken);

            return View(sale);
        }


   
        if (sale.Id == 0)
        {
            sale.SaleDate = DateTime.Now;

            await _saleRepository.AddSaleAsync(
                sale,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Sale created successfully.";
        }


        else
        {
            await _saleRepository.UpdateSaleAsync(
                sale,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Sale updated successfully.";
        }

        return RedirectToAction(nameof(Index));
    }

 
    [HttpGet]
    public async Task<IActionResult> Delete( long id,CancellationToken cancellationToken)
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

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id, CancellationToken cancellationToken)
    {
        if (id <= 0)
        {
            return BadRequest("Invalid Sale Id.");
        }

        await _saleRepository.DeleteSaleAsync(id, cancellationToken);

        TempData["SuccessMessage"] = "Sale deleted successfully.";
        return RedirectToAction(nameof(Index));
    }
}