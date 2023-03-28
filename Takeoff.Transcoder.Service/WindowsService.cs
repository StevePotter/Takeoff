using System;
using System.IO;
using System.ServiceProcess;
using Takeoff.Jobs;
using Takeoff.Transcoder;

namespace Mediascend.Transcoder
{
    partial class WindowsService : ServiceBase
    {
        public WindowsService(Supervisor supervisor)
        {
            InitializeComponent();
            Program.Directory = Path.GetDirectoryName(Environment.CommandLine.Strip("\""));
            Supervisor = supervisor;
        }

        private Supervisor Supervisor;

        protected override void OnStart(string[] args)
        {
            Supervisor.Start();
        }


        protected override void OnStop()
        {
            Supervisor.Disable();
        }
    }
}
