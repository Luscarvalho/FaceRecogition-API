public class ResponseSource
{
    public string name { get; set; }
    public string position { get; set; }
}

public class Hit
{
    public ResponseSource _source { get; set; }
    public double _score { get; set; }
}

public class Hits
{
    public List<Hit> hits { get; set; }
    public double max_score { get; set; }
}

public class ResponseObject
{
    public Hits hits { get; set; }
}
