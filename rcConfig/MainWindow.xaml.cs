using Microsoft.Win32;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.NetworkInformation;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace rcConfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Initial code for populating textboxes with existing criteria
            #region populate fields
            try
            {
                //Populate search criteria contents with existing criteria
                CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                if (ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt").Count > 0)
                {
                    List<string> lstSearchInput = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                    if (lstSearchInput.Count > 0)
                    {
                        foreach (string s in lstSearchInput)
                        {
                            rtfSearchTerms.AppendText(s + "\n");
                        }
                    }
                }
            
                //Populate subreddit with existing sub to monitor
                CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt");
                if (ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt").Count != 0)
                {
                    string sub = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt").First();
                    if (sub.Count() > 3)
                    {
                        txtSubreddit.Text = sub;
                    }
                }

                //Populate email field with existing email. 
                CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
                if (ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt").Count != 0)
                {
                    List<string> emailCredentials = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
                    if (emailCredentials.Count > 1)
                    {
                        txtEmail.Text = DecodePassword(emailCredentials[0]);
                        pwdEmail.Password = DecodePassword(emailCredentials[1]);
                    }
                }

                //Populate login fields with existing credentials
                CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                if (ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt").Count != 0)
                {
                    List<string> redditCredentials = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                    if (redditCredentials.Count > 1)
                    {
                        txtRedditLogin.Text = DecodePassword(redditCredentials[0]);
                        pwdReddit.Password = DecodePassword(redditCredentials[1]);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                DebugLog(ex);
            }
            #endregion
        }

        //Methods for inputting new user data
        #region NewCriteria   

        /// <summary>
        /// Takes user Reddit credentials, encrypts them using <see cref="EncodePassword(string)"/>, and saves them to a local text
        /// file for future use. 
        /// </summary>
        /// <param name="user">String representing user's Reddit user name.</param>
        /// <param name="password">String representing user's Reddit password</param>
        /// <returns>
        ///     <c>true</c> if login credentials are valid and successfully saved, otherwise <c>false</c>.
        /// </returns>
        private bool NewLogin(string user, string password)
        {
            try
            {
                //Ensures rcLogin.txt exists
                CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                List<string> credentials = new List<string>();

                //Test login credentials, catching exception if var login fails
                var reddit = new Reddit();
                bool testNetwork = IsNetworkAvailable();
                if (testNetwork == false)
                    MessageBox.Show("Please check your internet connection.");
                var login = reddit.LogIn(user, password);         
                credentials.Add(EncodePassword(user));
                credentials.Add(EncodePassword(password));

                //Now that the login has been successful, write the credentials to rcLogin.txt
                WriteToFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt", credentials, false);
                return true;
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                MessageBox.Show("Invalid reddit login credentials");
                return false;
            }
        }

        /// <summary>
        /// Takes subreddit details in string form, and saves them to a local text file for future use. 
        /// </summary>
        /// <param name="sub">Subreddit for RedditCrawler to monitor in string format</param>.
        private void NewSub(string sub)
        {
            //Retrieves filepath of intended rcSubreddit.txt
            string subFilePath = Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt";
            try
            {
                //Ensures rcSubreddit.txt exists
                CheckFileExists(subFilePath);
                List<string> subList = new List<string>();
                subList.Add(sub);
                WriteToFile(subFilePath, subList, false);
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                MessageBox.Show(ex.Message);
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
        private bool NewEmail(string email, string pass)
        {
            //set the name of the file path that contains the information of the user
            string emailFilePath = Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt";
            try
            {
                //Ensures file exists
                CheckFileExists(emailFilePath);
                //Creates list with input credentials
                List<string> subList = new List<string>();
                subList.Add(EncodePassword(email));
                subList.Add(EncodePassword(pass));

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
                WriteToFile(emailFilePath, subList, false);
                return true;
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                MessageBox.Show("Invalid email credentials: " + ex.Message);
                return false;
            }
        }

        #endregion

        //Miscellaneous helper methods for writing, reading, encrypting, etc. 
        #region HelperMethods

        /// <summary>
        /// Takes user input string, converts it using UTF8, and returns the encoded password.
        /// </summary>
        /// <param name="password">String representing user's password</param>
        /// <returns>
        ///     <param name="=encodedPassword">, user's input password encoded using UTF8.</param>
        /// </returns>
        private string EncodePassword(string password)
        {
            //Initialize byte array & encoded password field
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

        /// /// <summary>
        /// Reads encoded password from file, decrypts it, and returns it to calling method. 
        /// </summary>
        /// <param name="encodedPassword">String representing user's encodedpassword</param>
        /// <returns>
        ///     <param name="=decodedPassword">, user's original input password.</param>
        /// </returns>
        private string DecodePassword(string encodedPassword)
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
        private void CheckFileExists(string filePath)
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
        private List<string> ReadFile(string filePath)
        {
            //List to append the lines of the file to
            List<string> lstRead = new List<string>();

            try
            {
                CheckFileExists(filePath);
                //using statement to handle the StreamReader
                using (StreamReader sr = new StreamReader(filePath))
                {
                    //string to hold one line from file
                    string line;

                    //loops through the file and ends
                    //when the end of the file is reached
                    while ((line = sr.ReadLine()) != null)
                    {
                        lstRead.Add(line); //add line to list
                    }
                    sr.Close();
                }
                return lstRead; //return list with appended lines  
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                Console.Clear();
            }
            return null;
        }

        /// <summary>
        /// Takes a filepath from the calling method, check if it exists, and write input to the file. 
        /// </summary>
        /// <param name="filePath">String representing the file to write to</param>
        /// <param name="list">List containing all items to write to <paramref name="filePath"/></param>
        /// <param name="append">Boolean instructing the method to either append or overwrite the contents</param>
        private void WriteToFile(string filePath, List<string> list, bool append)
        {
            try
            {
                CheckFileExists(filePath);
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
            catch (Exception ex)
            {
                DebugLog(ex);
            }

        }

        /// <summary>
        /// Takes a filepath from the calling method, check if it exists, and write input TextRange to the file. 
        /// </summary>
        /// <param name="filePath">String representing the file to write to</param>
        /// <param name="tr">TextRange containing all items to write to <paramref name="filePath"/></param>
        /// <param name="append">Boolean instructing the method to either append or overwrite the contents</param>
        private void WriteTRToFile(string filePath, TextRange tr, bool append)
        {
            try
            {
                //Ensures file exists
                CheckFileExists(filePath);
                using (StreamWriter sw = new StreamWriter(filePath, append))
                {
                    sw.Write(tr.Text);
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
        /// Takes a passed exception, and logs the details to local file DebugLog.txt
        /// </summary>
        /// <param name="e">Exception thrown in calling method. </param>
        private void DebugLog(Exception e)
        {
            //Ensures debug log exists
            CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcErrorLog.txt");

            List<string> lstError = new List<string>();
            //Creates string containing error details
            string s = "Error occurred: " + DateTime.Now.ToShortDateString() + "\nSource: " + e.Source + "\nStack trace: " + e.StackTrace
                + "\nTarget site: " + e.TargetSite + "\nData: " + e.Data + "\nMessage: " + e.Message;
            Console.WriteLine(s);
            lstError.Add(s);
            //Writes error details to rcErrorLog.txt
            WriteToFile(Directory.GetCurrentDirectory().ToString() + "/rcErrorLog.txt", lstError, true);
        }

        /// <summary>
        /// Takes passed string and checks to confirm if it is in valid email format. 
        /// </summary>
        /// <param name="email">String containing user's email address</param>
        /// <returns>
        ///     <c>true</c> if email is in valid format, otherwise <c>false</c>.
        /// </returns>
        private bool IsEmailValid(string email)
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
        /// Indicates whether any network connection is available.
        /// Filter connections below a specified speed, as well as virtual network cards.
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

        #endregion

        //Region containing all button events
        #region buttonClicks

        /// <summary>
        /// Confirms contents of subreddit textbox and writes contents to rcSubreddit.txt using <see cref="NewSub(string)"/> if valid. 
        /// </summary>
        private void btnSubmitSub_click(object sender, RoutedEventArgs e)
        {
            try
            {
                //initialize all variables from subreddit field
                #region variable_Initialization
                //Initialize subreddit variable
                string sub = null;
                //Check for valid input in txtSubreddit textbox
                if (txtSubreddit.Text != null && txtSubreddit.Text.Count() > 2)
                {
                    //Confirm subreddit correctly includes initial /r/ format
                    if (txtSubreddit.Text[0] == '/' && txtSubreddit.Text[1].ToString().ToLower() == "r" && txtSubreddit.Text[2] == '/')
                    {
                        if (txtSubreddit.Text.Count() > 3)
                        {
                            sub = txtSubreddit.Text;
                        }
                    }
                    //If invalid, display error message & return. 
                    else
                    {
                        MessageBox.Show("Please ensure your subreddit begins with '/r/'");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Please input the subreddit you'd like to monitor.");
                    return;
                }
                #endregion

                if (sub != null)
                {
                    NewSub(sub);
                    MessageBox.Show("Subreddit accepted");
                }
            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
        }

        /// <summary>
        /// Confirms contents of email and email password boxes and encrypts/writes contents to rcEmail.txt using <see cref="NewEmail(string, string)"/> if valid. 
        /// </summary>
        private void btnSubmitEmail_click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Initialize all variables from email and ePass fields.
                #region variable_Initialization
                //Initialize email variable
                string email = null;
                //Check for valid input in txtEmail textbox
                if (txtEmail.Text != null)
                {
                    //Confirm valid email format
                    if (IsEmailValid(txtEmail.Text) == true)
                    {
                        email = txtEmail.Text;
                    }
                    //If invalid, display error & return
                    else
                    {
                        MessageBox.Show("Please enter a valid email address");
                        return;
                    }
                }
                else
                {
                    MessageBox.Show("Please enter an email address");
                    return;
                }

                //Initialize email password variable.
                string ePass = null;
                if (pwdEmail.Password.Count() > 0)
                {
                    ePass = pwdEmail.Password;
                }
                else
                {
                    MessageBox.Show("Please input your email password.");
                    return;
                }
                #endregion

                if (email != null && ePass != null)
                {
                    //Tests email credentials & writes them to file if valid. 
                    bool eLog = NewEmail(email, ePass);
                    if (eLog == false)
                        return;
                    MessageBox.Show("Email credentials accepted & saved");
                }
            }
            catch(Exception ex)
            {
                DebugLog(ex);
            }
        }

        /// <summary>
        /// Confirms contents of login & password boxes and encrypts/writes contents to rcLogin.txt using <see cref="NewLogin(string, string)"/> if valid. 
        /// </summary>
        private void btnSubmitLogin_click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Initialize variables from login and password fields
                #region variable_Initialization
                //Initialize reddit username as rUser variable. 
                string rUser = null;
                //Confirm valid entry in txtRedditLogin textbox
                if (txtRedditLogin.Text != null && txtRedditLogin.Text.Count() > 2)
                {
                    rUser = txtRedditLogin.Text;
                }
                //If invalid entry, display error & return
                else
                {
                    MessageBox.Show("Please input a valid reddit username");
                    return;
                }

                //Initialize reddit password variable
                string rPass = null;
                if (pwdReddit.Password.Count() > 0)
                {
                    rPass = pwdReddit.Password;
                }
                else
                {
                    MessageBox.Show("Please input your reddit password.");
                    return;
                }
                #endregion

                if (rUser != null && rPass != null)
                {
                    //Tests login credentials
                    bool rLog = NewLogin(rUser, rPass);
                    if (rLog == false)
                        return;
                    MessageBox.Show("Login credentials accepted & saved");
                }

            }
            catch (Exception ex)
            {
                DebugLog(ex);
            }
        }

        /// <summary>
        /// Confirms contents of search criteria box and writes contents to rcSearchCriteria.txt 
        /// using <see cref="WriteTRToFile(string, TextRange, bool)"/> if valid. 
        /// </summary>
        private void btnSubmitSearch_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //Initialize all search-based variables.
                #region variable_Initialization
                
                   
                TextRange searchCriteria = null;
                //analyzes entries in search terms rich textbox, confirms there is input present. 
                var start = rtfSearchTerms.Document.ContentStart;
                var end = rtfSearchTerms.Document.ContentEnd;
                int difference = start.GetOffsetToPosition(end);
                if (difference != 4)
                {                   
                    searchCriteria = new TextRange(rtfSearchTerms.Document.ContentStart, rtfSearchTerms.Document.ContentEnd);
                }
                else
                {
                    MessageBox.Show("Please input the terms you'd like to listen for. \nBe sure to put each term on a new line");
                    return;
                }

                #endregion

                //Checks that each created variable is not null
                if (searchCriteria != null)
                {                                      
                    WriteTRToFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt", searchCriteria, false);
                    MessageBox.Show("Criteria successfully updated. You may now run RedditCrawler");                  
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                DebugLog(ex);
            }
        }
        #endregion
    }
}
