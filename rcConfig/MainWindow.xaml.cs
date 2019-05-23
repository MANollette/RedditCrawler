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
using rcListenLibrary;
using System.Windows.Controls.Primitives;

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
            rcHelper rc = new rcHelper();

            //Initial code for populating textboxes with existing criteria
            #region populate fields
            try
            {
                //Toggle toast toggle button based on existing criteria
                rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcToast.txt");
                if (rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcToast.txt").Count > 0)
                {
                    if (rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcToast.txt")[0] == "yes")
                        tbtnToast.IsChecked = true;
                }

                //Populate search criteria contents with existing criteria
                rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                if (rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt").Count > 0)
                {
                    List<string> lstSearchInput = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                    if (lstSearchInput.Count > 0)
                    {
                        foreach (string s in lstSearchInput)
                        {
                            rtfSearchTerms.AppendText(s + "\n");
                        }
                    }
                }
            
                //Populate subreddit with existing sub to monitor
                rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt");
                if (rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt").Count != 0)
                {
                    string sub = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt").First();
                    if (sub.Count() > 3)
                    {
                        txtSubreddit.Text = sub;
                    }
                }

                //Populate email field with existing email. 
                rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
                if (rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt").Count != 0)
                {
                    List<string> emailCredentials = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
                    if (emailCredentials.Count > 1)
                    {
                        txtEmail.Text = rc.DecodePassword(emailCredentials[0]);
                        pwdEmail.Password = rc.DecodePassword(emailCredentials[1]);
                    }
                }

                //Populate login fields with existing credentials
                rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                if (rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt").Count != 0)
                {
                    List<string> redditCredentials = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                    if (redditCredentials.Count > 1)
                    {
                        txtRedditLogin.Text = rc.DecodePassword(redditCredentials[0]);
                        pwdReddit.Password = rc.DecodePassword(redditCredentials[1]);
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                rc.DebugLog(ex);
            }
            #endregion
        }

        //Region containing all button events
        #region buttonClicks

        /// <summary>
        /// Confirms contents of subreddit textbox and writes contents to rcSubreddit.txt using <see cref="NewSub(string)"/> if valid. 
        /// </summary>
        private void btnSubmitSub_click(object sender, RoutedEventArgs e)
        {
            rcHelper rc = new rcHelper();
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
                    rc.NewSub(sub);
                    MessageBox.Show("Subreddit accepted");
                }
            }
            catch (Exception ex)
            {
                rc.DebugLog(ex);
            }
        }

        /// <summary>
        /// Confirms contents of email and email password boxes and encrypts/writes contents to rcEmail.txt using <see cref="NewEmail(string, string)"/> if valid. 
        /// </summary>
        private void btnSubmitEmail_click(object sender, RoutedEventArgs e)
        {
            rcHelper rc = new rcHelper();
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
                    if (rc.IsEmailValid(txtEmail.Text) == true)
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
                    bool eLog = rc.NewEmail(email, ePass);
                    if (eLog == false)
                        return;
                    MessageBox.Show("Email credentials accepted & saved");
                }
            }
            catch(Exception ex)
            {
                rc.DebugLog(ex);
            }
        }

        /// <summary>
        /// Confirms contents of login & password boxes and encrypts/writes contents to rcLogin.txt using <see cref="NewLogin(string, string)"/> if valid. 
        /// </summary>
        private void btnSubmitLogin_click(object sender, RoutedEventArgs e)
        {
            rcHelper rc = new rcHelper();

            try
            {
                //Confirm an internet connection is present
                bool testNetwork = rcConnectivity.IsNetworkAvailable(0);
                if (testNetwork == false)
                {
                    MessageBox.Show("Please check your internet connection.");
                    return;
                }

                //Initialize variables from login and password fields, confirming formats
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
                    bool rLog = rc.NewLogin(rUser, rPass);
                    if (rLog == false)
                        return;
                    MessageBox.Show("Login credentials accepted & saved");
                }

            }
            catch (Exception ex)
            {
                rc.DebugLog(ex);
            }
        }

        /// <summary>
        /// Confirms contents of search criteria box and writes contents to rcSearchCriteria.txt 
        /// using <see cref="WriteTRToFile(string, TextRange, bool)"/> if valid. 
        /// </summary>
        private void btnSubmitSearch_Click(object sender, RoutedEventArgs e)
        {
            rcHelper rc = new rcHelper();

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
                    try
                    {
                        //Ensures file exists
                        rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                        using (StreamWriter sw = new StreamWriter(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt", false))
                        {
                            sw.Write(searchCriteria.Text);
                            sw.Flush();
                            sw.Close();
                        }
                    }
                    catch (Exception ex)
                    {
                        rc.DebugLog(ex);
                    }
                    MessageBox.Show("Criteria successfully updated. You may now run RedditCrawler");                  
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
                rc.DebugLog(ex);
            }
        }

        /// <summary>
        /// Saves status of toast notifications.
        /// </summary>
        private void tbtnToggleToast_Click(object sender, RoutedEventArgs e)
        {
            rcHelper rc = new rcHelper();
            try
            {
                if ((sender as ToggleButton).IsChecked ?? false)
                {
                    rc.ToggleToast("yes");
                    MessageBox.Show("Toast notifications are now enabled");
                }
                else
                {
                    rc.ToggleToast("no");
                    MessageBox.Show("Toast notifications are now disabled");
                }
            }
            catch(Exception ex)
            {
                rc.DebugLog(ex);
            }
        }
        #endregion
    }
}
