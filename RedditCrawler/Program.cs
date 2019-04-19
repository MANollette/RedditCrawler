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
            //Retrieve username and password from user.
            Tuple<string, string> credentials = p.Login();
            string user = credentials.Item1;
            string pass = credentials.Item2;

            Console.WriteLine("Welcome to RedditCrawler!\nPlease select from the following menu.\n\n" +
                              "A. Input or change notification email\n" +
                              "B. Select a new SubReddit\n" +
                              "C. Input new search criteria\n" +
                              "D. Run application with saved settings\n" +
                              "E. Exit application.");
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
                        || cResponse == 'e')
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
                                        p.Listen(user, pass);
                                        break;
                                    case 'e':
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
            catch(Exception e)
            {
                Console.WriteLine("Error. \n" + e.Message);
            }
        }

        //Methods for creation of new user-specified information. 
        #region NewCriteria    
        //Method for changing the subreddit to monitor
        private void NewSub()
        {
            /*TODO:
             -Check for existence of user file.
             -If nonexistent, create.
             -If blank, amend
             -If exists, modify sub details
             -Return to main menu
             */
        }

        //Method for changing email address
        private void NewEmail()
        {
            /*TODO:
             -Check for existence of user file.
             -If nonexistent, create.
             -If blank, amend
             -If exists, modify email.
             -Return to main menu
             */
        }

        //Method for initializing, changing, or adding new search criteria
        private void NewSearch()
        {            
            /*TODO:
             -Check for existence of user file.
             -If nonexistent, create and skip to amending.
             -If blank, skip to amending.
             -If exists, list current search criteria
                -Ask if user would like to add new criteria, or rewrite criteria.
                -LATER: Add option to remove specific criteria. 
             -Return to main menu
             */
        }
        #endregion

        //Methods for interacting with Reddit, Email.
        #region Connectivity
        //Method for sending email notification to user.
        void NotifyUser(string result)
        {
            //TODO Retrieve email and password from rcEmail.txt.
            string email = "";
            string password = "";

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
                Console.WriteLine(ex.ToString());
            }
        }

        //Method for inputting login
        private Tuple<string, string> Login()
        {
            try
            {
                Console.WriteLine("Please enter your Reddit username");
                string user = Console.ReadLine();
                Console.WriteLine("Please enter your password");
                string password = Console.ReadLine();
                return Tuple.Create(user, password);
            }
            catch (Exception e)
            {
                Console.WriteLine("Error: " + e.Message);
                Console.ReadLine();
            }
            return null;
        }

        //Method for retrieving posts from website.
        List<string> GetPosts(string user, string password, string sub)
        {
            Thread.Sleep(30000);
            var reddit = new Reddit();
            var login = reddit.LogIn(user, password);
            var subreddit = reddit.GetSubreddit("/r/" + sub);
            subreddit.Subscribe();
            List<string> resultList = new List<string>();
            foreach (var post in subreddit.New.Take(25))
            {
                resultList.Add(post.Title.ToString());

                /*For testing purposes
                Console.WriteLine(post.Title.ToString());
                Console.ReadLine();*/
            }
            return resultList;
        }
        #endregion

        //Continuous method for monitoring sub.
        private void Listen(string user, string password)
        {
            while (true)
            {
                try
                {
                    //searchInput needs to be acquired from locally saved text file rcSearchCriteria.tx after being input, and will be used to filter the results
                    List<string> lstSearchInput = new List<string>();
                    //subreddit needs to be acquired from locally saved text file rcSubreddit.
                    string subreddit = "";

                    //List should also be acquired from a locally saved text file, and will consist of
                    //all previous entries the user has already been notified of. 
                    List<string> lstDuplicateList = new List<string>();

                    //gets list of 25 most recent posts from designated sub.
                    List<string> lstResultList = new Program().GetPosts(user, password, subreddit);

                    //Remove posts that do not match criteria
                    foreach (string s in lstResultList)
                    {
                        foreach (string si in lstSearchInput)
                        {
                            if (!s.Contains(si))
                                lstResultList.Remove(s);
                        }
                    }

                    //cycle through the list of already-posted results
                    if (lstResultList.Count > 0 && lstDuplicateList.Count > 0)
                    {
                        foreach (string s in lstDuplicateList)
                        {
                            //cycle through current result list
                            foreach (string s2 in lstResultList)
                            {
                                //compare result to potential duplicate, removing it if duplicate
                                if (s == s2)
                                {
                                    lstResultList.Remove(s2);
                                }
                            }
                        }
                    }

                    //Now that the list has been trimmed, notify user of all results
                    if (lstResultList.Count > 1)
                    {
                        foreach (string s in lstResultList)
                        {
                            new Program().NotifyUser(s);
                            lstDuplicateList.Remove(s);
                            //TO ADD HERE: Code to remove s from rcSearchRecords.txt
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            }
        }
    }

}
