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
    public partial class BitcoinService : ServiceBase
    {
        private TraceSource trace = new TraceSource("BitcoinService");

        private Process bitcoindProcess;

        private string bitcoindPath;

        public string MainArgs
        {
            get;
            set;
        }

        public BitcoinService()
        {
            trace.TraceEvent(TraceEventType.Information, 1001, "BitcoinService Initialize");
            // Read bitcoindPath
            bitcoindPath = ConfigurationManager.AppSettings["BitcoinInstallDirectory"];
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            trace.TraceEvent(TraceEventType.Information, 1100, "BitcoinService Starting");
            try
            {
                if (string.IsNullOrWhiteSpace(bitcoindPath))
                {
                    bitcoindPath = Environment.GetEnvironmentVariable("ProgramW6432");
                }
                bitcoindPath += "\\daemon\\bitcoind.exe";

                trace.TraceEvent(TraceEventType.Verbose, 0, $"Path: '{bitcoindPath}'");

                bitcoindProcess = new Process {StartInfo = new ProcessStartInfo(bitcoindPath)};
                string startArgs = string.Join(" ", args);
                trace.TraceEvent(TraceEventType.Verbose, 0, $"StartArgs: '{startArgs}'");

                if (!string.IsNullOrEmpty(startArgs))
                {
                    trace.TraceEvent(TraceEventType.Verbose, 0, "Using startArgs");
                    bitcoindProcess.StartInfo.Arguments = startArgs;
                }
                else if (!string.IsNullOrEmpty(MainArgs))
                {
                    trace.TraceEvent(TraceEventType.Verbose, 0, "Using MainArgs");
                    bitcoindProcess.StartInfo.Arguments = MainArgs;
                }

                bitcoindProcess.ErrorDataReceived += Bitcoind_ErrorDataReceived;
                bitcoindProcess.OutputDataReceived += Bitcoind_OutputDataReceived;
                bitcoindProcess.Exited += Bitcoind_Exited;
                bitcoindProcess.EnableRaisingEvents = true;

                bool started = bitcoindProcess.Start();
                trace.TraceEvent(TraceEventType.Verbose, 0, $"Started: {started}");
            }
            catch (Exception ex)
            {
                trace.TraceEvent(TraceEventType.Error, 9100, $"BitcoinService error starting: {ex}");
            }
        }

        private void Bitcoind_Exited(object sender, EventArgs eventArgs)
        {
            int exitCode = bitcoindProcess.ExitCode;
            trace.TraceEvent(TraceEventType.Verbose, 3, $"EXITED: {exitCode}");
        }

        private void Bitcoind_OutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            trace.TraceEvent(TraceEventType.Verbose, 1, $"OUT: {eventArgs.Data}");
        }

        private void Bitcoind_ErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            trace.TraceEvent(TraceEventType.Warning, 2, $"ERROR: {eventArgs.Data}");
        }

        protected override void OnStop()
        {
            trace.TraceEvent(TraceEventType.Information, 8100, "BitcoinService Stopping");
            try
            {
                // This no longer seems to work, due to security issues
                // Process.Start(bitcoindPath, "stop");
                bitcoindProcess.Kill();
                bool exited = bitcoindProcess.WaitForExit(60000);
                trace.TraceEvent(TraceEventType.Verbose, 0,
                    $"Bitcoin exit code: {(exited ? bitcoindProcess.ExitCode.ToString() : exited.ToString())}");
            }
            catch (Exception arg)
            {
                trace.TraceEvent(TraceEventType.Error, 9101, $"BitcoinService error stopping: {arg}");
            }
        }

        public void OnDebug(string[] args)
        {
            this.OnStart(args);
        }
    }
}
