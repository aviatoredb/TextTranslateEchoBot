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

        public static async Task<T> SupportedLanguages<T>()
        {
            var path = $"languages?api-version=3.0&scope=translation";
            return await ExecuteAPI<T>(path, String.Empty);
        }

        public static async Task<T> DetectInputLanguageAsync<T>(string inputText)
        {
            var path = $"detect?api-version=3.0";
            return await ExecuteAPI<T>(path, inputText);
        }

        public static async Task<T> TranslateTextAsync<T>(string inputText, string outputLanguage)
        {
            var path = $"translate?api-version=3.0&to={outputLanguage}&includeSentenceLength=true";
            return await ExecuteAPI<T>(path, inputText);
        }
    
        private static async Task<T> ExecuteAPI<T>(string apiPath, string bodyText)
        {
            string requestBody = String.Empty;
            if (!String.IsNullOrEmpty(bodyText))
            {
                System.Object[] body = new System.Object[] { new { Text = bodyText } };
                requestBody = JsonConvert.SerializeObject(body);
            }

            string apiKey = ConfigurationManager.AppSettings["trns:APIKey"];
            string url = ConfigurationManager.AppSettings["trns:APIEndpoint"];
            var uri = new Uri($"{url}/{apiPath}");

            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = !String.IsNullOrEmpty(requestBody) ? HttpMethod.Post : HttpMethod.Get;
                request.RequestUri = uri;
                request.Content = !String.IsNullOrEmpty(requestBody) ? new StringContent(requestBody, Encoding.UTF8, "application/json") : null;
                request.Headers.Add("Ocp-Apim-Subscription-Key", apiKey);

                var response = await client.SendAsync(request);
                var responseBody = await response.Content.ReadAsStringAsync();

                var setting = new JsonSerializerSettings();
                setting.StringEscapeHandling = StringEscapeHandling.EscapeNonAscii;

                var result = JsonConvert.DeserializeObject<T>(responseBody);

                return result;
            }
        }
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
        public LanguageTrnaslationSentenceLen sentLen { get; set; }
    }

    public class LanguageTrnaslationSentenceLen
    {
        public List<int> srcSentLen { get; set; }
        public List<int> transSentLen { get; set; }
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