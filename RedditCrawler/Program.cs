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

namespace RedditCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //Program initialization for method call
            var p = new Program();
            p.CheckFileExists("../../../rcLogin.txt");
            List<string> loginCheck = p.ReadFile("../../../rcLogin.txt");
            if (loginCheck.Count == 0 || loginCheck == null)
            {
                Exception e = new Exception("Login failed. Please configure your settings in rcConfig.");
                p.DebugLog(e);
                System.Environment.Exit(0);
            }
            else
            {
                List<string> credentials = p.ReadFile("../../../rcLogin.txt");
                string user = credentials[0];
                string password = credentials[1];
                p.Listen(user, password);
            }

        }      

        //Methods for interacting with Reddit, Email.
        #region Connectivity

        //Method for sending email notification to user.
        void NotifyUser(string result)
        {
            CheckFileExists("../../../rcEmail.txt");
            List<string> emailCredentials = ReadFile("../../../rcEmail.txt");
            if (emailCredentials.Count > 0)
            {
                string email = emailCredentials[0];
                string password = emailCredentials[1];
                Program p = new Program();

                try
                {
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                    mail.From = new MailAddress(email);
                    mail.To.Add(email);
                    mail.Subject = "New Reddit Post!";
                    mail.Body = "The following post was created " + DateTime.Now.ToShortDateString()
                        + ":\n\n" + result;

                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(email, password);
                    SmtpServer.EnableSsl = true;

                    SmtpServer.Send(mail);
                    Console.WriteLine("mail Sent");
                }
                catch (Exception ex)
                {
                    p.DebugLog(ex);
                }
            }
            else
            {
                Exception e = new Exception("Error caught validating email credentials in RedditCrawler service");
                Program p = new Program();
                p.DebugLog(e);
            }
        }
        
        //Method for retrieving posts from website.
        List<string> GetPosts(string user, string password, string sub)
        {
            Program p = new Program();
            try
            {              
                var reddit = new Reddit();
                var login = reddit.LogIn(user, password);
                var subreddit = reddit.GetSubreddit(sub);
                subreddit.Subscribe();
                List<string> lstResultList = new List<string>();
                foreach (var post in subreddit.New.Take(15))
                {
                    lstResultList.Add(post.Title.ToString());

                    /*For testing purposes
                    Console.WriteLine(post.Title.ToString());
                    Console.ReadLine();*/
                }
                return lstResultList;
            }
            catch(Exception ex)
            {
                p.DebugLog(ex);
            }
            return null;
        }
        #endregion

        //Methods for working with files
        #region HelperMethods

        //Check if the file at the specified file path exists
        //If it doesn't, go ahead and create one
        private void CheckFileExists(string filePath)
        {
            //Check if the file exists at the given file path
            if (!File.Exists(filePath))
            {
                //If not, create it
                File.Create(filePath).Dispose();
            }
        }

        //Reads all of the lines from the specified file and
        //returns a list with all of them
        private List<string> ReadFile(string filePath)
        {
            //List to append the lines of the file to
            List<string> lstRead = new List<string>();
            Program p = new Program();

            try
            {
                CheckFileExists(filePath);
                //using statement to handle the StreamReader
                using (StreamReader sr = new StreamReader(filePath))
                {
                    //string to hold one line from file
                    string line;

                    //loops through the file and ends
                    //when the end of the file is reached
                    while ((line = sr.ReadLine()) != null)
                    {
                        lstRead.Add(line); //add line to list
                    }
                    sr.Close();
                }
                return lstRead; //return list with appended lines  
            }
            catch(Exception ex)
            {
                p.DebugLog(ex);
            }
            return null;
        }

        //By default, OVERWRITES the content of the specified file with the
        //contents of a given list
        private void WriteToFile(string filePath, List<string> list, bool append)
        {
            Program p = new Program();
            try
            {
                CheckFileExists(filePath);
                using (StreamWriter sw = new StreamWriter(filePath, append))
                {
                    //Loop through each line in list
                    foreach (string line in list)
                    {
                        sw.WriteLine(line); //add it to the file
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            catch(Exception ex)
            {
                p.DebugLog(ex);
            }

        }

        //Method for logging error details to debug file and returning to menu.
        private void DebugLog(Exception e)
        {
            Program p = new Program();
            CheckFileExists("../../../rcErrorLog.txt");
            List<string> lstError = new List<string>();
            string s = "Error occurred: " + DateTime.Now.ToShortDateString() + "\nSource: " + e.Source + "\nStack trace: " + e.StackTrace
                + "\nTarget site: " + e.TargetSite + "\nData: " + e.Data + "\nMessage: " + e.Message;
            Console.WriteLine(s);
            lstError.Add(s);
            WriteToFile("../../../rcErrorLog.txt", lstError, true);
        }

        #endregion

        //Continuous method for monitoring sub.
        private void Listen(string user, string password)
        {
            Program p = new Program();
            while (true)
            {
                try
                {
                    //searchInput needs to be acquired from locally saved text file rcSearchCriteria.txt after being input, and will be used to filter the results
                    //If list is blank, user is notified to enter criteria from the main menu.
                    CheckFileExists("../../../rcSearchCriteria.txt");
                    List<string> lstSearchInput = ReadFile("../../../rcSearchCriteria.txt");
                    if (lstSearchInput.Count < 1)
                        System.Environment.Exit(0);


                    //subreddit needs to be acquired from locally saved text file rcSubreddit.txt
                    CheckFileExists("../../../rcSubreddit.txt");
                    string sub = ReadFile("../../../rcSubreddit.txt").First();
                    if (sub.Count() < 4)
                        System.Environment.Exit(0);

                    //List should also be acquired from a locally saved text file, and will consist of
                    //all previous entries the user has already been notified of. 
                    CheckFileExists("../../../rcSearchRecords.txt");
                    List<string> lstDuplicateList = ReadFile("../../../rcSearchRecords.txt");

                    //Double checks that rcEmail.txt exists and is not null
                    CheckFileExists("../../../rcEmail.txt");
                    if (ReadFile("../../../rcEmail.txt").Count < 2)
                        System.Environment.Exit(0);

                    //gets list of 25 most recent posts from designated sub.
                    List<string> lstResultList = new Program().GetPosts(user, password, sub);
                    List<string> lstPassedList = new List<string>();

                    //Remove posts that do not match criteria
                    for(int i = lstResultList.Count - 1; i >= 0; --i)
                    {
                        for (int i2 = 0; i2 < lstSearchInput.Count; i2++)
                        {
                            if (lstResultList[i].ToLower().Contains(lstSearchInput[i2].ToLower()))
                            {
                                lstPassedList.Add(lstResultList[i]);
                            }
                        }
                    }

                    lstPassedList = lstPassedList.Distinct().ToList();
                    //cycle through the list of already-posted results
                    if (lstPassedList.Count > 0 && lstDuplicateList.Count > 0)
                    {
                        for(int i = 0; i < lstDuplicateList.Count; ++i)
                        {
                            for(int i2 = 0; i2 < lstPassedList.Count; ++i2)
                            {
                                if (lstDuplicateList[i].ToLower() == lstPassedList[i2].ToLower())
                                {
                                    lstPassedList.RemoveAt(i2);
                                }
                            }
                        }
                    }

                    //Now that the list has been trimmed, notify user of all results
                    if (lstPassedList.Count > 1)
                    {
                        WriteToFile("../../../rcSearchRecords.txt", lstPassedList, true);
                        foreach (string s in lstPassedList)
                        {
                            Console.WriteLine("Sent: " + s);
                            new Program().NotifyUser(s);                          
                        }
                    }
                    lstResultList.Clear();
                    lstPassedList.Clear();
                    lstDuplicateList.Clear();
                    lstSearchInput.Clear();
                    Thread.Sleep(60000);
                }
                catch (Exception ex)
                {
                    p.DebugLog(ex);
                    System.Environment.Exit(0);
                }
            }
        }

        //Methods for running application as a Windows service
    }

}
