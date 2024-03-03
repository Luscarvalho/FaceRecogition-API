using System.ComponentModel.DataAnnotations;

namespace API.Model;

public class ResponseObject
{
    [Required]
    public Hits? hits { get; set; }
}

public class Hits
{
    [Required]
    public double? max_score { get; set; }
    [Required]
    public List<Hit>? hits { get; set; }
}

public class Hit
{
    [Required]
    public Source? _source { get; set; }
    [Required]
    public double? _score { get; set; }
}

public class Source
{
    [Required]
    public string? name { get; set; }
    [Required]
    public string? position { get; set; }
}