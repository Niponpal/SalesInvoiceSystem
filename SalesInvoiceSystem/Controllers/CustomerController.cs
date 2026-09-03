using Microsoft.AspNetCore.Mvc;
using SalesInvoiceSystem.Models;
using SalesInvoiceSystem.Repository;

namespace SalesInvoiceSystem.Controllers;

public class CustomerController : Controller
{
    private readonly ICustomerRepository _customerRepository;

    public CustomerController(ICustomerRepository customerRepository)
    {
        _customerRepository = customerRepository;
    }


    // =========================================================
    // CUSTOMER LIST
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var customers = await _customerRepository.GetAllCustomerAsync( cancellationToken);

        return View(customers);
    }


    // =========================================================
    // CUSTOMER DETAILS
    // =========================================================

    [HttpGet]
    public async Task<IActionResult> Details(
        long id,
        CancellationToken cancellationToken)
    {
        var customer =
            await _customerRepository.GetCustomerByIdAsync(
                id,
                cancellationToken);

        return View(customer);
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
            return View(new Customer());
        }

        // Edit
        var customer =
            await _customerRepository.GetCustomerByIdAsync(
                id.Value,
                cancellationToken);

        return View(customer);
    }


    // =========================================================
    // CREATE OR EDIT - POST
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateOrEdit(
        Customer customer,
        CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(customer);
        }


        // =====================================================
        // CREATE
        // =====================================================

        if (customer.Id == 0)
        {
            customer.CreatedDate = DateTime.Now;

            await _customerRepository.AddCustomerAsync(
                customer,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Customer created successfully.";
        }


        // =====================================================
        // UPDATE
        // =====================================================

        else
        {
            await _customerRepository.UpdateCustomerAsync(
                customer,
                cancellationToken);

            TempData["SuccessMessage"] =
                "Customer updated successfully.";
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
        var customer =
            await _customerRepository.GetCustomerByIdAsync(
                id,
                cancellationToken);

        return View(customer);
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
        await _customerRepository.DeleteCustomerAsync(
            id,
            cancellationToken);

        TempData["SuccessMessage"] =
            "Customer deleted successfully.";

        return RedirectToAction(nameof(Index));
    }
}