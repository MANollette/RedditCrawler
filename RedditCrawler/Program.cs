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

namespace RedditCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //Program initialization for method call
            var p = new Program();

            //Validates login, logs failures. 
            p.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
            List<string> loginCheck = p.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
            if (loginCheck.Count() == 0 || loginCheck == null)
            {
                Exception e = new Exception("Login failed. Please configure your settings in rcConfig.");
                p.DebugLog(e);
                System.Environment.Exit(0);
            }
            else
            {
                //If login check succeeds, provide login credentials for Listen() method.
                List<string> credentials = p.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                string user = p.DecodePassword(credentials[0]);
                string password = p.DecodePassword(credentials[1]);
                p.Listen(user, password);
            }

        }

        //Methods for interacting with Reddit, Email.
        #region Connectivity

        //Method for sending email notification to user.
        void NotifyUser(string result)
        {
            Program p = new Program();
            //Confirms rcEmail.txt exists
            CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
            //Retrieves email credentials from rcEmail.txt
            List<string> emailCredentials = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
            //If credentials exist, continue. 
            if (emailCredentials.Count > 0)
            {
                string email = p.DecodePassword(emailCredentials[0]);
                string password = p.DecodePassword(emailCredentials[1]);

                //Send details of post to the user in an email.
                try
                {
                    //Initialize mail object & client
                    MailMessage mail = new MailMessage();
                    SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");

                    //Adds details to mail object
                    mail.From = new MailAddress(email);
                    mail.To.Add(email);
                    mail.Subject = "New Reddit Post!";
                    mail.Body = "The following post was created " + DateTime.Now.ToShortDateString()
                        + ":\n\n" + result;

                    //Sets port, credentials, SSL, and sends mail object. 
                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(email, password);
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);
                    Console.WriteLine("mail Sent");
                    SmtpServer.Dispose();
                    mail.Dispose();
                }
                catch (Exception ex)
                {
                    p.DebugLog(ex);
                }
            }
            else
            {
                Exception e = new Exception("Error caught validating email credentials in RedditCrawler service");
                p.DebugLog(e);
            }
        }

        //Method for retrieving posts from website.
        List<string> GetPosts(string user, string password, string sub)
        {
            Program p = new Program();
            try
            {
                //Initialize instance of Reddit class using RedditSharp, then login with passed credentials. 
                var reddit = new Reddit();
                var login = reddit.LogIn(user, password);
                var subreddit = reddit.GetSubreddit(sub);
                subreddit.Subscribe();
                List<string> lstResultList = new List<string>();
                //Retrieves title of 15 posts, adds it to a fresh List<string>
                foreach (var post in subreddit.New.Take(15))
                {
                    lstResultList.Add(post.Title.ToString());               
                }
                //Returns list of post titles. 
                return lstResultList;
            }
            catch (Exception ex)
            {
                p.DebugLog(ex);
            }
            return null;
        }

        #endregion

        //Methods for working with files
        #region HelperMethods

        //Method for encoding password
        private string EncodePassword(string password)
        {
            //Initialize byte array & encoded password field
            Program p = new Program();
            string encodedPassword = String.Empty;
            
            //Try encoding password in byte array format
            try
            {
                byte[] data_byte = Encoding.UTF8.GetBytes(password);
                encodedPassword = HttpUtility.UrlEncode(Convert.ToBase64String(data_byte));                                
            }
            catch(Exception ex)
            {
                p.DebugLog(ex);
            }
            return encodedPassword;
        }

        //Method for decoding password from storage. 
        private string DecodePassword(string encodedPassword)
        {
            Program p = new Program();
            string decodedPassword = String.Empty;

            //Try decoding password back to string format
            try
            {
                byte[] data_byte = Convert.FromBase64String(HttpUtility.UrlDecode(encodedPassword));
                decodedPassword = Encoding.UTF8.GetString(data_byte);
            }
            catch(Exception ex)
            {
                p.DebugLog(ex);
            }
            return decodedPassword;
        }

        //Check if the file at the specified file path exists, creating one if not.
        private void CheckFileExists(string filePath)
        {
            //Check if the file exists at the given file path
            if (!File.Exists(filePath))
            {
                //If not, create it
                File.Create(filePath).Dispose();
            }
        }

        //Reads all of the lines from the specified file and returns a list with all of them
        private List<string> ReadFile(string filePath)
        {
            //List to append the lines of the file to
            List<string> lstRead = new List<string>();
            Program p = new Program();
            try
            {
                //Confirms file exists, creates it if not. 
                CheckFileExists(filePath);

                //Using statement to handle the StreamReader
                using (StreamReader sr = new StreamReader(filePath))
                {
                    //String to hold one line from file
                    string line;

                    //Loops through the file and ends when the end of the file is reached
                    while ((line = sr.ReadLine()) != null)
                    {
                        lstRead.Add(line); 
                    }
                    sr.Close();
                }
                //return list with appended lines  
                return lstRead; 
            }
            catch (Exception ex)
            {
                p.DebugLog(ex);
            }
            return null;
        }

        //Writes contents of the provided list to the filepath, overwriting or appending according to bool append
        private void WriteToFile(string filePath, List<string> list, bool append)
        {
            Program p = new Program();
            try
            {
                //Ensures filePath exists
                CheckFileExists(filePath);
                using (StreamWriter sw = new StreamWriter(filePath, append))
                {
                    //Loop through each line in list
                    foreach (string line in list)
                    {
                        sw.WriteLine(line); 
                    }
                    sw.Flush();
                    sw.Close();
                }
            }
            catch (Exception ex)
            {
                p.DebugLog(ex);
            }
        }

        //Method for filtering out duplicates & reporting matches in GetPosts()
        private List<string> NotificationList(List<string> lstPosts, List<string> lstSearchTerms, List<string> lstDuplicates)
        {
            List<string> lstPassedList = new List<string>();
            //Add posts that match search criteria to lstPassedList
            //Loop through provided post list
            for (int i = lstPosts.Count - 1; i >= 0; --i)
            {
                //Loop through search criteria for each loop through post list
                for (int i2 = 0; i2 < lstSearchTerms.Count; i2++)
                {
                    //If there is a match, add it to the passed list
                    if (lstPosts[i].ToLower().Contains(lstSearchTerms[i2].ToLower()))
                    {
                        lstPassedList.Add(lstPosts[i]);
                    }
                }
            }

            //Filter out duplicate entries that may match multiple criteria
            lstPassedList = lstPassedList.Distinct().ToList();

            //cycle through the list of already-posted results from lstDuplicates & remove matches
            if (lstPassedList.Count > 0 && lstDuplicates.Count > 0)
            {
                //Loop through all existing search records
                for (int i = 0; i < lstDuplicates.Count; ++i)
                {
                    //Loop through all passed list criteria for each existing search record
                    for (int i2 = 0; i2 < lstPassedList.Count; ++i2)
                    {
                        //If there is a match, the user has already been notified of this post; remove it. 
                        if (lstDuplicates[i].ToLower() == lstPassedList[i2].ToLower())
                        {
                            lstPassedList.RemoveAt(i2);
                        }
                    }
                }
            }
            //Return filtered list
            return lstPassedList;
        }

        //Method for logging error details to debug file and returning to menu.
        private void DebugLog(Exception e)
        {
            Program p = new Program();
            //Ensure error log exists.
            CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcErrorLog.txt");
            List<string> lstError = new List<string>();
            //Initialize error details for writing to rcErrorLog.txt
            string s = "Error occurred: " + DateTime.Now.ToShortDateString() + "\nSource: " + e.Source + "\nStack trace: " + e.StackTrace
                + "\nTarget site: " + e.TargetSite + "\nData: " + e.Data + "\nMessage: " + e.Message;
            Console.WriteLine(s);
            lstError.Add(s);
            //Write error details to rcErrorLog.txt
            WriteToFile(Directory.GetCurrentDirectory().ToString() + "/rcErrorLog.txt", lstError, true);
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
                    //Checks for valid entries in relevant text files, performs DebugLog() and exits environment on failure. 
                    #region criteriaCheck
                    //searchInput needs to be acquired from locally saved text file rcSearchCriteria.txt after being input, and will be used to filter the results
                    //If list is blank, user is notified to enter criteria from the main menu.
                    CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                    List<string> lstSearchInput = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                    if (lstSearchInput.Count < 1)
                    {
                        Exception ex = new Exception("You must ensure rcSearchCriteria.txt has search terms.");
                        p.DebugLog(ex);
                        System.Environment.Exit(0);
                    }

                    //subreddit needs to be acquired from locally saved text file rcSubreddit.txt
                    CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt");
                    string sub = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt").First();
                    if (sub.Count() < 4)
                        System.Environment.Exit(0);

                    //List should also be acquired from a locally saved text file, and will consist of
                    //all previous entries the user has already been notified of. 
                    CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt");
                    List<string> lstDuplicateList = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt");

                    //Double checks that rcEmail.txt exists and is not null
                    CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
                    if (ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt").Count < 2)
                    {
                        Exception ex = new Exception("You must ensure rcEmail.txt has a valid email address & password");
                        p.DebugLog(ex);
                        System.Environment.Exit(0);
                    }
                    #endregion

                    //gets list of 25 most recent posts from designated sub.
                    List<string> lstResultList = new Program().GetPosts(user, password, sub);
                    List<string> lstPassedList = NotificationList(lstResultList, lstSearchInput, lstDuplicateList);                   

                    //Now that the list has been trimmed, notify user of all results
                    if (lstPassedList.Count > 1)
                    {
                        WriteToFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt", lstPassedList, true);
                        for (int i = 0; i < lstPassedList.Count(); i++)
                        {
                            Console.WriteLine("Sent: " + lstPassedList[i]);
                            p.NotifyUser(lstPassedList[i]);
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
                    p.DebugLog(ex);
                    System.Environment.Exit(0);
                }
            }
        }        

        //TBD
        //Methods for running application as a Windows service
    }

}
