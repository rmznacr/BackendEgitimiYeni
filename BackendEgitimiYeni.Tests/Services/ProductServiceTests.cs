using BackendEgitimiYeni.Data;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Models;
using BackendEgitimiYeni.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace BackendEgitimiYeni.Tests.Services;

public class ProductServiceTests
{
    private AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }

    private ProductService CreateService(AppDbContext context)
    {
        var cache = new MemoryCache(
            new MemoryCacheOptions()
        );

        return new ProductService(
            context,
            NullLogger<ProductService>.Instance,
            cache
        );
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateProduct()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        var dto = new ProductCreateDto
        {
            Name = "Laptop",
            Price = 25000
        };

        // Act
        var result = await service.CreateAsync(dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Laptop", result.Name);
        Assert.Equal(25000, result.Price);
        Assert.Equal(1, await context.Products.CountAsync());
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnProduct_WhenProductExists()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.Add(new Product
        {
            Name = "Monitor",
            Price = 8000
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Monitor", result.Name);
        Assert.Equal(8000, result.Price);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateAsync_ShouldUpdateProduct_WhenProductExists()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.Add(new Product
        {
            Name = "Laptop",
            Price = 25000
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        var dto = new ProductUpdateDto
        {
            Name = "Gaming Laptop",
            Price = 35000
        };

        // Act
        var result = await service.UpdateAsync(1, dto);

        // Assert
        Assert.True(result);

        var product = await context.Products.FindAsync(1);

        Assert.NotNull(product);
        Assert.Equal("Gaming Laptop", product.Name);
        Assert.Equal(35000, product.Price);
    }

    [Fact]
    public async Task UpdateAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        var dto = new ProductUpdateDto
        {
            Name = "Test",
            Price = 100
        };

        // Act
        var result = await service.UpdateAsync(999, dto);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task DeleteAsync_ShouldDeleteProduct_WhenProductExists()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.Add(new Product
        {
            Name = "Keyboard",
            Price = 1500
        });

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.DeleteAsync(1);

        // Assert
        Assert.True(result);
        Assert.Empty(context.Products);
    }

    [Fact]
    public async Task DeleteAsync_ShouldReturnFalse_WhenProductDoesNotExist()
    {
        // Arrange
        var context = CreateDbContext();
        var service = CreateService(context);

        // Act
        var result = await service.DeleteAsync(999);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task GetAllAsync_FirstPage_ShouldReturnFirstTwoProducts()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.AddRange(
            new Product
            {
                Name = "Product 1",
                Price = 100
            },
            new Product
            {
                Name = "Product 2",
                Price = 200
            },
            new Product
            {
                Name = "Product 3",
                Price = 300
            },
            new Product
            {
                Name = "Product 4",
                Price = 400
            },
            new Product
            {
                Name = "Product 5",
                Price = 500
            }
        );

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(
            new ProductQueryDto
            {
                Page = 1,
                PageSize = 2
            }
        );

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Product 1", result.Items[0].Name);
        Assert.Equal("Product 2", result.Items[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_SecondPage_ShouldReturnNextTwoProducts()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.AddRange(
            new Product
            {
                Name = "Product 1",
                Price = 100
            },
            new Product
            {
                Name = "Product 2",
                Price = 200
            },
            new Product
            {
                Name = "Product 3",
                Price = 300
            },
            new Product
            {
                Name = "Product 4",
                Price = 400
            },
            new Product
            {
                Name = "Product 5",
                Price = 500
            }
        );

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(
            new ProductQueryDto
            {
                Page = 2,
                PageSize = 2
            }
        );

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("Product 3", result.Items[0].Name);
        Assert.Equal("Product 4", result.Items[1].Name);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnCorrectPaginationInformation()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.AddRange(
            new Product
            {
                Name = "Product 1",
                Price = 100
            },
            new Product
            {
                Name = "Product 2",
                Price = 200
            },
            new Product
            {
                Name = "Product 3",
                Price = 300
            },
            new Product
            {
                Name = "Product 4",
                Price = 400
            },
            new Product
            {
                Name = "Product 5",
                Price = 500
            }
        );

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(
            new ProductQueryDto
            {
                Page = 1,
                PageSize = 2
            }
        );

        // Assert
        Assert.Equal(1, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task GetAllAsync_WithSearch_ShouldReturnMatchingProducts()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.AddRange(
            new Product
            {
                Name = "Gaming Laptop",
                Price = 35000
            },
            new Product
            {
                Name = "Office Laptop",
                Price = 20000
            },
            new Product
            {
                Name = "Monitor",
                Price = 8000
            },
            new Product
            {
                Name = "Keyboard",
                Price = 1500
            }
        );

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(
            new ProductQueryDto
            {
                Page = 1,
                PageSize = 10,
                Search = "laptop"
            }
        );

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);

        Assert.Contains(
            result.Items,
            p => p.Name == "Gaming Laptop"
        );

        Assert.Contains(
            result.Items,
            p => p.Name == "Office Laptop"
        );

        Assert.DoesNotContain(
            result.Items,
            p => p.Name == "Monitor"
        );

        Assert.DoesNotContain(
            result.Items,
            p => p.Name == "Keyboard"
        );
    }

    [Fact]
    public async Task GetAllAsync_SortByPriceDescending_ShouldReturnProductsInCorrectOrder()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.AddRange(
            new Product
            {
                Name = "Keyboard",
                Price = 1000
            },
            new Product
            {
                Name = "Laptop",
                Price = 25000
            },
            new Product
            {
                Name = "Monitor",
                Price = 8000
            }
        );

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(
            new ProductQueryDto
            {
                Page = 1,
                PageSize = 10,
                SortBy = "price",
                SortOrder = "desc"
            }
        );

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(25000, result.Items[0].Price);
        Assert.Equal(8000, result.Items[1].Price);
        Assert.Equal(1000, result.Items[2].Price);
    }

    [Fact]
    public async Task GetAllAsync_SortByNameAscending_ShouldReturnProductsInCorrectOrder()
    {
        // Arrange
        var context = CreateDbContext();

        context.Products.AddRange(
            new Product
            {
                Name = "Monitor",
                Price = 8000
            },
            new Product
            {
                Name = "Laptop",
                Price = 25000
            },
            new Product
            {
                Name = "Keyboard",
                Price = 1000
            }
        );

        await context.SaveChangesAsync();

        var service = CreateService(context);

        // Act
        var result = await service.GetAllAsync(
            new ProductQueryDto
            {
                Page = 1,
                PageSize = 10,
                SortBy = "name",
                SortOrder = "asc"
            }
        );

        // Assert
        Assert.Equal(3, result.Items.Count);
        Assert.Equal("Keyboard", result.Items[0].Name);
        Assert.Equal("Laptop", result.Items[1].Name);
        Assert.Equal("Monitor", result.Items[2].Name);
    }
    [Fact]
public async Task CreateAsync_ShouldInvalidateProductCache()
{
    // Arrange
    var context = CreateDbContext();

    context.Products.Add(
        new Product
        {
            Name = "Laptop",
            Price = 25000
        }
    );

    await context.SaveChangesAsync();

    var cache = new MemoryCache(
        new MemoryCacheOptions()
    );

    var service = new ProductService(
        context,
        NullLogger<ProductService>.Instance,
        cache
    );

    var queryDto = new ProductQueryDto
    {
        Page = 1,
        PageSize = 10
    };

    // İlk çağrı cache oluşturur
    var firstResult =
        await service.GetAllAsync(queryDto);

    Assert.Single(firstResult.Items);

    // Yeni ürün eklenir.
    // CreateAsync cache'i temizlemelidir.
    await service.CreateAsync(
        new ProductCreateDto
        {
            Name = "Monitor",
            Price = 8000
        }
    );

    // Act
    var secondResult =
        await service.GetAllAsync(queryDto);

    // Assert
    Assert.Equal(
        2,
        secondResult.Items.Count
    );

    Assert.Contains(
        secondResult.Items,
        p => p.Name == "Laptop"
    );

    Assert.Contains(
        secondResult.Items,
        p => p.Name == "Monitor"
    );
}
}