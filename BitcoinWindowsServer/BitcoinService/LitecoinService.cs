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
                    litecoindPath += "\\Litecoin\\daemon\\litecoind.exe";
                }

                trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("Path: '{0}'", litecoindPath));

                litecoindProcess = new Process();
                litecoindProcess.StartInfo = new ProcessStartInfo(litecoindPath);
                string startArgs = string.Join(" ", args);
                trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("StartArgs: '{0}'", startArgs));

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

                litecoindProcess.ErrorDataReceived += new DataReceivedEventHandler(Litecoind_ErrorDataReceived);
                litecoindProcess.OutputDataReceived += new DataReceivedEventHandler(Litecoind_OutputDataReceived);
                litecoindProcess.Exited += new EventHandler(Litecoind_Exited);
                litecoindProcess.EnableRaisingEvents = true;

                bool started = litecoindProcess.Start();
                trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("Started: {0}", started));
            }
            catch (Exception ex)
            {
                trace.TraceEvent(TraceEventType.Error, 9100, string.Format("LitecoinService error starting: {0}", ex));
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
                trace.TraceEvent(TraceEventType.Verbose, 0, string.Format("Litecoin exit code: {0}", exited ? litecoindProcess.ExitCode.ToString() : exited.ToString()));
            }
            catch (Exception arg)
            {
                trace.TraceEvent(TraceEventType.Error, 9101, string.Format("LitecoinService error stopping: {0}", arg));
            }
        }

        private void Litecoind_Exited(object sender, EventArgs eventArgs)
        {
            int exitCode = litecoindProcess.ExitCode;
            trace.TraceEvent(TraceEventType.Verbose, 3, string.Format("EXITED: {0}", exitCode));
        }

        private void Litecoind_OutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            trace.TraceEvent(TraceEventType.Verbose, 1, string.Format("OUT: {0}", eventArgs.Data));
        }

        private void Litecoind_ErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            trace.TraceEvent(TraceEventType.Warning, 2, string.Format("ERROR: {0}", eventArgs.Data));
        }

    }
}
