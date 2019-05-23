﻿using RedditSharp;
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
            rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
            List<string> loginCheck = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
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
                    //If login check succeeds, provide login credentials for Listen() method.
                    List<string> credentials = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcLogin.txt");
                    string user = rc.DecodePassword(credentials[0]);
                    string password = rc.DecodePassword(credentials[1]);
                    p.Listen(user, password);

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
        private void Listen(string user, string password)
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

                    //subreddit needs to be acquired from locally saved text file rcSubreddit.txt
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt");
                    string sub = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSubreddit.txt").First();
                    if (sub.Count() < 4)
                        System.Environment.Exit(0);

                    //List should also be acquired from a locally saved text file, and will consist of
                    //all previous entries the user has already been notified of. 
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt");
                    List<string> lstDuplicateList = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcSearchRecords.txt");

                    //Double checks that rcEmail.txt exists and is not null
                    rc.CheckFileExists(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt");
                    if (rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcEmail.txt").Count < 2)
                    {
                        Exception ex = new Exception("You must ensure rcEmail.txt has a valid email address & password");
                        rc.DebugLog(ex);
                        System.Environment.Exit(0);
                    }
                    #endregion

                    //gets list of 15 most recent posts from designated sub.
                    List<string> lstResultList = rcon.GetPosts(user, password, sub);
                    List<string> lstPassedList = rc.NotificationList(lstResultList, lstSearchInput, lstDuplicateList);

                    //Retrieve status on toast notifications from file. 
                    bool toastStatus = false;
                    List<string> toastList = rc.ReadFile(Directory.GetCurrentDirectory().ToString() + "/rcToast.txt");
                    if (toastList.Count == 1)
                    {
                        if (toastList[0] == "yes")
                            toastStatus = true;
                    }

                    //Now that the list has been trimmed, notify user of all results
                    if (lstPassedList.Count > 1)
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
                        }
                    }

                    //Clear all lists, sleep, and repeat
                    lstResultList.Clear();
                    lstPassedList.Clear();
                    lstDuplicateList.Clear();
                    lstSearchInput.Clear();
                    Thread.Sleep(60000);
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

        class ToastEvents
        {
            internal void ToastActivated(ToastNotification sender, object e)
            {
                Console.WriteLine("User activated the toast");
            }

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

            internal void ToastFailed(ToastNotification sender, ToastFailedEventArgs e)
            {
                Console.WriteLine("The toast encountered an error.");
            }
        }

        static class ShortCutCreator
        {
            // In order to display toasts, a desktop application must have
            // a shortcut on the Start menu.
            // Also, an AppUserModelID must be set on that shortcut.
            // The shortcut should be created as part of the installer.
            // The following code shows how to create
            // a shortcut and assign an AppUserModelID using Windows APIs.
            // You must download and include the Windows API Code Pack
            // for Microsoft .NET Framework for this code to function

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
