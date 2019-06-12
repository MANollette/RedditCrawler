using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using rcListenLibrary;
using RedditCrawler;

namespace RedditCrawlerTests
{
    [TestClass]
    public class rcCListenTests
    {

        [TestMethod]
        public void TestPasswordEncoding()
        {
            rcHelper rc = new rcHelper();
            string pass = "password";
            string ePass = rc.EncodePassword(pass);
            ePass = rc.DecodePassword(ePass);
            Assert.AreEqual(ePass, pass);
        }

        [TestMethod]
        public void TestJsonCreationAndExistence()
        {
            rcHelper rc = new rcHelper();
            List<string> sc = new List<string>();
            RCDetails rcd = new RCDetails("rLog", "rPass", "email", "ePass", "sub", "toast", sc, sc);
            rc.WriteToFile(rcd);
            bool b = File.Exists(Directory.GetCurrentDirectory().ToString() + "/rcData.json");
            File.Delete(Directory.GetCurrentDirectory().ToString() + "/rcData.json");
            Assert.AreEqual(b, true);            
        }

        [TestMethod]
        public void TestValidEmailFormat()
        {
            rcHelper rc = new rcHelper();
            string email = "abc123@gmail.com";
            bool b = rc.IsEmailValid(email);
            Assert.AreEqual(b, true);
        }

        [TestMethod]
        public void TestInvalidEmailFormat()
        {
            rcHelper rc = new rcHelper();
            string email = "cd2";
            bool b = rc.IsEmailValid(email);
            Assert.AreEqual(b, false);
        }

        [TestMethod]
        public void TestForInternetConnection()
        {
            bool b = rcConnectivity.IsNetworkAvailable(0);
            Assert.AreEqual(b, true);
        }

        //Method must have valid username and password to Reddit in order to succeed.
        [TestMethod]
        public void TestRedditPostRetrieval()
        {
            rcConnectivity rcc = new rcConnectivity();
            var getLists = rcc.GetPosts("username", "password", "/r/miniswap");
            List<string> lstResultList = getLists.Item1;
            List<string> lstUrl = getLists.Item2;
            bool b = false;
            if (lstResultList != null && lstUrl != null)
                b = true;
            Assert.IsTrue(b);
        }
    }
}
