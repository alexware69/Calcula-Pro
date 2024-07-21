using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using QuoteTree;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using ElectronNET.API; 
using ElectronNET.API.Entities;
using System.ComponentModel;

namespace OnlinePriceSystem.Controllers
{
    public class HomeController : Controller
    {	
        public ActionResult Index()
        {
            var result = Electron.WindowManager.BrowserWindows.First().IsMaximizedAsync();
            if (result.Result) 
            {
                FirstRunManager.startedMaximized = true;
                var size = Electron.WindowManager.BrowserWindows.First().GetSizeAsync();
                int width = size.Result.First();
                int height = size.Result.Last();
                //This is a nasty hack needed to fix the restore after closed maximized behaviour.
                Electron.WindowManager.BrowserWindows.First().SetSize(width * 90 / 100, height * 90 / 100);
                Electron.WindowManager.BrowserWindows.First().Maximize();
            }

            bool quoteOnly = QuoteOnly().Result;
            TempData["quoteOnly"] = quoteOnly.ToString();
            return View();
        }

        public async Task<bool> QuoteOnly()
        {
            return (await Electron.App.CommandLine.HasSwitchAsync("quoteonly"));
        }

		 public ActionResult About()
        {
            return View();
        }

        //very cool method I found online
        public static void CopyFolder(DirectoryInfo source, DirectoryInfo target)
        {
            foreach (DirectoryInfo dir in source.GetDirectories())
                CopyFolder(dir, target.CreateSubdirectory(dir.Name));
            foreach (FileInfo file in source.GetFiles())
                file.CopyTo(Path.Combine(target.FullName, file.Name));
        }
    }
}
