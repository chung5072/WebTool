using AspNetBackend.Models.Dtos;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AspNetBackend.Models.Services
{
    public class DocumentService : IDocumentService
    {
        public async Task<PdfDocSaveResult> SaveUploadedPdfDocAsync(HttpPostedFileBase pdfDoc)
        {
            try
            {
                // 파일 저장
                string pdfDocName = Path.GetFileName(pdfDoc.FileName);

                // 저장 경로 설정
                string docDirectoryPath = HttpContext.Current.Server.MapPath("~/uploads/docs");
                if (!Directory.Exists(docDirectoryPath))
                {
                    Directory.CreateDirectory(docDirectoryPath);
                }

                string pdfDocPath = Path.Combine(docDirectoryPath, pdfDocName);

                await Task.Run(() =>
                {
                    pdfDoc.SaveAs(pdfDocPath);
                });

                return new PdfDocSaveResult
                {
                    IsSuccess = true,
                    PdfDocName = pdfDocName,
                };
            }
            catch (Exception ex)
            {
                return new PdfDocSaveResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message,
                };
            }
        }

        public async Task<PdfDocAnalysisResult> AnalyzePdfDocAsync(string publicKeyPem, string pdfDocName)
        {
            using (var client = new HttpClient())
            {
                var fastApiUrl = "http://localhost:8000/api/analyze";
                // 이미지 경로 구성
                string basePath = HttpContext.Current.Server.MapPath("~/uploads/docs");
                string pdfDocPath = Path.Combine(basePath, pdfDocName);

                // Create an anonymous object with the correct structure
                var requestData = new
                {
                    public_key_pem = publicKeyPem,
                    pdf_doc_path = pdfDocPath,
                };

                var response = await client.PostAsJsonAsync(fastApiUrl, requestData);
                if (response.IsSuccessStatusCode)
                {
                    string resultFilePath = await response.Content.ReadAsStringAsync();
                    
                    return new PdfDocAnalysisResult
                    {
                        IsSuccess = true,
                        ResultDocName = resultFilePath.Trim('"') // Trim quotes from JSON string
                    };
                }

                return new PdfDocAnalysisResult { IsSuccess = false, ResultDocName = null };
            }
        }
    }
}