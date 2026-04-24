namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IPaymentInvoiceService
    {
        Task<byte[]?> GenerateInvoicePdfAsync(int paymentId, string userId, bool isAdmin = false);
    }
}