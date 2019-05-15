using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RedditSharp.Things.VotableThing;
using System.Security;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Mail;
using System.IO;
using System.Web;
using rcListenLibrary;

namespace RedditCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            rcHelper rc = new rcHelper();
            Program p = new Program();

            //Validates login, logs failures. 
            rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
            List<string> loginCheck = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
            if (loginCheck.Count() == 0 || loginCheck == null)
            {
                Exception e = new Exception("Login failed. Please configure your settings in rcConfig.");
                rc.DebugLog(e);
                System.Environment.Exit(0);
            }
            else
            {
                //If login check succeeds, provide login credentials for Listen() method.
                List<string> credentials = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                string user = rc.DecodePassword(credentials[0]);
                string password = rc.DecodePassword(credentials[1]);
                p.Listen(user, password);
            }

        }

        /// <summary>
        /// Takes user reddit user name and password as input, confirms existence of all relevant text files (rcEmail.txt, rcLogin.txt, 
        /// rcSearchCriteria.txt, and rcSubreddit.txt) and their contents, then retrieves 15 posts using <see cref="GetPosts(string, string, string)"/>
        /// Once these posts have been retrieved, the method checks them against duplicates from rcSearchRecords.txt and matches from rcSearchCriteria.txt,
        /// then uses <see cref="NotifyUser(string)"/> to notify the user of any matches. After this, all variables are cleared, the thread sleeps for 
        /// 60,000 ticks, and runs again. 
        /// </summary>
        /// <param name="user">Reddit username</param>
        /// <param name="password">Reddit password</param>
        private void Listen(string user, string password)
        {
            rcHelper rc = new rcHelper();
            rcConnectivity rcon = new rcConnectivity();

            while (true)
            {
                try
                {
                    //Checks for valid entries in relevant text files, performs DebugLog() and exits environment on failure. 
                    #region criteriaCheck

                    //searchInput needs to be acquired from locally saved text file rcSearchCriteria.txt after being input, and will be used to filter the results
                    //If list is blank, user is notified to enter criteria from the main menu.
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                    List<string> lstSearchInput = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                    if (lstSearchInput.Count < 1)
                    {
                        Exception ex = new Exception("You must ensure rcSearchCriteria.txt has search terms.");
                        rc.DebugLog(ex);
                        System.Environment.Exit(0);
                    }

                    //subreddit needs to be acquired from locally saved text file rcSubreddit.txt
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt");
                    string sub = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt").First();
                    if (sub.Count() < 4)
                        System.Environment.Exit(0);

                    //List should also be acquired from a locally saved text file, and will consist of
                    //all previous entries the user has already been notified of. 
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt");
                    List<string> lstDuplicateList = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt");

                    //Double checks that rcEmail.txt exists and is not null
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
                    if (rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt").Count < 2)
                    {
                        Exception ex = new Exception("You must ensure rcEmail.txt has a valid email address & password");
                        rc.DebugLog(ex);
                        System.Environment.Exit(0);
                    }
                    #endregion

                    //gets list of 15 most recent posts from designated sub.
                    List<string> lstResultList = rcon.GetPosts(user, password, sub);
                    List<string> lstPassedList = rc.NotificationList(lstResultList, lstSearchInput, lstDuplicateList);                   

                    //Now that the list has been trimmed, notify user of all results
                    if (lstPassedList.Count > 1)
                    {
                        rc.WriteToFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt", lstPassedList, true);
                        for (int i = 0; i < lstPassedList.Count(); i++)
                        {
                            Console.WriteLine("Sent: " + lstPassedList[i]);
                            rcon.NotifyUser(lstPassedList[i]);
                        }
                    }

                    //Clear all lists, sleep, and repeat
                    lstResultList.Clear();
                    lstPassedList.Clear();
                    lstDuplicateList.Clear();
                    lstSearchInput.Clear();
                    Thread.Sleep(60000);
                }
                catch (Exception ex)
                {
                    rc.DebugLog(ex);
                    System.Environment.Exit(0);
                }
            }
        }        

        //TBD
        //Methods for running application as a Windows service
        
    }

}
