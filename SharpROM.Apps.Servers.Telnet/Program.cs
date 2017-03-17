using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SharpROM.Events;
using SharpROM.Events.Abstract;
using SharpROM.Net;
using SharpROM.Net.Abstract;
using SharpROM.Net.Telnet;
using System;

namespace SharpROM.Apps.Servers.Telnet
{
    class Program
    {
        public static IConfigurationRoot Configuration { get; set; }
        public static ServiceCollection services = new ServiceCollection();
        static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder().AddJsonFile("appsettings.json");
            Configuration = builder.Build();

            services.AddOptions();
            services.Configure<SocketListenerSettings>(Configuration);
            services.AddSingleton<TelnetServer>();
            //todo: eventually we will need to pass a factory for a list of IEventManagers
            services.AddSingleton<IEventManager, EventManager>();
            services.AddSingleton<ISocketListener, SocketListener>();
            services.AddSingleton<ISocketReceiveParserFactory, TelnetSocketReceiveParserFactory>();
            services.AddSingleton<IEventRoutingService, EventRoutingService>();
            //ILoggerFactory loggerFactory = new LoggerFactory().AddConsole();
            services.AddLogging();

            var provider = services.BuildServiceProvider();

            //ILoggerFactory loggerFactory 
            provider.GetService<ILoggerFactory>().AddConsole(LogLevel.Trace);

            using (var TelnetService = provider.GetService<TelnetServer>())
            {
                TelnetService.StartServer();
                string closeString = "Z";
                string stringToCompare = "";
                while (stringToCompare != closeString)
                {
                    stringToCompare = Console.ReadLine().ToUpper();
                }
                TelnetService.StopServer();
            }
        }
    }
}