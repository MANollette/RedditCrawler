using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rcListenLibrary;
using RedditSharp;
using System.IO;
using System.Net.Mail;

namespace rcListenLibrary
{
    /// <summary>
    /// rcConnectivity contains methods for interacting with Reddit and notifying the user of any changes
    /// </summary>
    /// <remarks>
    /// N/A
    /// </remarks>
    public class rcConnectivity
    {
        /// <summary>
        /// Takes a string from the passing method, and uses <see cref="CheckFileExists(string)"/> and <see cref="ReadFile(string)"/> to retrieve
        /// saved email credentials. These are then decoded using <see cref="DecodePassword(string)"/> and used to send a MailMessage object using 
        /// SmtpClient to the user, informing them of a posted match. 
        /// </summary>
        /// <param name="result">Passed string for notification purposes.</param>
        public void NotifyUser(string result)
        {
            rcHelper rc = new rcHelper();

            //Confirms rcEmail.txt exists
            rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
            //Retrieves email credentials from rcEmail.txt
            List<string> emailCredentials = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
            //If credentials exist, continue. 
            if (emailCredentials.Count > 0)
            {
                string email = rc.DecodePassword(emailCredentials[0]);
                string password = rc.DecodePassword(emailCredentials[1]);

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
                    rc.DebugLog(ex);
                }
            }
            else
            {
                Exception e = new Exception("Error caught validating email credentials in RedditCrawler service");
                rc.DebugLog(e);
            }
        }

        /// <summary>
        /// Takes a user name, password, and subreddit from calling method. Uses these to run <see cref="Reddit.GetSubreddit(string).New.Take(15)"/>
        /// to retrieve posts from the specified subreddit. The titles of these are then passed to <paramref name="lstResultList"/> and returned.
        /// </summary>
        /// <param name="user">Passed Reddit username.</param>
        /// <param name="password">Passed Reddit password.</param>
        /// <param name="sub">Passed subreddit in string format.</param>
        /// <returns>
        ///     <param name="=lstResultList">Returns list of titles retrieved from subreddit.</param>
        /// </returns>
        public List<string> GetPosts(string user, string password, string sub)
        {
            rcHelper rc = new rcHelper();
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
                rc.DebugLog(ex);
            }
            return null;
        }
    }
}
