using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using Newtonsoft.Json.Linq;
using TextTranslateEchoBot.Utilities;

namespace TextTranslateEchoBot.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        private ILanguageUtilities _languageUtilities;

        public RootDialog(ILanguageUtilities languageUtilities)
        {
            _languageUtilities = languageUtilities;
        }
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            context.PrivateConversationData.TryGetValue<string>("ISOLanguageCode", out string lang);

            var languages = await _languageUtilities.SupportedLanguagesAsync<JObject>();

            var englishText = $"You sent '{activity.Text}', which was originally written in {languages["translation"][lang]["name"].ToString()}";
                
            await context.PostAsync(englishText);            

            context.Wait(MessageReceivedAsync);
        }
    }
}