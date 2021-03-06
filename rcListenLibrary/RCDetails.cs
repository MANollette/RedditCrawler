﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rcListenLibrary
{
    /// <summary>
    /// Class RCDetails exists to assist in the creation, modification, and preservation of 
    /// the JSON object this application's Listen method uses to run. 
    /// </summary>
    public class RCDetails
    {
        public string email { get; set; }
        public string ePass { get; set; }
        public string sub { get; set; }
        public string toast { get; set; }
        public List<string> searchCriteria { get; set; }
        public List<string> dupResults { get; set; }

        public RCDetails() { }
        public RCDetails(string email, string ePass, string sub, string toast, List<string> searchCriteria, List<string> dupResults)
        {
            this.email = email;
            this.ePass = ePass;
            this.sub = sub;
            this.toast = toast;
            this.searchCriteria = searchCriteria;
            this.dupResults = dupResults;
        }
    }
}
