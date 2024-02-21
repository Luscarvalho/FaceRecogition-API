using API.Model;
using FaceRecognitionDotNet;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]

public class ImagemController : ControllerBase
{
    [HttpPost("enviar-imagem")]
    public IActionResult EnviarImagem([FromForm] ImagemModel imagemModel)
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

                    var unknownData = new { name = "Ramon Rodrigues", face_embeding = unknownEncoding.GetRawEncoding(), position = "frontal" };
                    var unknownJson = JsonSerializer.Serialize(unknownData);
                    System.IO.File.WriteAllText("C:/Users/Luscarvalho/Pictures/Teste-images/pessoa.json", unknownJson);

                    var pessoas = JsonSerializer.Deserialize<List<Pessoa>>(System.IO.File.ReadAllText
                        ("C:/Users/Luscarvalho/Pictures/teste/FacesKnow.json"));

                    //var pessoas = JsonSerializer.Deserialize<Pessoa>(System.IO.File.ReadAllText
                        //("C:/Users/Luscarvalho/Pictures/teste/FacesKnow.json"));

                    foreach (var pessoa in pessoas)
                    {
                        var imagepath = pessoa.Photo;
                        var knownImage = FaceRecognition.LoadImageFile(imagepath);
                        //var t = FaceRecognition.LoadFaceEncoding(knownImage);
                        var knowEncoding = faceRecognition.FaceEncodings(knownImage).FirstOrDefault();
                        var resultado = FaceRecognition.CompareFace(knowEncoding, unknownEncoding);
                        if (resultado)
                        {
                            Console.WriteLine($"O rosto pertence a: {pessoa.Nome}");
                            return Ok($"O rosto pertence a: {pessoa.Nome}");
                        }
                    }
                    return Ok("O rosto não pertence a nenhum cliente");
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
}