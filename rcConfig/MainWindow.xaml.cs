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
using Newtonsoft.Json;

namespace rcConfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //Global variable initialization
        static string jsonFilePath = "/rcData.json";

        public MainWindow()
        {
            InitializeComponent();

            //Set working directory to match RedditCrawler's
            string s = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\RedditCrawler";
            if (!Directory.Exists(s))
                Directory.CreateDirectory(s);
            Directory.SetCurrentDirectory(s);

            //Initialize helper classes
            rcHelper rc = new rcHelper();
            RCDetails data = new RCDetails();

            //Initial code for populating textboxes with existing criteria
            #region populate fields
            try
            {
                //Toggle toast toggle button and field auto-fill based on existing criteria
                if (File.Exists(Directory.GetCurrentDirectory().ToString() + jsonFilePath))
                {
                    var datRaw = File.ReadAllText(Directory.GetCurrentDirectory().ToString() + jsonFilePath);
                    data = JsonConvert.DeserializeObject<RCDetails>(datRaw);
                    if (data.toast == "yes")
                        tbtnToast.IsChecked = true;
                    if (data.sub != null)
                        txtSubreddit.Text = data.sub;
                    if (data.email != null)
                        txtEmail.Text = rc.DecodePassword(data.email);
                    if (data.ePass != null)
                        pwdEmail.Password = rc.DecodePassword(data.ePass);
                    if (data.searchCriteria.Count > 0)
                        foreach (string s2 in data.searchCriteria)
                        {
                            //Ensures no new line is created at end of list
                            if (data.searchCriteria.IndexOf(s2) != data.searchCriteria.Count - 1)
                                rtfSearchTerms.AppendText(s2 + "\n");
                            else rtfSearchTerms.AppendText(s2);
                        }
                }
            }
            //Generic exception handling
            catch (Exception ex)
            {
                rc.DebugLog(ex);
            }
            #endregion
        }

        //Region containing all button events
        #region buttonClicks

        /// <summary>
        /// Confirms contents of subreddit textbox and writes contents to JSON object using <see cref="NewSub(string)"/> if valid. 
        /// </summary>
        private void btnSubmitSub_click(object sender, RoutedEventArgs e)
        {
            rcHelper rc = new rcHelper();
            try
            {
                //Initialize all variables from subreddit field
                #region variable_Initialization
                //Initialize subreddit variable
                string sub = null;

                //Check for valid input in txtSubreddit textbox
                if (txtSubreddit.Text != null && txtSubreddit.Text.Count() > 3 && txtSubreddit.Text.StartsWith("/r/"))
                {
                    sub = txtSubreddit.Text;                    
                }
                //Display error message if subreddit textbox is empty or contains improperly formatted text.
                else
                {
                    MessageBox.Show("Please input the subreddit you'd like to monitor. \nMake sure it begins with \"/r/\"");
                    return;
                }
                #endregion

                //Write details of text boxes to JSON object, displaying message if it was accepted.
                if (sub != null)
                {
                    rc.NewSub(sub);
                    MessageBox.Show("Subreddit accepted");
                    if (rc.ApplicationReady() == true)
                        MessageBox.Show("You may now run RedditCrawler!");
                }
            }
            //Generic exception handling.
            catch (Exception ex)
            {
                rc.DebugLog(ex);
            }
        }

        /// <summary>
        /// Confirms contents of email and email password boxes and encrypts/writes contents to JSON object using <see cref="NewEmail(string, string)"/> if valid. 
        /// </summary>
        private void btnSubmitEmail_click(object sender, RoutedEventArgs e)
        {
            //Initialize helper class
            rcHelper rc = new rcHelper();

            try
            {
                //Initialize all variables from email and ePass fields.
                #region variable_Initialization
                //Initialize email and password variables
                string email = null;
                string ePass = null;

                //Check for valid input in txtEmail textbox
                if (txtEmail.Text != null && rc.IsEmailValid(txtEmail.Text) == true)
                {
                    email = txtEmail.Text;               
                    
                }
                //If invalid, display error & return
                else
                {
                    MessageBox.Show("Please enter a valid email address");
                    return;
                }

                //Ensure password exists
                if (pwdEmail.Password.Count() > 0)
                {
                    ePass = pwdEmail.Password;
                }
                //If null or empty, display error & return
                else
                {
                    MessageBox.Show("Please input your email password.");
                    return;
                }
                #endregion

                //Tests email credentials & writes them to JSON object if valid. 
                if (email != null && ePass != null)
                {                  
                    bool eLog = rc.NewEmail(email, ePass);
                    if (eLog == false)
                    {
                        MessageBox.Show("Your email address or password was incorrect.");
                        return;
                    }
                    MessageBox.Show("Email credentials accepted & saved");
                    if (rc.ApplicationReady() == true)
                        MessageBox.Show("You may now run RedditCrawler!");
                }
            }
            //Generic exception handling
            catch (Exception ex)
            {
                rc.DebugLog(ex);
            }
        }

        /// <summary>
        /// Confirms contents of search criteria box and writes contents 
        /// to JSON object after conversion to a list
        /// </summary>
        private void btnSubmitSearch_Click(object sender, RoutedEventArgs e)
        {
            //Initialize helper class
            rcHelper rc = new rcHelper();

            try
            {
                //Initialize all search-based variables.
                #region variable_Initialization
                //Initialize TextRange
                TextRange searchCriteria = null;

                //analyzes entries in search terms rich textbox, confirms there is input present. 
                var start = rtfSearchTerms.Document.ContentStart;
                var end = rtfSearchTerms.Document.ContentEnd;
                int difference = start.GetOffsetToPosition(end);
                if (difference > 1)
                {
                    searchCriteria = new TextRange(start, end);
                }
                //If input is not present or is too small, user is directed to input terms in the RTF box
                else
                {
                    MessageBox.Show("Please input the terms you'd like to listen for. \nBe sure to put each term on a new line");
                    return;
                }

                #endregion

                //Only continues if searchCriteria is not null
                if (searchCriteria != null)
                {
                    try
                    {
                        //Retrieves text from searchCriteria TextRange as a string
                        string sl = searchCriteria.Text.ToString();

                        //Separates searchCriteria string sl into a list of strings separated by line breaks. 
                        List<string> scList = sl.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();

                        //Removes empty and NewLine entries from list
                        for (int i = 0; i < scList.Count; i++)
                            if (scList[i] == "" || scList[i] == null || scList[i] == Environment.NewLine)
                                scList.RemoveAt(i);

                        //Runs filtered list into NewSearchCriteria method.
                        rc.NewSearchCriteria(scList);
                    }
                    //Generic exception handling.
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        rc.DebugLog(ex);
                        Environment.Exit(1);
                    }
                    MessageBox.Show("Criteria successfully updated.");
                    if (rc.ApplicationReady() == true)
                        MessageBox.Show("You may now run RedditCrawler!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                rc.DebugLog(ex);
                Environment.Exit(1);
            }
        }

        /// <summary>
        /// Saves status of toast notifications to JSON object.
        /// </summary>
        private void tbtnToggleToast_Click(object sender, RoutedEventArgs e)
        {
            //Initialize helper class
            rcHelper rc = new rcHelper();

            //Handles toggling of toast notifications through rcHelper ToggleToast method.
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
            //Generic exception handling
            catch (Exception ex)
            {
                rc.DebugLog(ex);
                Environment.Exit(1);
            }
        }
        #endregion

        
    }
}
