using AspNetBackend.Models.Services;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AspNetBackend.Controllers
{
    [RoutePrefix("api/document")]
    public class DocumentController : ApiController
    {
        private readonly IDocumentService _documentService;

        public DocumentController(IDocumentService documentService)
        {
            _documentService = documentService;
        }

        /// <summary>
        /// 요청: PDF 문서 업로드
        /// 방향: Vue.js -> ASP.NET MVC 5
        /// HTTP 본문에 업로드한 PDF 데이터가 들어있는 것을 의미합니다. 
        /// HTTP 본문은 HTTP 요청의 마지막 부분에 위치하며, 클라이언트가 서버로 데이터를 전송하는 데 사용됩니다. 
        /// PDF 업로드의 경우, HTTP 본문에는 PDF 데이터가 포함되어 전송됩니다.
        /// PDF 데이터는 HTTP 본문에 다음과 같은 형식으로 포함됩니다:
        /// Content-Type: multipart/form-data로 설정되어, 
        /// 여러 파일을 포함할 수 있는 멀티파트 본문으로 구성됩니다.
        /// Content-Length: 본문의 길이를 지정하여, 서버가 데이터를 올바르게 처리할 수 있도록 합니다.
        /// PDF 데이터 자체는 HTTP 본문에 포함되어 전송되며, 
        /// 서버에서는 이 데이터를 처리하여 PDF를 저장하거나 처리할 수 있습니다.
        /// 예를 들어, ASP.NET MVC 5에서는 HttpPostedFileBase를 사용하여 
        /// HTTP 본문에서 PDF 데이터를 추출하고 처리할 수 있습니다.
        /// </summary>
        /// <returns>요청 상태 및 PDF 저장 경로</returns>
        [HttpPost]
        [Route("upload-pdf-doc")]
        public async Task<IHttpActionResult> UploadPdfDoc()
        {
            try
            {
                Console.WriteLine("Entered UploadPdfDoc method");

                // HttpRequestBase로 변환
                var requestBase = new HttpRequestWrapper(HttpContext.Current.Request);

                // HttpPostedFileBase로 변환
                var publicKeyPem = requestBase.Form["publicKeyPem"];
                var pdfDoc = requestBase.Files["pdfDoc"];
                Console.WriteLine("PDF filename: " + pdfDoc.FileName);

                if (pdfDoc == null || string.IsNullOrEmpty(publicKeyPem))
                {
                    return BadRequest("Invalid PublicKeyPem or PDF document.");
                }

                Console.WriteLine("Public Key: " + publicKeyPem);
                Console.WriteLine("PDF filename: " + pdfDoc.FileName);

                // 1. PDF 문서 저장 처리
                var pdfDocSaveResult = await _documentService.SaveUploadedPdfDocAsync(pdfDoc);
                if (!pdfDocSaveResult.IsSuccess)
                {
                    return BadRequest(pdfDocSaveResult.ErrorMessage);
                }

                // 2. AI를 활용한 분석 처리
                var pdfDocAnalysisResult = await _documentService.AnalyzePdfDocAsync(publicKeyPem, pdfDocSaveResult.PdfDocName);
                if (!pdfDocAnalysisResult.IsSuccess)
                {
                    return BadRequest("Failed to analyze");
                }

                return Ok(new
                {
                    PdfDocName = $"{pdfDocSaveResult.PdfDocName}",
                    ResultDocName = pdfDocAnalysisResult.ResultDocName
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception occurred: " + ex.Message);
                return InternalServerError(ex);
            }
        }
    }
}