using Microsoft.Extensions.Configuration;
using PathWeaver.Services;
using Moq;
using Moq.Protected;
using System.Net;
using Xunit;

namespace PathWeaver.Tests
{
    public class WebSearchServiceTests
    {
        [Fact]
        public async Task SearchWeb_WithoutApiKey_ReturnsHelpfulMessage()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["TavilyApiKey"]).Returns((string?)null);
            
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            
            var webSearchService = new WebSearchService(httpClient, mockConfiguration.Object);

            // Act
            var result = await webSearchService.SearchWeb("test query");

            // Assert
            Assert.Contains("API not configured", result);
            Assert.Contains("test query", result);
        }

        [Fact]
        public async Task SearchEducationalContent_BuildsCorrectQuery()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["TavilyApiKey"]).Returns((string?)null);
            
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            
            var webSearchService = new WebSearchService(httpClient, mockConfiguration.Object);

            // Act
            var result = await webSearchService.SearchEducationalContent("JavaScript", "tutorial");

            // Assert
            Assert.Contains("JavaScript tutorial beginner guide learn", result);
            Assert.Contains("API not configured", result);
        }

        [Fact]
        public async Task SearchBestPractices_BuildsCorrectQuery()
        {
            // Arrange
            var mockConfiguration = new Mock<IConfiguration>();
            mockConfiguration.Setup(c => c["TavilyApiKey"]).Returns((string?)null);
            
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            var httpClient = new HttpClient(mockHttpMessageHandler.Object);
            
            var webSearchService = new WebSearchService(httpClient, mockConfiguration.Object);

            // Act
            var result = await webSearchService.SearchBestPractices("React", 2024);

            // Assert
            Assert.Contains("React best practices 2024 industry standards", result);
            Assert.Contains("API not configured", result);
        }
    }
}