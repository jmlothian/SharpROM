using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharpROM.Events.Abstract;
using SharpROM.Net.Abstract;
using SharpROM.Net.Telnet;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpROM.Apps.Servers.Telnet
{
    public class TelnetServer : IDisposable
    {

        // Injected services.
        protected IEventRoutingService EventRoutingService { get; set; }
        protected ISocketListener SocketListener { get; set; }

        // Other members.
        //private List<IEventManager> EventManagers { get; set; }

        protected bool _isRunning;

        public TelnetServer(
            IEventRoutingService eventRoutingService,
            ISocketListener socketListener            
            )
        {
            //Log = LogManager.GetCurrentClassLogger();

            // Store references to all injected services.
            // TODO

            EventRoutingService = eventRoutingService;
            SocketListener = socketListener;
             
            //
            // Perform additional initialization and configuration.
            //

            // TODO ?

            // At this point all services should be initialized but not started or processing.
            //Log.Debug("SharpROM Server service construction completed.");
          
        }
        public virtual void StartServer()
        {
            //Log.Info("Starting SharpROM Server!");

            // TODO - start services
            EventRoutingService.ProcessQueues();
            //Log.Info("SharpROM Server is now running!");
            _isRunning = true;
        }

        public virtual void StopServer()
        {
            //Log.Info("Stopping SharpROM Server!");

            // TODO - stop services and clean up

            //Log.Info("SharpROM Server is no longer running!");
            _isRunning = false;
        }

        public virtual bool IsRunning()
        {
            return _isRunning;
        }

        public virtual void Dispose()
        {
            EventRoutingService.Dispose();
        }
    }
}
