namespace GameChatPoster
{
    using GameChatPoster.Models;
    using HtmlAgilityPack;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Reddit;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;
    using System.Linq;

    public class WorkerRole : RoleEntryPoint
    {
        public override void Run()
        {
            Trace.WriteLine("$projectname$ entry point called", "Information");
            var config = new Configuration();

            while (true)
            {
                string postTimeOffsetFromConfig = config.GetSetting(Configuration.POST_OFFSET);
                int postTimeOffset = 3;
                if (!int.TryParse(postTimeOffsetFromConfig, out postTimeOffset))
                {
                    Trace.WriteLine("Could not parse postTimeOffsetFromConfig value= " + postTimeOffsetFromConfig);
                }
                Trace.WriteLine(postTimeOffset);

                var gameDayEntries = GameDayEntries.GetAllGetAllGameDayEntries(true);
                foreach (var game in gameDayEntries)
                {
                    var startDateAndTime = DateTime.Parse(game.StartDate + " " + game.StartTime + config.GetSetting(Configuration.TIMEZONE_OFFSET));

                    var hourBeforeGameTime = startDateAndTime.Subtract(new TimeSpan(postTimeOffset, 0, 0));
                    if (hourBeforeGameTime.Date.CompareTo(DateTime.Today.Date) == 0)
                    {
                        int compare = hourBeforeGameTime.CompareTo(DateTime.Now);
                        if (compare < 1)  // within n hours of the game (# hours read from config)
                        {
                            // POST!
                            //Get starting pitchers from ESPN.com
                            string dodgersPitcher = string.Empty;
                            string otherPitcher = string.Empty;
                            this.GetStartingPitchers(game, out dodgersPitcher, out otherPitcher);
                            game.DodgersPitcher = dodgersPitcher;
                            game.OtherPitcher = otherPitcher;

                            Trace.WriteLine("Creating new post: " + game, "Information");
                            var api = new RedditAPI(config.GetSetting(Configuration.REDDIT_USER), config.GetSetting(Configuration.REDDIT_PASSWORD));
                            api.PostSelf(game.GetSelfText(), game.ToString(), config.GetSetting(Configuration.SUBREDDIT));
                            game.Posted = "true";
                            GameDayEntries.UpdateEntry(game);
                            continue;
                        }
                    }
                }
                config.ClearCache();
                Thread.Sleep(10000);
                Trace.WriteLine("Working", "Information");
            }

            //upload game to table in production
            // UploadGamesToTable();
        }

        // Scrapes the dodger schedule on ESPN.com
        private List<EspnGameDay> GetEspnGameEntries()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://espn.go.com/mlb/team/schedule/_/name/lad/los-angeles-dodgers");
            var response = (HttpWebResponse)request.GetResponse();
            var stream = response.GetResponseStream();

            var html = new HtmlDocument();
            html.Load(stream);

            List<EspnGameDay> espnGameDayEntries = new List<EspnGameDay>();

            var oddRows = html.DocumentNode.SelectNodes("//tr[contains(@class,'oddrow')]");
            foreach (var oddRow in oddRows)
            {
                espnGameDayEntries.Add(new EspnGameDay(oddRow));
            }
            var evenRows = html.DocumentNode.SelectNodes("//tr[contains(@class,'evenrow')]");
            foreach (var evenRow in evenRows)
            {
                espnGameDayEntries.Add(new EspnGameDay(evenRow));
            }
            return espnGameDayEntries;
        }

        /// <summary>
        /// Scrapes the dodgers schedule from ESPN.com, if it finds a match, set the pitchers
        /// </summary>
        /// <param name="game">Game entry that will be posted</param>
        /// <param name="dodgersPitcher">Dodgers pitcher string</param>
        /// <param name="otherPitcher">Other team pitcher string</param>
        private void GetStartingPitchers(GameDayEntry game, out string dodgersPitcher, out string otherPitcher)
        {
            dodgersPitcher = string.Empty;
            otherPitcher = string.Empty;

            // Get the schedule from ESPN
            var espnGameDayEntries = this.GetEspnGameEntries();

            var htmlDate = DateTime.Parse(game.StartDate).ToString("ddd, MMM d");
            EspnGameDay espnGame = (from g in espnGameDayEntries
                                    where g.Date.Equals(htmlDate, StringComparison.InvariantCultureIgnoreCase)
                                    select g).FirstOrDefault();
            if (espnGame != null)
            {
                dodgersPitcher = espnGame.DodgersPitcher;
                otherPitcher = espnGame.AwayPitcher;
            }
        }

        public void UploadGamesToTable()
        {
            // load csv file into azure
            var count = 0;
            var games = File.ReadAllLines("schedule.csv");
            foreach (var game in games)
            {
                var gameSplit = game.Split(',');
                var gameDayEntry = new GameDayEntry(count, gameSplit[0], gameSplit[1], gameSplit[3], gameSplit[5], (gameSplit.Length == 18) ? "true" : "false", "", "");
                Trace.WriteLine(gameDayEntry);
                count++;
                var tableServiceContext = Helpers.TableStorage.GetTableServiceContext("GameDayEntries");
                Helpers.TableStorage.AddEntity(gameDayEntry, "GameDayEntries");

            }
        }

        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            //DiagnosticMonitor.Start("DiagnosticsConnectionString"); 

            //DiagnosticMonitorConfiguration dmc = DiagnosticMonitor.GetDefaultInitialConfiguration();
            //dmc.Logs.ScheduledTransferPeriod = TimeSpan.FromHours(1);
            //dmc.Logs.ScheduledTransferLogLevelFilter = LogLevel.Verbose;
            //DiagnosticMonitor.Start("DiagnosticsConnectionString", dmc); 


            return base.OnStart();
        }
    }
}
