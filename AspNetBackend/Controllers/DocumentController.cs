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
        /// 클라이언트에서 업로드한 파일 저장 및 파일 요약
        /// </summary>
        /// <returns>요청 상태 및 PDF 저장 경로</returns>
        [HttpPost]
        [Route("upload-pdf-doc")]
        public async Task<IHttpActionResult> UploadPdfDoc()
        {
            try
            {
                //System.Diagnostics.Debug.WriteLine("UploadPdfDoc 메서드 실행");

                // HttpRequestBase로 변환
                var requestBase = new HttpRequestWrapper(HttpContext.Current.Request);

                // HttpPostedFileBase로 변환
                var publicKeyPem = requestBase.Form["publicKeyPem"];
                var pdfDoc = requestBase.Files["pdfDoc"];
                //System.Diagnostics.Debug.WriteLine("암호화할 Public Key: " + publicKeyPem);
                //System.Diagnostics.Debug.WriteLine("저장할 PDF filename: " + pdfDoc.FileName);

                if (pdfDoc == null || string.IsNullOrEmpty(publicKeyPem))
                {
                    return BadRequest("공개키 혹은 pdf 문서가 존재하지 않음");
                }

                // 1. PDF 문서 저장 처리
                var pdfDocSaveResult = await _documentService.SaveUploadedPdfDocAsync(pdfDoc);
                if (!pdfDocSaveResult.IsSuccess)
                {
                    return BadRequest(pdfDocSaveResult.ErrorMessage);
                }
                //System.Diagnostics.Debug.WriteLine("저장된 PDF 파일 이름: " + pdfDocSaveResult.PdfDocName);

                // 2. AI를 활용한 분석 처리
                var pdfDocAnalysisResult = await _documentService.SummarizePdfDocAsync(publicKeyPem, pdfDocSaveResult.PdfDocName);
                if (!pdfDocAnalysisResult.IsSuccess)
                {
                    return BadRequest("분석 처리 실패");
                }

                return Ok(new
                {
                    PdfDocName = $"{pdfDocSaveResult.PdfDocName}",
                    pdfDocAnalysisResult.ResultDocName,
                    pdfDocAnalysisResult.DecryptionKey,
                    pdfDocAnalysisResult.EncryptionInitialState,
                    pdfDocAnalysisResult.AuthTag
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("업로드 및 분석 요청 중 예외: " + ex.Message);
                return InternalServerError(ex);
            }
        }
    }
}