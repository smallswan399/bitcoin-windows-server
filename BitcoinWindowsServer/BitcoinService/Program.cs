﻿using System.ServiceProcess;

namespace BitcoinService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new BitcoinService()
            };
            ServiceBase.Run(ServicesToRun);
        }
    }
}
