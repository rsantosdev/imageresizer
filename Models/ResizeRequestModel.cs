using System.ComponentModel.DataAnnotations;

namespace imageresizer.Models
{
    public class ResizeRequestModel
    {
        [Required]
        public string Name { get; set; }
        [Required]
        public string Format { get; set; }
        public int? Height { get; set; }
        public int? Width { get; set; }
        public int? Quality { get; set; }
    }
}