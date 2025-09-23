namespace WordSoulApi.Models.Entities
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty ;
        public string? ImageUrl { get; set; }
        public ItemType Type { get; set; }
        public DateTime CreatedDate { get; set;} = DateTime.Now;

        public List<UserItem> UserItems { get; set; } = new List<UserItem>();
    }

    public enum ItemType
    {
        Currency,
        Evolution,
        Booster
    }
}
