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
        public void TestFileCreationAndExistence()
        {
            rcHelper rc = new rcHelper();
            rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/test.txt");
            bool b = File.Exists(Directory.GetCurrentDirectory().ToString() + "/test.txt");
            File.Delete(Directory.GetCurrentDirectory().ToString() + "/test.txt");
            Assert.AreEqual(b, true);            
        }

        [TestMethod]
        public void TestFileReadWrite()
        {
            rcHelper rc = new rcHelper();
            List<string> sl = new List<string>();
            List<string> sl2 = new List<string>();
            sl.Add("Test text");
            string filePath = Directory.GetCurrentDirectory().ToString() + "/test.txt";
            rc.CheckFileExists(filePath);
            rc.WriteToFile(filePath, sl, false);
            sl2 = rc.ReadFile(filePath);
            Assert.AreEqual(sl[0], sl2[0]);
            File.Delete(filePath);
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
            List<string> sl = rcc.GetPosts("username", "password", "/r/miniswap");
            Assert.IsNotNull(sl);
        }
    }
}
