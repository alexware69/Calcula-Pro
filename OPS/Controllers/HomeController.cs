using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using QuoteTree;
using System.Security.Claims;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace OnlinePriceSystem.Controllers
{
	[Route("default")]
    public class HomeController : Controller
    {
		[Route("")]
        [Route("index")]
        [Route("~/")]
        public ActionResult Index()
        {
			List<StoreProductsUtil> retList = new List<StoreProductsUtil> ();
			try
			{
                ops_inhouseEntities dc = new ops_inhouseEntities();
				string user = User.Identity.Name;
				var stores = from store in dc.stores select store;
				var stores_list = stores.ToList();
				foreach (store s in stores_list) 
				{
					var products = from prod in dc.products 
							where prod.store_id == s.id && prod.active == true 
						select new {
						id = prod.id,
						name = prod.name
					};
					StoreProductsUtil spu = new StoreProductsUtil ();
					spu.Store = s;
					spu.Products = new  List<ProductUtility>();
					foreach (var p in products)
					{
						ProductUtility pu = new ProductUtility();
						pu.id = p.id;
						pu.name = p.name;
						spu.Products.Add(pu);

					}
					retList.Add (spu);
				}
			}
			catch(Exception e)
			{
				string message = e.Message;
			}

			return View(retList);
        }


        public ActionResult About()
        {
            ViewBag.Message = "Your app description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
			
    }

	public class StoreProductsUtil
	{
		public store Store;
		public List<ProductUtility> Products;
	}

	public class ProductUtility
	{
		public int id;
		public string name;
	}

}
