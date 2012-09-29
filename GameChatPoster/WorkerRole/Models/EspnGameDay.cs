namespace GameChatPoster.Models
{
    using HtmlAgilityPack;

    public class EspnGameDay
    {
        private HtmlNode rootNode;

        public EspnGameDay(HtmlNode node)
        {
            rootNode = node;
        }

        public string Date
        {
            get
            {
                return this.GetElementInnerText("td/nobr");
            }
        }

        public string HomeTeam
        {
            get
            {
                return string.Empty;
            }
        }

        public string AwayTeam
        {
            get
            {
                return string.Empty;
            }
        }

        public string DodgersPitcher
        {
            get
            {
                return this.GetElementInnerText("td[5]");
            }
        }

        public string AwayPitcher
        {
            get
            {
                return this.GetElementInnerText("td[6]");
            }
        }

        private string GetElementInnerText(string xpath)
        {
            try
            {
                var element = rootNode.SelectSingleNode(xpath);
                if (element != null)
                {
                    return element.InnerText;
                }
            }
            catch
            {
                return string.Empty;
            }
            return string.Empty;
        }
    }
}
