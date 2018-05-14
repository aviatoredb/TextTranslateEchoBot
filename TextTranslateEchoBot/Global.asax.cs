using Autofac;
using Autofac.Integration.WebApi;
using Microsoft.Bot.Builder.Azure;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Internals;
using Microsoft.Bot.Builder.Internals.Fibers;
using Microsoft.Bot.Builder.Luis;
using Microsoft.Bot.Connector;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Http;
using System.Web.Routing;
using TextTranslateEchoBot.Dialogs;
using TextTranslateEchoBot.Utilities;

namespace TextTranslateEchoBot
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            var store = new InMemoryDataStore();
            var config = GlobalConfiguration.Configuration;

            Conversation.UpdateContainer(
                       builder =>
                       {
                           builder.RegisterModule(new AzureModule(Assembly.GetExecutingAssembly()));

                           builder.RegisterModule<MainModule>();

                           builder.Register(c => store)
                                             .Keyed<IBotDataStore<BotData>>(AzureModule.Key_DataStore)
                                             .AsSelf()
                                             .SingleInstance();

                           builder.Register(c => new CachingBotDataStore(store,
                                      CachingBotDataStoreConsistencyPolicy
                                      .ETagBasedConsistency))
                                      .As<IBotDataStore<BotData>>()
                                      .AsSelf()
                                      .InstancePerLifetimeScope();

                           builder.RegisterApiControllers(Assembly.GetExecutingAssembly());
                           builder.RegisterWebApiFilterProvider(config);
                       });
            config.DependencyResolver = new AutofacWebApiDependencyResolver(Conversation.Container);

            GlobalConfiguration.Configure(WebApiConfig.Register);
        }
    }
}
