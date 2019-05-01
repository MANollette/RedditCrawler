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
                        txtEmail.Text = emailCredentials[0];
                        pwdEmail.Password = emailCredentials[1];
                    }
                }

                //Populate login fields with existing credentials
                CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                if (ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt").Count != 0)
                {
                    List<string> redditCredentials = ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
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
        private bool NewLogin(string user, string password)
        {
            try
            {
                CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                List<string> credentials = new List<string>();

                //Test login credentials
                var reddit = new Reddit();
                var login = reddit.LogIn(user, password);
                credentials.Add(user);
                credentials.Add(password);

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

        //Method for changing the subreddit to monitor
        private void NewSub(string sub)
        {
            string subFilePath = Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt";

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
        private bool NewEmail(string email, string pass)
        {
            //set the name of the file path that contains 
            //the information of the user
            string emailFilePath = Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt";

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
            CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcErrorLog.txt");
            List<string> lstError = new List<string>();
            string s = "Error occurred: " + DateTime.Now.ToShortDateString() + "\nSource: " + e.Source + "\nStack trace: " + e.StackTrace
                + "\nTarget site: " + e.TargetSite + "\nData: " + e.Data + "\nMessage: " + e.Message;
            Console.WriteLine(s);
            lstError.Add(s);
            WriteToFile(Directory.GetCurrentDirectory().ToString() + "/rcErrorLog.txt", lstError, true);
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

                if (rUser != null && rPass != null && email != null && ePass != null && sub != null && searchCriteria != null)
                {
                    try
                    {
                        bool rLog = NewLogin(rUser, rPass);
                        if (rLog == false)
                            return;
                        bool eLog = NewEmail(email, ePass);
                        if (eLog == false)
                            return;
                        NewSub(sub);
                        WriteTRToFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt", searchCriteria, false);
                        MessageBox.Show("Criteria successfully updated. You may now run RedditCrawler");
                    }
                    catch(Exception ex)
                    {

                    }
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
