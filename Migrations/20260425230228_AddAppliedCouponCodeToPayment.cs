using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecondHandMarketplaceAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddAppliedCouponCodeToPayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AppliedCouponCode",
                table: "Payments",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AppliedCouponCode",
                table: "Payments");
        }
    }
}
