using AspNetBackend.Models.Dtos;
using System.Threading.Tasks;
using System.Web;

namespace AspNetBackend.Models.Services
{
    public interface IImageService
    {
        Task<ImageSaveResult> SaveUploadedImagesAsync(HttpPostedFileBase prevImage, HttpPostedFileBase afterImage);
        Task<ComparisonResult> SendToFastApiForComparisonAsync(string prevPath, string afterPath);
    }
}
