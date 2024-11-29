using AspNetBackend.Controllers;
using AspNetBackend.Models.Dtos;
using AspNetBackend.Models.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace UnitTestAboutImage
{
    [TestClass]
    public class ImageControllerTest
    {
        private Mock<IImageService> _mockImageService;
        private ImageController _controller;

        [TestInitialize]
        public void Setup()
        {
            _mockImageService = new Mock<IImageService>();
            _controller = new ImageController(_mockImageService.Object);
        }

        /// <summary>
        /// 잘못된 이미지 업로드 테스트
        /// 실제 비교를 위해서는 2개의 이미지를 업로드 해야함.
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task UploadCompareImages_WithLessThanTwoFiles_ReturnsBadRequest()
        {
            // Arrange
            var mockHttpContext = new Mock<HttpContextBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            var mockFiles = new Mock<HttpFileCollectionBase>();

            mockFiles.Setup(f => f.Count).Returns(1);
            mockRequest.Setup(r => r.Files).Returns(mockFiles.Object);
            mockHttpContext.Setup(c => c.Request).Returns(mockRequest.Object);
            HttpContext.Current = new HttpContext(new HttpRequest(null, "http://example.com", null), new HttpResponse(null));

            // Act
            IHttpActionResult result = await _controller.UploadCompareImages();

            // Assert
            Assert.IsInstanceOfType(result, typeof(BadRequestErrorMessageResult));
            var badRequestResult = result as BadRequestErrorMessageResult;
            Assert.AreEqual("Two images are required", badRequestResult.Message);
        }

        /// <summary>
        /// FastAPI에 요청을 보내는 service 함수 테스트
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        public async Task SendToFastApi_ReturnsComparisonResult()
        {
            // Arrange
            var prevImageName = "test1.jpg";
            var afterImageName = "test2.jpg";

            var expectedComparisonResult = new ComparisonResult
            {
                IsSuccess = true,
                Result = "comparison result"
            };

            _mockImageService
                .Setup(s => s.SendToFastApiForComparisonAsync(
                    prevImageName,
                    afterImageName))
                .ReturnsAsync(expectedComparisonResult);

            // Act
            // _imageService 대신 _mockImageService.Object 사용
            var result = await _mockImageService.Object.SendToFastApiForComparisonAsync(prevImageName, afterImageName);

            // Assert
            Assert.IsTrue(result.IsSuccess);
            Assert.AreEqual(expectedComparisonResult.Result, result.Result);
        }
    }
}
