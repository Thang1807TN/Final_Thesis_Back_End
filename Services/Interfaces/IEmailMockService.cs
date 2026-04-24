namespace SecondHandMarketplaceAPI.Services.Interfaces
{
    public interface IEmailMockService
    {
        Task SendAsync(string to, string subject, string body);
    }
}