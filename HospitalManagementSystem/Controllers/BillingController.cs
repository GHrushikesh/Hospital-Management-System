using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HospitalManagementSystem.Data.Repositories;
using HospitalManagementSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace HospitalManagementSystem.Controllers
{
    public class BillingController : Controller
    {
        private readonly BillingRepository _billingRepository;

        public BillingController(BillingRepository billingRepository)
        {
            _billingRepository = billingRepository;
        }

        // GET: /Billing
        public async Task<IActionResult> Index()
        {
            var bills = await _billingRepository.GetAllAsync();
            return View(bills);
        }

        // GET: /Billing/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var bill = await _billingRepository.GetByIdAsync(id);
            if (bill == null)
            {
                return NotFound();
            }

            return View(bill);
        }

        // GET: /Billing/Create
        public IActionResult Create()
        {
            var model = new Bill();
            for (int i = 0; i < 5; i++)
            {
                model.Items.Add(new BillItem());
            }

            return View(model);
        }

        // POST: /Billing/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Bill bill, List<BillItem>? items)
        {
            items ??= new List<BillItem>();

            bool hasValidItem = items.Any(i => !string.IsNullOrWhiteSpace(i.Description) && i.Quantity > 0 && i.UnitPrice >= 0);
            if (!hasValidItem)
            {
                ModelState.AddModelError(string.Empty, "At least one valid bill item is required.");
            }

            if (!ModelState.IsValid)
            {
                bill.Items = items;
                return View(bill);
            }

            foreach (var item in items)
            {
                item.Amount = item.UnitPrice * item.Quantity;
            }

            bill.SubTotal = items.Sum(i => i.Amount);
            bill.Total = bill.SubTotal + bill.Tax - bill.Discount;
            if (bill.BillDate == default)
            {
                bill.BillDate = DateTime.UtcNow;
            }

            try
            {
                int newBillId = await _billingRepository.AddAsync(bill, items);
                TempData["Success"] = "Bill created successfully.";
                return RedirectToAction(nameof(Details), new { id = newBillId });
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Error: " + ex.Message;
                bill.Items = items;
                return View(bill);
            }
        }

        // GET: /Billing/PatientBills/5
        public async Task<IActionResult> PatientBills(int patientId)
        {
            var bills = await _billingRepository.GetByPatientAsync(patientId);
            return View("Index", bills);
        }

        // POST: /Billing/UpdatePaymentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int id, string paymentStatus, string? paymentMethod)
        {
            bool updated = await _billingRepository.UpdatePaymentStatusAsync(id, paymentStatus, paymentMethod);
            if (updated)
            {
                TempData["Success"] = "Payment status updated.";
            }
            else
            {
                TempData["Error"] = "Failed to update payment status.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: /Billing/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool deleted = await _billingRepository.DeleteAsync(id);
            if (deleted)
            {
                TempData["Success"] = "Bill deleted.";
            }
            else
            {
                TempData["Error"] = "Failed to delete bill.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
