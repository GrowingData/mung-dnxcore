using System.ComponentModel.DataAnnotations;

namespace GrowingData.Mung.Web.Models
{
    public class Artist
    {
        public int ArtistId { get; set; }

        [Required]
        public string Name { get; set; }
    }
}