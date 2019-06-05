﻿using Microsoft.Win32;
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
        static string jsonFilePath = "/rcData.json";

        public MainWindow()
        {
            InitializeComponent();
            rcHelper rc = new rcHelper();
            RCDetails data = new RCDetails();

            //Initial code for populating textboxes with existing criteria
            #region populate fields
            try
            {
                //Toggle toast toggle button based on existing criteria
                bool b = (File.Exists(Directory.GetCurrentDirectory().ToString() + jsonFilePath));
                if (b == true)
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
                    if (data.rLogin != null)
                        txtRedditLogin.Text = rc.DecodePassword(data.rLogin);
                    if (data.rPass != null)
                        pwdReddit.Password = rc.DecodePassword(data.rPass);
                    if (data.searchCriteria.Count > 0)
                        foreach (string s in data.searchCriteria)
                        {
                            if (data.searchCriteria.IndexOf(s) != data.searchCriteria.Count - 1)
                                rtfSearchTerms.AppendText(s + "\n");
                            else rtfSearchTerms.AppendText(s);
                        }
                }
            }
            catch (Exception ex)
            {
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
                //Initialize all variables from subreddit field
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

                //Write details of text boxes to rcSubreddit.txt, displaying message if it was accepted.
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
            catch (Exception ex)
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
                    searchCriteria = new TextRange(start, end);
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
                        string sl = searchCriteria.Text.ToString();
                        List<string> scList = sl.Split(new[] { Environment.NewLine }, StringSplitOptions.None).ToList();
                        foreach (string s in scList)
                            if (s == "" || s == null)
                                scList.Remove(s);
                        rc.NewSearchCriteria(scList);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message);
                        rc.DebugLog(ex);
                    }
                    MessageBox.Show("Criteria successfully updated. You may now run RedditCrawler");
                }
            }
            catch (Exception ex)
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
            catch (Exception ex)
            {
                rc.DebugLog(ex);
            }
        }
        #endregion
    }
}
