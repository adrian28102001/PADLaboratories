using ApplicationManagementService.Controllers;
using ApplicationManagementService.Entities;
using ApplicationManagementService.Models;
using ApplicationManagementService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ApplicationManagementService.Services;
using Microsoft.Extensions.Options;

namespace ApplicationManagementService.Tests
{
    [TestFixture]
    public class ApplicationControllerTests
    {
        private Mock<IRepository<Application>> _mockRepository;
        private Mock<IEmailService> _mockEmailService;
        private Mock<IFileStorageService> _mockFileStorageService;
        private IOptions<EmailSettings> _emailSettings;
        private ApplicationController _controller;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new Mock<IRepository<Application>>();
            _mockEmailService = new Mock<IEmailService>();
            _mockFileStorageService = new Mock<IFileStorageService>();
            var emailSettings = new EmailSettings
            {
                Recipient = "gherman.adrian28@gmail.com",
                Subject = "CV"
            };
            
            _emailSettings = Options.Create(emailSettings);

            _controller = new ApplicationController(_mockRepository.Object, _mockEmailService.Object,
                _mockFileStorageService.Object, _emailSettings);
        }

        [Test]
        public async Task GetApplications_ReturnsAllApplications()
        {
            // Arrange
            var applications = new List<Application>
            {
                new Application(),
                new Application(),
                new Application()
            };
            _mockRepository.Setup(repo => repo.GetAllAsync()).ReturnsAsync(applications);

            // Act
            var result = await _controller.GetApplications();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            var returnValue = okResult.Value as List<Application>;
            Assert.AreEqual(3, returnValue.Count);
        }

        [Test]
        public async Task GetApplicationById_ReturnsNotFound()
        {
            // Arrange
            int testId = 1;
            _mockRepository.Setup(repo => repo.GetByIdAsync(testId)).ReturnsAsync((Application)null);

            // Act
            var result = await _controller.GetApplication(testId);

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }
    }
}