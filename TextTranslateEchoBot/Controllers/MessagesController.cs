﻿using System;
using System.Collections.Generic;
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
        private ILanguageUtilities _languageUtilities;

        public MessagesController(ILanguageUtilities languageUtilities, IBotDataStore<BotData> botDataStore)
        {
            _botDataStore = botDataStore;
            _languageUtilities = languageUtilities;
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
                    var msgLanguage = await _languageUtilities.DetectInputLanguageAsync<List<AltLanguageDetectResult>>(activity.Text);
                    outputLanguage = msgLanguage[0].language;

                    var key = Address.FromActivity(activity);

                    var userData = await _botDataStore.LoadAsync(key, BotStoreType.BotPrivateConversationData, CancellationToken.None);

                    var storedLangugageCode = userData.GetProperty<string>("ISOLanguageCode");
                    storedLangugageCode = storedLangugageCode ?? _languageUtilities.DefaultLanguage;

                    if (!storedLangugageCode.Equals(outputLanguage))
                    {
                        //save new code
                        userData.SetProperty("ISOLanguageCode", outputLanguage);
                        await _botDataStore.SaveAsync(key, BotStoreType.BotPrivateConversationData, userData, CancellationToken.None);
                        await _botDataStore.FlushAsync(key, CancellationToken.None);
                    }

                    List<AltLanguageTranslateResult> translatedObj = null;
                    //we're assuming English is the bot language. So we will translate incoming non-english to english for processing
                    if (!msgLanguage.Equals(_languageUtilities.DefaultLanguage))
                        translatedObj = await _languageUtilities.TranslateTextAsync<List<AltLanguageTranslateResult>>(activity.Text, _languageUtilities.DefaultLanguage);
                    activity.Text = translatedObj[0].translations[0].text;
                }

                using (var scope = DialogModule.BeginLifetimeScope(Conversation.Container, message))
                {
                    await Conversation.SendAsync(activity, () => scope.Resolve<MyLuisDialog>());
                    //await Conversation.SendAsync(activity, () => scope.Resolve<RootDialog>());
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