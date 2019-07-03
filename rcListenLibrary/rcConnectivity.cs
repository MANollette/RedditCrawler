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
        static string jsonFilePath = "/rcData.json";

        /// <summary>
        /// Takes a string from the passing method, and checks file existence, then uses <see cref="GetJson(string)"/> to retrieve
        /// saved JSON object. The JSON's email and password attributes are then decoded using <see cref="DecodePassword(string)"/> 
        /// and used to send a MailMessage object using SmtpClient to the user, informing them of a posted match. 
        /// </summary>
        /// <param name="result">Passed string for notification purposes.</param>
        /// <param name="url">Passed URL for ease of access</param>
        public void NotifyUser(List<string> result, List<string> url)
        {
            //Initialize helper class
            rcHelper rc = new rcHelper();

            //Confirms rcData.json exists
            if (!File.Exists(Path.Combine(Directory.GetCurrentDirectory().ToString() + jsonFilePath)))
            {
                rc.DebugLog(new Exception("JSON has not been created"));
                Environment.Exit(0);
            }

            //Retrieves json object from file path
            RCDetails json = rc.GetJson(jsonFilePath);
            string email = rc.DecodePassword(json.email);
            string password = rc.DecodePassword(json.ePass);

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
                    StringBuilder strMailContent = new StringBuilder();
                    mail.From = new MailAddress(email);
                    mail.To.Add(email);
                    mail.Subject = "New Reddit Post!";                   
                    for(int i = 0; i < result.Count(); i++)
                    {
                        strMailContent.Append(result[i] + "\n" + url[i] + "\n\n");
                    }
                    mail.Body = "The following posts were created " + DateTime.Now.ToShortDateString()
                        + ":\n\n" + strMailContent;
                    //Sets port, credentials, SSL, and sends mail object. 
                    SmtpServer.Port = 587;
                    SmtpServer.Credentials = new System.Net.NetworkCredential(email, password);
                    SmtpServer.EnableSsl = true;
                    SmtpServer.Send(mail);
                    Console.WriteLine("mail Sent");
                    SmtpServer.Dispose();
                    mail.Dispose();
                }
                //Generic exception handling
                catch (Exception ex)
                {
                    rc.DebugLog(ex);
                }
            }
        }

        /// <summary>
        /// Takes a user name, password, and subreddit from calling method. Uses these to run <see cref="Reddit.GetSubreddit(string).New.Take(15)"/>
        /// to retrieve posts from the specified subreddit. The titles and URLs of these are then passed to 
        /// <paramref name="lstResultList"/> and <paramref name="lstUrl"/> and returned.
        /// </summary>
        /// <param name="user">Passed Reddit username.</param>
        /// <param name="password">Passed Reddit password.</param>
        /// <param name="sub">Passed subreddit in string format.</param>
        /// <returns>
        ///     <param name="=lstResultList">Returns list of titles retrieved from subreddit.</param>
        ///     <param name="=lstUrl">Returns list of post URLs retrieved from subreddit.</param>
        /// </returns>
        public Tuple<List<string>, List<string>> GetPosts(string sub)
        {
            //Initialize helper class
            rcHelper rc = new rcHelper();

            try
            {
                //Initialize instance of Reddit class using RedditSharp, then login with passed credentials. 
                var reddit = new Reddit();
                var subreddit = reddit.GetSubreddit(sub);
                var posts = subreddit.Posts.GetListing(15);

                //Initialize lists for later return
                List<string> lstResultList = new List<string>();
                List<string> lstUrl = new List<string>();

                //Retrieves title and URL of 15 posts, adds them to a lists
                foreach (var post in posts)
                {
                    lstResultList.Add(post.Title.ToString());
                    lstUrl.Add(post.Url.ToString());
                }
                //Returns list of post titles. 
                return Tuple.Create(lstResultList, lstUrl);
            }
            //Generic exception handling
            catch (Exception ex)
            {
                rc.DebugLog(ex);
                return null;
            }
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
