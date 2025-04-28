using Microsoft.AspNetCore.Mvc;
using BlogAPI.Helper;

namespace TefaTodoList.Controllers
{
    [Route("api/files")]
    [ApiController]
    public class FilesManagementController : ControllerBase
    {
        [HttpGet("{*pathFile}")]
        public IActionResult GetFile(string pathFile, [FromQuery] bool download = false)
        {
            // Tentukan path volume Docker tempat file disimpan
            var uploadsFolder =
                "wwwroot"; // Sesuaikan dengan path volume Docker kamu

            // Gabungkan path dengan nama file untuk mendapatkan lokasi file
            var decodedFileName = Uri.UnescapeDataString(pathFile);
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), uploadsFolder, decodedFileName);
            
            // Cek apakah file ada
            if (!System.IO.File.Exists(filePath))
            {
                return ApiResponse.NotFound("File not found");
            }

            // Ambil file sebagai stream
            var fileBytes = System.IO.File.ReadAllBytes(filePath);

            // Dapatkan extension file untuk menentukan content type
            var fileExtension = Path.GetExtension(pathFile).ToLowerInvariant();
            string contentType = fileExtension switch
            {
                ".jpg" => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".pdf" => "application/pdf",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".txt" => "text/plain", 
                _ => "application/octet-stream",
            };

            // Jika download = true, beri header agar file diunduh
            if (download)
            {
                return File(fileBytes, contentType, decodedFileName);
            }

            // Jika tidak, kembalikan file untuk ditampilkan di browser
            return File(fileBytes, contentType);
        }
    }
}