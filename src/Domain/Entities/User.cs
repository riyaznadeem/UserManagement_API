namespace Domain.Entities
{
    public class User : BaseEntity
    {
        public Guid Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public byte[] PasswordHash { get; set; }
        public byte[] PasswordSalt { get; set; }
        public Guid RoleId { get; set; }         // Foreign Key
        public Role? Role { get; set; }           // Navigation Property
    }
}
