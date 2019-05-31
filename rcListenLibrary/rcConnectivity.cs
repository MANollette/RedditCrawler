using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using rcListenLibrary;
using RedditSharp;
using System.IO;
using System.Net.Mail;
using System.Net.NetworkInformation;

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
        static string txtFilePath = "/rcData.txt";

        /// <summary>
        /// Takes a string from the passing method, and uses <see cref="CheckFileExists(string)"/> and <see cref="ReadFile(string)"/> to retrieve
        /// saved email credentials. These are then decoded using <see cref="DecodePassword(string)"/> and used to send a MailMessage object using 
        /// SmtpClient to the user, informing them of a posted match. 
        /// </summary>
        /// <param name="result">Passed string for notification purposes.</param>
        public void NotifyUser(string result)
        {
            rcHelper rc = new rcHelper();

            //Confirms rcData.txt exists
            rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + txtFilePath);
            //Retrieves email credentials from rcData.txt
            List<string> dataList = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + txtFilePath);
            string email = null;
            string password = null;            
            if (dataList.Count > 0)
            {
                for(int i = 0; i < dataList.Count; i++)
                {
                    if (dataList[i] == "EMAIL")
                    {
                        email = rc.DecodePassword(dataList[i + 1]);
                        password = rc.DecodePassword(dataList[i + 2]);
                    }
                }

                //If credentials exist, continue. 
                if (email != null && password != null)
                {
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

        /// <summary>
        /// Indicates whether any network connection is available
        /// Filter connections below a specified speed, as well as VN cards.
        /// </summary>
        /// <returns>
        ///     <c>true</c> if a network connection is available; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNetworkAvailable()
        {
            return IsNetworkAvailable(0);
        }

        /// <summary>
        /// Indicates whether any network connection is available. The default 
        /// <see cref="NetworkInterface.GetIsNetworkAvailable()"/> can have some trouble with virtual 
        /// cards, and does not handle filtering/discarding network availability based on speed. 
        /// 
        /// This method filters connections below a specified speed, as well as virtual network cards.
        /// </summary>
        /// <param name="minimumSpeed">The minimum speed required. Passing 0 will not filter connection using speed.</param>
        /// <returns>
        ///     <c>true</c> if a network connection is available; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsNetworkAvailable(long minimumSpeed)
        {
            if (!NetworkInterface.GetIsNetworkAvailable())
                return false;

            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                // discard because of standard reasons
                if ((ni.OperationalStatus != OperationalStatus.Up) ||
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback) ||
                    (ni.NetworkInterfaceType == NetworkInterfaceType.Tunnel))
                    continue;

                // this allow to filter modems, serial, etc.
                // I use 10000000 as a minimum speed for most cases
                if (ni.Speed < minimumSpeed)
                    continue;

                // discard virtual cards (virtual box, virtual pc, etc.)
                if ((ni.Description.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0) ||
                    (ni.Name.IndexOf("virtual", StringComparison.OrdinalIgnoreCase) >= 0))
                    continue;

                // discard "Microsoft Loopback Adapter", it will not show as NetworkInterfaceType.Loopback but as Ethernet Card.
                if (ni.Description.Equals("Microsoft Loopback Adapter", StringComparison.OrdinalIgnoreCase))
                    continue;

                return true;
            }
            return false;
        }
    }
}
