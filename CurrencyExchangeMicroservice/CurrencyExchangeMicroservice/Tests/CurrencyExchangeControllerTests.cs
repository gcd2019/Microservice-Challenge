using Xunit;
using CurrencyExchangeMicroservice.Controllers;
using CurrencyExchangeMicroservice.Models;
using CurrencyExchangeMicroservice.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;


namespace CurrencyExchangeMicroservice.Tests
{
    public class CurrencyExchangeControllerTests
    {
        private readonly CurrencyExchangeController _controller;
        private readonly CurrencyExchangeDbContext _dbContext;
        private readonly FixerApiClient _fixerApiClient;
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<CurrencyExchangeController> _logger;
        private readonly ITestOutputHelper _output;

        public CurrencyExchangeControllerTests(ITestOutputHelper output)
        {
            _output = output;

            var services = new ServiceCollection();
            services.AddHttpClient();
            services.AddMemoryCache();

            // Add a configuration for the test environment.
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    { "FixerApi:ApiKey", "your_api_key" }
                })
                .Build();
            services.AddSingleton<IConfiguration>(configuration);

            services.AddDbContext<CurrencyExchangeDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));
            services.AddScoped<FixerApiClient>();
            services.AddSingleton<ILogger<CurrencyExchangeController>>(NullLogger<CurrencyExchangeController>.Instance);
            var serviceProvider = services.BuildServiceProvider();

            _dbContext = serviceProvider.GetRequiredService<CurrencyExchangeDbContext>();
            _fixerApiClient = serviceProvider.GetRequiredService<FixerApiClient>();
            _memoryCache = serviceProvider.GetRequiredService<IMemoryCache>();
            _logger = serviceProvider.GetRequiredService<ILogger<CurrencyExchangeController>>();

            _controller = new CurrencyExchangeController(_fixerApiClient, _dbContext, _memoryCache, _logger);
        }

        [Fact]
        public async Task Post_ReturnsBadRequest_WhenFromCurrencyIsInvalid()
        {
            // Arrange
            var request = new CurrencyExchangeRequest
            {
                FromCurrency = "INVALID",
                ToCurrency = "USD",
                Amount = 100
            };

            // Act
            var result = await _controller.Post(request);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task Post_InvalidInput_ReturnsBadRequest()
        {
            // Arrange
            var invalidRequest = new CurrencyExchangeRequest
            {
                FromCurrency = "INVALID",
                ToCurrency = "USD",
                Amount = 100
            };

            // Act
            var result = await _controller.Post(invalidRequest);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
        }

        [Fact]
        public async Task DeleteAllTrades_NoTrades_ReturnsNotFound()
        {
            // Act
            var result = await _controller.DeleteAllTrades();

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        //[Fact]
        //public async Task Post_ValidInput_ReturnsOk()
        //{
        //    // Arrange
        //    var request = new CurrencyExchangeRequest
        //    {
        //        FromCurrency = "USD",
        //        ToCurrency = "EUR",
        //        Amount = 100
        //    };

        //    // Act
        //    var result = await _controller.Post(request);

        //    // Assert
        //    if (result.Result is BadRequestObjectResult badRequestResult)
        //    {
        //        _output.WriteLine($"Error: {badRequestResult.Value}");
        //    }

        //    Assert.IsType<OkObjectResult>(result.Result);
        //}

        [Fact]
        public async Task Post_ExceedTradeLimit_ReturnsStatusCode429()
        {
            // Arrange
            var request = new CurrencyExchangeRequest
            {
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100
            };

            // Simulate exceeding the trade limit by making multiple requests
            string clientId = "192.168.0.1"; // Use a valid IPv4 address
            var remoteIpAddress = System.Net.IPAddress.Parse(clientId);

            for (int i = 0; i < 10; i++)
            {
                // Set the client's IP address in the HttpContext
                _controller.ControllerContext.HttpContext = new DefaultHttpContext
                {
                    Connection = { RemoteIpAddress = remoteIpAddress }
                };

                // Make a request to increment the trade count
                await _controller.Post(request);
            }

            // Set the client's IP address in the HttpContext for the 11th request
            _controller.ControllerContext.HttpContext = new DefaultHttpContext
            {
                Connection = { RemoteIpAddress = remoteIpAddress }
            };

            // Act
            var result = await _controller.Post(request);

            // Assert
            Assert.IsType<ObjectResult>(result.Result);
            var objectResult = result.Result as ObjectResult;
            Assert.Equal(429, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetTrades_ReturnsTradeHistory()
        {
            // Arrange
            // Add trades to the in-memory database
            var trade1 = new CurrencyExchangeTrade
            {
                Id = Guid.NewGuid(),
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100,
                ConvertedAmount = 85,
                ExchangeRate = 0.85m,
                Timestamp = DateTime.UtcNow
            };

            var trade2 = new CurrencyExchangeTrade
            {
                Id = Guid.NewGuid(),
                FromCurrency = "EUR",
                ToCurrency = "USD",
                Amount = 100,
                ConvertedAmount = 115,
                ExchangeRate = 1.15m,
                Timestamp = DateTime.UtcNow
            };

            _dbContext.CurrencyExchangeTrades.AddRange(trade1, trade2);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.GetTrades();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var trades = Assert.IsType<List<CurrencyExchangeTrade>>(okResult.Value);
            Assert.Equal(2, trades.Count);
            Assert.Contains(trade1, trades);
            Assert.Contains(trade2, trades);
        }


        [Fact]
        public async Task DeleteAllTrades_WithTrades_ReturnsOk()
        {
            // Arrange
            // Add a trade to the in-memory database
            _dbContext.CurrencyExchangeTrades.Add(new CurrencyExchangeTrade
            {
                Id = Guid.NewGuid(),
                FromCurrency = "USD",
                ToCurrency = "EUR",
                Amount = 100,
                ConvertedAmount = 85,
                ExchangeRate = 0.85m,
                Timestamp = DateTime.UtcNow
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _controller.DeleteAllTrades();

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }


    }
}
