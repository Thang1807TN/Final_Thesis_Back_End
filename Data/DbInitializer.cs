using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SecondHandMarketplaceAPI.Models;
using SecondHandMarketplaceAPI.Models.Enums;

namespace SecondHandMarketplaceAPI.Data
{
    public static class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var dbContext = serviceProvider.GetRequiredService<ApplicationDbContext>();

            string[] roles = { "Admin", "User" };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            var adminEmail = "admin@gmail.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FullName = "System Admin",
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(adminUser, "Admin@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            var demoSellerEmail = "seller@gmail.com";
            var demoSeller = await userManager.FindByEmailAsync(demoSellerEmail);

            if (demoSeller == null)
            {
                demoSeller = new ApplicationUser
                {
                    FullName = "Demo Seller",
                    UserName = demoSellerEmail,
                    Email = demoSellerEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-60)
                };

                var result = await userManager.CreateAsync(demoSeller, "Seller@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoSeller, "User");
                }
            }

            var demoBuyerEmail = "buyer@gmail.com";
            var demoBuyer = await userManager.FindByEmailAsync(demoBuyerEmail);

            if (demoBuyer == null)
            {
                demoBuyer = new ApplicationUser
                {
                    FullName = "Demo Buyer",
                    UserName = demoBuyerEmail,
                    Email = demoBuyerEmail,
                    EmailConfirmed = true,
                    CreatedAt = DateTime.UtcNow.AddDays(-45)
                };

                var result = await userManager.CreateAsync(demoBuyer, "Buyer@123");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(demoBuyer, "User");
                }
            }

            if (!await dbContext.Categories.AnyAsync())
            {
                dbContext.Categories.AddRange(
                    new Category { Name = "Electronics" },
                    new Category { Name = "Furniture" },
                    new Category { Name = "Books" },
                    new Category { Name = "Clothing" },
                    new Category { Name = "Home Appliances" }
                );

                await dbContext.SaveChangesAsync();
            }

            if (!await dbContext.Products.AnyAsync() && demoSeller != null)
            {
                var categories = await dbContext.Categories.ToListAsync();

                var electronicsId = categories.First(c => c.Name == "Electronics").Id;
                var furnitureId = categories.First(c => c.Name == "Furniture").Id;
                var booksId = categories.First(c => c.Name == "Books").Id;

                var products = new List<Product>
                {
                    new Product
                    {
                        Title = "Used iPhone 12",
                        Description = "A well-maintained iPhone 12 in very good condition.",
                        Price = 2200,
                        Location = "Warsaw",
                        Condition = ProductCondition.VeryGood,
                        CategoryId = electronicsId,
                        SellerId = demoSeller.Id,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-50),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://placehold.co/600x400?text=iPhone+12", BlobName = "seed-iphone.jpg" }
                        }
                    },
                    new Product
                    {
                        Title = "Wooden Study Desk",
                        Description = "Solid desk, suitable for students and remote work.",
                        Price = 480,
                        Location = "Kraków",
                        Condition = ProductCondition.Good,
                        CategoryId = furnitureId,
                        SellerId = demoSeller.Id,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-40),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://placehold.co/600x400?text=Desk", BlobName = "seed-desk.jpg" }
                        }
                    },
                    new Product
                    {
                        Title = "Algorithms Textbook",
                        Description = "University-level algorithms book with minor notes.",
                        Price = 70,
                        Location = "Rzeszów",
                        Condition = ProductCondition.Good,
                        CategoryId = booksId,
                        SellerId = demoSeller.Id,
                        IsAvailable = true,
                        CreatedAt = DateTime.UtcNow.AddDays(-25),
                        Images = new List<ProductImage>
                        {
                            new ProductImage { ImageUrl = "https://placehold.co/600x400?text=Book", BlobName = "seed-book.jpg" }
                        }
                    }
                };

                dbContext.Products.AddRange(products);
                await dbContext.SaveChangesAsync();
            }

            if (!await dbContext.Coupons.AnyAsync())
            {
                dbContext.Coupons.AddRange(
                    new Coupon
                    {
                        Code = "GREEN10",
                        DiscountPercent = 10,
                        IsActive = true,
                        UsageLimit = 100,
                        UsedCount = 0,
                        ExpiresAt = DateTime.UtcNow.AddMonths(3),
                        CreatedAt = DateTime.UtcNow
                    },
                    new Coupon
                    {
                        Code = "WELCOME5",
                        DiscountPercent = 5,
                        IsActive = true,
                        UsageLimit = 200,
                        UsedCount = 0,
                        ExpiresAt = DateTime.UtcNow.AddMonths(2),
                        CreatedAt = DateTime.UtcNow
                    }
                );

                await dbContext.SaveChangesAsync();
            }
        }
    }
}