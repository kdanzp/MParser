using ZennoLab.CommandCenter;
using ZennoLab.InterfacesLibrary.Enums.Http;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace MvideoParser.Core
{
    public class ZRequest
    {
        public int MaxRedirectCount { get; set; }
        public int Timeout { get; set; }
        public bool UseRedirect { get; set; }
        public bool UseOriginalUrl { get; set; }
        public bool ThrowExceptionOnError { get; set; }
        public bool RemoveDefaultHeaders { get; set; }
        public string ContentPostingType { get; set; }
        public string Proxy { get; set; }
        public string Encoding { get; set; }
        public string Cookies { get; set; }
        public string UserAgent { get; set; }
        public string DownloadPath { get; set; }
        public ResponceType ResponseType { get; set; }
        public ICookieContainer CookieContainer { get; set; }


        public ZRequest(IZennoPosterProjectModel project)
        {
            Proxy = project.GetProxy();
            UserAgent = project.Profile.UserAgent;
            ResponseType = ResponceType.BodyOnly;
            ContentPostingType = "application/x-www-form-urlencoded";
            Encoding = "UTF-8";
            Timeout = 30000;
            UseRedirect = true;
            MaxRedirectCount = 3;
            UseOriginalUrl = false;
            ThrowExceptionOnError = true;
            CookieContainer = project.Profile.CookieContainer;
            RemoveDefaultHeaders = false;
        }

        /// <summary>
        /// Get запрос.
        /// </summary>
        /// <param name="url">Url запроса</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns></returns>
        public string Get(string url, string[] headers = null) => Request(HttpMethod.GET, url, "", headers);

        /// <summary>
        /// Get запрос, с возвратом byteArr.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public byte[] GetBytes(string url, string[] headers = null) => RequestBytes(HttpMethod.GET, url, null, headers);

        /// <summary>
        /// Post запрос.
        /// </summary>
        /// <param name="url">Url запроса</param>
        /// <param name="content">Тело запроса</param>
        /// <param name="headers">Заголовки запроса</param>
        /// <returns></returns>
        public string Post(string url, string content, string[] headers = null) => Request(HttpMethod.POST, url, content, headers);

        /// <summary>
        /// Post запрос.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="content"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        public string Post(string url, byte[] content, string[] headers = null) => Request(HttpMethod.POST, url, content, headers);

        private string Request(HttpMethod httpMethod, string url, string content, string[] headers = null)
        {
            return ZennoPoster.HTTP.Request(
                method: httpMethod,
                url: url,
                content: content,
                AdditionalHeaders: headers,
                contentPostingType: ContentPostingType,
                proxy: Proxy,
                Encoding: Encoding,
                respType: ResponseType,
                Timeout: Timeout,
                Cookies: Cookies,
                UserAgent: UserAgent,
                UseRedirect: UseRedirect,
                MaxRedirectCount: MaxRedirectCount,
                DownloadPath: DownloadPath,
                UseOriginalUrl: UseOriginalUrl,
                throwExceptionOnError: ThrowExceptionOnError,
                cookieContainer: CookieContainer,
                removeDefaultHeaders: RemoveDefaultHeaders);
        }
        private string Request(HttpMethod httpMethod, string url, byte[] content, string[] headers = null)
        {
            return ZennoPoster.HTTP.Request(
                method: httpMethod,
                url: url,
                content: content,
                AdditionalHeaders: headers,
                contentPostingType: ContentPostingType,
                proxy: Proxy,
                Encoding: Encoding,
                respType: ResponseType,
                Timeout: Timeout,
                Cookies: Cookies,
                UserAgent: UserAgent,
                UseRedirect: UseRedirect,
                MaxRedirectCount: MaxRedirectCount,
                DownloadPath: DownloadPath,
                UseOriginalUrl: UseOriginalUrl,
                throwExceptionOnError: ThrowExceptionOnError,
                cookieContainer: CookieContainer,
                removeDefaultHeaders: RemoveDefaultHeaders);
        }

        private byte[] RequestBytes(HttpMethod httpMethod, string url, byte[] content, string[] headers = null)
        {
            return ZennoPoster.HTTP.RequestBytes(
                method: httpMethod,
                url: url,
                content: content,
                AdditionalHeaders: headers,
                contentPostingType: ContentPostingType,
                proxy: Proxy,
                Encoding: Encoding,
                respType: ResponseType,
                Timeout: Timeout,
                Cookies: Cookies,
                UserAgent: UserAgent,
                UseRedirect: UseRedirect,
                MaxRedirectCount: MaxRedirectCount,
                DownloadPath: DownloadPath,
                UseOriginalUrl: UseOriginalUrl,
                throwExceptionOnError: ThrowExceptionOnError,
                cookieContainer: CookieContainer,
                removeDefaultHeaders: RemoveDefaultHeaders);
        }
    }
}
