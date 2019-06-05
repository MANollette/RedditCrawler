using Newtonsoft.Json;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace rcListenLibrary
{
    /// <summary>
    /// rcHelper contains methods for assisting with other program operations.
    /// </summary>
    /// <remarks>
    /// Operations include password encoding, file checking and writing, notification listing, and debug logging. 
    /// </remarks>
    public class rcHelper
    {
        static string jsonFilePath = "/rcData.json";

        /// <summary>
        /// Takes user input string, converts it using UTF8, and returns the encoded password.
        /// </summary>
        /// <param name="password">String representing user's password</param>
        /// <returns>
        ///     <param name="=encodedPassword">, user's input password encoded using UTF8.</param>
        /// </returns>
        public string EncodePassword(string password)
        {
            string encodedPassword = String.Empty;

            //Try encoding password in byte array format
            try
            {
                byte[] data_byte = Encoding.UTF8.GetBytes(password);
                encodedPassword = HttpUtility.UrlEncode(Convert.ToBase64String(data_byte));
            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
            return encodedPassword;
        }

        /// <summary>
        /// Reads encoded password from file, decrypts it, and returns it to calling method. 
        /// </summary>
        /// <param name="encodedPassword">String representing user's encodedpassword</param>
        /// <returns>
        ///     <param name="=decodedPassword">, user's original input password.</param>
        /// </returns>
        public string DecodePassword(string encodedPassword)
        {
            string decodedPassword = String.Empty;

            //Try decoding password back to string format
            try
            {
                byte[] data_byte = Convert.FromBase64String(HttpUtility.UrlDecode(encodedPassword));
                decodedPassword = Encoding.UTF8.GetString(data_byte);
            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
            return decodedPassword;
        }

        /// <summary>
        /// Takes a filepath from the calling method, checks if it exists, and reads the data from it. 
        /// </summary>
        /// <param name="filePath">String representing the filepath to read from.</param>
        /// <returns>
        ///     <param name="=lstRead">Returns list of lines read from text file at <paramref name="filePath"/></param>
        /// </returns>
        public RCDetails GetJson(string filePath)
        {
            try
            {
                //Confirms file exists
                bool b = File.Exists(Directory.GetCurrentDirectory().ToString() + filePath);

                if (b == true)
                {
                    var json = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + filePath);
                    RCDetails parsedDetails = JsonConvert.DeserializeObject<RCDetails>(json);
                    return parsedDetails;
                }
                else return null;
            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
            return null;
        }

        /// <summary>
        /// Takes three lists from calling method, compares <paramref name="lstPosts"/> to <paramref name="lstSearchTerms"/> to remove non-matches, and then
        /// compares to <paramref name="lstDuplicates"/> to check for duplicate entries, removing them as well. 
        /// </summary>
        /// <param name="lstPosts">List of retrieved posts to check against.<paramref name="lstSearchTerms"/> and <paramref name="lstDuplicates"/></param>
        /// <param name="lstSearchTerms">List of user-specified search terms to search for.</param>
        /// <param name="lstDuplicates">List of post titles the user has already been notified of.</param>
        /// <returns>
        ///     <param name="=lstPassedList">Returns the trimmed version of lstPosts, minus all duplicates and non-search matches.</param>
        /// </returns>
        public Tuple<List<string>, List<string>> NotificationList(List<string> lstPosts, List<string> lstUrl, List<string> lstSearchTerms, List<string> lstDuplicates)
        {
            List<string> lstPassedList = new List<string>();
            List<string> lstUrlUpdated = new List<string>();
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
                        lstUrlUpdated.Add(lstUrl[i]);
                    }
                }
            }

            //Filter out duplicate entries that may match multiple criteria
            lstPassedList = lstPassedList.Distinct().ToList();

            //cycle through the list of already-posted results from lstDuplicates & remove matches
            if (lstDuplicates != null)
            {
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
                                lstUrlUpdated.RemoveAt(i2);
                            }
                        }
                    }
                }
            }
            //Return filtered list
            return Tuple.Create(lstPassedList, lstUrlUpdated);
        }

        /// <summary>
        /// Takes a passed exception, and logs the details to local file DebugLog.txt
        /// </summary>
        /// <param name="e">Exception thrown in calling method. </param>
        public void DebugLog(Exception e)
        {
            //Ensure error log exists.
            string filePath = Directory.GetCurrentDirectory().ToString() + "/rcDebugLog.txt";
            //Initialize error details for writing to rcErrorLog.txt
            string s = "Error occurred: " + DateTime.Now.ToShortDateString() + "\nSource: " + e.Source + "\nStack trace: " + e.StackTrace
                + "\nTarget site: " + e.TargetSite + "\nData: " + e.Data + "\nMessage: " + e.Message;
            //Write error details to rcErrorLog.txt
            if (!File.Exists(filePath))
                File.Create(filePath).Dispose();
            
            using (StreamWriter sw = new StreamWriter(filePath, true))
            {
                sw.Write(s);
                sw.Flush();
                sw.Close();
            }
        }

        /// <summary>
        /// Takes an instance of the RCDetails class, serializes it & saves it as a Json object
        /// </summary>
        /// <param name="rcd">Instance of classe detailing all saved information</param>
        public void WriteToFile(RCDetails rcd)
        {
            string JSONresult = JsonConvert.SerializeObject(rcd, Formatting.Indented);
            if (File.Exists(Directory.GetCurrentDirectory().ToString() + jsonFilePath))
                File.Delete(Directory.GetCurrentDirectory().ToString() + jsonFilePath);
            File.WriteAllText(Directory.GetCurrentDirectory().ToString() + jsonFilePath, JSONresult);
        }

        /// <summary>
        /// Takes user Reddit credentials, encrypts them using <see cref="EncodePassword(string)"/>, and saves them to a local text
        /// file for future use. 
        /// </summary>
        /// <param name="user">String representing user's Reddit user name.</param>
        /// <param name="password">String representing user's Reddit password</param>
        /// <returns>
        ///     <c>true</c> if login credentials are valid and successfully saved, otherwise <c>false</c>.
        /// </returns>
        public bool NewLogin(string user, string password)
        {
            try
            {
                RCDetails json = GetJson(jsonFilePath);
                if (json == null)
                {
                    json = new RCDetails();
                    json.rLogin = EncodePassword(user);
                    json.rPass = EncodePassword(password);
                    WriteToFile(json);
                    return false;
                }
                json.rLogin = EncodePassword(user);
                json.rPass = EncodePassword(password);
                WriteToFile(json);
                return true;
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                return false;
            }
        }

        /// <summary>
        /// Takes subreddit details in string form, and saves them to a local text file for future use. 
        /// </summary>
        /// <param name="sub">Subreddit for RedditCrawler to monitor in string format</param>.
        public void NewSub(string sub)
        {
            try
            {
                RCDetails json = GetJson(jsonFilePath);
                if (json == null)
                    json = new RCDetails();
                json.sub = sub;
                WriteToFile(json);
            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
        }

        /// <summary>
        /// Takes user email credentials, encrypts them using <see cref="EncodePassword(string)"/>, and saves them to a local text
        /// file for future use. 
        /// </summary>
        /// <param name="email">String representing user's email address</param>
        /// <param name="pass">String representing user's email password</param>
        /// <returns>
        ///     <c>true</c> if credentials are valid and successfully saved, otherwise <c>false</c>.
        /// </returns>
        public bool NewEmail(string email, string pass)
        {
            RCDetails json = GetJson(jsonFilePath);
            bool ret = true;
            try
            {                    
                if (json == null)
                {
                    json = new RCDetails();
                    ret = false;
                }
                json.email = EncodePassword(email);
                json.ePass = EncodePassword(pass);
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                return false;
            }
            try
            {
                //Tests input credentials by sending a test email. 
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(email, pass);
                SmtpServer.EnableSsl = true;
                MailMessage mail = new MailMessage();
                mail.From = new MailAddress(email); mail.To.Add(email);
                mail.Subject = "Redditcrawler Email Test";
                mail.Body = "Your RedditCrawler email has been successfully verified!";
                SmtpServer.Send(mail);
                WriteToFile(json);
                return ret;
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                return false;
            }
        }

        ///<summary>
        ///Takes search criteria as a List, and appends it to the json object
        /// </summary>
        /// <param name="sr">List containing all user's search criteria separated into individual strings</param>
        public void NewSearchCriteria(List<string> sr)
        {
            try
            {
                RCDetails json = GetJson(jsonFilePath);
                if (json == null)
                    json = new RCDetails();
                json.searchCriteria = sr;
                WriteToFile(json);
            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
        }

        /// <summary>
        /// Handles saving of whether or not toast notifications are enabled through the UI
        /// </summary>
        /// <returns>
        ///     <c>true</c> if toast notifications are enabled, otherwise <c>false</c>.
        /// </returns>
        public void ToggleToast(string toastStatus)
        {
            try
            {
                RCDetails json = GetJson(jsonFilePath);
                if (json == null)
                    json = new RCDetails();
                    json.toast = toastStatus;
                    WriteToFile(json);
                
            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
        }

        /// <summary>
        /// Takes passed string and checks to confirm if it is in valid email format. 
        /// </summary>
        /// <param name="email">String containing user's email address</param>
        /// <returns>
        ///     <c>true</c> if email is in valid format, otherwise <c>false</c>.
        /// </returns>
        public bool IsEmailValid(string email)
        {
            try
            {
                MailAddress m = new MailAddress(email);
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Handles saving of whether or not toast notifications are enabled through the UI
        /// </summary>
        /// <returns>
        ///     <c>true</c> if toast notifications are enabled, otherwise <c>false</c>.
        /// </returns>
        public void NewDuplicateResult(List<string> addedDupResultsJson)
        {
            try
            {
                RCDetails json = GetJson(jsonFilePath);
                if (json != null)
                {
                    List<string> dupResults = json.dupResults;
                    foreach (string s in addedDupResultsJson)
                        dupResults.Add(s);
                    json.dupResults = dupResults;
                    WriteToFile(json);
                }
            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
        }
    }
}
