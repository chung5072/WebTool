using AspNetBackend.Models.Services;
using System;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace AspNetBackend.Controllers
{
    [RoutePrefix("api/images")]
    public class ImageController : ApiController
    {
        private readonly IImageService _imageService;

        public ImageController(IImageService imageService)
        {
            _imageService = imageService;
        }

        /// <summary>
        /// 요청: 이미지 업로드
        /// 방향: Vue.js -> ASP.NET MVC 5
        /// HTTP 본문에 업로드한 실제 이미지 데이터가 들어있는 것을 의미합니다. 
        /// HTTP 본문은 HTTP 요청의 마지막 부분에 위치하며, 클라이언트가 서버로 데이터를 전송하는 데 사용됩니다. 
        /// 이미지 업로드의 경우, HTTP 본문에는 이미지 데이터가 포함되어 전송됩니다.
        /// 이미지 데이터는 HTTP 본문에 다음과 같은 형식으로 포함됩니다:
        /// Content-Type: multipart/form-data로 설정되어, 
        /// 여러 파일을 포함할 수 있는 멀티파트 본문으로 구성됩니다.
        /// Content-Length: 본문의 길이를 지정하여, 서버가 데이터를 올바르게 처리할 수 있도록 합니다.
        /// 이미지 데이터 자체는 HTTP 본문에 포함되어 전송되며, 
        /// 서버에서는 이 데이터를 처리하여 이미지를 저장하거나 처리할 수 있습니다.
        /// 예를 들어, ASP.NET MVC 5에서는 HttpPostedFileBase를 사용하여 
        /// HTTP 본문에서 이미지 데이터를 추출하고 처리할 수 있습니다.
        /// </summary>
        /// <returns>요청 상태 및 이미지 저장 경로</returns>
        [HttpPost]
        [Route("upload")]
        public async Task<IHttpActionResult> UploadCompareImages()
        {
            try
            {
                Console.WriteLine("Entered UploadCompareImages method");

                // HttpRequestBase로 변환
                var requestBase = new HttpRequestWrapper(HttpContext.Current.Request);
                Console.WriteLine("Number of files received: " + requestBase.Files.Count);

                // 이미지 비교를 위해 2개의 이미지가 필요
                if (requestBase.Files.Count != 2)
                {
                    return BadRequest("Two images are required");
                }

                // HttpPostedFileBase로 변환
                var prevImage = requestBase.Files[0];
                Console.WriteLine("Prev image filename: " + prevImage.FileName);
                var afterImage = requestBase.Files[1];
                Console.WriteLine("After image filename: " + afterImage.FileName);

                // 1. 이미지 저장 처리
                var imageResult = await _imageService.SaveUploadedImagesAsync(prevImage, afterImage);
                if (!imageResult.IsSuccess)
                {
                    return BadRequest(imageResult.ErrorMessage);
                }

                // 2. FastAPI 비교 처리
                var comparisonResult = await _imageService.SendToFastApiForComparisonAsync(imageResult.PrevImageName, imageResult.AfterImageName);
                if (!comparisonResult.IsSuccess)
                {
                    return BadRequest("Failed to compare images");
                }

                return Ok(new
                {
                    ComparisonResult = comparisonResult.Result,
                    PrevImageUrl = $"/uploads/images/{imageResult.PrevImageName}",
                    AfterImageUrl = $"/uploads/images/{imageResult.AfterImageName}"
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