using Microsoft.Win32;
using RedditSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
                CheckFileExists("../../../rcSearchCriteria.txt");
                if (ReadFile("../../../rcSearchCriteria.txt").Count != 0)
                {
                    List<string> lstSearchInput = ReadFile("../../../rcSearchCriteria.txt");
                    if (lstSearchInput.Count > 1)
                    {
                        foreach (string s in lstSearchInput)
                        {
                            rtfSearchTerms.AppendText(s);
                        }
                    }
                }
            
                //Populate subreddit with existing sub to monitor
                CheckFileExists("../../../rcSubreddit.txt");
                if (ReadFile("../../../rcSubreddit.txt").Count != 0)
                {
                    string sub = ReadFile("../../../rcSubreddit.txt").First();
                    if (sub.Count() > 3)
                    {
                        txtSubreddit.Text = sub;
                    }
                }

                //Populate email field with existing email. 
                CheckFileExists("../../../rcEmail.txt");
                if (ReadFile("../../../rcEmail.txt").Count != 0)
                {
                    List<string> emailCredentials = ReadFile("../../../rcEmail.txt");
                    if (emailCredentials.Count > 1)
                    {
                        txtEmail.Text = emailCredentials[0];
                        pwdEmail.Password = emailCredentials[1];
                    }
                }

                //Populate login fields with existing credentials
                CheckFileExists("../../../rcLogin.txt");
                if (ReadFile("../../../rcLogin.txt").Count != 0)
                {
                    List<string> redditCredentials = ReadFile("../../../rcLogin.txt");
                    if (redditCredentials.Count > 1)
                    {
                        txtRedditLogin.Text = redditCredentials[0];
                        pwdReddit.Password = redditCredentials[1];
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

        #region NewCriteria   

        //Method for changing login credentials
        private void NewLogin(string user, string password)
        {
            try
            {
                CheckFileExists("../../../rcLogin.txt");
                List<string> credentials = new List<string>();

                //Test login credentials
                var reddit = new Reddit();
                var login = reddit.LogIn(user, password);
                credentials.Add(user);
                credentials.Add(password);

                WriteToFile("../../../rcLogin.txt", credentials, false);
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                MessageBox.Show("Invalid reddit login credentials");
            }
        }

        //Method for changing the subreddit to monitor
        private void NewSub(string sub)
        {
            string subFilePath = "../../../rcSubreddit.txt";

            try
            {
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

        //Method for changing email address
        private void NewEmail(string email, string pass)
        {
            //set the name of the file path that contains 
            //the information of the user
            string emailFilePath = "../../../rcEmail.txt";

            try
            {
                CheckFileExists(emailFilePath);
                List<string> subList = new List<string>();
                subList.Add(email);
                subList.Add(pass);
                SmtpClient SmtpServer = new SmtpClient("smtp.gmail.com");
                SmtpServer.Port = 587;
                SmtpServer.Credentials = new System.Net.NetworkCredential(email, pass);
                SmtpServer.EnableSsl = true;
                WriteToFile(emailFilePath, subList, false);
            }
            catch (Exception ex)
            {
                DebugLog(ex);
                MessageBox.Show("Invalid email credentials: " + ex.Message);
            }
        }

        #endregion

        #region HelperMethods

        //Check if the file at the specified file path exists
        //If it doesn't, go ahead and create one
        private void CheckFileExists(string filePath)
        {
            //Check if the file exists at the given file path
            if (!File.Exists(filePath))
            {
                //If not, create it
                File.Create(filePath).Dispose();
            }
        }

        //Reads all of the lines from the specified file and
        //returns a list with all of them
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

        //Writes list to file. bool append determines whether or not to overwrite existing
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
                Console.Clear();
            }

        }

        //Writes TextRange to file. bool append determines whether or not to overwrite existing
        private void WriteTRToFile(string filePath, TextRange tr, bool append)
        {
            try
            {
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

        //Method for logging error details to debug file and returning to menu.
        private void DebugLog(Exception e)
        {
            CheckFileExists("rcErrorLog.txt");
            List<string> lstError = new List<string>();
            string s = "Error occurred: " + DateTime.Now.ToShortDateString() + "\nSource: " + e.Source + "\nStack trace: " + e.StackTrace
                + "\nTarget site: " + e.TargetSite + "\nData: " + e.Data + "\nMessage: " + e.Message;
            Console.WriteLine(s);
            lstError.Add(s);
            WriteToFile("rcErrorLog.txt", lstError, true);
            Console.ReadLine();
        }

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

        #endregion


        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                #region variable_Initialization
                //Initialize all variables.
                string rUser = null;
                if (txtRedditLogin.Text != null && txtRedditLogin.Text.Count() > 2)
                {
                    rUser = txtRedditLogin.Text;
                }
                string email = null;
                if (txtEmail.Text != null)
                {
                    if (IsEmailValid(txtEmail.Text) == true)
                    {
                        email = txtEmail.Text;
                    }
                }
                string sub = null;
                if (txtSubreddit.Text[0] == '/' && txtSubreddit.Text[1].ToString().ToLower() == "r" && txtSubreddit.Text[2] == '/')
                {
                    if (txtSubreddit.Text.Count() > 3)
                    {
                        sub = txtSubreddit.Text;
                    }
                }
                else
                {
                    MessageBox.Show("Please ensure your subreddit begins with '/r/'");
                }

                string rPass = null;
                if (pwdReddit.Password != null)
                {
                    rPass = pwdReddit.Password;
                }
                string ePass = null;
                if (pwdEmail.Password != null)
                {
                    ePass = pwdEmail.Password;
                }

                TextRange searchCriteria = null;
                var start = rtfSearchTerms.Document.ContentStart;
                var end = rtfSearchTerms.Document.ContentEnd;
                int difference = start.GetOffsetToPosition(end);
                if (difference != 0)
                {
                    searchCriteria = new TextRange(rtfSearchTerms.Document.ContentStart, rtfSearchTerms.Document.ContentEnd);
                }

                #endregion

                if (rUser != null && rPass != null && email != null && ePass != null && sub != null && searchCriteria != null)
                {
                    NewLogin(rUser, rPass);
                    NewEmail(email, ePass);
                    NewSub(sub);
                    WriteTRToFile("../../../rcSearchCriteria.txt", searchCriteria, false);
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                DebugLog(ex);
            }

        }
    }
}
