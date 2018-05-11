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

        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);

            return Task.CompletedTask;
        }

        private async Task MessageReceivedAsync(IDialogContext context, IAwaitable<object> result)
        {
            var activity = await result as Activity;

            context.PrivateConversationData.TryGetValue<string>("ISOLanguageCode", out string lang);

            var languages = await LanguageUtilities.SupportedLanguages<JObject>();

            if (lang.Equals("en") && !String.IsNullOrEmpty(activity.Text))
            {
                await context.PostAsync($"You sent '{activity.Text}' which is {activity.Text.Length} characters");
            }
            else {
                var translatedtext = await LanguageUtilities.TranslateTextAsync<List<AltLanguageTranslateResult>>($"You sent '{activity.Text}', which was written in {languages["translation"][lang]["nativeName"].ToString()}", lang);

                var englishText = await LanguageUtilities.TranslateTextAsync<List<AltLanguageTranslateResult>>($"You sent '{activity.Text}', which was originally written in {languages["translation"][lang]["name"].ToString()}", "en");

                var transString = $"{translatedtext[0].translations[0].text}";
                var engString = $"{englishText[0].translations[0].text}";

                // return our reply to the user
                await context.PostAsync($"{transString}\n\n{engString}");
            }

            context.Wait(MessageReceivedAsync);
        }
    }
}