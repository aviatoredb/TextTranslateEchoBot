using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
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

            // calculate something for us to return
            int length = (activity.Text ?? string.Empty).Length;

            var translatedtext = await LanguageUtilities.TranslateTextAsync($"You sent {activity.Text} which was {length} characters", lang);

            // return our reply to the user
            await context.PostAsync(translatedtext.Result[0].translations[0].text);

            context.Wait(MessageReceivedAsync);
        }
    }
}