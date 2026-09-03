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


    [HttpGet]
    public async Task<IActionResult> Index( CancellationToken cancellationToken)
    {
        var products = await _productRepository.GetAllProductAsync(cancellationToken);

        return View(products);
    }


    [HttpGet]
    public async Task<IActionResult> Details(long id,CancellationToken cancellationToken)
    {
        var product =await _productRepository.GetProductByIdAsync( id,cancellationToken);
        return View(product);
    }


    [HttpGet]
    public async Task<IActionResult> CreateOrEdit(long? id, CancellationToken cancellationToken)
   {
     
        if (id == null || id == 0)
        {
            return View(new Product());
        }

        var product = await _productRepository.GetProductByIdAsync(id.Value, cancellationToken);

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit( Product product,CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(product);
        }

        if (product.Id == 0)
        {
            product.CreatedDate = DateTime.Now;

            await _productRepository.AddProductAsync(product,cancellationToken);

            TempData["SuccessMessage"] = "Product created successfully.";
        }


        else
        {
            await _productRepository.UpdateProductAsync( product, cancellationToken);

            TempData["SuccessMessage"] = "Product updated successfully.";
        }

        return RedirectToAction(nameof(Index));
    }



    [HttpGet]
    public async Task<IActionResult> Delete( long id, CancellationToken cancellationToken)
    {
        var product = await _productRepository
            .GetProductByIdAsync(id, cancellationToken);

        return View(product);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id,CancellationToken cancellationToken)
    {
        await _productRepository.DeleteProductAsync( id,cancellationToken);

        TempData["SuccessMessage"] = "Product deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}