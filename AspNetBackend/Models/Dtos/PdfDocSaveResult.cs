namespace AspNetBackend.Models.Dtos
{
    public class PdfDocSaveResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        // 업로드한 파일 경로
        public string PdfDocPath { get; set; }
    }
}