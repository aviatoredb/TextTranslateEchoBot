using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace TextTranslateEchoBot.Utilities
{
    public static class LanguageUtilities
    {

        public static async Task<LanguageAPIResult<T>> DetectInputLanguageAsync<T>(string inputText)
        {
            var path = $"detect?api-version=3.0";
            return await ExecuteAPI<T>(path, inputText);
        }

        public static async Task<LanguageAPIResult<T>> TranslateTextAsync<T>(string inputText, string outputLanguage)
        {
            var path = $"translate?api-version=3.0&to={outputLanguage}";
            return await ExecuteAPI<T>(path, inputText);
        }

        private static async Task<LanguageAPIResult<T>> ExecuteAPI<T>(string apiPath, string bodyText)
        {
            System.Object[] body = new System.Object[] { new { Text = bodyText } };
            var requestBody = JsonConvert.SerializeObject(body);

            string apiKey = ConfigurationManager.AppSettings["trns:APIKey"];
            string url = ConfigurationManager.AppSettings["trns:APIEndpoint"];
            var uri = new Uri($"{url}/{apiPath}");

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = uri;
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                var setting = new JsonSerializerSettings();
                setting.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;
                var result = JsonConvert.DeserializeObject<List<T>>(responseBody);

                return new LanguageAPIResult<T>(result);
            }
        }
    }

    public class LanguageAPIResult<T>
    {
        public LanguageAPIResult(List<T> t)
        {
            Result = t;
        }

        public List<T> Result { get; set; }
    }

    public class AltLanguageTranslateResult
    {
        public LanguageDetectResult detectedLanguage { get; set; }
        public List<LanguageTranslation> translations { get; set; }
    }

    public class LanguageTranslation
    {
        public string text { get; set; }
        public string to { get; set; }
    }

    public class AltLanguageDetectResult
    {
        public string language { get; set; }
        public double score { get; set; }
        public bool isTranslationSupported { get; set; }
        public bool isTransliterationSupported { get; set; }
    }

    public class LanguageDetectResult
    {
        public string language { get; set; }
        public double score { get; set; }
        public bool isTranslationSupported { get; set; }
        public bool isTransliterationSupported { get; set; }
        public List<AltLanguageDetectResult> alternatives { get; set; }
    }

}