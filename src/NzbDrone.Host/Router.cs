using System;
using NLog;
using NzbDrone.Common;
using NzbDrone.Common.EnvironmentInfo;
using NzbDrone.Common.Processes;
using IServiceProvider = NzbDrone.Common.IServiceProvider;


namespace NzbDrone.Host
{
    public class Router
    {
        private readonly INzbDroneServiceFactory _nzbDroneServiceFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConsoleService _consoleService;
        private readonly IProcessProvider _processProvider;
        private readonly Logger _logger;

        public Router(INzbDroneServiceFactory nzbDroneServiceFactory,
                      IServiceProvider serviceProvider,
                      IConsoleService consoleService,
                      IProcessProvider processProvider,
                      Logger logger)
        {
            _nzbDroneServiceFactory = nzbDroneServiceFactory;
            _serviceProvider = serviceProvider;
            _consoleService = consoleService;
            _processProvider = processProvider;
            _logger = logger;
        }

        public void Route(ApplicationModes applicationModes)
        {
            _logger.Info("Application mode: {0}", applicationModes);

            switch (applicationModes)
            {
                case ApplicationModes.Service:
                    {
                        _logger.Debug("Service selected");
                        _serviceProvider.Run(_nzbDroneServiceFactory.Build());
                        break;
                    }

                case ApplicationModes.Interactive:
                    {
                        _logger.Debug("Console selected");
                        _nzbDroneServiceFactory.Start();
                        break;
                    }
                case ApplicationModes.InstallService:
                    {
                        _logger.Debug("Install Service selected");
                        if (_serviceProvider.ServiceExist(ServiceProvider.SERVICE_NAME))
                        {
                            _consoleService.PrintServiceAlreadyExist();
                        }
                        else
                        {
                            _serviceProvider.Install(ServiceProvider.SERVICE_NAME);
                            _serviceProvider.SetPermissions(ServiceProvider.SERVICE_NAME);

                            // Start the service and exit.
                            // Ensures that there isn't an instance of Sonarr already running that the service account cannot stop.
                            _processProvider.SpawnNewProcess("sc.exe", $"start {ServiceProvider.SERVICE_NAME}", null, true);
                        }
                        break;
                    }
                case ApplicationModes.UninstallService:
                    {
                        _logger.Debug("Uninstall Service selected");
                        if (!_serviceProvider.ServiceExist(ServiceProvider.SERVICE_NAME))
                        {
                            _consoleService.PrintServiceDoesNotExist();
                        }
                        else
                        {
                            _serviceProvider.Uninstall(ServiceProvider.SERVICE_NAME);
                        }

                        break;
                    }
                default:
                    {
                        _consoleService.PrintHelp();
                        break;
                    }
            }
        }


    }
}
