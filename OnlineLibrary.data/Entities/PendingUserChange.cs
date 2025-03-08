using System;
using System.ComponentModel.DataAnnotations;

namespace OnlineLibrary.Data.Entities
{
    public class PendingUserChange
    {
        [Key]
        public Guid Id { get; set; }

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        public string FieldName { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public DateTime ChangeRequestedAt { get; set; }
        public bool IsApproved { get; set; }
        public DateTime? ApprovedAt { get; set; }
    }
}
