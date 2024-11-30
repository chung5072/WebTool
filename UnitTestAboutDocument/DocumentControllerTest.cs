using AspNetBackend.Controllers;
using AspNetBackend.Models.Dtos;
using AspNetBackend.Models.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;

namespace UnitTestAboutDocument
{
    [TestClass]
    public class DocumentControllerTest
    {
        private Mock<IDocumentService> _mockDocumentService;
        private DocumentController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockDocumentService = new Mock<IDocumentService>();
            _controller = new DocumentController(_mockDocumentService.Object);
        }

        [TestMethod]
        public async Task SendToFastApi_ReturnAnalyzeResult()
        {
            // Arrange
            var pdfDocName = "test1.jpg";

            var expectedAnalysisResult = new PdfDocAnalysisResult
            {
                IsSuccess = true,
                AnalysisResult = "Analysis Result"
            };

            _mockDocumentService
                .Setup(service => service.AnalyzePdfDocAsync(pdfDocName))
                .ReturnsAsync(expectedAnalysisResult);

            // Act
            var result = await _mockDocumentService.Object.AnalyzePdfDocAsync(pdfDocName);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(expectedAnalysisResult.AnalysisResult, result.AnalysisResult);
        }
    }
}
