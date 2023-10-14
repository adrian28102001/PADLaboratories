using ApplicationManagementService.Controllers;
using ApplicationManagementService.Entities;
using ApplicationManagementService.Models;
using ApplicationManagementService.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Threading.Tasks;
using ApplicationManagementService.Services;
using Microsoft.AspNetCore.Http;
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
                Recipient = "gherman.adrian2001@gmail.com",
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

        [Test]
        public async Task PostApplication_ReturnsCreatedAtAction_SendsEmail()
        {
            // Arrange
            var applicationModel = new ApplicationModel
            {
                /* ... initialize ... */
            };
            _mockFileStorageService.Setup(service => service.SaveFileAsync(It.IsAny<IFormFile>()))
                .ReturnsAsync("path/to/file");

            // Act
            var result = await _controller.PostApplication(applicationModel);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result.Result);
            var createdAtActionResult = result.Result as CreatedAtActionResult;
            Assert.That(createdAtActionResult?.ActionName, Is.EqualTo("GetApplication"));

            _mockEmailService.Verify(service => service.SendEmailWithAttachmentAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()), Times.Once);
        }
    }
}