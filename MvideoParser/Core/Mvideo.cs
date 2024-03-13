using Global.ZennoLab.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using ZennoLab.InterfacesLibrary.ProjectModel;

namespace MvideoParser.Core
{
    public class Mvideo
    {
        public List<DataProduct> LstDataProduct { get; set; }

        private readonly ZRequest _r;
        private readonly IZennoPosterProjectModel _p;

        public Mvideo(IZennoPosterProjectModel project)
        {
            _p = project;
            _r = new ZRequest(project);
            _r.Get("https://www.mvideo.ru/");
            LstDataProduct = new List<DataProduct>();
        }

        /// <summary>
        /// Получаем данные Товары Дня.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<DataProduct> GoodsOfDay()
        {
            var url = "https://www.mvideo.ru/bff/settings/shelf-product-sets?tags=goodofday&tags=goodofday2&type=daily";

            var lstId = GetListId(url, "Товары Дня");

            if (lstId.Count == 0)
                throw new Exception($"Не удалось получить ID \"Товары Дня\"!");

            return LstDataProduct;
        }

        /// <summary>
        /// Получаем данные Новые Товары.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<DataProduct> NewGoods()
        {
            var url = "https://www.mvideo.ru/bff/settings/shelf-product-sets?tags=newgoods";

            var lstId = GetListId(url, "Новые Товары");

            if (lstId.Count == 0)
                throw new Exception($"Не удалось получить ID \"Новые Товары\"!");

            return LstDataProduct;
        }

        /// <summary>
        /// Получаем данные Больше просмотров.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public List<DataProduct> MostViewed()
        {
            var url = "https://www.mvideo.ru/bff/settings/shelf-product-sets?tags=mostviewed";

            var lstId = GetListId(url, "Больше просмотров");

            if (lstId.Count == 0)
                throw new Exception($"Не удалось получить ID \"Больше Просмотров\"!");

            return LstDataProduct;
        }

        /// <summary>
        /// Возвращает список ID товаров.
        /// </summary>
        /// <param name="products"></param>
        /// <returns></returns>
        private List<DataProduct> GetListId(string url, string name)
        {
            try
            {
                //Собираем ID.
                var resp = _r.Get(url);

                var json = JObject.Parse(resp);
                JArray products = (JArray)json["body"]["items"][0]["products"];

                foreach (var product in products)
                {
                    var data = new DataProduct();
                    data.Name = name;
                    data.Id = product.Value<string>();

                    LstDataProduct.Add(data);
                }

                //Получаем информацию о товаре.
                GetProductDetails();
                GetProductPrices();
                ClearBadDataProduct();

                //Рисуем новые карточки.
                for (int i = 0; i < LstDataProduct.Count; i++)
                {
                    var data = LstDataProduct[i];
                    data.ProductCardByteArr = DrawingСard(data);
                }

                return LstDataProduct;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Получаем информацию о товарах.
        /// </summary>
        /// <returns></returns>
        private void GetProductDetails()
        {
            var url = "https://www.mvideo.ru/bff/product-details/list";
            var ids = string.Join(",", LstDataProduct.Select(s => $"\"{s.Id}\""));
            var content = $"{{\"productIds\":[{ids}],\"mediaTypes\":[\"images\"],\"status\":true," +
                "\"category\":true,\"categories\":true,\"brand\":true,\"propertyTypes\":[\"KEY\"]}";

            _r.ContentPostingType = "application/json";
            var resp = _r.Post(url, content, new[] { "Referer: https://www.mvideo.ru/", "Connection: keep-alive" });

            if (!resp.Contains("success\":true"))
                throw new Exception($"Не удалось получить Информацию о продукте.\n{resp}");

            var products = JObject.Parse(resp)["body"]["products"];

            //for (int i = 0; i < products.Count(); i++)
            //{
            //    var data = LstDataProduct[i];

            //    data.ProductName = products[i].SelectToken("name").ToString();
            //    data.Url = "https://www.mvideo.ru/products/" + products[i].SelectToken("nameTranslit") + $"-{data.Id}";
            //    data.ImgUrl = "http://static.mvideo.ru/" + products[i].SelectToken("image");
            //}

            foreach (var product in products)
            {
                var productIdString = product["productId"].Value<string>();
                int productId;
                if (int.TryParse(productIdString, out productId))
                {
                    var data = LstDataProduct.FirstOrDefault(d => d.Id == productId.ToString());
                    if (data != null)
                    {
                        data.ProductName = product["name"].ToString();
                        data.Url = "https://www.mvideo.ru/products/" + product["nameTranslit"] + $"-{productId}";
                        data.ImgUrl = "http://static.mvideo.ru/" + product["image"];
                    }
                }
            }
        }

        /// <summary>
        /// Получаем информацию о ценах и скидках.
        /// </summary>
        /// <param name="lstDataProduct"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private void GetProductPrices()
        {
            var ids = string.Join(",", LstDataProduct.Select(s => s.Id));
            var url = $"https://www.mvideo.ru/bff/products/prices?productIds={ids}&isPromoApplied=true&addBonusRubles=true";

            _r.ContentPostingType = "application/json";
            var resp = _r.Get(url);

            if (!resp.Contains("success\":true"))
                throw new Exception($"Не удалось получить Информацию о Ценах.\n{resp}");

            var products = JObject.Parse(resp)["body"]["materialPrices"];

            foreach (var product in products)
            {
                var productId = product["productId"].Value<int>();
                var data = LstDataProduct.FirstOrDefault(d => d.Id == productId.ToString());

                if (data != null)
                {
                    data.BasePrice = product["price"]["basePrice"].Value<decimal>();
                    data.SalePrice = product["price"]["salePrice"].Value<decimal>();
                    data.BonusRubles = product["bonusRubles"]["total"].Value<decimal>();
                    data.Discount = data.BasePrice - data.SalePrice;
                    data.Percent = Math.Round(100 - (data.SalePrice / data.BasePrice * 100), 0);
                }
            }
        }

        /// <summary>
        /// Рисуем новую карточку
        /// </summary>
        /// <param name="dataProduct"></param>
        private byte[] DrawingСard(DataProduct dataProduct)
        {
            // Загрузка изображения из массива байт
            var byteArr = _r.GetBytes(dataProduct.ImgUrl);

            Image imgProduct;
            using (MemoryStream ms = new MemoryStream(byteArr))
            {
                imgProduct = Image.FromStream(ms);
            }

            Image imgSample = Image.FromFile(_p.Directory + @"\Sample.png");

            // Создание нового изображения для рисования
            using (Image bmp = new Bitmap(550, 550))
            {
                // Создание объекта Graphics для рисования на новом изображении
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);

                    // Отрисовка изображения товара на новом изображении
                    g.DrawImage(imgProduct, (bmp.Width - imgProduct.Width) / 2, (bmp.Height - imgProduct.Height) / 2);
                    g.DrawImage(imgSample, new Rectangle(0, 0, bmp.Width, bmp.Height));

                    // Отрисовка дополнительной информации (например, цена, скидка и процент скидки)
                    Font font = new Font("Arial", 18, FontStyle.Bold);
                    SolidBrush brush = new SolidBrush(Color.White);

                    g.DrawString($"-{dataProduct.Percent}%", font, brush, 15, 20);
                    g.DrawString($"Скидка: {dataProduct.Discount} руб.", font, brush, 300, 20);

                    // Отрисовка зачеркнутой линии
                    Pen pen = new Pen(Color.White, 3);
                    g.DrawLine(pen, 100, 525, 160, 525);
                    g.DrawString(dataProduct.SalePrice + " руб.", font, brush, 90, 470);
                    g.DrawString($"Цена:  {dataProduct.BasePrice} руб.", font, brush, 10, 510);
                    g.DrawString($"М.Бонусы: +{dataProduct.BonusRubles} руб.", font, brush, 270, 510);

                    // Сохранение измененного изображения в файл
                    using (MemoryStream ms = new MemoryStream())
                    {
                        bmp.Save(ms, ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                }
            }
        }

        private void ClearBadDataProduct()
        {
            var tempList = new List<DataProduct>();

            foreach (var dataProduct in LstDataProduct)
            {
                if (string.IsNullOrEmpty(dataProduct.ProductName))
                    continue;
                tempList.Add(dataProduct);
            }

            LstDataProduct.Clear();
            LstDataProduct.AddRange(tempList);
        }
    }
}
