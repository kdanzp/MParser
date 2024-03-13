using System.Collections.Generic;
using System.Text;

namespace MvideoParser.Core
{
    public class ContentBuilder
    {
        private List<string> ListContent;
        private StringBuilder Content;

        public ContentBuilder()
        {
            ListContent = new List<string>();
            Content = new StringBuilder();
        }

        public string Create()
        {
            var content = string.Join(",", ListContent);

            var result = Content
                .Append("[")
                .Append(content)
                .Append("]")
                .ToString();

            ListContent.Clear();

            return result;
        }

        public ContentBuilder AddImg(string urlImg)
        {
            var content = "{\"tag\":\"figure\",\"children\":" +
                "[{\"tag\":\"div\",\"attrs\":" +
                "{\"class\":\"figure_wrapper\"},\"children\":" +
                "[{\"tag\":\"img\",\"attrs\":" +
                    $"{{\"src\":\"{urlImg}\"}}}}]}}," +
            "{\"tag\":\"figcaption\",\"attrs\":" +
                "{\"dir\":\"auto\"},\"children\":[\"\"]}]}";

            ListContent.Add(content);

            return this;
        }

        public ContentBuilder AddText(string text)
        {
            var encodeText = ZennoLab.Macros.TextProcessing.UrlEncode(text);
            var content = "{\"tag\":\"p\",\"attrs\":{\"dir\":\"auto\"},\"children\":" +
                $"[\"{encodeText}\"]}}";

            ListContent.Add(content);

            return this;
        }

        public ContentBuilder AddLink(string link, string name)
        {
            var encodeText = ZennoLab.Macros.TextProcessing.UrlEncode(name);

            var content = "{\"tag\":\"p\",\"attrs\":{\"dir\":\"auto\"},\"children\":[{\"tag\":\"br\"}]}," +
                "{\"tag\":\"p\",\"attrs\":{\"dir\":\"auto\"},\"children\":" +
                    $"[{{\"tag\":\"a\",\"attrs\":{{\"href\":\"{link}\"}}," +
                    $"\"children\":[\"{encodeText}\"]}}]}}";

            ListContent.Add(content);

            return this;
        }
    }
}
