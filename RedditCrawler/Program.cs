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

namespace RedditCrawler
{
    class Program
    {
        static void Main(string[] args)
        {
            //Program initialization for method call
            var p = new Program();

            char c = new Program().MainMenuInput();
            if (c != 'n')
            {
                try
                {
                    switch (c)
                    {
                        case 'a':
                            p.NewUser();
                            break;
                        case 'b':
                            p.NewEmail();
                            break;
                        case 'c':
                            p.NewSub();
                            break;
                        case 'd':
                            p.NewSearch();
                            break;
                        case 'e':
                            p.Listen();
                            break;
                        case 'f':
                            Environment.Exit(0);
                            break;
                    }
                }
                catch(Exception e)
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

        //Method for initial menu input retrieval
        private char MainMenuInput()
        {
            Console.WriteLine("Welcome to RedditCrawler!\nPlease select from the following menu.\n\n" +
                              "A. Input new user information\n" +
                              "B. Input new notification email\n" +
                              "C. Select a new SubReddit\n" +
                              "D. Input new search criteria\n" +
                              "E. Run application with saved settings\n" +
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
                        return cResponse;
                    }
                    else
                    {
                        Console.WriteLine("Please input a single character A-F");
                        MainMenuInput();
                    }
                }
                else
                {
                    Console.WriteLine("Please input a single character A-F");
                    MainMenuInput();
                }              
            }
            catch(Exception e)
            {
                Console.WriteLine("Error. \n" + e.Message);
            }
            return 'n';
        }

        //Method for initializing or changing the user credentials.
        private void NewUser()
        {
            /*TODO:
             -Check for existence of user file.
             -If nonexistent, create.
             -If blank, amend
             -If exists, modify credentials
             -Return to main menu
             */
        }

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

        //Continuous method for monitoring sub.
        private void Listen()
        {
            while (true)
            {
                try
                {
                    //input needs to be acquired from a locally saved text file after being input, and will be used to filter the results
                    string input = "";

                    //List should also be acquired from a locally saved text file, and will consist of
                    //all previous entries the user has already been notified of. 
                    List<string> duplicateList = new List<string>();

                    //login credentials to be saved to text file
                    string user = "";
                    string password = "";
                    string sub = "";

                    //gets list of 25 most recent posts from designated sub.
                    List<string> resultList = new Program().GetPosts(user, password, sub);

                    //Remove posts that do not match criteria
                    foreach (string s in resultList)
                    {
                        if (s == input)
                            resultList.Remove(s);
                    }

                    //cycle through the list of already-posted results
                    if (resultList.Count > 0)
                    {
                        foreach (string s in duplicateList)
                        {
                            //cycle through current result list
                            foreach (string s2 in resultList)
                            {
                                //compare result to potential duplicate, removing it if duplicate
                                if (s == s2)
                                {
                                    resultList.Remove(s2);
                                }
                            }
                        }
                    }

                    //Now that the list has been trimmed, notify user of all results
                    if (resultList.Count > 1)
                    {
                        foreach (string s in resultList)
                            new Program().NotifyUser(s);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            }
        }

        //Method for sending email notification to user.
        void NotifyUser(string result)
        {
            //code to notify user of the result.
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
    }

}
