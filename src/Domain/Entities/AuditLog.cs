using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class AuditLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string TableName { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty; // e.g., primary key value
        public string? Action { get; set; }             // Insert / Update / Delete
        public string? PerformedBy { get; set; }
        public string? IPAddress { get; set; }
        public DateTime PerformedAt { get; set; }

        public string? Changes { get; set; }            // Optional: JSON string of field changes
    }

}
