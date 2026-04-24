using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using SecondHandMarketplaceAPI.Data;
using SecondHandMarketplaceAPI.Services.Interfaces;
using System.Reflection.Metadata;

namespace SecondHandMarketplaceAPI.Services
{
    public class PaymentInvoiceService : IPaymentInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public PaymentInvoiceService(ApplicationDbContext context)
        {
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        public async Task<byte[]?> GenerateInvoicePdfAsync(int paymentId, string userId, bool isAdmin = false)
        {
            var payment = await _context.Payments
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Product)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Buyer)
                .Include(p => p.Transaction)
                    .ThenInclude(t => t.Seller)
                .FirstOrDefaultAsync(p => p.Id == paymentId);

            if (payment == null)
            {
                return null;
            }

            if (!isAdmin &&
                payment.Transaction?.BuyerId != userId &&
                payment.Transaction?.SellerId != userId)
            {
                return null;
            }

            var pdfBytes = QuestPDF.Fluent.Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Margin(30);

                    page.Header()
                        .Text("GreenMarket Payment Invoice")
                        .FontSize(22)
                        .Bold();

                    page.Content().Column(col =>
                    {
                        col.Spacing(10);

                        col.Item().Text($"Invoice Date: {DateTime.UtcNow:yyyy-MM-dd}");
                        col.Item().Text($"Payment ID: {payment.Id}");
                        col.Item().Text($"Transaction ID: {payment.TransactionId}");
                        col.Item().Text($"Product: {payment.Transaction?.Product?.Title}");
                        col.Item().Text($"Buyer: {payment.Transaction?.Buyer?.FullName}");
                        col.Item().Text($"Seller: {payment.Transaction?.Seller?.FullName}");
                        col.Item().Text($"Method: {payment.PaymentMethod}");
                        col.Item().Text($"Status: {payment.PaymentStatus}");
                        col.Item().Text($"Amount: {payment.Amount} PLN");

                        if (!string.IsNullOrWhiteSpace(payment.ExternalTransactionCode))
                        {
                            col.Item().Text($"Reference: {payment.ExternalTransactionCode}");
                        }
                    });

                    page.Footer()
                        .AlignCenter()
                        .Text("GreenMarket Mock Invoice")
                        .FontSize(10)
                        .FontColor(Colors.Grey.Medium);
                });
            }).GeneratePdf();

            return pdfBytes;
        }
    }
}