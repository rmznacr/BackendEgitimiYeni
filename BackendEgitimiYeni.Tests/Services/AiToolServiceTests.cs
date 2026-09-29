using BackendEgitimiYeni.DTOs;
using BackendEgitimiYeni.Services;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace BackendEgitimiYeni.Tests.Services;

public class AiToolServiceTests
{
    private readonly Mock<IAiService> _aiServiceMock;
    private readonly Mock<IAiToolExecutor> _toolExecutorMock;
    private readonly Mock<ILogger<AiToolService>> _loggerMock;

    private readonly AiToolService _service;

    public AiToolServiceTests()
    {
        _aiServiceMock =
            new Mock<IAiService>();

        _toolExecutorMock =
            new Mock<IAiToolExecutor>();

        _loggerMock =
            new Mock<ILogger<AiToolService>>();

        _service =
            new AiToolService(
                _aiServiceMock.Object,
                _toolExecutorMock.Object,
                _loggerMock.Object
            );
    }

    [Fact]
    public async Task ProcessAsync_WhenProductToolIsSelected_ShouldExecuteTool()
    {
        var request =
            new AiToolRequestDto
            {
                Message =
                    "1000 TL'den ucuz ürünleri göster."
            };

        _aiServiceMock
            .SetupSequence(x =>
                x.AskAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new AiResponseDto
                {
                    Response =
                        """
                        {
                          "useTool": true,
                          "toolName": "get_products",
                          "arguments": {
                            "search": null,
                            "minPrice": null,
                            "maxPrice": 1000
                          }
                        }
                        """
                }
            )
            .ReturnsAsync(
                new AiResponseDto
                {
                    Response =
                        "1000 TL'den ucuz ürünler bulundu."
                }
            );

        _toolExecutorMock
            .Setup(x =>
                x.ExecuteGetProductsAsync(
                    It.IsAny<GetProductsToolArgumentsDto>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new List<ProductResponseDto>
                {
                    new ProductResponseDto
                    {
                        Id = 1,
                        Name = "Mouse",
                        Price = 500
                    },

                    new ProductResponseDto
                    {
                        Id = 2,
                        Name = "Keyboard",
                        Price = 900
                    }
                }
            );

        var result =
            await _service.ProcessAsync(
                request
            );

        Assert.True(
            result.ToolUsed
        );

        Assert.Equal(
            "get_products",
            result.ToolName
        );

        _toolExecutorMock.Verify(
            x =>
                x.ExecuteGetProductsAsync(
                    It.Is<GetProductsToolArgumentsDto>(
                        arguments =>
                            arguments.MaxPrice == 1000 &&
                            arguments.MinPrice == null &&
                            arguments.Search == null
                    ),
                    It.IsAny<CancellationToken>()
                ),
            Times.Once
        );
    }

    [Fact]
    public async Task ProcessAsync_WhenToolIsNotNeeded_ShouldNotExecuteTool()
    {
        var request =
            new AiToolRequestDto
            {
                Message =
                    "Dependency Injection nedir?"
            };

        _aiServiceMock
            .SetupSequence(x =>
                x.AskAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new AiResponseDto
                {
                    Response =
                        """
                        {
                          "useTool": false,
                          "toolName": null,
                          "arguments": null
                        }
                        """
                }
            )
            .ReturnsAsync(
                new AiResponseDto
                {
                    Response =
                        "Dependency Injection bir bağımlılık yönetim yaklaşımıdır."
                }
            );

        var result =
            await _service.ProcessAsync(
                request
            );

        Assert.False(
            result.ToolUsed
        );

        Assert.Null(
            result.ToolName
        );

        _toolExecutorMock.Verify(
            x =>
                x.ExecuteGetProductsAsync(
                    It.IsAny<GetProductsToolArgumentsDto>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }

    [Fact]
    public async Task ProcessAsync_WhenLlmReturnsInvalidJson_ShouldNotExecuteTool()
    {
        var request =
            new AiToolRequestDto
            {
                Message =
                    "Bana yardımcı olur musun?"
            };

        _aiServiceMock
            .SetupSequence(x =>
                x.AskAsync(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(
                new AiResponseDto
                {
                    Response =
                        "Bu geçerli bir JSON değil."
                }
            )
            .ReturnsAsync(
                new AiResponseDto
                {
                    Response =
                        "Elbette, nasıl yardımcı olabilirim?"
                }
            );

        var result =
            await _service.ProcessAsync(
                request
            );

        Assert.False(
            result.ToolUsed
        );

        _toolExecutorMock.Verify(
            x =>
                x.ExecuteGetProductsAsync(
                    It.IsAny<GetProductsToolArgumentsDto>(),
                    It.IsAny<CancellationToken>()
                ),
            Times.Never
        );
    }
}