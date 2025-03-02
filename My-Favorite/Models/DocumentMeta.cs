using System.ComponentModel.DataAnnotations;

namespace WebApplicationUser.Models
{
    namespace WebApplicationUser.Models
    {
        public class DocumentMeta
        {
            [Key]
            public int Id { get; set; }

            [Required]
            public string FileName { get; set; }

            [Required]
            public long FileSize { get; set; }

            public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
            [Required]
            public string FilePath { get; set; }
            [Required]
           public string ContentType { get; set; }
           public string Content { get; set; }
        }
    }

}


