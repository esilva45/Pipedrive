using System.ServiceProcess;
using System.Threading;
using System;
using System.Xml.Linq;

namespace ServiceIntegration {
    public partial class Service : ServiceBase {
        private Timer _timer;

        public Service() {
            InitializeComponent();
        }

        protected override void OnStart(string[] args) {
            XElement configXml = XElement.Load(AppDomain.CurrentDomain.BaseDirectory + @"\config.xml");
            string license = configXml.Element("LicenseKey").Value.ToString();

            if (!License.VerifyLicence(license)) {
                this.Stop();
            }

            _timer = new Timer(ProcessorManager, null, 0, 60000);
        }

        protected override void OnStop() { }

        private void ProcessorManager(object state) {
            SendMessage.Message();
        }
    }
}
