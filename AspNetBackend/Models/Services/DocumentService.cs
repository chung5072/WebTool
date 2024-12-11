using AspNetBackend.Models.Dtos;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using Azure.Storage.Blobs;
using System.Configuration;

namespace AspNetBackend.Models.Services
{
    public class DocumentService : IDocumentService
    {
        private readonly HttpContextBase _context;
        // 싱글톤 패턴 - 하나의 HttpClient 인스턴스만 사용
        private readonly IHttpClientService _httpClient;

        public DocumentService(HttpContextBase context, IHttpClientService httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        /// <summary>
        /// 업로드한 pdf 파일 저장
        /// </summary>
        /// <param name="pdfDoc"></param>
        /// <returns></returns>
        public async Task<PdfDocSaveResult> SaveUploadedPdfDocAsync(HttpPostedFileBase pdfDoc)
        {
            try
            {
                // 로컬 저장소
                //// 저장 경로 설정
                //string docDirectoryPath = _context.Server.MapPath("~/uploads/docs");

                //if (!Directory.Exists(docDirectoryPath))
                //{
                //    Directory.CreateDirectory(docDirectoryPath);
                //}

                //string pdfDocPath = Path.Combine(docDirectoryPath, pdfDocName);
                ////System.Diagnostics.Debug.WriteLine("파일 저장 경로: " + pdfDocPath);

                //// 비동기적으로 파일 저장
                //using (var fileStream = new FileStream(pdfDocPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                //{
                //    await pdfDoc.InputStream.CopyToAsync(fileStream);
                //}

                //return new PdfDocSaveResult
                //{
                //    IsSuccess = true,
                //    PdfDocName = pdfDocName,
                //};

                // PDF 파일 이름
                string pdfDocName = Path.GetFileName(pdfDoc.FileName);

                // 클라우드 저장소
                string connectionString = ConfigurationManager.AppSettings["AzureConnectionString"];
                string containerName = ConfigurationManager.AppSettings["AzureContainerName"];

                // 경로 포함하여 blobName 설정
                string blobPath = Path.Combine("uploads", "docs", pdfDocName).Replace("\\", "/");

                // Blob 서비스 클라이언트 생성
                BlobServiceClient blobServiceClient = new BlobServiceClient(connectionString);
                BlobContainerClient containerClient = blobServiceClient.GetBlobContainerClient(containerName);

                // 컨테이너가 없으면 생성
                await containerClient.CreateIfNotExistsAsync();

                // Blob 클라이언트 생성
                BlobClient blobClient = containerClient.GetBlobClient(blobPath);

                // 비동기적으로 파일 업로드
                using (var stream = pdfDoc.InputStream)
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                return new PdfDocSaveResult
                {
                    IsSuccess = true,
                    PdfDocPath = blobPath
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

        /// <summary>
        /// 저장한 pdf 파일을 요약 요청
        /// </summary>
        /// <param name="publicKeyPem">요약한 내용을 암호화할 공개키</param>
        /// <param name="pdfDocName"></param>
        /// <returns></returns>
        public async Task<PdfDocSummarizedResult> SummarizePdfDocAsync(string publicKeyPem, string pdfDocPath)
        {
            // using 문
            // 객체의 리소스를 자동으로 해제
            // 네트워크 연결, 파일 스트림 등의 리소스를 안전하게 해제하는데 사용
            // Dispose() : 가비지 컬렉터가 자동으로 수거하지 못하는 리소스들을 수동으로 해제
            //using (var client = new HttpClient())
            //{
            //}

            // pdf 경로 구성
            // 로컬 서버 
            //string basePath = HttpContext.Current.Server.MapPath("~/uploads/docs");
            //string pdfDocPath = Path.Combine(basePath, pdfDocName);
            //System.Diagnostics.Debug.WriteLine("저장된 pdf 파일 경로: " + pdfDocPath);

            var fastApiUrl = "http://127.0.0.1:8000/api/summary-pdf";

            // 클라우드 서버 
            // var fastApiUrl = "/api/summary-pdf";

            // 요청을 위한 익명 클래스
            var requestData = new
            {
                PdfDocPath = pdfDocPath,
                PublicKeyPem = publicKeyPem,
            };

            try
            {
                return await _httpClient.PostAsync<PdfDocSummarizedResult>(fastApiUrl, requestData);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("[에러] " + ex.Message);
                return new PdfDocSummarizedResult
                {
                    IsSuccess = false,
                    ResultSummarizedContent = null,
                    DecryptionKey = null,
                    EncryptionInitialState = null,
                    AuthTag = null,
                };
            }
        }
    }
}