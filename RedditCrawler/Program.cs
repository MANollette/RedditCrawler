using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RedditSharp.Things.VotableThing;

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
                    GetPosts();
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.ReadLine();
                }
            }
        }

        static public void GetPosts()
        {
            Thread.Sleep(30000);
            var reddit = new Reddit();
            var user = reddit.LogIn("user", "password");
            var subreddit = reddit.GetSubreddit("/r/MiniSwap");
            subreddit.Subscribe();
            foreach (var post in subreddit.New.Take(25))
            {
                /*if (post.Title == "What is my karma?")
                {
                    // Note: This is an example. Bots are not permitted to cast votes automatically.
                    post.Upvote();
                    var comment = post.Comment(string.Format("You have {0} link karma!", post.Author.LinkKarma));
                    comment.Distinguish(DistinguishType.Moderator);
                }*/
                Console.WriteLine(post.Title.ToString());
                Console.ReadLine();
            }
        }
    }

}
