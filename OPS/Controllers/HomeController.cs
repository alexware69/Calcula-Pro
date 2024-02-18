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
