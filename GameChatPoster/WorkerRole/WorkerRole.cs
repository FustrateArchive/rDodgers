namespace GameChatPoster
{
    using GameChatPoster.Models;
    using Microsoft.WindowsAzure.ServiceRuntime;
    using Reddit;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Threading;

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
                        if (compare < 1)  // within n hours of the game (n hours read from config)
                        {
                            // POST!
                            Trace.WriteLine("Creating new post: " + game, "Information");
                            var api = new RedditAPI(config.GetSetting(Configuration.REDDIT_USER), config.GetSetting(Configuration.REDDIT_PASSWORD));
                            api.PostSelf("", game.ToString(), config.GetSetting(Configuration.SUBREDDIT));
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

        public void UploadGamesToTable()
        {
            // load csv file into azure
            var count = 0;
            var games = File.ReadAllLines("schedule.csv");
            foreach (var game in games)
            {
                var gameSplit = game.Split(',');
                var gameDayEntry = new GameDayEntry(count, gameSplit[0], gameSplit[1], gameSplit[3], gameSplit[5], (gameSplit.Length == 18) ? "true" : "false");
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
