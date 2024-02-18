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

namespace OnlinePriceSystem.Controllers
{
    public class DialogsController : Controller
    {
        private static bool openAdded;
        /*public IActionResult Index()
        {
            if (!HybridSupport.IsElectronActive || saveAdded) return Ok();
            Electron.IpcMain.On("save-dialog", async (args) =>
            {
                var mainWindow = Electron.WindowManager.BrowserWindows.First();
                var options = new SaveDialogOptions
                {
                    Title = "Save contact as JSON",
                    Filters = new FileFilter[]
                    {
                        new FileFilter { Name = "JSON", 
                                        Extensions = new string[] {"json" } }
                    }
                };
                var result = await 
                    Electron.Dialog.ShowSaveDialogAsync(mainWindow, options);
                Electron.IpcMain.Send(mainWindow, "save-dialog-reply", result);
            });
            saveAdded = true;
            return Ok();
        }*/

		public async void OpenNew()
        {
            var mainWindow = Electron.WindowManager.BrowserWindows.First();
				var options = new OpenDialogOptions
				{
					Title = "Open Definition Directory",
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openDirectory
                    }
					/*Filters = new FileFilter[]
					{
						new FileFilter { Name = "JSON", 
										Extensions = new string[] {"json" } }
					}*/
				};
				var result = await 
					Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
				Electron.IpcMain.Send(mainWindow, "new-dialog-reply", result);
        }

		public async void Open()
        {
			var mainWindow = Electron.WindowManager.BrowserWindows.First();
				var options = new OpenDialogOptions
				{
					Title = "Open Definition Directory",
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openDirectory
                    }
					/*Filters = new FileFilter[]
					{
						new FileFilter { Name = "JSON", 
										Extensions = new string[] {"json" } }
					}*/
				};
				var result = await 
					Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
				Electron.IpcMain.Send(mainWindow, "open-dialog-reply", result);
		}

		public async void OpenView()
        {
			var mainWindow = Electron.WindowManager.BrowserWindows.First();
				var options = new OpenDialogOptions
				{
					Title = "Open Definition Directory",
                    Properties = new OpenDialogProperty[] {
                        OpenDialogProperty.openDirectory
                    }
					/*Filters = new FileFilter[]
					{
						new FileFilter { Name = "JSON", 
										Extensions = new string[] {"json" } }
					}*/
				};
				var result = await 
					Electron.Dialog.ShowOpenDialogAsync(mainWindow, options);
				Electron.IpcMain.Send(mainWindow, "open-dialog-view-reply", result);
		}

        public IActionResult Index(string type)
		{
			//if (!HybridSupport.IsElectronActive || openAdded) return Ok();
			switch (type)
			{
				//Open New Definition dialog
				case "new-dialog":
				 OpenNew();
				 break;

				//Open Edit Definition dialog
				 case "open-dialog":
				 Open();
				 break;

				//Open View Quote dialog
				 case "open-dialog-view":
				 OpenView();
				 break;				
			}
			return Ok();
		}
    }
}