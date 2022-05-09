using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
//using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using QuoteTree;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using ElectronNET.API; 
using ElectronNET.API.Entities;

namespace OnlinePriceSystem.Controllers
{
    public class HomeController : Controller
    {	
        public ActionResult Index(int id)
        {
            return View();
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
