using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using QuoteTree;
using OPS.Models;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using System.Configuration;
using System.Xml.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.AspNetCore.Components.Authorization;
using System.Collections.Specialized;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OnlinePriceSystem.Controllers
{
    public class TreeViewController : Controller
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session == null)/* ||
                            !context.HttpContext.Session.TryGetValue("tree", out byte[] val))*/
            {
                context.Result =
                    new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Home",
                                                                            action = "Index" }));
            }
            base.OnActionExecuting(context);
        }
        public ActionResult Index(string product)
        {
            ops_inhouseEntities dc = new ops_inhouseEntities();
            QTree tree;

            //Get the product from database
            var products = dc.products.Select(p => p).Where(p => p.name == product);

            //Deserialize the product
            BinaryFormatter formater = new BinaryFormatter();
            byte[] bytes_stream;
            product1 prod = products.First();
            bytes_stream = prod.product.ToArray();
            MemoryStream memory_stream = new MemoryStream(bytes_stream);
            tree = (formater.Deserialize(memory_stream)) as QTree;

            TempData["treeDBID"] = "";
            TempData["tree"] = tree;

            return View(tree.Root);
        }

        
        
		//this need to be changed, instead of the product name pass the product id and save it to session
        public ActionResult NewQuote(int id)
        {
            ops_inhouseEntities dc = new ops_inhouseEntities();
            QTree tree;

            //Get the product from database
			//var products = dc.products.Select(p => p).Where(p => p.id == id);
			var qry = from p in dc.products
					where p.id == id
				select new {
				id = p.id,
				product = p.product,
				store_id = p.store_id
			};

            //Deserialize the product
            byte[] bytes_stream;
			var prod = qry.First();
            bytes_stream = prod.product.ToArray();
            tree = QTree.Deserialize(bytes_stream);


			var store_id = prod.store_id;
			var store = from str in dc.stores where str.id == store_id select str;
			string store_name = store.First ().name;
            //TempData["store_name"] = store_name;

            TempData["treeDBID"] = "";
            //TempData["tree"] = tree;
            TempData["product_id"] = id;

            //TempData["tree"] = tree;
            TempData["root"] = tree.Root;

            HttpContext.Session.SetInt32("product_id", id);
            HttpContext.Session.SetString("store_name", store_name);
            var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            });    
            HttpContext.Session.SetString("tree", toJson);


            return View("ViewQuote");
        }


        public ActionResult LoadTreeView(string id)
        {

            ops_inhouseEntities dc = new ops_inhouseEntities();
            IQueryable<quote1> quotes = dc.quotes.Select(q => q).Where(q => q.id.ToString() == id);
            //Now deserialize the quote tree
            byte[] bytes_stream;
            quote1 quote = quotes.First();
            bytes_stream = quote.quote.ToArray();
            QTree tree = QTree.Deserialize(bytes_stream);

            List<ANode> nodeList = new List<ANode>();
            nodeList.Add(tree.Root);
            //ViewData["nodes"] = nodeList;
            // This is needed only to save quote as a revision, so that the view knows what id to put in the url of QuoteDetails link

            TempData["treeDBID"] = id;
            TempData["tree"] = tree;
			TempData["product_id"] = quote.product_id;
            HttpContext.Session.SetInt32("product_id", int.Parse(id));

            return View("Index", tree.Root);
        }

        
        
        public ActionResult LoadQuote(string id)
        {

            ops_inhouseEntities dc = new ops_inhouseEntities();
            //IQueryable<quote> quotes = dc.quotes.Select(q => q).Where(q => q.id.ToString() == id);
			var qry = from q in dc.quotes
					where q.id.ToString() == id
				select new {
				id = q.id,
				quote = q.quote,
				product_id = q.product_id,
				store_id = q.store_id
			};
            //Now deserialize the quote tree
            byte[] bytes_stream;
			var quote = qry.First();
            bytes_stream = quote.quote.ToArray();
            QTree tree = QTree.Deserialize(bytes_stream);

            List<ANode> nodeList = new List<ANode>();
            nodeList.Add(tree.Root);
            //ViewData["nodes"] = nodeList;
            // This is needed only to save quote as a revision, so that the view knows what id to put in the url of QuoteDetails link

            TempData["treeDBID"] = id;
            
			TempData["product_id"] = quote.product_id;

			var store_id = quote.store_id;
			var stores = from str in dc.stores where str.id == store_id select str;
			string store_name = stores.First ().name;

			TempData["store_name"] = store_name;
            HttpContext.Session.SetString("store_name", store_name);

            TempData["root"] = tree.Root;

            var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            });    
            HttpContext.Session.SetString("tree", toJson);

			return View("ViewQuote");
        }

		
		
		public ActionResult Edit(string product)
		{

			QTree tree;
            TempData["renamed"] = null;

			if (product != null && product != "") 
			{
				if (product == "uploaded") 
				{
					string jsonString = HttpContext.Session.GetString("tree");
                    var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    });
                    tree = fromJson;
					//pass zero as product id, this means is a new product
                    HttpContext.Session.SetInt32("product_id", 0);
					//ViewBag.id = 0;
				}
				else
				if (product == "new") 
				{
					tree = new QTree ();
					TempData["treeDBID"] = "";
					var toJson1 = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        PreserveReferencesHandling = PreserveReferencesHandling.All,
                        Formatting = Formatting.Indented
                    });    
                    HttpContext.Session.SetString("tree", toJson1);
					//pass zero as product id, this means is a new product
					HttpContext.Session.SetInt32("product_id", 0);
                    //ViewBag.id = 0;
				}
				else
				{
                    ops_inhouseEntities dc = new ops_inhouseEntities();

					//Get the product from database
					//var products = dc.products.Select(p => p).Where(p => p.id == int.Parse(product));

					var qry = from p in dc.products
							where p.id.ToString() == product
						select new {
						id = p.id,
						product = p.product
					};

					//Deserialize the product
					byte[] bytes_stream;
					var prod = qry.First();
					bytes_stream = prod.product.ToArray();
                    tree = QTree.Deserialize(bytes_stream);

					TempData["treeDBID"] = "";
					//TempData["tree"] = tree;
					TempData["product_id"] = prod.id;
					ViewBag.id = prod.id;
                    var id = prod.id;
                    HttpContext.Session.SetInt32("product_id", id);
                    string user = HttpContext.Session.GetString("username");
					var users = from usr in dc.user_accounts where usr.user == user select usr;
					var store_id = users.First ().store_id;
					var store = from str in dc.stores where str.id == store_id select str;
					string store_name = store.First ().name;
					TempData["store_name"] = store_name;
					ViewBag.store_name = store_name;

                    //HttpContext.Session.SetInt32("product_id", int.Parse(product));
                    HttpContext.Session.SetString("store_name", store_name);
				}
			}
			else tree = TempData["tree"] as QTree;

            TempData["root"] = tree.Root;
            var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All,
                Formatting = Formatting.Indented
            });    
            HttpContext.Session.SetString("tree", toJson);

			return View("EditProduct");
		}

        
        public ContentResult ChildNodes(string id)
        {
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            ANode node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));

            List<NodeData> nodes = new List<NodeData>();
            if (node != null && node.Children != null)
            {
                foreach (ANode child in node.Children)
                {
                    string Expression;
                    bool EditChildren = false;
                    switch (child.Type) 
                    {
                        case NodeType.Math:
                            Expression = (child as MathNode).Formula;
                            EditChildren = (child as MathNode).EditChildren;
                            break;
                        case NodeType.Range:
                            Expression = (child as RangeNode).Range;
                            break;
                        case NodeType.ConditionalRules:
                            Expression = (child as ConditionalRulesNode).Expression;
                            break;
                        case NodeType.Conditional:
                            Expression = (child as ConditionalNode).Formula;
                            break;
                        case NodeType.Reference:
                            Expression = (child as ReferenceNode).Target;
                            break;
                        default:
                            Expression = "";
                            break;
                    }

                   
                    bool leaf = child.Children == null || child.Children.Count == 0 ? true : false;
                    string dep = "";
                    foreach (string n in child.Dependents) dep = dep + n + ";";

                    nodes.Add(new NodeData(child.Name, child.Id, Expression, Url.Content("~/") + child.Url, child.CheckBox, child.Type.ToString(), child.Selected, child.IsComplete(), tree.Root.TotalStr, child.Optional, child.TotalStr, leaf, child.Hidden, child.ExpandedLevels, dep, EditChildren, child.Min.ToString(), child.Max.ToString(), child.Discount.ToString(), child.Order.ToString(), child.Report, child.ReportValue, child.Units, child.Parent != null && child.Parent.Type == NodeType.Decision, child.Template, child.HasErrors(), child.Error, child.ReadOnly, child.DisableCondition, child.Disabled, child.DisabledMessage));
                }
            }

            string response = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            return Content(response);
            //return Json(nodes, JsonRequestBehavior.AllowGet);
        }
        
        public ContentResult DependentNodes(string id)
        {
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            ANode node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));

            List<NodeData> nodes = new List<NodeData>();
            if (node != null && node.Dependents != null)
            {
                foreach (string dependent in node.Dependents)
                {
                    ANode dep = tree.GetNodeFromId(dependent);
                    string Expression;
                    bool EditChildren = false;
                    switch (dep.Type)
                    {
                        case NodeType.Math:
                            Expression = (dep as MathNode).Formula;
                            EditChildren = (dep as MathNode).EditChildren;
                            break;
                        case NodeType.Range:
                            Expression = (dep as RangeNode).Range;
                            break;
                        case NodeType.ConditionalRules:
                            Expression = (dep as ConditionalRulesNode).Expression;
                            break;
                        case NodeType.Conditional:
                            Expression = (dep as ConditionalNode).Formula;
                            break;
                        default:
                            Expression = "";
                            break;
                    }


                    bool leaf = dep.Children == null || dep.Children.Count == 0 ? true : false;
                    string depStr = "";
                    foreach (string n in dep.Dependents) depStr = depStr + n + ";";
                    nodes.Add(new NodeData(dep.Name, dep.Id, Expression, Url.Content("~/") + dep.Url, dep.CheckBox, dep.Type.ToString(), dep.Selected, dep.IsComplete(), tree.Root.TotalStr, dep.Optional, dep.TotalStr, leaf, dep.Hidden, dep.ExpandedLevels, depStr, EditChildren, dep.Min.ToString(), dep.Max.ToString(), dep.Discount.ToString(), dep.Order.ToString(), dep.Report, dep.ReportValue, dep.Units, dep.Parent != null && dep.Parent.Type == NodeType.Decision, dep.Template, dep.HasErrors(), dep.Error, dep.ReadOnly, dep.DisableCondition, dep.Disabled, dep.DisabledMessage));
                }
            }

            string response = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            return Content(response);
            //return Json(nodes, JsonRequestBehavior.AllowGet);
        }
        
        public ContentResult SetCheckboxState(string id, string state)
        {
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            ANode node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));

            node.Selected = state == "true" ? true : false;
            var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });    
            HttpContext.Session.SetString("tree", toJson);
            //string Expression = node.type == NodeType.math ? (node as MathNode).formula : "";


            string Expression;
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode).Formula;
                    EditChildren = (node as MathNode).EditChildren;
                    break;
                case NodeType.Range:
                    Expression = (node as RangeNode).Range;
                    break;
                case NodeType.ConditionalRules:
                    Expression = (node as ConditionalRulesNode).Expression;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode).Formula;
                    break;
                default:
                    Expression = "";
                    break;
            }




            bool leaf = node.Children == null || node.Children.Count == 0 ? true : false;
            string dep = "";
            foreach (string n in node.Dependents) dep = dep + n + ";";
            string total;
            try
            {
                total = tree.Root.Total().ToString("C");
            }
            catch 
            {
                total = "error";
            }
            NodeData nodeData = new NodeData(node.Name, node.Id, Expression, Url.Content("~/") + node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), total, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);
           
            string response = JsonConvert.SerializeObject(nodeData, Formatting.Indented);
            return Content(response);
        }

        
        public ActionResult QuoteDetails(string id)
        {
        QTree tree;

            //tree = TempData["tree"] as QTree;
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
            tree = fromJson;
            TempData["root"] = tree.Root;                
            Dictionary<string, string> selection;
            try
            {
                selection = tree.GetSelections();
            }
            catch (Exception e)
            {
                ViewData["error"] = e.Message;
                return View();
            }

            return View(selection);
        }
		public ActionResult EditProducts()
		{
			//OnlinePriceSystemDataContext dc = new OnlinePriceSystemDataContext();
			//QTree tree;
			//product product;

			//Read product names from dir
			string[] dirs = Directory.GetDirectories("~/Products");
			//string[] temp;
			//for (int i = 0; i < dirs.Length; i++)
			//{
			//temp = dirs[i].Split("\\".ToCharArray());
			//tree = new QTree(dirs[i]);
			//product = new product();
			//product.product = tree.Serialize().ToArray();
			//product.name = temp[temp.Length - 1];
			//dc.products.InsertOnSubmit(product);
			//}
			//dc.SubmitChanges();
			return View(dirs);
		}
        //To save products to database
        public ActionResult ReloadProducts()
        {
            //OnlinePriceSystemDataContext dc = new OnlinePriceSystemDataContext();
            //QTree tree;
            //product product;

            //Read product names from dir
            string[] dirs = Directory.GetDirectories("~/Products");
            //string[] temp;
            //for (int i = 0; i < dirs.Length; i++)
            //{
                //temp = dirs[i].Split("\\".ToCharArray());
                //tree = new QTree(dirs[i]);
                //product = new product();
                //product.product = tree.Serialize().ToArray();
                //product.name = temp[temp.Length - 1];
                //dc.products.InsertOnSubmit(product);
            //}
            //dc.SubmitChanges();
            return View(dirs);
        }
        //To save products to database
        [HttpPost]
        public ActionResult ReloadProducts(FormCollection form)
        {
            ops_inhouseEntities dc = new ops_inhouseEntities();
            QTree tree;
            product1 product;

            foreach(string key in form.Keys)
            {
                string value = form[key];
                tree = new QTree(value, true);
                
                //Reset the entered property
                tree.ResetEntered(tree.Root);

                var qry = from prod in dc.products where prod.name==key select prod;
                if (qry.Count() > 0)
                {
                    var item = qry.Single();
                    item.product = tree.Serialize().ToArray();
                    dc.SaveChanges();
                }
                else
                {
                    product = new product1();
                    product.product = tree.Serialize().ToArray();
                    product.name = key;
                    dc.products.Add(product);
                    dc.SaveChanges();
                }
            }


            //Read product names from dir
            //string[] dirs = Directory.GetDirectories(Server.MapPath("~/Products"));
            //string[] temp;
            //for (int i = 0; i < dirs.Length; i++)
            //{
            //    temp = dirs[i].Split("\\".ToCharArray());
            //    tree = new QTree(dirs[i]);
            //    product = new product();
            //    product.product = tree.Serialize().ToArray();
            //    product.name = temp[temp.Length - 1];
            //    dc.products.InsertOnSubmit(product);
            //}
            var qry1 = from prod in dc.products select prod.name;
            string[] allproducts = qry1.ToArray();
            //return View("../Home/Index", allproducts);
            return RedirectToAction("Index", "Home");
        }
		
        
        public ActionResult SaveQuote()
        {
            try
            {
                QTree tree;
                string jsonString = HttpContext.Session.GetString("tree");
                var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    PreserveReferencesHandling = PreserveReferencesHandling.All
                });
                tree = fromJson;
                if (tree.Root.IsComplete())
                {
                    ops_inhouseEntities dc = new ops_inhouseEntities();

                    //string user = User.Identity.Name;
                    //string id = TempData["product_id"].ToString();
                    string id = HttpContext.Session.GetInt32("product_id").ToString();
                    var store_id = from prod in dc.products where prod.id.ToString() == id select prod.store_id;

                    quote1 Quote = new quote1();
                    Quote.date = DateTime.Now;
                    Quote.total = tree.Root.Total();
                    Quote.product_name = tree.Root.Name;
                    Quote.quote = tree.Serialize().ToArray();
                    Quote.item = tree.Root.Name;
                    Quote.store_id = store_id.First();
                    Quote.product_id = int.Parse(id);
                    string xml = tree.SerializeToString();
                    Quote.quote_xml = xml;
                    if (TempData["treeDBID"] != null && TempData["treeDBID"].ToString() != "")
                    {
                        int parent_quote = int.Parse(TempData["treeDBID"].ToString());
                        var quotes = from quote in dc.quotes where quote.id == parent_quote select quote;
                        if (quotes.First().revision != null) Quote.revision = quotes.First().revision;
                        else Quote.revision = int.Parse(TempData["treeDBID"].ToString());
                    }
                    string user = HttpContext.Session.GetString("username");
                    Quote.user = user;

                    dc.quotes.Add(Quote);
                    dc.SaveChanges();
                    TempData["tree"] = null;
                }
            }
            catch (Exception e) 
            {
                string s = e.Message;
            }
			return RedirectToAction("Index", "MyQuotes",new {id=1});
        }

        //This one is not used any more, can be deleted
        [HttpPost]
        public ActionResult SaveQuotePost()
        {
            if (TempData["tree"] != null)
            {
                QTree tree = TempData["tree"] as QTree;
                if (tree.Root.IsComplete())
                {
                    ops_inhouseEntities dc = new ops_inhouseEntities();
                    quote1 Quote = new quote1();
                    Quote.date = DateTime.Now;
                    Quote.total = tree.Root.Total();
                    Quote.product_name = tree.Root.Name;
                    Quote.quote = tree.Serialize().ToArray();
                    Quote.item = tree.Root.Name;
                    if (TempData["treeDBID"] != null && TempData["treeDBID"].ToString() != "")
                        Quote.revision = int.Parse(TempData["treeDBID"].ToString());
                    Quote.user = User.Identity.Name;

                    dc.quotes.Add(Quote);
                    dc.SaveChanges();
                }
            }
            return RedirectToAction("Index", "MyQuotes");
        }
        
        public ContentResult GetTotalPrice()
        {
            QTree tree = TempData["tree"] as QTree;
            string total;
            try
            {
                total = tree.Root.Total().ToString("C");
            }
            catch
            {
                total = "error";
            }
            return Content(total);
        }
        
        public ActionResult ChangeTreeValue(string id)
        {
            var product_id = HttpContext.Session.GetInt32("product_id");
            var store_name = HttpContext.Session.GetString("store_name");

            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.All
            });
            QTree tree = fromJson;
            MathNode node = (MathNode)tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));
            TempData["node"] = node;       
            TempData["store_name"] = store_name;   
            
            return View();
        }
        
        public ActionResult Description(string id)
        {
            var product_id = HttpContext.Session.GetInt32("product_id");
            var store_name = HttpContext.Session.GetString("store_name");

            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;

            //MathNode node = (MathNode)tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));
            ANode node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));
            TempData["node"] = node;       
            TempData["store_name"] = store_name;   
            return View();
        }
        
        public ActionResult AppendNodes(string id)
        {
           var product_id = HttpContext.Session.GetInt32("product_id");
            var store_name = HttpContext.Session.GetString("store_name");

            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            ANode node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));

            return View((SumSetNode)node);
        }

        [HttpGet]
        
        public ContentResult CommitTreeValue()
        {
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            NameValueCollection keys = new NameValueCollection();
            keys = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString());
            foreach (var key in keys)
            {
                if (key.ToString() != "_")
                {
                    var value = keys.Get(key.ToString());
                    string nodeID = key.ToString().Replace("NodeValue", "");
                    ANode node = tree.GetNodeFromId(nodeID);
                    decimal result;
                    if (Decimal.TryParse(value, out result))
                    {
                        (node as MathNode).Formula = value;
                    }
                }
            }

            var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });    
            HttpContext.Session.SetString("tree", toJson);
            return Content(tree.Root.TotalStr);
        }

        [HttpGet]
        
        public ContentResult AppendNode(string sourceId, string targetId)
        {
            var product_id = HttpContext.Session.GetInt32("product_id");
            var store_name = HttpContext.Session.GetString("store_name");

            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            ANode node = tree.CloneTemplate(sourceId, targetId);

            string Expression;
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode).Formula;
                    EditChildren = (node as MathNode).EditChildren;
                    break;
                case NodeType.Range:
                    Expression = (node as RangeNode).Range;
                    break;
                case NodeType.ConditionalRules:
                    Expression = (node as ConditionalRulesNode).Expression;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode).Formula;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0 ? true : false;
            string dep = "";
            foreach (string n in node.Dependents) dep = dep + n + ";";
            NodeData nodedata = new NodeData(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);

            TempData["tree"] = tree;
            string response = JsonConvert.SerializeObject(nodedata, Formatting.Indented);
            return Content(response);
        }

		[HttpGet]
		
		public ContentResult CommitAnyTreeValue(string id, string value)
		{
			var product_id = HttpContext.Session.GetInt32("product_id");
            var store_name = HttpContext.Session.GetString("store_name");

            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
			ANode node = tree.GetNodeFromId(id);

			switch (node.Type)
			{
				case NodeType.Math:
					(node as MathNode).Formula = value;
					break;
				case NodeType.Range:
					(node as RangeNode).Range = value;
					break;
				case NodeType.ConditionalRules:
					(node as ConditionalRulesNode).Expression = value;
					break;
				case NodeType.Conditional:
					(node as ConditionalNode).Formula = value;
					break;
				default:
					break;
			}

			var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });    
            HttpContext.Session.SetString("tree", toJson);
            string total;
            try
            {
                total = tree.Root.Total().ToString("C");
            }
            catch
            {
                total = "error";
            }
			return Content(total);
		}

		[HttpGet]
		
		public ContentResult removeNodes(string ids)
		{
			string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            string[] parsedIds = ids.Trim().Split(";".ToCharArray());
            foreach (string id in parsedIds)
            {
                if (id.Trim() != "")
                {
                    ANode node = tree.GetNodeFromId(id);
                    if (node != null) node.Remove();
                }
            }
			var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });    
            HttpContext.Session.SetString("tree", toJson);
            string total;
            try
            {
                total = tree.Root.Total().ToString("C");
            }
            catch
            {
                total = "error";
            }
			return Content(total);
		}

		
		public ActionResult SaveProduct(int id)
		{
			if (HttpContext.Session.GetString("tree") != null)
			{
				//get store name...this is better to do it just one time during login and save to session variable
                ops_inhouseEntities dc = new ops_inhouseEntities();
				string user = HttpContext.Session.GetString("username");
				var users = from usr in dc.user_accounts where usr.user == user select usr;
				var store_id = users.First ().store_id;
				var store = from str in dc.stores where str.id == store_id select str;
				string store_name = store.First ().name;

				string jsonString = HttpContext.Session.GetString("tree");
                var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                });
                QTree tree = fromJson;
	
				//if is a new product check if exists one with same name for the same store, if so exit
				var qry = from prod in dc.products where (prod.name == tree.Root.Name && prod.store_id == store_id) select prod.id;
				if (id == 0)
				{
                    if (qry.Count() > 0) return RedirectToAction("Index", "MyProducts", new { id = 1 });
				}

                Dictionary<ANode, string> renamed = TempData["renamed"] != null ? TempData["renamed"] as Dictionary<ANode, string> : null;

				//Save to folder
				tree.SaveTreeTo (tree.Root, "~" + Path.DirectorySeparatorChar + "Products" + Path.DirectorySeparatorChar + store_name, renamed);

                //Reset the entered property
                tree.ResetEntered(tree.Root);

                //reset the renamed dictionary
                TempData["renamed"] = null;

                //Save to database
				var qry2 = from prod in dc.products
				           where prod.id == id
				           select prod;
					
				if (qry2.Count() > 0)
				{
					var item = qry2.Single();
					item.name = tree.Root.Name;
					MemoryStream stream = tree.Serialize ();
					item.product = stream.ToArray();
					item.modified_by = user;
					item.modified = DateTime.Now;
					item.size = (int)stream.Length;
                    string xml = tree.SerializeToString();
                    item.product_xml = xml;
					dc.SaveChanges();
				}
				else
				{
					product1 product = new product1();
					MemoryStream stream = tree.Serialize ();
					product.product = stream.ToArray();
					product.name = tree.Root.Name;
					product.created_by = user;
					product.modified_by = user;
					product.created = DateTime.Now;
					product.modified = DateTime.Now;
					product.store_id = store_id;
					product.size = (int)stream.Length;
                    string xml = tree.SerializeToString();
                    product.product_xml = xml;
					product.active = false;
					dc.products.Add(product);
					dc.SaveChanges();
				}

			}
            return RedirectToAction("Index", "MyProducts", new { id = 1 });
		}

		[HttpGet]
		
		public ContentResult SaveNodeInfo()
		{
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            ANode node;

            //if the node was renamed add node and old name to session variable (this code has been commented...needs to be fixed and uncommented...have to serialize and save to session)
            /*string id = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString())["id"];
            node = tree.GetNodeFromId(id);
            string oldname = node.Name;
            string newname = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString())["name"];
            Dictionary<ANode, string> renamed = TempData["renamed"] == null ? new Dictionary<ANode, string>() : TempData["renamed"] as Dictionary<ANode, string>;
            if (oldname.Trim() != newname.Trim())
            {
                renamed.Add(node, oldname.Trim());
                //Implement refactor
                //tree.Refactor(node.References, oldname, newname);
            }
            TempData["renamed"] = renamed;*/

            //save node
            node = tree.SaveNodeInfo(HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString()));
            tree.TotalCounter = 0;
            string Expression = "";
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode).Formula;
                    EditChildren = (node as MathNode).EditChildren;
                    break;
                case NodeType.Range:
                    Expression = (node as RangeNode).Range;
                    EditChildren = (node as RangeNode).EditChildren;
                    break;
                case NodeType.Date:                   
                    EditChildren = (node as DateNode).EditChildren;
                    break;
                case NodeType.ConditionalRules:
                    Expression = (node as ConditionalRulesNode).Expression;
                    EditChildren = (node as ConditionalRulesNode).EditChildren;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode).Formula;
                    EditChildren = (node as ConditionalNode).EditChildren;
                    break;
                case NodeType.SumSet:
                    EditChildren = (node as SumSetNode).EditChildren;
                    break;
                case NodeType.Reference:
                    Expression = (node as ReferenceNode).Target;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0 ? true : false;
            string dep = "";
            foreach (string n in node.Dependents) dep = dep + n + ";";
            NodeData nodedata = new NodeData(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);

            var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });    
            HttpContext.Session.SetString("tree", toJson);

            string response = JsonConvert.SerializeObject(nodedata, Formatting.Indented);
            return Content(response);
		}

        public void pruneTree(ANode n, int RootExpandedLevels)
        {
            int countDots;
            if (n.Id != "1")
            {
                countDots = n.Id.Split(".".ToCharArray()).Length - 1;
                if (countDots == RootExpandedLevels)
                {
                    n.Children.Clear();
                    return;
                }
            }
            for (int i = 0; i < n.Children.Count; i++)
            {
                pruneTree(n.Children[i], RootExpandedLevels);
            }
        }

        //public void pruneTree2(dynamic parsedJson, int RootExpandedLevels)
        //{
           
            
        //    int countDots;
        //    if (parsedJson.features[0] != "1")
        //    {
        //        countDots = n.Id.Split(".".ToCharArray()).Length - 1;
        //        if (countDots == RootExpandedLevels)
        //        {
        //            n.Children.Clear();
        //            return;
        //        }
        //    }



        //    foreach (var Children in parsedJson)
        //    {
        //        feature.geometry.Replace(
        //                JObject.FromObject(
        //                            new
        //                            {
        //                                type = "Point",
        //                                coordinates = feature.geometry.coordinates[0][0]
        //                            }));
        //    }
        //    for (int i = 0; i < n.Children.Count; i++)
        //    {
        //        pruneTree(n.Children[i], RootExpandedLevels);
        //    }
        //}

        public JsonResult getJson()
        {
            /*var id = HttpContext.Session.GetInt32("product_id");

            if (id != 0)
            {
            ops_inhouseEntities dc = new ops_inhouseEntities();
            QTree tree;

            //Get the product from database
            //var products = dc.products.Select(p => p).Where(p => p.id == id);
            var qry = from p in dc.products
                      where p.id == id
                      select new
                      {
                          id = p.id,
                          product = p.product,
                          store_id = p.store_id
                      };

            //Deserialize the product
            byte[] bytes_stream;
            var prod = qry.First();
            bytes_stream = prod.product.ToArray();

            tree = QTree.Deserialize(bytes_stream);


            var settings = new JsonSerializerSettings
            {
                Converters = new List<JsonConverter> { new TreeConverter() },
                Formatting = Formatting.Indented,
                TypeNameHandling = TypeNameHandling.Auto,
                ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            };
            this.pruneTree(tree.Root, tree.Root.ExpandedLevels);
            string json = JsonConvert.SerializeObject(tree.Root, settings);
            //dynamic parsedJson = JObject.Parse(json);
            //pruneTree2(parsedJson, tree.Root.ExpandedLevels);
            return Json(Compress(json));
            }
            else 
            {*/
                if (HttpContext.Session.GetString("tree") != null)
                {
                    string jsonString = HttpContext.Session.GetString("tree");
                    var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    });
                    QTree tree = fromJson;

                    var settings = new JsonSerializerSettings
                    {
                        Converters = new List<JsonConverter> { new TreeConverter() },
                        Formatting = Formatting.Indented,
                        TypeNameHandling = TypeNameHandling.Auto,
                        ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects
                    };
                    if (tree.Root != null)
                    {
                        //this.pruneTree(tree.Root, tree.Root.ExpandedLevels);
                        string json = JsonConvert.SerializeObject(tree.Root, settings);
                        //dynamic parsedJson = JObject.Parse(json);
                        //pruneTree2(parsedJson, tree.Root.ExpandedLevels);
                        return Json(Compress(json));
                    }
                    else return Json(null);
                }
                else return Json(null);
            //}
        }
        public static string Compress(string s)
        {
            var dict = new Dictionary<string, int>();
            char[] data = s.ToArray();
            var output = new List<char>();
            char currChar;
            string phrase = data[0].ToString();
            int code = 256;

            for (var i = 1; i < data.Length; i++)
            {
                currChar = data[i];
                var temp = phrase + currChar;
                if (dict.ContainsKey(temp))
                    phrase += currChar;
                else
                {
                    if (phrase.Length > 1)
                        output.Add((char)dict[phrase]);
                    else
                        output.Add((char)phrase[0]);
                    dict[phrase + currChar] = code;
                    code++;
                    phrase = currChar.ToString();
                }
            }

            if (phrase.Length > 1)
                output.Add((char)dict[phrase]);
            else
                output.Add((char)phrase[0]);

            return new string(output.ToArray());
        }

        [HttpGet]
        
        public ContentResult cloneNodes(string sourceId, string targetId)
        {
            //get store name...this is better to do it just one time during login and save to session variable
            ops_inhouseEntities dc = new ops_inhouseEntities();
            string user = HttpContext.Session.GetString("username");
            var users = from usr in dc.user_accounts where usr.user == user select usr;
            var store_id = users.First().store_id;
            var store = from str in dc.stores where str.id == store_id select str;
            string store_name = store.First().name;
            TempData["store_name"] = store_name;
            HttpContext.Session.SetString("store_name", store_name);

            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;

            string[] idsParsed = sourceId.Split(";".ToCharArray());
			List<NodeData> nodeDataList = new List<NodeData>();

            foreach (string nodeID in idsParsed)
            {
                if (nodeID != String.Empty)
                {
                    ANode node = tree.CloneNode(nodeID, targetId);

                    string Expression;
                    bool EditChildren = false;
                    switch (node.Type)
                    {
                        case NodeType.Math:
                            Expression = (node as MathNode).Formula;
                            EditChildren = (node as MathNode).EditChildren;
                            break;
                        case NodeType.Range:
                            Expression = (node as RangeNode).Range;
                            break;
                        case NodeType.ConditionalRules:
                            Expression = (node as ConditionalRulesNode).Expression;
                            break;
                        case NodeType.Conditional:
                            Expression = (node as ConditionalNode).Formula;
                            break;
                        case NodeType.Reference:
                            Expression = (node as ReferenceNode).Target;
                            break;
                        default:
                            Expression = "";
                            break;
                    }
                    bool leaf = node.Children == null || node.Children.Count == 0 ? true : false;
                    string dep = "";
                    foreach (string n in node.Dependents) dep = dep + n + ";";
                    NodeData nodedata = new NodeData(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);
                    nodeDataList.Add(nodedata);
                }
            }
            var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });    
            HttpContext.Session.SetString("tree", toJson);
            string response = JsonConvert.SerializeObject(nodeDataList, Formatting.Indented);
            return Content(response);
        }

		[HttpGet]
		
		public ContentResult NewNode()
		{
			//get store name...this is better to do it just one time during login and save to session variable
            ops_inhouseEntities dc = new ops_inhouseEntities();
            string user = HttpContext.Session.GetString("username");
			var users = from usr in dc.user_accounts where usr.user == user select usr;
			var store_id = users.First ().store_id;
			var store = from str in dc.stores where str.id == store_id select str;
			string store_name = store.First ().name;
			TempData["store_name"] = store_name;

            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;

            ANode node = tree.NewNode(HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString()));
            string Expression = "";
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode).Formula;
                    EditChildren = (node as MathNode).EditChildren;
                    break;
                case NodeType.Date:
                    EditChildren = (node as DateNode).EditChildren;
                    break;
                case NodeType.Range:
                    Expression = (node as RangeNode).Range;
                    break;
                case NodeType.ConditionalRules:
                    Expression = (node as ConditionalRulesNode).Expression;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode).Formula;
                    break;
                case NodeType.Reference:
                    Expression = (node as ReferenceNode).Target;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0 ? true : false;
            string dep = "";
            foreach (string n in node.Dependents) dep = dep + n + ";";
            NodeData nodedata = new NodeData(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root != null ? tree.Root.TotalStr : "0", node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);
			
            var toJson = JsonConvert.SerializeObject(tree, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });    
            HttpContext.Session.SetString("tree", toJson);
            string response = JsonConvert.SerializeObject(nodedata, Formatting.Indented);
            return Content(response);
		}

        [HttpGet]
        
        public ContentResult BuildDependencies() 
        {
            var product_id = HttpContext.Session.GetInt32("product_id");
            var store_name = HttpContext.Session.GetString("store_name");

            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;
            Stack<ANode> stack = new Stack<ANode>();
            Tuple<ANode, ANode> tuple = null;
            tuple = tree.SetDependents();
            
            List<NodeData> nodeDataList = new List<NodeData>();
            //Check for circular references
            if (tuple != null)
            {
                ANode node1 = tuple.Item1;
                NodeData nodedata1 = new NodeData(node1.Name, node1.Id, "", node1.Url, node1.CheckBox, node1.Type.ToString(), node1.Selected, node1.IsComplete(), tree.Root.TotalStr, node1.Optional, node1.TotalStr, false, node1.Hidden, node1.ExpandedLevels, "", false, node1.Min.ToString(), node1.Max.ToString(), node1.Discount.ToString(), node1.Order.ToString(), node1.Report, node1.ReportValue, node1.Units, node1.Parent != null && node1.Parent.Type == NodeType.Decision, node1.Template, node1.HasErrors(), node1.Error, node1.ReadOnly, node1.DisableCondition, node1.Disabled, node1.DisabledMessage);
                ANode node2 = tuple.Item2;
                NodeData nodedata2 = new NodeData(node2.Name, node2.Id, "", node2.Url, node2.CheckBox, node2.Type.ToString(), node2.Selected, node2.IsComplete(), tree.Root.TotalStr, node2.Optional, node2.TotalStr, false, node2.Hidden, node2.ExpandedLevels, "", false, node2.Min.ToString(), node2.Max.ToString(), node2.Discount.ToString(), node2.Order.ToString(), node2.Report, node2.ReportValue, node2.Units, node2.Parent != null && node2.Parent.Type == NodeType.Decision, node2.Template, node2.HasErrors(), node2.Error, node2.ReadOnly, node2.DisableCondition, node2.Disabled, node2.DisabledMessage);
                nodeDataList.Add(nodedata1);
                nodeDataList.Add(nodedata2);
            }
            else 
            {
                ANode node1 = tree.Root;
                NodeData nodedata1 = new NodeData(node1.Name, node1.Id, "", node1.Url, node1.CheckBox, node1.Type.ToString(), node1.Selected, node1.IsComplete(), tree.Root.TotalStr, node1.Optional, node1.TotalStr, false, node1.Hidden, node1.ExpandedLevels, "", false, node1.Min.ToString(), node1.Max.ToString(), node1.Discount.ToString(), node1.Order.ToString(), node1.Report, node1.ReportValue, node1.Units, node1.Parent != null && node1.Parent.Type == NodeType.Decision, node1.Template, node1.HasErrors(), node1.Error, node1.ReadOnly, node1.DisableCondition, node1.Disabled, node1.DisabledMessage);
                nodeDataList.Add(nodedata1);
            }
            string response = JsonConvert.SerializeObject(nodeDataList, Formatting.Indented);
            return Content(response);
        }

        
        public JsonResult jstreeChildNodes(string id)
        {
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;

            ANode node = tree.GetNodeFromId(id.Replace("ckbx_", ""));

            List<NodeDatajsTree> nodes = new List<NodeDatajsTree>();
            if (node != null && node.Children != null)
            {
                foreach (ANode child in node.Children)
                {
                    attributes attr = new attributes(child.Id);
                    nodes.Add(new NodeDatajsTree(child.Name, attr));
                }
            }


            //string response = JsonConvert.SerializeObject(nodes, Formatting.Indented);

            //string response = new JavaScriptSerializer().Serialize(new NodeDatajsTree(node.children[0].name,new attributes(node.children[0].id)));
            return Json(nodes);
            //return Json(new NodeDatajsTree(node.children[0].name, new attributes(node.children[0].id)), JsonRequestBehavior.AllowGet);
        }
        
        public ContentResult NodeInfo(string id)
        {
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;

            ANode node = tree.GetNodeFromId(id.Replace("li_ul_", ""));

            string Expression = "";
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode).Formula;
                    EditChildren = (node as MathNode).EditChildren;
                    break;
                case NodeType.Range:
                    Expression = (node as RangeNode).Range;
                    EditChildren = (node as RangeNode).EditChildren;
                    break;
                case NodeType.Date:
                    EditChildren = (node as DateNode).EditChildren;
                    break;
                case NodeType.ConditionalRules:
                    Expression = (node as ConditionalRulesNode).Expression;
                    EditChildren = (node as ConditionalRulesNode).EditChildren;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode).Formula;
                    EditChildren = (node as ConditionalNode).EditChildren;
                    break;
                case NodeType.SumSet:
                    EditChildren = (node as SumSetNode).EditChildren;
                    break;
                case NodeType.Reference:
                    Expression = (node as ReferenceNode).Target;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0 ? true : false;
            string dep = "";
            foreach (string n in node.Dependents) dep = dep + n + ";";
            NodeData nodedata = new NodeData(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);

            string response = JsonConvert.SerializeObject(nodedata, Formatting.Indented);
            return Content(response);
        }
        
        [HttpPost]
        public ContentResult NodesInfo(List<string> array)
        {
            string jsonString = HttpContext.Session.GetString("tree");
            var fromJson = JsonConvert.DeserializeObject<QTree>(jsonString, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects
            });
            QTree tree = fromJson;

			//string[] idsParsed = ids.Split(";".ToCharArray());
			List<NodeData> nodeDataList = new List<NodeData>();

			foreach (string nodeID in array)
			{
				if (nodeID != String.Empty)
				{
					ANode node = tree.GetNodeFromId(nodeID);

					string Expression;
					bool EditChildren = false;
                    if (node != null)
                    {
                        switch (node.Type)
                        {
                            case NodeType.Math:
                                Expression = (node as MathNode).Formula;
                                EditChildren = (node as MathNode).EditChildren;
                                break;
                            case NodeType.Date:
                                Expression = "";
                                EditChildren = (node as DateNode).EditChildren;
                                break;
                            case NodeType.Range:
                                Expression = (node as RangeNode).Range;
                                break;
                            case NodeType.ConditionalRules:
                                Expression = (node as ConditionalRulesNode).Expression;
                                break;
                            case NodeType.Conditional:
                                Expression = (node as ConditionalNode).Formula;
                                break;
                            case NodeType.Reference:
                                Expression = (node as ReferenceNode).Target;
                                break;
                            default:
                                Expression = "";
                                break;
                        }
                        bool leaf = node.Children == null || node.Children.Count == 0 ? true : false;
                        string dep = "";
                        //foreach (ANode n in node.Dependents) dep = dep + n.Id + ";";
                        dep = node.DependentsStr;
                        NodeData nodedata = new NodeData(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);
                        nodeDataList.Add(nodedata);
                    }
				}
			}
			string response = JsonConvert.SerializeObject(nodeDataList, Formatting.Indented);
			return Content(response);
        }
    }

    class NodeData
    {
        public string name;
        public string id;
        public string expression;
        public string url;
        public bool checkbox;
        public string type;
        public bool selected;
        public bool complete;
        public string total;
        public bool optional;
        public string subtotal;
        public bool leaf;
        public bool hidden;
        public int expandedLevels;
        public string dependents;
        public bool editChildren;
		public string min;
		public string max;
		public string discount;
		public string order;
		public bool report;
		public bool reportValue;
		public string units;
        public bool parentIsDecision;
        public bool template;
        public bool hasErrors;
        public string error;
        public bool readOnly;
        public bool disabled;
        public string disableCondition;
        public string disabledMessage;

        public NodeData()
        {
            name = "";
            id = "";
            expression = "";
            url = "";
            checkbox = false;
            type = "";
            selected = false;
            complete = false;
            total = "";
            optional = false;
            expandedLevels = 0;
            dependents = "";
            
        }
		public NodeData(string Name, string Id,string Expression, string Url, bool Checkbox, string Type, bool Selected, bool Complete, string Total, bool Optional, string Subtotal, bool isLeaf, bool isHidden, int ExpandedLevels, string Dependents, bool EditChildren, string Min, string Max, string Discount, string Order, bool Report, bool ReportValue, string Units, bool ParentIsDecision, bool Template, bool HasErrors, string Error, bool ReadOnly, string DisableCondition, bool Disabled, string DisabledMessage)
        {
            name = Name;
            id = Id;
            expression = Expression;
            url = Url;
            checkbox = Checkbox;
            type = Type;
            selected = Selected;
            complete = Complete;
            total = Total;
            optional = Optional;
            subtotal = Subtotal;
            leaf = isLeaf;
            hidden = isHidden;
            expandedLevels = ExpandedLevels;
            dependents = Dependents;
            editChildren = EditChildren;
			min = Min;
			max = Max;
			discount = Discount;
			order = Order;
			report = Report;
			reportValue = ReportValue;
			units = Units;
            parentIsDecision = ParentIsDecision;
            template = Template;
            hasErrors = HasErrors;
            error = Error;
            readOnly = ReadOnly;
            disableCondition = DisableCondition;
            disabled = Disabled;
            disabledMessage = DisabledMessage;
        }
    }

    class NodeDatajsTree
    {
        public string data;
        public attributes attr;

        public NodeDatajsTree()
        {
            data = "";
            attr = new attributes();
        }
        public NodeDatajsTree(string Data, attributes Attributes)
        {
            data = Data;
            attr = Attributes;
        }
    }

    class attributes
    {
        public string id;
        public attributes()
        {
            id = "";
        }
        public attributes(string ID)
        {
            id = ID;
        }
    }

    static class Helper
    {
        public static string ReplaceSpaces(string s, char c)
        {
            try
            {
                char[] array = s.ToCharArray();
                int index = 0;
                if (array.Length > 0)
                {
                    while (array[index] == ' ')
                    {
                        array[index] = c;
                        index++;
                    }
                    //index = array.Length - 1;
                    //while (array[index] == ' ')
                    //{
                    //    array[index] = ':';
                    //    index--;
                    //}
                    return new string(array);
                }
                else return "";
            }
            catch (Exception) { return null; }
        }
    }

    public class TreeConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ANode));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            //node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, 
            //dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, 
            //node.ReportValue, node.Units, node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), 
            //node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage
            ANode node = (ANode)value;
            string Expression = "";
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode).Formula;
                    EditChildren = (node as MathNode).EditChildren;
                    break;
                case NodeType.Range:
                    Expression = (node as RangeNode).Range;
                    EditChildren = (node as RangeNode).EditChildren;
                    break;
                case NodeType.ConditionalRules:
                    Expression = (node as ConditionalRulesNode).Expression;
                    EditChildren = (node as ConditionalRulesNode).EditChildren;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode).Formula;
                    EditChildren = (node as ConditionalNode).EditChildren;
                    break;
                case NodeType.SumSet:
                    EditChildren = (node as SumSetNode).EditChildren;
                    break;
                case NodeType.Reference:
                    Expression = (node as ReferenceNode).Target;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0 ? true : false;
            string dep = "";
            foreach (string n in node.Dependents) dep = dep + n + ";";
            JObject obj = new JObject();
            obj.Add("Name", node.Name);
            obj.Add("Id", node.Id);
            obj.Add("Expression", Expression);
            obj.Add("Url", node.Url);
            obj.Add("Checkbox", node.CheckBox);
            obj.Add("Type", node.Type.ToString());
            obj.Add("Selected", node.Selected);
            obj.Add("IsComplete", node.IsComplete());
            obj.Add("Optional", node.Optional);
            obj.Add("TotalStr", node.TotalStr);
            obj.Add("Hidden", node.Hidden);
            obj.Add("ExpandedLevels", node.ExpandedLevels);
            obj.Add("Dependents", dep);
            obj.Add("EditChildren", EditChildren);
            obj.Add("Min", node.Min.ToString());
            obj.Add("Max", node.Max.ToString());
            obj.Add("Discount", node.Discount.ToString());
            obj.Add("Order", node.Order.ToString());
            obj.Add("Report", node.Report);
            obj.Add("ReportValue", node.ReportValue);
            obj.Add("Units", node.Units);
            obj.Add("ParentIsDecision", node.Parent != null && node.Parent.Type == NodeType.Decision);
            obj.Add("Template", node.Template);
            obj.Add("HasErrors", node.HasErrors());
            obj.Add("Error", node.Error);
            obj.Add("ReadOnly", node.ReadOnly);
            obj.Add("DisableCondition", node.DisableCondition);
            obj.Add("Disabled", node.Disabled);
            obj.Add("DisabledMessage", node.DisabledMessage);
            obj.Add("Children", JArray.FromObject(node.Children, serializer));
            obj.WriteTo(writer);
        }

        public override bool CanRead
        {
            get { return false; }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
