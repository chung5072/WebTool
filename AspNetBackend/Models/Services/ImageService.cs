using AspNetBackend.Models.Dtos;
using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace AspNetBackend.Models.Services
{
    public class ImageService : IImageService
    {
        /// <summary>
        /// 기능: 이미지 저장 
        /// </summary>
        /// <param name="prevImage"></param>
        /// <param name="afterImage"></param>
        /// <returns>이미지 저장 상태</returns>
        public async Task<ImageSaveResult> SaveUploadedImagesAsync(HttpPostedFileBase prevImage, HttpPostedFileBase afterImage)
        {
            try
            {
                // 파일 저장 로직...
                string prevFileName = Path.GetFileName(prevImage.FileName);
                string afterFileName = Path.GetFileName(afterImage.FileName);

                // 저장 경로 설정...
                string savePath = HttpContext.Current.Server.MapPath("~/uploads/images");

                if (!Directory.Exists(savePath))
                {
                    Directory.CreateDirectory(savePath);
                }

                string prevFilePath = Path.Combine(savePath, prevFileName);
                string afterFilePath = Path.Combine(savePath, afterFileName);

                // 비동기로 파일 저장
                await Task.Run(() =>
                {
                    prevImage.SaveAs(prevFilePath);
                    afterImage.SaveAs(afterFilePath);
                });

                return new ImageSaveResult
                {
                    IsSuccess = true,
                    PrevImageName = prevFileName,
                    AfterImageName = afterFileName
                };
            }
            catch (Exception ex)
            {
                return new ImageSaveResult
                {
                    IsSuccess = false,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<ComparisonResult> SendToFastApiForComparisonAsync(string prevImageName, string afterImageName)
        {
            using (var client = new HttpClient())
            {
                var fastApiUrl = "http://localhost:8000/compare";
                // 이미지 경로 구성
                string basePath = HttpContext.Current.Server.MapPath("~/uploads/images");
                string prevPath = Path.Combine(basePath, prevImageName);
                string afterPath = Path.Combine(basePath, afterImageName);

                var requestData = new
                {
                    prevImagePath = prevPath,
                    afterImagePath = afterPath,
                };

                var response = await client.PostAsJsonAsync(fastApiUrl, requestData);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsStringAsync();
                    return new ComparisonResult
                    {
                        IsSuccess = true,
                        Result = result
                    };
                }

                return new ComparisonResult { IsSuccess = false };
            }
        }
    }
}