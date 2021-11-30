using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QuoteTree;
using Pager;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnlinePriceSystem.Controllers
{
    public class MyQuotesController : Controller
    {
		public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session == null ||
                            !context.HttpContext.Session.TryGetValue("username", out byte[] val))
            {
                context.Result =
                    new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Account",
                                                                            action = "Index" }));
            }
            base.OnActionExecuting(context);
        }
        // GET: /MyQuotes/
		public ActionResult Index(int id)
        {
            ops_inhouseEntities dc = new ops_inhouseEntities();
			string user = HttpContext.Session.GetString("username");
            if(user == null) return RedirectToAction("Index","Account");

			var myquotesview = from q in dc.quotes
				join s in dc.stores on q.store_id equals s.id
					where q.user == user
				select new QuoteUtil { product_name = q.product_name, store = s.name, total = q.total, date = q.date.ToString(), id = q.id, revision = q.revision };
            
			List<QuoteUtil> returnList = myquotesview.ToList();
			PagedList<QuoteUtil> pagedlist = new PagedList<QuoteUtil>(returnList, 8);
			pagedlist.CurrentPage = id;
			pagedlist.OffSet = 1;
			return View(pagedlist);
        }

		public string GetStoreName(Guid id)
		{
            ops_inhouseEntities dc = new ops_inhouseEntities();
			var store = from str in dc.stores where str.id == id select str;
			string store_name = store.First ().name;
			return store_name;
		}
    }
}

public class QuoteUtil
{
	public string store;
	public string product_name;
	public decimal total;
	public string date;
	public int id;
	public int? revision;

	public QuoteUtil()
	{
	}
}

