using System;

using System.ComponentModel.DataAnnotations;


namespace RegistrationProject.Models
{
    public class Member
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Class { get; set; }
        [Required]
        public string AdmissionNumber { get; set; }

        [Required]

        public ulong DiscordId { get; set; }

        [Required]

        public bool IsApproved { get; set; }
        public string WhatsApp { get; set; }

        public DateTime RegisteredAt { get; private set; }
        public Member(bool isApproved)
        {
            IsApproved = isApproved;
            RegisteredAt = DateTime.Now;
        }
    }
}
