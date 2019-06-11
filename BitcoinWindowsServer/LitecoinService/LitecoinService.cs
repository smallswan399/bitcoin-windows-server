using Serilog;
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace LitecoinService
{
    partial class LitecoinService : ServiceBase
    {
        private Process _litecoindProcess;

        private readonly string _litecoindPath;
        private readonly string _cliPath;
        private readonly string _dataDir;

        public LitecoinService()
        {
            var installDir = ConfigurationManager.AppSettings["InstallDirectory"];
            if (string.IsNullOrWhiteSpace(installDir)) installDir = Environment.GetEnvironmentVariable("ProgramW6432");
            if (string.IsNullOrWhiteSpace(installDir))
                throw new Exception();
            _litecoindPath = Path.Combine(installDir, "daemon\\litecoind.exe");
            if (!File.Exists(_litecoindPath))
                throw new FileNotFoundException(null, _litecoindPath);

            _cliPath = Path.Combine(installDir, "daemon\\litecoin-cli.exe");
            if (!File.Exists(_cliPath))
                throw new FileNotFoundException(null, _cliPath);

            _dataDir = GetDataDir();
            InitializeComponent();
        }

        private string GetDataDir()
        {
            var dataDir = ConfigurationManager.AppSettings["DataDir"];
            if (string.IsNullOrWhiteSpace(dataDir) || HasWhiteSpace(dataDir))
            {
                throw new Exception("Invalid datadir");
            }

            return dataDir;
        }

        private string GetArgs()
        {
            return $"-datadir={_dataDir}";
        }


        private bool HasWhiteSpace(string s)
        {
            if (s == null)
                throw new ArgumentNullException("s");

            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsWhiteSpace(s[i]))
                    return true;
            }
            return false;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                var startArgs = GetArgs();
                _litecoindProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(_litecoindPath, startArgs)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };

                _litecoindProcess.OutputDataReceived += Litecoind_OutputDataReceived;
                _litecoindProcess.Exited += Litecoind_Exited;
                _litecoindProcess.EnableRaisingEvents = true;

                _litecoindProcess.Start();
                _litecoindProcess.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LitecoinService error starting");
            }
        }

        protected override void OnStop()
        {
            try
            {
                Process.Start(_cliPath, GetArgs() + " stop")?.WaitForExit();
                while (!_litecoindProcess.HasExited)
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception arg)
            {
                Log.Error(arg, "LitecoinService error stopping");
            }
        }

        private void Litecoind_Exited(object sender, EventArgs eventArgs)
        {
            int exitCode = _litecoindProcess.ExitCode;
            Log.Verbose("EXITED: {exitCode}", exitCode);
        }

        private void Litecoind_OutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            Log.Verbose("OUT: {data}", eventArgs.Data);
        }

        private void Litecoind_ErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            //_trace.TraceEvent(TraceEventType.Warning, 2, $"ERROR: {eventArgs.Data}");
        }
    }
}
