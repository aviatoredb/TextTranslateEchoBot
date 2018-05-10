using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using Autofac;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using TextTranslateEchoBot.Dialogs;
using TextTranslateEchoBot.Utilities;

namespace TextTranslateEchoBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        private IBotDataStore<BotData> _botDataStore;
        public MessagesController(IBotDataStore<BotData> botDataStore)
        {
            _botDataStore = botDataStore;
        }

        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                var message = activity as IMessageActivity;

                string outputLanguage = string.Empty;

                if (activity.Text != null)
                {
                    //detect incoming langugage
                    var msgLanguage = await LanguageUtilities.DetectInputLanguageAsync<LanguageDetectResult>(activity.Text);
                    outputLanguage = msgLanguage.Result[0].language;

                    var key = Address.FromActivity(activity);

                    try
                    {
                        var userData = await _botDataStore.LoadAsync(key, BotStoreType.BotPrivateConversationData, CancellationToken.None);

                        var storedLangugageCode = userData.GetProperty<string>("ISOLanguageCode");
                        storedLangugageCode = storedLangugageCode ?? "en";

                        if (!storedLangugageCode.Equals(outputLanguage))
                        {
                            //save new code
                            userData.SetProperty("ISOLanguageCode", outputLanguage);
                            await _botDataStore.SaveAsync(key, BotStoreType.BotPrivateConversationData, userData, CancellationToken.None);
                            await _botDataStore.FlushAsync(key, CancellationToken.None);
                        }
                    }
                    catch (Exception ex)
                    {
                        //yes, need proper exception handling here
                    }

                    //we're assumign English is the bot language. So we will translate incoming non-english to english for processing
                    //if (!msgLanguage.Equals("en"))
                    var translatedObj = await LanguageUtilities.TranslateTextAsync<AltLanguageTranslateResult>(activity.Text, "en");
                    activity.Text = translatedObj.Result[0].translations[0].text;

                }

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    await Conversation.SendAsync(activity, () => scope.Resolve<RootDialog>());
                }
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}