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

            //Initialize main menu
            p.MainMenuInput();

        }

        //Method for initial menu input retrieval.
        private void MainMenuInput()
        {
            //Instantiate new program
            Program p = new Program();

            p.CheckFileExists("rcLogin.txt");
            List<string> loginCheck = p.ReadFile("rcLogin.txt");
            if (loginCheck.Count == 0 || loginCheck == null)
                p.NewLogin();

            List<string> credentials = p.ReadFile("rcLogin.txt");
            string user = credentials[0];
            string password = credentials[1];

            Console.Clear();
            Console.WriteLine("Welcome to RedditCrawler!\nPlease select from the following menu.\n\n" +
                              "A. Input or change notification email\n" +
                              "B. Select a new SubReddit\n" +
                              "C. Input new search criteria\n" +
                              "D. Run application with saved settings\n" +
                              "E. Delete all current search criteria.\n" +
                              "F. Exit application.");
            try
            {
                string response = Console.ReadLine().ToLower();
                char cResponse = 'n';
                if (response.Count() == 1)
                {
                    cResponse = response[0];
                    if (cResponse == 'a'
                        || cResponse == 'b'
                        || cResponse == 'c'
                        || cResponse == 'd'
                        || cResponse == 'e'
                        || cResponse == 'f')
                    {
                        if (cResponse != 'n')
                        {
                            try
                            {
                                switch (cResponse)
                                {
                                    case 'a':
                                        p.NewEmail();
                                        break;
                                    case 'b':
                                        p.NewSub();
                                        break;
                                    case 'c':
                                        p.NewSearch();
                                        break;
                                    case 'd':
                                        p.Listen(user, password);
                                        break;
                                    case 'e':
                                        p.DeleteSearchCriteria();
                                        break;
                                    case 'f':
                                        Environment.Exit(0);
                                        break;
                                }
                            }
                            catch (Exception e)
                            {
                                Console.WriteLine("Error! Details: " + e.Message);
                                Console.ReadLine();
                            }
                        }
                        else
                        {
                            Console.WriteLine("Returned input from MainMenuInput not valid.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Please input a single character A-E");
                        MainMenuInput();
                    }
                }
                else
                {
                    Console.WriteLine("Please input a single character A-E");
                    MainMenuInput();
                }              
            }
            catch(Exception ex)
            {
                CheckFileExists("rcErrorLog.txt");
                List<string> sl = new List<string>();
                string s = "Error occurred. Source: " + ex.Source + "\nStack trace: " + ex.StackTrace + "\nTarget site: " + ex.TargetSite
                    + "\nData: " + ex.Data + "\nMessage:" + ex.Message;
                sl.Add(s);
                WriteToFile("rcErrorLog.txt", sl, true);
            }
        }

        //Methods for creation of new user-specified information. 
        #region NewCriteria   
        
        //Method for changing login credentials
        private void NewLogin()
        {
            Program p = new Program();

            try
            {
                p.CheckFileExists("rcLogin.txt");
                Console.WriteLine("Please enter your Reddit username");
                string user = Console.ReadLine();
                Console.WriteLine("Please enter your password");
                string password = Console.ReadLine();
                List<string> credentials = new List<string>();

                //Test login credentials
                var reddit = new Reddit();
                var login = reddit.LogIn(user, password);
                credentials.Add(user);
                credentials.Add(password);

                p.WriteToFile("rcLogin.txt", credentials, false);
            }
            catch (Exception ex)
            {
                CheckFileExists("rcErrorLog.txt");
                List<string> sl = new List<string>();
                string s = "Error occurred. Source: " + ex.Source + "\nStack trace: " + ex.StackTrace + "\nTarget site: " + ex.TargetSite
                    + "\nData: " + ex.Data + "\nMessage:" + ex.Message;
                sl.Add(s);
                WriteToFile("rcErrorLog.txt", sl, true);
                Console.ReadLine();
                Console.Clear();
                p.NewLogin();
            }
        }

        //Method for changing the subreddit to monitor
        private void NewSub()
        {
            string subFilePath = "rcSubreddit.txt";
            Program p = new Program();

            try
            {
                CheckFileExists(subFilePath);
                Console.WriteLine("Please enter a subreddit name.");
                string sub = Console.ReadLine();
                List<string> subList = new List<string>();
                subList.Add(sub);
                WriteToFile(subFilePath, subList, false);
                p.MainMenuInput();
            }
            catch(Exception ex)
            {
                CheckFileExists("rcErrorLog.txt");
                List<string> sl = new List<string>();
                string s = "Error occurred. Source: " + ex.Source + "\nStack trace: " + ex.StackTrace + "\nTarget site: " + ex.TargetSite
                    + "\nData: " + ex.Data + "\nMessage:" + ex.Message;
                sl.Add(s);
                WriteToFile("rcErrorLog.txt", sl, true);
                Console.WriteLine("Error has been logged. Returning to menu.");
                p.MainMenuInput();
            }
        }

        //Method for changing email address
        private void NewEmail()
        {
            //set the name of the file path that contains 
            //the information of the user
            string emailFilePath = "rcEmail.txt";
            Program p = new Program();

            try
            {
                CheckFileExists(emailFilePath);
                Console.WriteLine("Please enter your email address");
                string email = Console.ReadLine();
                Console.WriteLine("Please enter your email password");
                string pass = Console.ReadLine();
                List<string> subList = new List<string>();
                subList.Add(email);
                subList.Add(pass);
                WriteToFile(emailFilePath, subList, false);
                p.MainMenuInput();
            }
            catch (Exception ex)
            {
                CheckFileExists("rcErrorLog.txt");
                List<string> sl = new List<string>();
                string s = "Error occurred. Source: " + ex.Source + "\nStack trace: " + ex.StackTrace + "\nTarget site: " + ex.TargetSite
                    + "\nData: " + ex.Data + "\nMessage:" + ex.Message;
                sl.Add(s);
                WriteToFile("rcErrorLog.txt", sl, true);
                Console.WriteLine("An error has occurred. Returning to menu.");
                p.MainMenuInput();
            }
        }

        //Method for initializing, changing, or adding new search criteria
        private void NewSearch()
        {            
            string searchCriteriaFilePath = "rcSearchCriteria.txt";
            Program p = new Program();

            try
            {
                CheckFileExists(searchCriteriaFilePath);
                Console.WriteLine("Please enter the search term you'd like to listen for.");
                string searchTerm = Console.ReadLine().ToLower();
                List<string> subList = new List<string>();
                subList.Add(searchTerm);
                WriteToFile(searchCriteriaFilePath, subList, true);
                p.MainMenuInput();
            }
            catch (Exception ex)
            {
                CheckFileExists("rcErrorLog.txt");
                List<string> sl = new List<string>();
                string s = "Error occurred. Source: " + ex.Source + "\nStack trace: " + ex.StackTrace + "\nTarget site: " + ex.TargetSite
                    + "\nData: " + ex.Data + "\nMessage:" + ex.Message;
                sl.Add(s);
                WriteToFile("rcErrorLog.txt", sl, true);
                Console.WriteLine("An error has occurred. Returning to menu.");
                p.MainMenuInput();
            }
        }

        private void DeleteSearchCriteria()
        {
            Program p = new Program();
            Console.WriteLine("Are you sure you want to delete all current search criteria?");
            try
            {
                string r = Console.ReadLine();
                if (r.ToLower() == "y" || r.ToLower() == "yes")
                {
                    CheckFileExists("rcSearchCriteria.txt");
                    StreamWriter sw = File.CreateText("rcSearchCriteria.txt");
                    sw.Flush();
                    sw.Close();
                }
                else if (r.ToLower() == "n" || r.ToLower() == "no")
                {
                    p.MainMenuInput();
                }
                else
                {
                    Console.WriteLine("Please enter 'yes' or 'no'");
                    p.DeleteSearchCriteria();
                }
            }
            catch(Exception ex)
            {
                CheckFileExists("rcErrorLog.txt");
                List<string> sl = new List<string>();
                string s = "Error occurred. Source: " + ex.Source + "\nStack trace: " + ex.StackTrace + "\nTarget site: " + ex.TargetSite
                    + "\nData: " + ex.Data + "\nMessage:" + ex.Message;
                sl.Add(s);
                WriteToFile("rcErrorLog.txt", sl, true);
                Console.WriteLine("An error has occurred. Returning to menu.");
                p.MainMenuInput();
            }
        }
        
        #endregion

        //Methods for interacting with Reddit, Email.
        #region Connectivity

        //Method for sending email notification to user.
        void NotifyUser(string result)
        {
            Console.WriteLine(result);
            CheckFileExists("rcEmail.txt");
            List<string> emailCredentials = ReadFile("rcEmail.txt");
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
                    CheckFileExists("rcErrorLog.txt");
                    List<string> sl = new List<string>();
                    string s = "Error occurred. Source: " + ex.Source + "\nStack trace: " + ex.StackTrace + "\nTarget site: " + ex.TargetSite
                        + "\nData: " + ex.Data + "\nMessage:" + ex.Message;
                    sl.Add(s);
                    WriteToFile("rcErrorLog.txt", sl, true);
                    Console.WriteLine("An error has occurred. Returning to menu.");
                    p.MainMenuInput();
                }
            }
            else
            {
                Console.WriteLine("Please enter a valid email/password combination through the main menu\n");
            }
        }
        
        //Method for retrieving posts from website.
        List<string> GetPosts(string user, string password, string sub)
        {
            var reddit = new Reddit();
            var login = reddit.LogIn(user, password);
            var subreddit = reddit.GetSubreddit(sub);
            subreddit.Subscribe();
            List<string> resultList = new List<string>();
            foreach (var post in subreddit.New.Take(15))
            {
                resultList.Add(post.Title.ToString());

                /*For testing purposes
                Console.WriteLine(post.Title.ToString());
                Console.ReadLine();*/
            }
            return resultList;
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
            List<string> list = new List<string>();

            //using statement to handle the StreamReader
            using (StreamReader sr = new StreamReader(filePath))
            {
                //string to hold one line from file
                string line;

                //loops through the file and ends
                //when the end of the file is reached
                while ((line = sr.ReadLine()) != null)
                {
                    list.Add(line); //add line to list
                }
                sr.Close();
            }
            return list; //return list with appended lines           
        }

        //By default, OVERWRITES the content of the specified file with the
        //contents of a given list
        private void WriteToFile(string filePath, List<string> list, bool append)
        {
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
        #endregion

        //Continuous method for monitoring sub.
        private void Listen(string user, string password)
        {
            while (true)
            {
                try
                {
                    //searchInput needs to be acquired from locally saved text file rcSearchCriteria.txt after being input, and will be used to filter the results
                    CheckFileExists("rcSearchCriteria.txt");
                    List<string> lstSearchInput = ReadFile("rcSearchCriteria.txt");

                    //subreddit needs to be acquired from locally saved text file rcSubreddit.txt
                    CheckFileExists("rcSubreddit.txt");
                    string sub = "/r/" + ReadFile("rcSubreddit.txt").First();

                    //List should also be acquired from a locally saved text file, and will consist of
                    //all previous entries the user has already been notified of. 
                    CheckFileExists("rcSearchRecords.txt");
                    List<string> lstDuplicateList = ReadFile("rcSearchRecords.txt");

                    //gets list of 25 most recent posts from designated sub.
                    List<string> lstResultList = new Program().GetPosts(user, password, sub);


                    //Remove posts that do not match criteria
                    for(int i = lstResultList.Count - 1; i >= 0; --i)
                    {
                        for (int i2 = 0; i2 < lstSearchInput.Count; i2++)
                        {
                            if (!lstResultList[i].ToLower().Contains(lstSearchInput[i2]))
                            {
                                lstResultList.RemoveAt(i);
                                break;
                            }
                        }
                    }

                    //cycle through the list of already-posted results
                    if (lstResultList.Count > 0 && lstDuplicateList.Count > 0)
                    {
                        for(int i = 0; i < lstDuplicateList.Count; ++i)
                        {
                            for(int i2 = 0; i2 < lstResultList.Count; ++i2)
                            {
                                if (lstDuplicateList[i] == lstResultList[i2])
                                {
                                    lstResultList.RemoveAt(i2);
                                    break;
                                }
                            }
                        }
                    }

                    //Now that the list has been trimmed, notify user of all results
                    if (lstResultList.Count > 1)
                    {
                        CheckFileExists("rcSearchRecords.txt");
                        WriteToFile("rcSearchRecords.txt", lstResultList, true);
                        foreach (string s in lstResultList)
                        {
                            new Program().NotifyUser(s);                          
                        }
                    }
                    lstResultList.Clear();
                    lstDuplicateList.Clear();
                    lstSearchInput.Clear();
                    Thread.Sleep(60000);
                }
                catch (Exception ex)
                {
                    CheckFileExists("rcErrorLog.txt");
                    List<string> sl = new List<string>();
                    string s = "Error occurred. Source: " + ex.Source + "\nStack trace: " + ex.StackTrace + "\nTarget site: " + ex.TargetSite 
                        + "\nData: " + ex.Data + "\nMessage:" + ex.Message + "\n\n";
                    sl.Add(s);
                    WriteToFile("rcErrorLog.txt", sl, true);
                    Console.WriteLine(s);
                    Console.ReadLine();
                    Program p = new Program();
                    p.MainMenuInput();
                }
            }
        }
    }

}
