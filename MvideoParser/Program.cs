using MvideoParser.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Resources;
using System.Text;
using ZennoLab.CommandCenter;
using ZennoLab.Emulation;
using ZennoLab.InterfacesLibrary.ProjectModel;
using ZennoLab.InterfacesLibrary.ProjectModel.Enums;

namespace MvideoParser
{
    /// <summary>
    /// Класс для запуска выполнения скрипта
    /// </summary>
    public class Program : IZennoExternalCode
    {
        /// <summary>
        /// Метод для запуска выполнения скрипта
        /// </summary>
        /// <param name="instance">Объект инстанса выделеный для данного скрипта</param>
        /// <param name="project">Объект проекта выделеный для данного скрипта</param>
        /// <returns>Код выполнения скрипта</returns>		
        public int Execute(Instance instance, IZennoPosterProjectModel project)
        {
            var select = project.Variables["select"].Value;

            var mvideo = new Mvideo(project);
            var t = new MyTelegraph(project);

            List<DataProduct> lstDataProduct = new List<DataProduct>(); 
            switch (select)
            {
                case "Товары Дня":
                    {
                        lstDataProduct = mvideo.GoodsOfDay();
                        break;
                    }

                case "Новые Товары":
                    {
                        lstDataProduct = mvideo.NewGoods();
                        break;
                    }
                case "Больше просмотров":
                    {
                        lstDataProduct = mvideo.MostViewed();
                        break;
                    }
                default:
                    throw new Exception("Ошибка Switch");
            }

            t.CreateAccount("Товары М.Видео");
            var creator = t.ContentBuilder;

            foreach (var dataProduct in lstDataProduct)
            {
                var cardProduct = t.UploadImg(dataProduct.ProductCardByteArr);
                creator.AddText(dataProduct.ProductName);
                creator.AddImg(cardProduct);
                creator.AddLink(dataProduct.Url, "Купить");
                creator.AddText("_________________");
            }

            var content = creator.Create();
            var link = t.CreatePage(lstDataProduct[0].Name, content);

            instance.ActiveTab.Navigate(link);

            return 0;
        }
    }
}