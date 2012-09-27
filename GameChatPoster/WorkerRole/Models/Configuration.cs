namespace GameChatPoster
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GameChatPoster.Models;

    public class Configuration
    {
        private const string TABLE_NAME = "GameDayConfig";

        public const string POST_OFFSET = "PostOffset";
        public const string REDDIT_USER = "RedditUser";
        public const string REDDIT_PASSWORD = "RedditPW";
        public const string TIMEZONE_OFFSET = "TimeZoneOffset";
        public const string SUBREDDIT = "Subreddit";

        private Dictionary<string, ConfigEntry> entriesCache;

        public Configuration()
        {
            this.entriesCache = new Dictionary<string, ConfigEntry>();
        }

        public string GetSetting(string setting)
        {
            if (this.entriesCache.ContainsKey(setting))
            {
                return this.entriesCache[setting].Value;
            }

            var tableServiceContext = Helpers.TableStorage.GetTableServiceContext(TABLE_NAME);

            var partitionQuery =
                        (from e in tableServiceContext.CreateQuery<ConfigEntry>(TABLE_NAME)
                         where e.Name.Equals(setting, StringComparison.InvariantCultureIgnoreCase)
                         select e);
            var configEntry =  partitionQuery.First();
            if (configEntry == null)
            {
                return string.Empty;
            }
            else
            {
                this.entriesCache.Add(setting, configEntry);
                return configEntry.Value;
            }
        }

        public void ClearCache()
        {
            this.entriesCache.Clear();
        }

        public void CreateSetting(string name, string value)
        {
            var configEntry = new ConfigEntry(name.GetHashCode(), name, value);
            var tableServiceContext = Helpers.TableStorage.GetTableServiceContext(TABLE_NAME);
            Helpers.TableStorage.AddEntity(configEntry, TABLE_NAME);
        }
    }
}
