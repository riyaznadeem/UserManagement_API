﻿namespace Domain.Entities
{
    public class Role :BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public ICollection<User> Users { get; set; } = new List<User>();  // Inverse Navigation
    }
}
