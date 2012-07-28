namespace GameChatPoster.Models
{
    using Microsoft.WindowsAzure.StorageClient;

    public class ConfigEntry : TableServiceEntity
    {
        public string Name { get; set; }
        public string Value { get; set; }

        public ConfigEntry()
        {
            this.PartitionKey = "ConfigEntry";
        }

        public ConfigEntry(int entryId,
            string name,
            string value)
            : base("ConfigEntry", entryId.ToString())
        {
            this.Name = name;
            this.Value = value;
        }

        public override bool Equals(object obj)
        {
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }
    }
}
