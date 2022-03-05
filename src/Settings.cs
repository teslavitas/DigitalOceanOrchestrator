using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalOceanOrchestrator
{
    internal record Settings
    {
        public string DigitalOceanToken { get; set; }
        public int TotalDroplets { get; set; }
        public string SshFingerprint { get; set; }
        public string SshPrivateKeyFilePath { get; set; }
        public string[] SshCommands { get; set; }
        public int DeleteDropletsAfterMinutes { get; set; }
        public string Tag { get; set; }
    }
}
