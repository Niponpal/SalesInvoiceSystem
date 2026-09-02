using Microsoft.AspNetCore.Mvc;
using SalesInvoiceSystem.Models;
using SalesInvoiceSystem.Repository;

namespace SalesInvoiceSystem.Controllers;

public class ProductController : Controller
{
    private readonly IProductRepository _productRepository;

    public ProductController(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }


    // =========================================================
    // PRODUCT LIST
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(
        CancellationToken cancellationToken)
    {
        var products =
            await _productRepository.GetAllProductAsync(
                cancellationToken);

        return View(products);
    }


    // =========================================================
    // PRODUCT DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(
        long id,
        CancellationToken cancellationToken)
    {
        var product =
            await _productRepository.GetProductByIdAsync(
                id,
                cancellationToken);

        return View(product);
    }


    // =========================================================
    // CREATE OR EDIT - GET
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> CreateOrEdit(
        long? id,
        CancellationToken cancellationToken)
    {
        // Create
        if (id == null || id == 0)
        {
            return View(new Product());
        }

        // Edit
        var product =
            await _productRepository.GetProductByIdAsync(
                id.Value,
                cancellationToken);

        return View(product);
    }


    // =========================================================
    // CREATE OR EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit(
        Product product,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        // =====================================================
        // CREATE
        // =====================================================

        if (product.Id == 0)
        {
            product.CreatedDate = DateTime.Now;

            await _productRepository.AddProductAsync(
                product,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Product created successfully.";
        }

        // =====================================================
        // UPDATE
        // =====================================================

        else
        {
            await _productRepository.UpdateProductAsync(
                product,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Product updated successfully.";
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
        var product =
            await _productRepository.GetProductByIdAsync(
                id,
                cancellationToken);

        return View(product);
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
        await _productRepository.DeleteProductAsync(
            id,
            cancellationToken);

        TempData["SuccessMessage"] =
            "Product deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}