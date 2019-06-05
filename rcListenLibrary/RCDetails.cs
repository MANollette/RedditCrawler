using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rcListenLibrary
{
    public class RCDetails
    {
        public string rLogin { get; set; }
        public string rPass { get; set; }
        public string email { get; set; }
        public string ePass { get; set; }
        public string sub { get; set; }
        public string toast { get; set; }
        public List<string> searchCriteria { get; set; }
        public List<string> dupResults { get; set; }

        public RCDetails() { }
        public RCDetails(string rLogin, string rPass, string email, string ePass, string sub, string toast, List<string> searchCriteria, List<string> dupResults)
        {
            this.rLogin = rLogin;
            this.rPass = rPass;
            this.email = email;
            this.ePass = ePass;
            this.sub = sub;
            this.toast = toast;
            this.searchCriteria = searchCriteria;
            this.dupResults = dupResults;
        }
    }
}
