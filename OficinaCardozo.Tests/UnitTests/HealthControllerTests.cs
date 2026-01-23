using OficinaCardozo.API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Moq;
using OficinaCardozo.Application.Interfaces;

namespace OficinaCardozo.Tests.UnitTests
{
    public class HealthControllerTests
    {
        [Fact]
        public void Live_ReturnsOk()
        {
            // Arrange
            var mockHealthService = new Mock<IHealthService>();
            mockHealthService.Setup(s => s.IsDatabaseHealthy()).Returns(true);
            var controller = new HealthController(
                mockHealthService.Object,
                Mock.Of<Microsoft.Extensions.Logging.ILogger<HealthController>>()
            );

            // Act
            var result = controller.Live();

            // Assert
            Assert.IsType<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(200, okResult.StatusCode ?? 200);
        }
    }
}