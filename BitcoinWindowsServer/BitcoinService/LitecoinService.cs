using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace BitcoinService
{
    partial class LitecoinService : ServiceBase
    {
        private TraceSource trace = new TraceSource("LitcoinService");

        private Process litecoindProcess;

        private string litecoindPath;

        public string MainArgs
        {
            get;
            set;
        }
        public LitecoinService()
        {
            trace.TraceEvent(TraceEventType.Information, 1001, "LitecoinService Initialize");
            // Read LitecoindPath
            litecoindPath = ConfigurationManager.AppSettings["LitcoinInstallDirectory"];
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            trace.TraceEvent(TraceEventType.Information, 1100, "LitecoinService Starting");
            try
            {
                if (string.IsNullOrWhiteSpace(litecoindPath))
                {
                    litecoindPath = Environment.GetEnvironmentVariable("ProgramW6432");
                }
                litecoindPath += "\\daemon\\litecoind.exe";

                trace.TraceEvent(TraceEventType.Verbose, 0, $"Path: '{litecoindPath}'");

                litecoindProcess = new Process();
                litecoindProcess.StartInfo = new ProcessStartInfo(litecoindPath);
                string startArgs = string.Join(" ", args);
                trace.TraceEvent(TraceEventType.Verbose, 0, $"StartArgs: '{startArgs}'");

                if (!string.IsNullOrEmpty(startArgs))
                {
                    trace.TraceEvent(TraceEventType.Verbose, 0, "Using startArgs");
                    litecoindProcess.StartInfo.Arguments = startArgs;
                }
                else if (!string.IsNullOrEmpty(MainArgs))
                {
                    trace.TraceEvent(TraceEventType.Verbose, 0, "Using MainArgs");
                    litecoindProcess.StartInfo.Arguments = MainArgs;
                }

                litecoindProcess.ErrorDataReceived += Litecoind_ErrorDataReceived;
                litecoindProcess.OutputDataReceived += Litecoind_OutputDataReceived;
                litecoindProcess.Exited += Litecoind_Exited;
                litecoindProcess.EnableRaisingEvents = true;

                bool started = litecoindProcess.Start();
                trace.TraceEvent(TraceEventType.Verbose, 0, $"Started: {started}");
            }
            catch (Exception ex)
            {
                trace.TraceEvent(TraceEventType.Error, 9100, $"LitecoinService error starting: {ex}");
            }
        }

        protected override void OnStop()
        {
            trace.TraceEvent(TraceEventType.Information, 8100, "LitecoinService Stopping");
            try
            {
                // This no longer seems to work, due to security issues
                // Process.Start(LitecoindPath, "stop");
                litecoindProcess.Kill();
                bool exited = litecoindProcess.WaitForExit(60000);
                trace.TraceEvent(TraceEventType.Verbose, 0,
                    $"Litecoin exit code: {(exited ? litecoindProcess.ExitCode.ToString() : exited.ToString())}");
            }
            catch (Exception arg)
            {
                trace.TraceEvent(TraceEventType.Error, 9101, $"LitecoinService error stopping: {arg}");
            }
        }

        private void Litecoind_Exited(object sender, EventArgs eventArgs)
        {
            int exitCode = litecoindProcess.ExitCode;
            trace.TraceEvent(TraceEventType.Verbose, 3, $"EXITED: {exitCode}");
        }

        private void Litecoind_OutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            trace.TraceEvent(TraceEventType.Verbose, 1, $"OUT: {eventArgs.Data}");
        }

        private void Litecoind_ErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            trace.TraceEvent(TraceEventType.Warning, 2, $"ERROR: {eventArgs.Data}");
        }

    }
}
