namespace AspNetBackend.Models.Dtos
{
    public class PdfDocSummarizedResult
    {
        public bool IsSuccess { get; set; }
        // 암호화된 요약 결과
        public string ResultSummarizedContent { get; set; }
        public string DecryptionKey { get; set; }
        public string EncryptionInitialState { get; set; }
        public string AuthTag { get; set; }
    }
}