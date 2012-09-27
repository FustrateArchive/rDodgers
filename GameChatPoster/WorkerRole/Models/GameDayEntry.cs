namespace GameChatPoster.Models
{
    using Microsoft.WindowsAzure.StorageClient;
    using System;
    using System.Text;

    class GameDayEntry : TableServiceEntity
    {
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string Teams { get; set; }
        public string Broadcast { get; set; }
        public string Posted { get; set; }
        public string DodgersPitcher { get; set; }
        public string OtherPitcher { get; set; }

        public GameDayEntry()
        {
            this.PartitionKey = "GameDayEntry";
        }

        public GameDayEntry(int entryId,
            string startDate,
            string startTime,
            string teams,
            string broadcast,
            string dodgersPitcher,
            string otherPitcher,
            string posted)
            : base("GameDayEntry", entryId.ToString())
        {
            this.StartDate = startDate;
            this.StartTime = startTime;
            this.Teams = teams;
            this.Broadcast = broadcast;
            this.DodgersPitcher = dodgersPitcher;
            this.OtherPitcher = otherPitcher;
            this.Posted = posted;
        }

        public override string ToString()
        {
            var start = DateTime.Parse(StartDate);
            return string.Format("Game chat: {0}/{1} {2} {3}", start.Month, start.Day, Teams, string.Format("{0:t}", StartTime));
        }

        public string SelfText
        {
            get
            {
                var selfText = new StringBuilder();

                //broadcast
                if (!string.IsNullOrEmpty(this.Broadcast))
                {
                    var prepareBroadcastString = this.Broadcast.Replace("-----", "/");
                    prepareBroadcastString = prepareBroadcastString.Replace(" --", ",");
                    prepareBroadcastString = prepareBroadcastString.Trim('"');

                    var split = prepareBroadcastString.Split('/');
                    selfText.AppendLine(split[0]);

                    if (split.Length == 2)
                    {
                        selfText.AppendLine();
                        selfText.AppendLine(split[1]);
                    }
                }

                selfText.AppendLine();
                selfText.AppendLine();

                //pitchers
                if (!string.IsNullOrEmpty(this.DodgersPitcher) && !string.IsNullOrEmpty(this.OtherPitcher))
                {
                    var pitchers = string.Empty;

                    var teams = this.Teams.Split(' ');
                    var dodgerPitcherSplit = this.DodgersPitcher.Split(' ');
                    var otherPitcherSplit = this.OtherPitcher.Split(' ');
                    if (teams[0].Equals("Dodgers"))
                    {
                        pitchers = string.Format("**{0}** {1} vs **{2}** {3}", dodgerPitcherSplit[0], dodgerPitcherSplit[1], otherPitcherSplit[0], otherPitcherSplit[1]);
                    }
                    else
                    {
                        pitchers = string.Format("**{0}** {1} vs **{2}** {3}", otherPitcherSplit[0], otherPitcherSplit[1], dodgerPitcherSplit[0], dodgerPitcherSplit[1]);
                    }
                    selfText.AppendLine(pitchers);
                }

                return selfText.ToString();
            }
        }
    }
}
