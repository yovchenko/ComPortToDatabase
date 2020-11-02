using System;
using Topshelf;
using log4net.Config;

namespace com_port_to_database
{
    class Program
    {
        static void Main(string[] args)
        {
            //Load configuration from log4net.config 
            XmlConfigurator.Configure();

            // The console application can be installed as a service using Topshelf
            // To register this application as a Windows service: Service.exe install
            // To remove this application as a Windows service: Service.exe uninstall
            var exitCode = HostFactory.Run(hostConfig => {
                hostConfig.UseAssemblyInfoForServiceInfo();
                hostConfig.RunAsLocalSystem();
                hostConfig.UseLog4Net();
                hostConfig.Service<Service>(serviceConfig =>
                {
                    serviceConfig.ConstructUsing(service => new Service());
                    serviceConfig.WhenStarted(service => service.OnStart());
                    serviceConfig.WhenStopped(service => service.OnStop());
                });
            });
            int exitCodeValue = (int)Convert.ChangeType(exitCode, exitCode.GetTypeCode());
            Environment.ExitCode = exitCodeValue;
        }
    }
}
