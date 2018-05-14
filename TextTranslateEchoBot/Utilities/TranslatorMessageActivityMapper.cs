using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using TextTranslateEchoBot.Utilities;

namespace TextTranslateEchoBot.Utilities
{
    public class TranslatorMessageActivityMapper : IMessageActivityMapper
    {
        IBotDataStore<BotData> _botDataStore;
        ILanguageUtilities _languageUtilities;

        public TranslatorMessageActivityMapper(ILanguageUtilities languageUtilities, IBotDataStore<BotData> botDataStore)
        {
            _botDataStore = botDataStore;
            _languageUtilities = languageUtilities;
        }

        public IMessageActivity Map(IMessageActivity message)
        {
            if (String.IsNullOrEmpty(message.Text))
                return message;

            Task<string> translation = Task<String>.Factory.StartNew(() =>
            {
                //store key is based on user to bot data. We need to build this a little different
                var key = Address.FromActivity(message);
                var userKey = new Address(key.UserId, key.ChannelId, key.BotId, key.ConversationId, key.ServiceUrl);

                var userData = _botDataStore.LoadAsync(userKey, BotStoreType.BotPrivateConversationData, CancellationToken.None).Result;

                var storedLangugageCode = userData.GetProperty<string>("ISOLanguageCode");
                storedLangugageCode = storedLangugageCode ?? _languageUtilities.DefaultLanguage;

                var translatedText = _languageUtilities.TranslateTextAsync<List<AltLanguageTranslateResult>>(message.Text, storedLangugageCode).Result;

                return translatedText[0].translations[0].text;
            });

            message.Text = translation.Result;

            return message;
        }
    }
}