using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RedditSharp.Things.VotableThing;
using System.Security;

namespace RedditCrawler
{
    class Program
    {
        static void Main(string[] args)
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
                    foreach(string s in resultList)
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
                            new Program().notifyUser(s);
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            }
        }

        void notifyUser(string result)
        {
            //code to notify user of the result.
        }

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
