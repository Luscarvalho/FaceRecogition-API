using API.Model;
using FaceRecognitionDotNet;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]

public class ImagemController : ControllerBase
{
    [HttpPost("comparar-imagem")]
    public async Task<IActionResult> CompareFaces([FromForm] ImagemModel imagemModel)
    {
        if (imagemModel.Imagem != null && imagemModel.Imagem.Length > 0)
        {
            string nomeDaImagem = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

            var image = Path.Combine("C:/Users/Luscarvalho/Pictures/teste", nomeDaImagem);

            using (var stream = new FileStream(image, FileMode.Create))
            {
                imagemModel.Imagem.CopyTo(stream);
            }

            string modelsDirectory = Environment.CurrentDirectory + "\\models";
            using (var faceRecognition = FaceRecognition.Create(modelsDirectory))
            using (var unknownImage = FaceRecognition.LoadImageFile(image))
            {
                var unknownEncoding = faceRecognition.FaceEncodings(unknownImage).FirstOrDefault();
                if (unknownEncoding != null)
                {
                    using var httpClient = new HttpClient();
                    var requestBody = new
                    {
                        knn = new
                        {
                            field = "face_embeding",
                            query_vector = unknownEncoding.GetRawEncoding(),
                            k = 2,
                            num_candidates = 10
                        },
                        _source = new[] { "name", "position" }
                    };

                    var requestJson = JsonSerializer.Serialize(requestBody);
                    System.IO.File.WriteAllText("C:/Users/Luscarvalho/Pictures/Teste-images/pessoa.json", requestJson);

                    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync("http://localhost:9200/faces/_knn_search", content);

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = await response.Content.ReadAsStringAsync();
                        var responseObject = JsonSerializer.Deserialize<ResponseObject>(responseContent);

                        var hits = responseObject.hits.hits.Select(hit => new
                        {
                            name = hit._source.name,
                            score = hit._score,
                            position = hit._source.position
                        }).ToList();

                        var importantInfo = new
                        {
                            max_score = responseObject.hits.max_score,
                            hits = hits
                        };

                        var importantInfoJson = JsonSerializer.Serialize(importantInfo);

                        Console.WriteLine(importantInfoJson);
                        return Ok(importantInfoJson);
                    }
                    else
                    {
                        Console.WriteLine($"Erro ao enviar a solicitação: {response.StatusCode}");
                        return BadRequest("Imagem não salva");
                    }
                }
                else
                {
                    BadRequest("Nenhuma face encontrada na imagem");
                }
            }
            return Ok("Imagem recebida com sucesso!");
        }
        else
        {
            return BadRequest("Nenhuma imagem recebida.");
        }

    }


    [HttpPost("salvar-imagem")]
    public async Task<IActionResult> SalvarImagemAsync([FromForm] ImagemModel imagemModel, [FromForm] string nome)
    {
        if (imagemModel.Imagem != null && imagemModel.Imagem.Length > 0)
        {
            string nomeDaImagem = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".jpg";

            var image = Path.Combine("C:/Users/Luscarvalho/Pictures/teste", nomeDaImagem);

            using (var stream = new FileStream(image, FileMode.Create))
            {
                imagemModel.Imagem.CopyTo(stream);
            }

            string modelsDirectory = Environment.CurrentDirectory + "\\models";
            using (var faceRecognition = FaceRecognition.Create(modelsDirectory))
            using (var unknownImage = FaceRecognition.LoadImageFile(image))
            {
                var unknownEncoding = faceRecognition.FaceEncodings(unknownImage).FirstOrDefault();
                if (unknownEncoding != null)
                {
                    var requestBody = new
                    {
                        name = nome,
                        face_embeding = unknownEncoding.GetRawEncoding(),
                        position = "frontal"
                    };
                    var requestJson = JsonSerializer.Serialize(requestBody);
                    //System.IO.File.WriteAllText("C:/Users/Luscarvalho/Pictures/Teste-images/pessoa.json", requestJson);

                    using var httpClient = new HttpClient();
                    var content = new StringContent(requestJson, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync("http://localhost:9200/faces/_doc/", content);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Erro ao enviar a solicitação: {response.StatusCode}");
                        return BadRequest("Imagem não salva");
                    }
                }
                else
                {
                    BadRequest("Nenhuma face encontrada na imagem");
                }
            }
            return Ok("Imagem recebida e salva com sucesso!");
        }
        else
        {
            return BadRequest("Nenhuma imagem recebida.");
        }

    }
}