using AspNetBackend.Models.Dtos;
using System.Threading.Tasks;
using System.Web;

namespace AspNetBackend.Models.Services
{
    public interface IDocumentService
    {
        Task<PdfDocSaveResult> SaveUploadedPdfDocAsync(HttpPostedFileBase pdfDoc);
        Task<PdfDocSummarizedResult> SummarizePdfDocAsync(string publicKeyPem, string pdfDocName);
    }
}
