namespace SecondHandMarketplaceAPI.DTOs.Admin
{
    public class AdminDashboardAnalyticsDto
    {
        public IEnumerable<SimpleChartItemDto> UsersByMonth { get; set; } = new List<SimpleChartItemDto>();
        public IEnumerable<SimpleChartItemDto> ListingsByCategory { get; set; } = new List<SimpleChartItemDto>();
        public IEnumerable<SimpleChartItemDto> TransactionsByStatus { get; set; } = new List<SimpleChartItemDto>();
        public IEnumerable<SimpleChartItemDto> ReportsByStatus { get; set; } = new List<SimpleChartItemDto>();
        public IEnumerable<SimpleChartItemDto> RevenueByMonth { get; set; } = new List<SimpleChartItemDto>();
    }

    public class SimpleChartItemDto
    {
        public string Name { get; set; } = string.Empty;
        public decimal Value { get; set; }
    }
}