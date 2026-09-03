using Microsoft.AspNetCore.Mvc;
using SalesInvoiceSystem.Repository;

namespace SalesInvoiceSystem.Controllers
{
    public class SaleDetailController : Controller
    {
        private readonly ISaleDetailRepository _saleDetailRepository;

        public SaleDetailController(
            ISaleDetailRepository saleDetailRepository)
        {
            _saleDetailRepository = saleDetailRepository;
        }

        // ==========================================
        // SALE DETAILS LIST
        // /SaleDetail/Index?saleId=2
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Index(
            int? saleId,
            CancellationToken cancellationToken)
        {
            // If no Sale ID is provided
            if (saleId == null || saleId <= 0)
            {
                return View(new List<SalesInvoiceSystem.Models.SaleDetail>());
            }

            var saleDetails =
                await _saleDetailRepository.GetBySaleIdAsync(
                    saleId.Value,
                    cancellationToken);

            ViewBag.SaleId = saleId.Value;

            return View(saleDetails);
        }


        // ==========================================
        // SALE DETAIL DETAILS
        // /SaleDetail/Details?id=3
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Details(
            int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Sale Detail Id.");
            }

            var saleDetail =
                await _saleDetailRepository.GetSaleDetailByIdAsync(
                    id,
                    cancellationToken);

            return View(saleDetail);
        }


        // ==========================================
        // DELETE PAGE
        // ==========================================

        [HttpGet]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Sale Detail Id.");
            }

            var saleDetail =
                await _saleDetailRepository.GetSaleDetailByIdAsync(
                    id,
                    cancellationToken);

            return View(saleDetail);
        }


        // ==========================================
        // DELETE CONFIRMED
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(
            int id,
            CancellationToken cancellationToken)
        {
            if (id <= 0)
            {
                return BadRequest("Invalid Sale Detail Id.");
            }

            var saleDetail =
                await _saleDetailRepository.DeleteSaleDetailAsync(
                    id,
                    cancellationToken);

            TempData["SuccessMessage"] =
                "Sale detail deleted successfully.";

            return RedirectToAction(
                nameof(Index),
                new
                {
                    saleId = saleDetail.SaleId
                });
        }
    }
}