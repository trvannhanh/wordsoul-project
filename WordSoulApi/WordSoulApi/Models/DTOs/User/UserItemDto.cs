namespace WordSoulApi.Models.DTOs.User
{
    public class UserItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Type { get; set; }
        public int Quantity { get; set; }
    }
}
