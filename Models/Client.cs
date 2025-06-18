using System.ComponentModel.DataAnnotations;

namespace tradetrackr.api.Models
{
    public class Client
    {
        [Key]
        public Guid Id { get; set; }
        public string UserId { get; set; } // From Auth0
        [Required]
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }

        //public ICollection<Job> Jobs { get; set; } = new List<Job>();
    }
}
