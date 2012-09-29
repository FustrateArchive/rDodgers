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

        /// <summary>
        /// Formats the self test - currently contains todays broadcast and starting pitchers
        /// </summary>
        /// <returns>Mark down formatted self text</returns>
        public string GetSelfText()
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
            var dodgersPitcherName = string.Empty;
            var dodgerPitcherRecord = string.Empty;

            var otherPitcherName = string.Empty;
            var otherPitcherRecord = string.Empty;

            // try to get the pitcher name and records from the entry.  if there is a problem, drop the pitchers altogether
            if (this.TryGetPitcherNameAndRecord(this.DodgersPitcher, out dodgersPitcherName, out dodgerPitcherRecord) &&
                this.TryGetPitcherNameAndRecord(this.OtherPitcher, out otherPitcherName, out otherPitcherRecord))
            {
                var pitchers = string.Empty;
                var teams = this.Teams.Split(' ');
                if (teams[0].Equals("Dodgers"))
                {
                    pitchers = string.Format("**{0}** {1} vs **{2}** {3}", dodgersPitcherName, dodgerPitcherRecord, otherPitcherName, otherPitcherRecord);
                }
                else
                {
                    pitchers = string.Format("**{0}** {1} vs **{2}** {3}", otherPitcherName, otherPitcherRecord, dodgersPitcherName, dodgerPitcherRecord);
                }
                selfText.AppendLine(pitchers);
            }
            return selfText.ToString();
        }

        /// <summary>
        /// Separates the pitcher from their record
        /// </summary>
        /// <param name="pitcherEntry">Pitcher entry</param>
        /// <param name="pitcherName">Pitcher name</param>
        /// <param name="pitcherRecord">Pitcher record</param>
        /// <returns></returns>
        private bool TryGetPitcherNameAndRecord(string pitcherEntry, out string pitcherName, out string pitcherRecord)
        {
            pitcherName = string.Empty;
            pitcherRecord = string.Empty;

            if (string.IsNullOrEmpty(pitcherEntry))
            {
                return false;
            }

            var pitcherEntrySplit = pitcherEntry.Split(' ');

            pitcherName = pitcherEntrySplit[0];
            // "de la rosa" 
            for (int i = 1; i < pitcherEntrySplit.Length - 1; i++)
            {
                pitcherName += " " + pitcherEntrySplit[i];
            }

            if (pitcherEntrySplit.Length > 1)
            {
                pitcherRecord = pitcherEntrySplit[pitcherEntrySplit.Length - 1];
                if (!pitcherRecord.StartsWith("("))
                {
                    return false;
                }
            }

            return true;
        }
    }

    
}
