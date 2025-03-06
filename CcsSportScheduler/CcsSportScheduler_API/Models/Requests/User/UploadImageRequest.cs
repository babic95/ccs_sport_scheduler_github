namespace CcsSportScheduler_API.Models.Requests.User
{
    public class UploadImageRequest
    {
        public int UserId { get; set; }
        public IFormFile Image { get; set; } = null!;
    }
}
