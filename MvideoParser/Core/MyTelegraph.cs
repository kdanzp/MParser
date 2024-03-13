using Global.ZennoLab.Json.Linq;
using System;
using System.Net.Http;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace MvideoParser.Core
{
    public class MyTelegraph
    {
        public ContentBuilder ContentBuilder;
        public string ShortName { get; private set; }
        public string AccessToken { get; private set; }

        readonly IZennoPosterProjectModel _p;
        readonly ZRequest _r;

        public MyTelegraph(IZennoPosterProjectModel project)
        {
            _p = project;
            _r = new ZRequest(project);
            ContentBuilder = new ContentBuilder();
        }

        /// <summary>
        /// Создаем Аккаунт Telegraf.
        /// </summary>
        /// <param name="shortName">Короткое имя</param>
        /// <param name="authorName">Автор</param>
        /// <param name="authorUrl">Ссылка на автора</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public bool CreateAccount(string shortName, string authorName = "", string authorUrl = "")
        {
            string url = "https://api.telegra.ph/createAccount";
            var content = $"short_name={Uri.EscapeDataString(shortName)}" +
                     $"&author_name={Uri.EscapeDataString(authorName)}" +
                     $"&author_url={Uri.EscapeDataString(authorUrl)}";

            var resp = _r.Post(url, content);

            if (!resp.Contains("ok\":true"))
            {
                throw new Exception("Ошибка CreateAccount:" + resp);
            }

            var json = JObject.Parse(resp);

            ShortName = (string)json["result"]["short_name"];
            AccessToken = (string)json["result"]["access_token"];

            return true;

        }

        /// <summary>
        /// Устанавливаем свой AccessToken.
        /// </summary>
        /// <param name="accessToken"></param>
        public void SetAccessToken(string accessToken) => AccessToken = accessToken;

        /// <summary>
        /// Создаем страницу
        /// </summary>
        /// <param name="title">Заголовок</param>
        /// <param name="content">Контент (массив Node)</param>
        /// <param name="authorName">Автор</param>
        /// <param name="authorUrl">Ссылка на автора</param>
        /// <param name="isReturnContent">Возвращенать поле контента в объекте страницы</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string CreatePage(string title, string content, string authorName = "", string authorUrl = "", bool isReturnContent = false)
        {
            if (string.IsNullOrEmpty(AccessToken))
            {
                throw new Exception("Пустой AccessToken");
            }

            var url = $"https://api.telegra.ph/createPage?";

            var reqContent = $"access_token={AccessToken}" +
                    $"&title={title}" +
                    $"&content={content}";

            var resp = _r.Post(url, reqContent);

            if (!resp.Contains("ok\":true"))
            {
                throw new Exception("Ошибка CreatePage:" + resp);
            }

            return JObject.Parse(resp)["result"]
                .SelectToken("url")
                .ToString();
        }

        /// <summary>
        /// Загружаем картинку.
        /// </summary>
        /// <param name="pathImg"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string UploadImg(string pathImg)
        {
            var requesImg = new ZRequest(_p) { ContentPostingType = "multipart/form-data" };
            var url = "https://telegra.ph/upload";

            var rnd = new Random().Next(100000, 999999);
            var boundary = $"-----------------------------{rnd}{rnd}";

            var content = $"{boundary}\r\n" +
                $"Content-Disposition: form-data; name=\"file\"; filename=\"blob\"\r\n" +
                $"Content-Type: image/jpeg\r\n" +
                "\r\n" +
                $"{pathImg}\r\n" +
                $"{boundary}--";

            var resp = requesImg.Post(url, content);

            //Результат
            if (!resp.Contains("src"))
                throw new Exception($"Не удалось загрузить картинку!\n{resp}");

            var result = JArray.Parse(resp)[0].SelectToken("src").ToString();

            return "https://telegra.ph" + result;
        }

        /// <summary>
        /// Загружаем картинку byteArr.
        /// </summary>
        /// <param name="pathImg"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public string UploadImg(byte[] byteArr)
        {
            var url = "https://telegra.ph/upload";

            var rnd = ZennoLab.Macros.TextProcessing.RandomText(16, "d");
            var multipart = new MultipartFormDataContent($"boundary=----WebKitFormBoundary{rnd}");
            var bytaArrayContent = new ByteArrayContent(byteArr, 0, byteArr.Length);
            bytaArrayContent.Headers.ContentDisposition = new System.Net.Http.Headers.ContentDispositionHeaderValue("form-data")
            {
                Name = "file",
                FileName = "image",
            };
            multipart.Add(bytaArrayContent, "image");

            byte[] bContent = multipart.ReadAsByteArrayAsync().Result;
            var contenttype = $"multipart/form-data;boundary=----WebKitFormBoundary{rnd}";
            var requesImg = new ZRequest(_p) { ContentPostingType = contenttype };

            var resp = requesImg.Post(url, bContent);

            //Результат
            if (!resp.Contains("src"))
                throw new Exception($"Не удалось загрузить картинку!\n{resp}");

            var result = JArray.Parse(resp)[0].SelectToken("src").ToString();

            return "https://telegra.ph" + result;
        }

    }
}
