namespace GameChatPoster.Models
{
    using System.Collections.Generic;
    using System.Linq;

    class GameDayEntries
    {
        private const string TableName = "GameDayEntries";

        public static List<GameDayEntry> GetAllGetAllGameDayEntries(bool onlyUnposted = false)
        {
            var tableServiceContext = Helpers.TableStorage.GetTableServiceContext(TableName);

            if (onlyUnposted)
            {
                var partitionQuery =
                    (from e in tableServiceContext.CreateQuery<GameDayEntry>(TableName)
                     where e.Posted == "false"
                     select e);
                return partitionQuery.ToList();
            }
            else
            {
                var partitionQuery =
                        (from e in tableServiceContext.CreateQuery<GameDayEntry>(TableName)
                         select e);
                return partitionQuery.ToList();
            }
        }

        public static void UpdateEntry(GameDayEntry entry)
        {
            var tableServiceContext = Helpers.TableStorage.GetTableServiceContext(TableName);

            GameDayEntry entryToUpdate =
                        (from e in tableServiceContext.CreateQuery<GameDayEntry>(TableName)
                         where e.RowKey == entry.RowKey
                         select e).FirstOrDefault();

            entryToUpdate.StartDate = entry.StartDate;
            entryToUpdate.StartTime = entry.StartTime;
            entryToUpdate.Teams = entry.Teams;
            entryToUpdate.Posted = entry.Posted;

            tableServiceContext.UpdateObject(entryToUpdate);
            tableServiceContext.SaveChangesWithRetries();
        }

        
    }
}
