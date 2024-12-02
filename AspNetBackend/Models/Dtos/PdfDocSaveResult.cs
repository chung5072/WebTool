namespace AspNetBackend.Models.Dtos
{
    public class PdfDocSaveResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        // 업로드한 파일 이름
        public string PdfDocName { get; set; }
    }
}