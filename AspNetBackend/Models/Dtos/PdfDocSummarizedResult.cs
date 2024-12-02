namespace AspNetBackend.Models.Dtos
{
    public class PdfDocSummarizedResult
    {
        public bool IsSuccess { get; set; }
        // 요약 내용을 저장한 파일 이름
        public string ResultDocName { get; set; }
        public string DecryptionKey { get; set; }
        public string EncryptionInitialState { get; set; }
        public string AuthTag { get; set; }
    }
}