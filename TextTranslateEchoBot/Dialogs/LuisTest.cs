using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Luis.Models;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using TextTranslateEchoBot.Utilities;

namespace TextTranslateEchoBot.Dialogs
{
    //[LuisModel("eacc009f-0bdd-4540-96da-8623a0fa9e76", "6e088988b6324afd8335b03c0f919f91", LuisApiVersion.V2)]
    [Serializable]
    public class MyLuisDialog : LuisDialog<IMessageActivity>
    {
        private readonly ILanguageUtilities _langUtil;

        public MyLuisDialog(ILuisService luis, ILanguageUtilities langUtil) : base(luis)
        {
            _langUtil = langUtil;
        }

        [LuisIntent("")]
        [LuisIntent("None")]
        public async Task None(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You have triggered the 'None' intent.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("Greeting")]
        public async Task Greeting(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You have triggered the 'Greeting' intent.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("HelpMe")]
        public async Task HelpMe(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You have triggered the 'HelpMe' intent.");
            context.Wait(MessageReceived);
        }

        [LuisIntent("ResetPassword")]
        public async Task ResetPassword(IDialogContext context, LuisResult result)
        {
            await context.PostAsync("You have triggered the 'Resetpassword' intent.");

            context.Wait(MessageReceived);
        }
    }
}