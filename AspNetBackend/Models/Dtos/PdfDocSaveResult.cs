namespace AspNetBackend.Models.Dtos
{
    public class PdfDocSaveResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string PdfDocName { get; set; }
    }
}