using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Configuration;
using System.Messaging;
using System.Threading;
using System.IO;
using System.Xml.Linq;
using Takeoff.Jobs;

namespace Mule
{
    partial class WindowsService : ServiceBase
    {
        public WindowsService(Supervisor supervisor)
        {
            InitializeComponent();
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
