using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using EPMI.Core;

namespace EPMI.ServiceControl.Service
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            if (System.Environment.CommandLine.ToLower().Contains("-debug"))
            {
                using (ServiceControlService debugService = new ServiceControlService())
                {
                    debugService.StartUp();
                    Console.WriteLine("Service Started. Press 'Enter' to Exit");
                    string input = string.Empty;
                    while ((input = Console.ReadLine()) != string.Empty)
                    {
                        if (input == "start")
                        {
                            ServicesHandler s1 = new ServicesHandler();
                            s1.Start();
                        }
                        else
                        {

                        }
                    }
                    debugService.ShutDown();
                }
            }
            else
            {
                ServiceBase[] ServicesToRun;
                ServicesToRun = new ServiceBase[] { new ServiceControlService() };
                ServiceBase.Run(ServicesToRun);
            }
        }
    }
}
