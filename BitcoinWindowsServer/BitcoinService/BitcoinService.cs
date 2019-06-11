using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;
using Serilog;

namespace BitcoinService
{
    public partial class BitcoinService : ServiceBase
    {
        private Process _bitcoindProcess;

        private readonly string _bitcoindPath;
        private readonly string _cliPath;
        private readonly string _dataDir;

        public BitcoinService()
        {
            var installDir = ConfigurationManager.AppSettings["InstallDirectory"];
            if (string.IsNullOrWhiteSpace(installDir)) installDir = Environment.GetEnvironmentVariable("ProgramW6432");
            if (string.IsNullOrWhiteSpace(installDir))
                throw new Exception();
            _bitcoindPath = Path.Combine(installDir, "daemon\\bitcoind.exe");
            if (!File.Exists(_bitcoindPath))
                throw new FileNotFoundException(null, _bitcoindPath);

            _cliPath = Path.Combine(installDir, "daemon\\bitcoin-cli.exe");
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

        protected override void OnStart(string[] args)
        {
            try
            {
                var startArgs = GetArgs();
                _bitcoindProcess = new Process
                {
                    StartInfo = new ProcessStartInfo(_bitcoindPath, startArgs)
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };
                _bitcoindProcess.OutputDataReceived += Bitcoind_OutputDataReceived;
                _bitcoindProcess.Exited += Bitcoind_Exited;
                _bitcoindProcess.EnableRaisingEvents = true;

                _bitcoindProcess.Start();
                _bitcoindProcess.BeginOutputReadLine();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "BitcoinService error starting");
            }
        }

        public void Start()
        {
            OnStart(new string[0]);
        }

        private void Bitcoind_Exited(object sender, EventArgs eventArgs)
        {
            int exitCode = _bitcoindProcess.ExitCode;
            Log.Verbose("EXITED: {exitCode}", exitCode);
        }

        private void Bitcoind_OutputDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            Log.Verbose("OUT: {data}", eventArgs.Data);
            //_trace.TraceEvent(TraceEventType.Verbose, 1, $"OUT: {eventArgs.Data}");
        }

        private void Bitcoind_ErrorDataReceived(object sender, DataReceivedEventArgs eventArgs)
        {
            //_trace.TraceEvent(TraceEventType.Warning, 2, $"ERROR: {eventArgs.Data}");
        }

        protected override void OnStop()
        {
            try
            {
                Process.Start(_cliPath, GetArgs() + " stop")?.WaitForExit();
                while (!_bitcoindProcess.HasExited)
                {
                    Thread.Sleep(500);
                }
            }
            catch (Exception arg)
            {
                Log.Error(arg, "BitcoinService error stopping");
            }
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
    }
}
