using AspNetBackend.Controllers;
using AspNetBackend.Models.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace UnitTestAboutDocument
{
    [TestClass]
    public class DocumentControllerTest
    {
        [TestMethod]
        public async Task UploadPdfDoc_ReturnsBadRequest_WhenPdfDocIsNull()
        {
            // Arrange
            // 모킹 - 실제 HTTP 요청 없이도 컨트롤러의 동작을 독립적으로 테스트
            var mockDocumentService = new Mock<IDocumentService>();
            var mockContext = new Mock<HttpContextBase>();
            var mockRequest = new Mock<HttpRequestBase>();
            var mockFiles = new Mock<HttpFileCollectionBase>();

            // 파일이 없음을 시뮬레이션
            // PDF 문서가 업로드되지 않았을 때 -> BadRequest 응답을 반환 확인
            mockFiles.Setup(f => f["pdfDoc"]).Returns((HttpPostedFileBase)null);  // null 파일 설정

            // Form 데이터 설정
            // 폼 데이터를 설정하여 공개키가 제공되는 상황을 모킹
            var formCollection = new NameValueCollection();
            formCollection.Add("publicKeyPem", "somePublicKey");  // 공개키 설정

            mockRequest.Setup(r => r.Files).Returns(mockFiles.Object);
            mockRequest.Setup(r => r.Form).Returns(formCollection);

            // HttpContextBase의 Request 설정
            mockContext.Setup(c => c.Request).Returns(mockRequest.Object);

            var controller = new DocumentController(mockDocumentService.Object, mockContext.Object);

            // Act
            IHttpActionResult actionResult = await controller.UploadPdfDoc();

            // Assert
            Assert.IsInstanceOfType(actionResult, typeof(BadRequestErrorMessageResult));
        }
    }
}
