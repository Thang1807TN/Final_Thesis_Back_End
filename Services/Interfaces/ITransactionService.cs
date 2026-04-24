using SecondHandMarketplaceAPI.DTOs.Transactions;

namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<IEnumerable<TransactionResponseDto>> GetAllAsync(bool isAdmin, string userId);
        Task<TransactionResponseDto?> GetByIdAsync(int id, bool isAdmin, string userId);
        Task<IEnumerable<TransactionResponseDto>> GetMineAsync(string userId);
        Task<TransactionResponseDto?> CreateAsync(string buyerId, CreateTransactionDto dto);
        Task<TransactionResponseDto?> UpdateStatusAsync(int id, string userId, UpdateTransactionStatusDto dto, bool isAdmin);
        Task<TransactionResponseDto?> MarkCompletedAsync(int id, string userId, bool isAdmin = false);
    }
}