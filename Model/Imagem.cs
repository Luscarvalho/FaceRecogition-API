using System.ComponentModel.DataAnnotations;

namespace API.Model;

public class ImagemModel
{
    [Required]
    public IFormFile? Imagem { get; set; }
}
