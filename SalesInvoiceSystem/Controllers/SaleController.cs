using Microsoft.AspNetCore.Mvc;
using SalesInvoiceSystem.Models;
using SalesInvoiceSystem.Repository;

namespace SalesInvoiceSystem.Controllers;

public class SaleController : Controller
{
    private readonly ISaleRepository _saleRepository;

    public SaleController(ISaleRepository saleRepository)
    {
        _saleRepository = saleRepository;
    }

    // =========================================================
    // SALE LIST
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var sales = await _saleRepository.GetAllSalesAsync(
            cancellationToken);

        return View(sales);
    }


    // =========================================================
    // SALE DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(
        long id,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetSaleByIdAsync(
            id,
            cancellationToken);

        return View(sale);
    }


    // =========================================================
    // CREATE OR EDIT - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> CreateOrEdit(
        long? id,
        CancellationToken cancellationToken)
    {
        // CREATE
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


        // EDIT
        var existingSale =
            await _saleRepository.GetSaleByIdAsync(
                id.Value,
                cancellationToken);

        return View(existingSale);
    }


    // =========================================================
    // CREATE OR EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit(
        Sale sale,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(sale);
        }


        // CREATE
        if (sale.Id == 0)
        {
            sale.SaleDate = DateTime.Now;

            await _saleRepository.AddSaleAsync(
                sale,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Sale created successfully.";
        }


        // UPDATE
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


    // =========================================================
    // DELETE - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Delete(
        long id,
        CancellationToken cancellationToken)
    {
        var sale = await _saleRepository.GetSaleByIdAsync(
            id,
            cancellationToken);

        return View(sale);
    }


    // =========================================================
    // DELETE - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        long id,
        CancellationToken cancellationToken)
    {
        await _saleRepository.DeleteSaleAsync(
            id,
            cancellationToken);

        TempData["SuccessMessage"] =
            "Sale deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}

