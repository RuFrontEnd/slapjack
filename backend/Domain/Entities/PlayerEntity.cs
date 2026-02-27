namespace Domain.Entities
{
    public class PlayerEntity(string connectionId, string name)
    {
        public string? ConnectionId { get; set; } = connectionId;
        public string? Name { get; set; } = name;
    }
}
