using System.ComponentModel.DataAnnotations;

namespace API.Model;

public class Pessoa
{
    [Required]
    public string? Nome { get; set; }
    [Required]
    public string? Photo { get; set; }
}