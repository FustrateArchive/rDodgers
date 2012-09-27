namespace GameChatPoster.Models
{
    using Microsoft.WindowsAzure.StorageClient;
    using System;
    
    class GameDayEntry : TableServiceEntity
    {
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string Teams { get; set; }
        public string Broadcast { get; set; }
        public string Posted { get; set; }

        public GameDayEntry()
        {
            this.PartitionKey = "GameDayEntry";
        }

        public GameDayEntry(int entryId,
            string startDate,
            string startTime,
            string teams,
            string broadcast,
            string posted)
            : base("GameDayEntry", entryId.ToString())
        {
            this.StartDate = startDate;
            this.StartTime = startTime;
            this.Teams = teams;
            this.Broadcast = broadcast;
            this.Posted = posted;
        }

        public override string ToString()
        {
            var start = DateTime.Parse(StartDate);
            return string.Format("Game chat: {0}/{1} {2} {3}", start.Month, start.Day, Teams, string.Format("{0:t}", StartTime));
        }
    }
}
