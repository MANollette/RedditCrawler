# Reddit Crawler
Final BAS capstone project.

## Concept
This capstone project is being initially designed as a web crawler geared towards Reddit. Its purpose is to take text-based user input, and monitor subs of your choice for posts that match or contain that input. It will then notify you via email and local toast messages. As of 06/07/2019, version 1.0 of the application has been completed, and a release is pending. In its current state, I would not recommend using it unless you are confident in your device's security. The application currently consists of a WPF GUI for configuration of the details on what you'd like to monitor, and a console application to run on your device which listens for new posts. 

To use the application, simply start by running rcConfig, and configure each of your settings in turn, entering your credentials, desired subreddit, and search terms, as well as whether you would like to receive toast notifications or not. Once all of the necessary configurations have been made, the application will notify you that you are ready to run the service. Then, just run RedditCrawler and you're set!
