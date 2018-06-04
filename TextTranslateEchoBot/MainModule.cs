using Autofac;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Builder.Scorables;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using TextTranslateEchoBot.Dialogs;
using TextTranslateEchoBot.Utilities;

namespace TextTranslateEchoBot
{
    public sealed class MainModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            #region Register LUIS Dialog plus attributed, etc

            var luisModalAttr = new LuisModelAttribute(ConfigurationManager.AppSettings["luis:ModelId"],
                                                            ConfigurationManager.AppSettings["luis:SubscriptionId"],
                                                            LuisApiVersion.V2,
                                                            ConfigurationManager.AppSettings["luis:Domain"]
                                                            )
            {
                BingSpellCheckSubscriptionKey = ConfigurationManager.AppSettings["luis:BingSpellCheckSubScriptionId"],
                SpellCheck = true
            };

            builder.Register(c => luisModalAttr).AsSelf().AsImplementedInterfaces().SingleInstance();

            builder.Register(c => new MyLuisDialog(c.Resolve<ILuisService>(), c.Resolve<ILanguageUtilities>()))
                .As<MyLuisDialog>()
                .InstancePerDependency();

            builder.RegisterType<LuisService>()
                .Keyed<ILuisService>(FiberModule.Key_DoNotSerialize)
                .AsImplementedInterfaces()
                .SingleInstance();

            #endregion

            builder.Register(c => new RootDialog(c.Resolve<ILanguageUtilities>()))
                     .As<RootDialog>()
                     .InstancePerDependency();
           
            builder.RegisterType<LanguageUtilities>()
                     .As<ILanguageUtilities>()
                     .AsImplementedInterfaces()
                     .SingleInstance();

            // replace the type ChannelSpecificMapper here with your class that implements IMessageActivityMapper
            builder.Register(c => new TranslatorMessageActivityMapper(c.Resolve<ILanguageUtilities>(), c.Resolve<IBotDataStore<BotData>>()))
                     .As<IMessageActivityMapper>()
                     .InstancePerLifetimeScope();

         
        }
    }
}