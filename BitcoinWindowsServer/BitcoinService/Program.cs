using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinService
{
    static class Program
    {
        private static TraceSource btcTrace = new TraceSource("BitcoinService");
        private static TraceSource ltcTrace = new TraceSource("LitecoinService");

        static void Main(string[] args)
        {
            bool hasArgs = args.Length != 0;
            if (hasArgs)
            {
                var arg0 = args[0].ToLowerInvariant();

                bool help = arg0.Equals("/help") || arg0.Equals("-help") || arg0.Equals("help")
                    || arg0.Equals("/?") || arg0.Equals("-?") || arg0.Equals("?");
                if (help)
                {
                    ShowHelp();
                    return;
                }

                bool installBitcoin = arg0.Equals("/install-bitcoin") || arg0.Equals("-install-bitcoin") || arg0.Equals("install-bitcoin");
                bool installLitecoin = arg0.Equals("/install-litecoin") || arg0.Equals("-install-litecoin") || arg0.Equals("install-litecoin");
                if (installBitcoin)
                {
                    btcTrace.TraceEvent(TraceEventType.Information, 1000, "BitcoinService Install");
                    string mainArgsSuffix = string.Empty;
                    bool hasMainArgs = args.Length > 1;
                    if (hasMainArgs)
                    {
                        mainArgsSuffix = " " + args[1];
                    }
                    Process.Start("sc.exe", "create bitcoind binPath=\"" + Assembly.GetExecutingAssembly().Location + mainArgsSuffix + "\" start= auto obj= \"NT AUTHORITY\\Local Service\" password= \"\" DisplayName= \"Bitcoin Service\"").WaitForExit();
                    Process.Start("sc.exe", "config bitcoind start= delayed-auto").WaitForExit();
                    Console.WriteLine("Service Created");
                    Console.WriteLine("Copy bitcoin.conf to the bitcoin datadir");
                    Console.WriteLine("Default is C:\\Windows\\ServiceProfiles\\LocalService\\AppData\\Roaming\\Bitcoin");
                    return;
                }

                if (installLitecoin)
                {
                    ltcTrace.TraceEvent(TraceEventType.Information, 1000, "LitecoinService Install");
                    string mainArgsSuffix = string.Empty;
                    bool hasMainArgs = args.Length > 1;
                    if (hasMainArgs)
                    {
                        mainArgsSuffix = " " + args[1];
                    }
                    Process.Start("sc.exe", "create litecoind binPath=\"" + Assembly.GetExecutingAssembly().Location + mainArgsSuffix + "\" start= auto obj= \"NT AUTHORITY\\Local Service\" password= \"\" DisplayName= \"Bitcoin Service\"").WaitForExit();
                    Process.Start("sc.exe", "config litecoind start= delayed-auto").WaitForExit();
                    Console.WriteLine("Service Created");
                    Console.WriteLine("Copy litecoin.conf to the litecoin datadir");
                    Console.WriteLine("Default is C:\\Windows\\ServiceProfiles\\LocalService\\AppData\\Roaming\\Litecoin");
                    return;
                }

                bool removeBitcoin = arg0.Equals("/remove-bitcoin") || arg0.Equals("-remove-bitcoin") || arg0.Equals("remove-bitcoin");
                bool removelitecoin = arg0.Equals("/remove-litecoin") || arg0.Equals("-remove-litecoin") || arg0.Equals("remove-litecoin");
                if (removeBitcoin)
                {
                    btcTrace.TraceEvent(TraceEventType.Information, 8000, "BitcoinService Remove");
                    Process.Start("sc.exe", "stop bitcoind").WaitForExit();
                    Process.Start("sc.exe", "delete bitcoind").WaitForExit();
                    Console.WriteLine("Service Deleted");
                    return;
                }
                if (removelitecoin)
                {
                    ltcTrace.TraceEvent(TraceEventType.Information, 8000, "LitecoinService Remove");
                    Process.Start("sc.exe", "stop litecoind").WaitForExit();
                    Process.Start("sc.exe", "delete litecoind").WaitForExit();
                    Console.WriteLine("Service Deleted");
                    return;
                }
            }
            else
            {
                ShowHelp();
            }

            //trace.TraceEvent(TraceEventType.Information, 1002, "BitcoinService Main");
            //string mainArgs = string.Join(" ", args);
            //trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("MainArgs: '{0}'", mainArgs));
            //var servicesToRun = new ServiceBase[]
            //{
            //    new BitcoinService() {
            //        MainArgs = mainArgs
            //    }
            //};
            //ServiceBase.Run(servicesToRun);
            //trace.TraceEvent(TraceEventType.Verbose, 0, "Exit Main");
        }

        private static void ShowHelp()
        {
            Console.WriteLine("Bitcoin Windows Service");
            Console.WriteLine("");
            Console.WriteLine(".\\BitcoinService.exe /install [\"service_args\"]");
            Console.WriteLine("  Installs the Windows Service 'bitcoind', to run with the given arguments");
            Console.WriteLine("  e.g. .\\BitcoinService.exe /install \"-datadir=F:\\Bitcoin\"");
            Console.WriteLine("");
            Console.WriteLine(".\\BitcoinService.exe /remove");
            Console.WriteLine("  Removes the Windows Service");
            return;
        }
    }
}
