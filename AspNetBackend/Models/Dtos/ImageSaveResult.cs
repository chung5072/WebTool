namespace AspNetBackend.Models.Dtos
{
    public class ImageSaveResult
    {
        public bool IsSuccess { get; set; }
        public string ErrorMessage { get; set; }
        public string PrevImageName { get; set; }
        public string AfterImageName { get; set; }
    }
}