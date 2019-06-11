using System.IO;
using System.ServiceProcess;
using System.Threading;
using Serilog;

namespace BitcoinService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {
            var dir = System.Reflection.Assembly.GetExecutingAssembly().Location;
            dir = System.IO.Path.GetDirectoryName(dir);
            var file = Path.Combine(dir, "log-{Date}.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Verbose()
                .WriteTo
                .RollingFile(file, retainedFileCountLimit: 30)
                .CreateLogger();
            var servicesToRun = new ServiceBase[]
            {
                new BitcoinService()
            };
            ServiceBase.Run(servicesToRun);
        }
    }
}
