using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace StayGo.Services
{
    public class ReCaptchaService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;

        public ReCaptchaService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public async Task<bool> VerifyAsync(string token)
        {
            var secretKey = _configuration["GoogleReCaptcha:SecretKey"];
            var response = await _httpClient.PostAsync(
                $"https://www.google.com/recaptcha/api/siteverify?secret={secretKey}&response={token}",
                null
            );

            var jsonString = await response.Content.ReadAsStringAsync();
            var captchaResponse = JsonSerializer.Deserialize<ReCaptchaResponse>(jsonString);
            return captchaResponse.success && captchaResponse.score >= 0.5;
        }
    }

    public class ReCaptchaResponse
    {
        public bool success { get; set; }
        public float score { get; set; }
        public string action { get; set; }
        public DateTime challenge_ts { get; set; }
        public string hostname { get; set; }
    }
}
