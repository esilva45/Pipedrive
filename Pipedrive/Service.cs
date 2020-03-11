using System.ServiceProcess;
using System.Threading;
using System;

namespace ServiceIntegration {
    public partial class Service : ServiceBase {
        private Timer _timer;

        public Service() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            _timer = new Timer(ProcessorManager, null, 0, 60000);
        }

        protected override void OnStop() { }

        private void ProcessorManager(object state) {
            SendMessage.Message();
        }
    }
}
