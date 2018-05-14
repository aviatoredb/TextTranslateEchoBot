using System.Threading.Tasks;

namespace TextTranslateEchoBot.Utilities
{
    public interface ILanguageUtilities
    {
        string DefaultLanguage { get; }
        Task<T> DetectInputLanguageAsync<T>(string inputText);
        Task<T> SupportedLanguagesAsync<T>();
        Task<T> TranslateTextAsync<T>(string inputText, string outputLanguage);
    }
}