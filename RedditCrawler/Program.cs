using RedditSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static RedditSharp.Things.VotableThing;
using System.Security;
using static System.Net.Mime.MediaTypeNames;
using System.Net.Mail;
using System.IO;
using System.Web;
using rcListenLibrary;
using Windows.UI.Notifications;
using System.Xml;
using System.Diagnostics;
using MS.WindowsAPICodePack.Internal;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System.Runtime.InteropServices.ComTypes;
using ConsoleToast;

namespace RedditCrawler
{
    class Program
    {
        static string txtFilePath = "/rcData.txt";
        static string appID = "FightingMongooses.RedditCrawler";

        static void Main(string[] args)
        {
            //Initialize helper methods. 
            rcHelper rc = new rcHelper();
            Program p = new Program();

            //Create shortcut for application for toast notification
            try
            {
                ShortCutCreator.TryCreateShortcut(appID, "RedditCrawler");
            }
            catch (Exception ex)
            {
                rc.DebugLog(ex);
            }

            //Validates login, logs failures. 
            rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + txtFilePath);
            List<string> loginCheck = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + txtFilePath);
            if (loginCheck.Count() == 0 || loginCheck == null)
            {
                Exception e = new Exception("Login failed. Please configure your settings in rcConfig.");
                rc.DebugLog(e);
                System.Environment.Exit(0);
            }
            else
            {
                try
                {
                    string user = null;
                    string password = null;
                    //If login check succeeds, provide login credentials for Listen() method.
                    List<string> credentials = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + txtFilePath);
                    for(int i = 0; i < credentials.Count; i++)
                    {
                        if (credentials[i] == "LOGIN")
                        {
                            user = rc.DecodePassword(credentials[i + 1]);
                            password = rc.DecodePassword(credentials[i + 2]);
                        }
                    }
                    if (user != null && password != null)
                        p.Listen(user, password).Wait();
                    else
                    {
                        rc.DebugLog(new Exception("Please ensure a valid username and password are configured"));
                        Environment.Exit(0);
                    }
                }
                catch (Exception ex)
                {
                    rc.DebugLog(ex);
                }
            }

        }

        /// <summary>
        /// Takes user reddit user name and password as input, confirms existence of all relevant text files (rcEmail.txt, rcLogin.txt, 
        /// rcSearchCriteria.txt, and rcSubreddit.txt) and their contents, then retrieves 15 posts using <see cref="GetPosts(string, string, string)"/>
        /// Once these posts have been retrieved, the method checks them against duplicates from rcSearchRecords.txt and matches from rcSearchCriteria.txt,
        /// then uses <see cref="NotifyUser(string)"/> to notify the user of any matches. After this, all variables are cleared, the thread sleeps for 
        /// 60,000 ticks, and runs again. 
        /// </summary>
        /// <param name="user">Reddit username</param>
        /// <param name="password">Reddit password</param>
        private async Task Listen(string user, string password)
        {
            rcHelper rc = new rcHelper();
            rcConnectivity rcon = new rcConnectivity();

            while (true)
            {
                try
                {
                    //Checks for valid entries in relevant text files, performs DebugLog() and exits environment on failure. 
                    #region criteriaCheck

                    //searchInput needs to be acquired from locally saved text file rcSearchCriteria.txt after being input, and will be used to filter the results
                    //If list is blank, user is notified to enter criteria from the main menu.
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                    List<string> lstSearchInput = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchCriteria.txt");
                    if (lstSearchInput.Count < 1)
                    {
                        Exception ex = new Exception("You must ensure rcSearchCriteria.txt has search terms.");
                        rc.DebugLog(ex);
                        System.Environment.Exit(0);
                    }

                    //Retrieve status on toast notifications from file. 
                    bool toastStatus = false;
                    string sub = null;
                    bool subEx, emailEx;
                    subEx = emailEx = false;
                    
                    List<string> dataList = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + txtFilePath);
                    if (dataList.Count > 0)
                    {
                        for (int i = 0; i < dataList.Count; i++)
                        {
                            if (dataList[i] == "TOAST")
                            {
                                if (dataList[i + 1] == "yes")
                                    toastStatus = true;
                            }
                            if (dataList[i] == "SUBREDDIT")
                            {
                                sub = dataList[i + 1];
                                subEx = true;
                            }
                            if (dataList[i] == "EMAIL")
                                emailEx = true;
                        }
                    }
                    if (subEx == false)
                    {
                        rc.DebugLog(new Exception("Please check your subreddit existence & formatting."));
                        Environment.Exit(0);
                    }
                    if (emailEx == false)
                    {
                        rc.DebugLog(new Exception("Please check your email credentials existence & formatting."));
                        Environment.Exit(0);
                    }
                    
                    //List should also be acquired from a locally saved text file, and will consist of
                    //all previous entries the user has already been notified of. 
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt");
                    List<string> lstDuplicateList = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt");             
                    #endregion

                    //gets list of 15 most recent posts from designated sub.
                    List<string> lstResultList = rcon.GetPosts(user, password, sub);
                    List<string> lstPassedList = rc.NotificationList(lstResultList, lstSearchInput, lstDuplicateList);                   

                    //Now that the list has been trimmed, notify user of all results
                    if (lstPassedList.Count > 0)
                    {
                        rc.WriteToFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt", lstPassedList, true);
                        for (int i = 0; i < lstPassedList.Count(); i++)
                        {
                            Console.WriteLine("Sent: " + lstPassedList[i]);
                            rcon.NotifyUser(lstPassedList[i]);
                            if (toastStatus == true)
                            {
                                ShowTextToast(appID, "New Reddit Post!", lstPassedList[i]);
                            }
                            await (Task.Delay(5000));
                        }
                    }

                    //Clear all lists, sleep, and repeat
                    lstResultList.Clear();
                    lstPassedList.Clear();
                    lstDuplicateList.Clear();
                    lstSearchInput.Clear();
                    await Task.Delay(180000);
                }
                catch (Exception ex)
                {
                    rc.DebugLog(ex);
                    System.Environment.Exit(0);
                }
            }
        }

        //Methods for displaying toasts, setting up shortcut, and toastevents. 
        #region ToastHandlingMethods
        /// <summary>
        /// Displays a text-based Toast Message
        /// </summary>
        /// <param name="appId">string-based application ID for handling Toast messages</param>
        /// <param name="title">Title of the toast message to be displayed</param>
        /// <param name="message">Message for the toast to contain</param>
        static void ShowTextToast(string appId, string title, string message)
        {
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastText02);

            // Fill in the text elements
            Windows.Data.Xml.Dom.XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(message));

            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(toastXml);

            ToastEvents events = new ToastEvents();

            toast.Activated += events.ToastActivated;
            toast.Dismissed += events.ToastDismissed;
            toast.Failed += events.ToastFailed;

            // Show the toast. Be sure to specify the AppUserModelId
            // on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(appId).Show(toast);
        }

        /// <summary>
        /// Displays an image-based Toast Message
        /// </summary>
        /// <param name="appId">string-based application ID for handling Toast messages</param>
        /// <param name="title">Title of the toast message to be displayed</param>
        /// <param name="message">Message for the toast to contain</param>
        /// <param name="image">Image to display in the toast message</param>
        static void ShowImageToast(string appId, string title, string message, string image)
        {
            Windows.Data.Xml.Dom.XmlDocument toastXml = ToastNotificationManager.GetTemplateContent(
                ToastTemplateType.ToastImageAndText02);

            // Fill in the text elements
            Windows.Data.Xml.Dom.XmlNodeList stringElements = toastXml.GetElementsByTagName("text");
            stringElements[0].AppendChild(toastXml.CreateTextNode(title));
            stringElements[1].AppendChild(toastXml.CreateTextNode(message));

            // Specify the absolute path to an image
            String imagePath = "file:///" + image;
            Windows.Data.Xml.Dom.XmlNodeList imageElements = toastXml.GetElementsByTagName("image");
            imageElements[0].Attributes.GetNamedItem("src").NodeValue = imagePath;

            // Create the toast and attach event listeners
            ToastNotification toast = new ToastNotification(toastXml);

            ToastEvents events = new ToastEvents();

            toast.Activated += events.ToastActivated;
            toast.Dismissed += events.ToastDismissed;
            toast.Failed += events.ToastFailed;

            // Show the toast. Be sure to specify the AppUserModelId
            // on your application's shortcut!
            ToastNotificationManager.CreateToastNotifier(appId).Show(toast);
        }

        /// <summary>
        /// Local class for handling toast activation, dismissal, & failure
        /// </summary>
        class ToastEvents
        {
            /// <summary>
            /// Logs the activation of the toast to the ToastNotification object.
            /// </summary>
            internal void ToastActivated(ToastNotification sender, object e)
            {
                Console.WriteLine("User activated the toast");
            }

            /// <summary>
            /// Logs the dismissal & reason for dismissal of the toast to the ToastNotification object.
            /// </summary>
            internal void ToastDismissed(ToastNotification sender, ToastDismissedEventArgs e)
            {
                String outputText = "";
                switch (e.Reason)
                {
                    case ToastDismissalReason.ApplicationHidden:
                        outputText = "The app hid the toast using ToastNotifier.Hide";
                        break;
                    case ToastDismissalReason.UserCanceled:
                        outputText = "The user dismissed the toast";
                        break;
                    case ToastDismissalReason.TimedOut:
                        outputText = "The toast has timed out";
                        break;
                }

                Console.WriteLine(outputText);
            }
            
            /// <summary>
            /// Logs the encounter of an error to the ToastNotification object.
            /// </summary>
            internal void ToastFailed(ToastNotification sender, ToastFailedEventArgs e)
            {
                Console.WriteLine("The toast encountered an error.");
            }
        }

        /// <summary>
        /// Class for handling the creation of a windows shortcut in order to enable toast notifications. 
        /// </summary>
        static class ShortCutCreator
        {
            // In order to display toasts, a desktop application must have a shortcut on the Start menu.
            // Also, an AppUserModelID must be set on that shortcut (static string appID).
            // The shortcut should be created as part of the installer.
            // The following code shows how to create
            // a shortcut and assign an AppUserModelID using Windows APIs.
            // You must download and include the Windows API Code Pack
            // for Microsoft .NET Framework for this code to function

            /// <summary>
            /// Creates the shortcut within the specified folder path. 
            /// </summary>
            /// <param name="appId">Identifier for the application</param>
            /// <param name="appName">Name of the application</param>
            /// <returns><c>true</c> if the shortcut was successfully created & installed, otherwise <c>false</c></returns>
            internal static bool TryCreateShortcut(string appId, string appName)
            {
                String shortcutPath = Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData) +
                    "\\Microsoft\\Windows\\Start Menu\\Programs\\" + appName + ".lnk";
                if (!File.Exists(shortcutPath))
                {
                    InstallShortcut(appId, shortcutPath);
                    return true;
                }
                return false;
            }

            /// <summary>
            /// Installs the shortcut in the provided folder path. 
            /// </summary>
            /// <param name="appId">Identifier for the application</param>
            /// <param name="shortcutPath">Folder path for installation</param>
            static void InstallShortcut(string appId, string shortcutPath)
            {
                // Find the path to the current executable
                String exePath = Process.GetCurrentProcess().MainModule.FileName;
                IShellLinkW newShortcut = (IShellLinkW)new CShellLink();

                // Create a shortcut to the exe
                VerifySucceeded(newShortcut.SetPath(exePath));
                VerifySucceeded(newShortcut.SetArguments(""));

                // Open the shortcut property store, set the AppUserModelId property
                IPropertyStore newShortcutProperties = (IPropertyStore)newShortcut;

                using (PropVariant applicationId = new PropVariant(appId))
                {
                    VerifySucceeded(newShortcutProperties.SetValue(
                        SystemProperties.System.AppUserModel.ID, applicationId));
                    VerifySucceeded(newShortcutProperties.Commit());
                }

                // Commit the shortcut to disk
                ConsoleToast.IPersistFile newShortcutSave = (ConsoleToast.IPersistFile)newShortcut;

                VerifySucceeded(newShortcutSave.Save(shortcutPath, true));
            }

            /// <summary>
            /// Internal method for verifying the success or failure of shortcut installation
            /// </summary>
            /// <param name="hresult"></param>
            static void VerifySucceeded(UInt32 hresult)
            {
                if (hresult <= 1)
                    return;

                throw new Exception("Failed with HRESULT: " + hresult.ToString("X"));
            }
        }
        #endregion
    }
}
