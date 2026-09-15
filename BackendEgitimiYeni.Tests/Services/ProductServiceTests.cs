using BackendEgitimiYeni.Data;
using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.EntityFrameworkCore;

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

    [Fact]
    public async Task CreateAsync_ShouldCreateProduct()
    {
        // Arrange
        var context = CreateDbContext();
        var service = new ProductService(context);

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

    context.Products.Add(new()
    {
        Name = "Monitor",
        Price = 8000
    });

    await context.SaveChangesAsync();

    var service = new ProductService(context);

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
    var service = new ProductService(context);

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

    context.Products.Add(new()
    {
        Name = "Laptop",
        Price = 25000
    });

    await context.SaveChangesAsync();

    var service = new ProductService(context);

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
    var service = new ProductService(context);

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

    context.Products.Add(new()
    {
        Name = "Keyboard",
        Price = 1500
    });

    await context.SaveChangesAsync();

    var service = new ProductService(context);

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
    var service = new ProductService(context);

    // Act
    var result = await service.DeleteAsync(999);

    // Assert
    Assert.False(result);
}
}