using System.Threading.Tasks;

namespace TextTranslateEchoBot.Utilities
{
    public interface ILanguageUtilities
    {
        Task<T> DetectInputLanguageAsync<T>(string inputText);
        Task<T> SupportedLanguages<T>();
        Task<T> TranslateTextAsync<T>(string inputText, string outputLanguage);
    }
}