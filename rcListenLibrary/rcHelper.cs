using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// Takes a filepath from the calling method, checks if the file exists at that path, 
        /// and creates it if not. 
        /// </summary>
        /// <param name="filePath">String the file path to check for. </param>
        public void CheckFileExists(string filePath)
        {
            //Check if the file exists at the given file path
            if (!File.Exists(filePath))
            {
                //If not, create it
                File.Create(filePath).Dispose();
            }
        }

        /// <summary>
        /// Takes a filepath from the calling method, checks if it exists, and reads the data from it. 
        /// </summary>
        /// <param name="filePath">String representing the filepath to read from.</param>
        /// <returns>
        ///     <param name="=lstRead">Returns list of lines read from text file at <paramref name="filePath"/></param>
        /// </returns>
        public List<string> ReadFile(string filePath)
        {
            //List to append the lines of the file to
            List<string> lstRead = new List<string>();

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
                DebugLog(ex);
            }
            return null;
        }

        /// <summary>
        /// Takes a filepath from the calling method, check if it exists, and write input to the file. 
        /// </summary>
        /// <param name="filePath">String representing the file to write to</param>
        /// <param name="list">List containing all items to write to <paramref name="filePath"/></param>
        /// <param name="append">Boolean instructing the method to either append or overwrite the contents</param>
        public void WriteToFile(string filePath, List<string> list, bool append)
        {
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
                DebugLog(ex);
            }
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
        public List<string> NotificationList(List<string> lstPosts, List<string> lstSearchTerms, List<string> lstDuplicates)
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

        /// <summary>
        /// Takes a passed exception, and logs the details to local file DebugLog.txt
        /// </summary>
        /// <param name="e">Exception thrown in calling method. </param>
        public void DebugLog(Exception e)
        {
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
    }
}
