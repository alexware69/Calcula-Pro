﻿using System.Collections;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using QuoteTree;
using System.Runtime.Serialization.Formatters.Binary;
using Newtonsoft.Json;
using System.Collections.Specialized;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text;
using ElectronNET.API.Entities;

namespace OnlinePriceSystem.Controllers
{
    public class TreeViewController : Controller
    {
        //This is needed to get html of view in a string
        private readonly ICompositeViewEngine _viewEngine;

        public TreeViewController(ICompositeViewEngine viewEngine)
        {
            _viewEngine = viewEngine;
        }
        public ActionResult Edit(string product, string isNew)
        {
            QTree? tree = null;
            HttpContext.Session.SetString("renamed", "");
            HttpContext.Session.SetString("isNew", isNew);

            HttpContext.Session.SetString("dependentsVisibility", "visible");
            HttpContext.Session.SetString("referencesVisibility", "visible");

            if (!string.IsNullOrEmpty(product))
            {
                if (isNew == "true")
                {
                    HttpContext.Session.SetString("path", product);
                    tree = new QTree();

                    byte[] array = ObjectToByteArray(tree);
                    HttpContext.Session.Set("tree", array);
                }
                else
                {
                    HttpContext.Session.SetString("path", product);
                    try
                    {
                        tree = new QTree(product, true);
                    }
                    catch (Exception)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    //Reset the entered property
                    QTree.ResetEntered(tree.Root!);

                    byte[] array = ObjectToByteArray(tree);
                    HttpContext.Session.Set("tree", array);
                }
            }
            TempData["root"] = tree!.Root;
            TempData["dependentsVisibility"] = HttpContext.Session.GetString("dependentsVisibility");
            TempData["referencesVisibility"] = HttpContext.Session.GetString("referencesVisibility");
            return View("EditProduct");
        }

        public ContentResult SetDepVisibility(string visibility)
        {
            HttpContext.Session.SetString("dependentsVisibility", visibility);
            return Content("");
        }

        public ContentResult SetRefVisibility(string visibility)
        {
            HttpContext.Session.SetString("referencesVisibility", visibility);
            return Content("");
        }

        public ActionResult NewQuote(string product)
        {
            QTree? tree = null;

            if (!string.IsNullOrEmpty(product))
            {
                HttpContext.Session.SetString("path", product);
                try
                {
                    tree = new QTree(product, true);
                }
                catch (Exception)
                {
                    return RedirectToAction("Index", "Home");
                }
                //Reset the entered property
                QTree.ResetEntered(tree.Root!);

                byte[] array = ObjectToByteArray(tree);
                HttpContext.Session.Set("tree", array);
            }
            TempData["root"] = tree!.Root;
            return View("ViewQuote");
        }

        public ContentResult ChildNodes(string id)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode? node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));

            List<NodeData> nodes = new();
            if (node?.Children != null)
            {
                foreach (ANode child in node.Children)
                {
                    string Expression;
                    bool EditChildren = false;
                    switch (child.Type)
                    {
                        case NodeType.Math:
                            Expression = (child as MathNode)!.Expression;
                            EditChildren = (child as MathNode)!.EditChildren;
                            break;
                        case NodeType.Conditional:
                            Expression = (child as ConditionalNode)!.Expression;
                            break;
                        case NodeType.Text:
                            Expression = (node as TextNode)!.Expression;
                            break;
                        case NodeType.Reference:
                            Expression = (child as ReferenceNode)!.Expression;
                            break;
                        default:
                            Expression = "";
                            break;
                    }

                    bool leaf = child.Children == null || child.Children.Count == 0;
                    string dep = "";
                    foreach (string n in child.Dependents!) dep = dep + n + ";";

                    nodes.Add(new NodeData(child.Name, child.Id, Expression, Url.Content("~/") + child.Url, child.CheckBox, child.Type.ToString(), child.Selected, child.IsComplete(), tree.Root!.TotalStr, child.Optional, child.TotalStr, leaf, child.Hidden, child.ExpandedLevels, dep, EditChildren, child.Min.ToString(), child.Max.ToString(), child.Discount.ToString(), child.Order.ToString(), child.Report, child.ReportValue, child.Units, child.DecimalPlaces.ToString(), child.Parent != null && child.Parent.Type == NodeType.Decision, child.Template, child.HasErrors(), child.Error, child.ReadOnly, child.DisableCondition, child.Disabled, child.DisabledMessage));
                }
            }

            string response = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            return Content(response);
        }

        public ContentResult DependentNodes(string id)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode? node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));

            List<NodeData> nodes = new();
            if (node?.Dependents != null)
            {
                foreach (string dependent in node.Dependents)
                {
                    ANode dep = tree.GetNodeFromId(dependent)!;
                    string Expression;
                    bool EditChildren = false;
                    switch (dep.Type)
                    {
                        case NodeType.Math:
                            Expression = (dep as MathNode)!.Expression;
                            EditChildren = (dep as MathNode)!.EditChildren;
                            break;
                        case NodeType.Conditional:
                            Expression = (dep as ConditionalNode)!.Expression;
                            break;
                        default:
                            Expression = "";
                            break;
                    }

                    bool leaf = dep.Children == null || dep.Children.Count == 0;
                    string depStr = "";
                    foreach (string n in dep.Dependents!) depStr = depStr + n + ";";
                    nodes.Add(new NodeData(dep.Name, dep.Id, Expression, Url.Content("~/") + dep.Url, dep.CheckBox, dep.Type.ToString(), dep.Selected, dep.IsComplete(), tree.Root!.TotalStr, dep.Optional, dep.TotalStr, leaf, dep.Hidden, dep.ExpandedLevels, depStr, EditChildren, dep.Min.ToString(), dep.Max.ToString(), dep.Discount.ToString(), dep.Order.ToString(), dep.Report, dep.ReportValue, dep.Units, dep.DecimalPlaces.ToString(), dep.Parent != null && dep.Parent.Type == NodeType.Decision, dep.Template, dep.HasErrors(), dep.Error, dep.ReadOnly, dep.DisableCondition, dep.Disabled, dep.DisabledMessage));
                }
            }

            string response = JsonConvert.SerializeObject(nodes, Formatting.Indented);
            return Content(response);
        }

        public ContentResult SetCheckboxState(string id, string state)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode? node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));

            node!.Selected = state == "true";

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            string Expression;
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode)!.Expression;
                    EditChildren = (node as MathNode)!.EditChildren;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode)!.Expression;
                    break;
                case NodeType.Text:
                    Expression = (node as TextNode)!.Expression;
                    break;
                default:
                    Expression = "";
                    break;
            }

            bool leaf = node.Children == null || node.Children.Count == 0;
            string dep = "";
            foreach (string n in node.Dependents!) dep = dep + n + ";";
            string total;
            try
            {
                total = tree.Root!.TotalStr;
            }
            catch
            {
                total = "error";
            }
            NodeData nodeData = new(node.Name, node.Id, Expression, Url.Content("~/") + node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), total, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.DecimalPlaces.ToString(), node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);

            string response = JsonConvert.SerializeObject(nodeData, Formatting.Indented);
            return Content(response);
        }

        public ActionResult QuoteDetails()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            TempData["root"] = tree.Root!;
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

        public ContentResult GetTotalPrice()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            string total;
            try
            {
                total = tree.Root!.TotalStr;
            }
            catch
            {
                total = "error";
            }
            return Content(total);
        }

        public ActionResult ChangeTreeValue(string id)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode? node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));
            TempData["node"] = node;
            HttpContext.Session.SetString("nodeName", node!.Name);

            var path = HttpContext.Session.GetString("path")!;
            HttpContext.Session.SetString("nodeID", id);
            if (path.LastIndexOf("/") >= 0)
                path = path.Remove(path.LastIndexOf("/"));
            if (path.LastIndexOf("\\") >= 0)
                path = path.Remove(path.LastIndexOf("\\"));
            var url = path + "/" + node.GetPath() + "/homepage.htm";
            TempData["url"] = url;
            HttpContext.Session.SetString("url", url);
            TempData["dependentsVisibility"] = HttpContext.Session.GetString("dependentsVisibility");
            TempData["referencesVisibility"] = HttpContext.Session.GetString("referencesVisibility");
            return View();
        }

        public ActionResult Description(string id)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode? node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));
            TempData["node"] = node;
            HttpContext.Session.SetString("nodeName", node!.Name);

            var path = HttpContext.Session.GetString("path")!;
            HttpContext.Session.SetString("nodeID", id);
            if (path.LastIndexOf("/") >= 0)
                path = path.Remove(path.LastIndexOf("/"));
            if (path.LastIndexOf("\\") >= 0)
                path = path.Remove(path.LastIndexOf("\\"));

            var url = path + "/" + node.GetPath() + "/homepage.htm";
            TempData["url"] = url;
            HttpContext.Session.SetString("url", url);
            TempData["dependentsVisibility"] = HttpContext.Session.GetString("dependentsVisibility");
            TempData["referencesVisibility"] = HttpContext.Session.GetString("referencesVisibility");
            return View();
        }

        public ContentResult GetHtml()
        {
            var url = HttpContext.Session.GetString("url")!;
            url = url.Replace("\\", "/");
            string updatedStr = "";
            //url = url.Replace('/', Path.PathSeparator);
            string? urlTemp;
            try
            {
                FileStream fs = new(url, FileMode.Open, FileAccess.Read);
                //Set up the pictures
                var htmlDoc = new HtmlDocument();
                string contents;
                using (var sr = new StreamReader(fs))
                {
                    contents = sr.ReadToEnd();
                }
                htmlDoc.LoadHtml(contents);
                string mediaName = "";
                var path = "";
                var nodes = htmlDoc.DocumentNode.SelectNodes("//img");
                if (nodes == null || nodes.Count == 0)
                {
                    updatedStr = contents;
                }
                else
                {
                    foreach (var iNode in nodes)
                    {
                        mediaName = "";
                        updatedStr = "";
                        mediaName = iNode.Attributes["src"].Value;
                        path = HttpContext.Session.GetString("path");
                        int lastItem = url.LastIndexOf("/");
                        urlTemp = string.Concat(url.AsSpan(0, lastItem + 1), mediaName);
                        if (!urlTemp.StartsWith("file:///")) urlTemp = "file:///" + urlTemp;
                        iNode.SetAttributeValue("src", urlTemp);
                    }
                }
                updatedStr = htmlDoc.DocumentNode.OuterHtml;
            }
            catch //Check if the file has .html extension instead of .htm
            {
                try
                {
                    FileStream fs = new(url.Replace(".htm", ".html"), FileMode.Open, FileAccess.Read);
                    //Set up the pictures
                    var htmlDoc = new HtmlDocument();
                    string contents;
                    using (var sr = new StreamReader(fs))
                    {
                        contents = sr.ReadToEnd();
                    }
                    htmlDoc.LoadHtml(contents);
                    string mediaName;
                    string path;
                    var nodes = htmlDoc.DocumentNode.SelectNodes("//img");
                    if (nodes == null || nodes.Count == 0)
                    {
                        updatedStr = contents;
                    }
                    else
                    {
                        foreach (var iNode in nodes)
                        {
                            mediaName = "";
                            updatedStr = "";
                            mediaName = iNode.Attributes["src"].Value;
                            path = HttpContext.Session.GetString("path")!;
                            int lastItem = url.LastIndexOf("/");
                            urlTemp = string.Concat(url.AsSpan(0, lastItem + 1), mediaName);
                            if (!urlTemp.StartsWith("file:///")) urlTemp = "file:///" + urlTemp;
                            iNode.SetAttributeValue("src", urlTemp);
                        }
                    }
                    updatedStr = htmlDoc.DocumentNode.OuterHtml;
                }
                catch { }
            }

            return Content(updatedStr, "text/html");
        }
        public ActionResult AppendNodes(string id)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode? node = tree.GetNodeFromId(id.Replace("ckbx_", "").Replace("li_", ""));
            TempData["node"] = node;
            HttpContext.Session.SetString("nodeName", node!.Name);

            var path = HttpContext.Session.GetString("path")!;
            HttpContext.Session.SetString("nodeID", id);
            if (path.LastIndexOf("/") >= 0)
                path = path.Remove(path.LastIndexOf("/"));
            if (path.LastIndexOf("\\") >= 0)
                path = path.Remove(path.LastIndexOf("\\"));

            var url = path + "/" + node.GetPath() + "/homepage.htm";
            TempData["url"] = url;
            HttpContext.Session.SetString("url", url);
            TempData["dependentsVisibility"] = HttpContext.Session.GetString("dependentsVisibility");
            TempData["referencesVisibility"] = HttpContext.Session.GetString("referencesVisibility");
            return View();
        }

        [HttpGet]
        public ContentResult CommitTreeValue()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            NameValueCollection keys = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString());
            foreach (var key in keys)
            {
                if (key.ToString() != "_")
                {
                    var value = keys.Get(key.ToString())!;
                    string nodeID = key.ToString()!.Replace("NodeValue", "");
                    ANode node = tree.GetNodeFromId(nodeID)!;
                    if (decimal.TryParse(value, out _))
                    {
                        if (node.Type == NodeType.Math && !(node.Parent != null && node.Parent.Type == NodeType.Date))
                        {
                            (node as MathNode)!.Expression = value;
                        }
                        else
                        if (node.Type == NodeType.Math && node.Parent != null && node.Parent.Type == NodeType.Date)
                        {
                            string temp = (node as MathNode)!.Expression;
                            (node as MathNode)!.Expression = value;
                            if (DateTime.TryParse(((MathNode)node.Parent.Children![0]).Expression + "/" +
                            ((MathNode)node.Parent.Children![1]).Expression + "/" + ((MathNode)node.Parent.Children![2]).Expression, out _))
                            {
                                ((DateNode)node.Parent).Expression = ((MathNode)node.Parent.Children![0]).Expression + "/" + ((MathNode)node.Parent.Children![1]).Expression + "/" + ((MathNode)node.Parent.Children![2]).Expression;
                            }
                            else
                            {
                                (node as MathNode)!.Expression = temp;
                            }
                        }

                        //if (node.Parent != null && node.Parent.Type == NodeType.Today)
                        //{
                        //    ((TodayNode)node.Parent).Expression = ((MathNode)node.Parent.Children![0]).Expression + "/" + ((MathNode)node.Parent.Children![1]).Expression + "/" +  ((MathNode)node.Parent.Children![2]).Expression;
                        //}
                    }
                    if (node.Type == NodeType.Text) (node as TextNode)!.Expression = value;
                }
            }

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            return Content(tree.Root!.TotalStr);
        }

        [HttpGet]
        public ContentResult AppendNode(string sourceId, string targetId)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode node = tree.CloneTemplate(sourceId, targetId)!;

            string Expression;
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode)!.Expression;
                    EditChildren = (node as MathNode)!.EditChildren;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode)!.Expression;
                    break;
                case NodeType.Text:
                    Expression = (node as TextNode)!.Expression;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0;
            string dep = "";
            foreach (string n in node.Dependents!) dep = dep + n + ";";
            NodeData nodedata = new(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root!.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.DecimalPlaces.ToString(), node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            string response = JsonConvert.SerializeObject(nodedata, Formatting.Indented);
            return Content(response);
        }

        [HttpGet]
        public ContentResult CommitAnyTreeValue(string id, string value)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode node = tree.GetNodeFromId(id)!;

            switch (node.Type)
            {
                case NodeType.Math:
                    (node as MathNode)!.Expression = value;
                    break;
                case NodeType.Conditional:
                    (node as ConditionalNode)!.Expression = value;
                    break;
                case NodeType.Text:
                    (node as TextNode)!.Expression = value;
                    break;
                default:
                    break;
            }

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            string total;
            try
            {
                total = tree.Root!.TotalStr;
            }
            catch
            {
                total = "error";
            }
            return Content(total);
        }

        [HttpGet]
        public ContentResult RemoveNodes(string ids)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            string[] parsedIds = ids.Trim().Split(";".ToCharArray());
            string? currentRemoved = HttpContext.Session.GetString("removeNodesFromDirectory") ?? "";
            foreach (string id in parsedIds)
            {
                ANode? node = tree.GetNodeFromId(id);
                if (node != null)
                    if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return Content("");
            }
            foreach (string id in parsedIds)
            {
                if (id.Trim() != "")
                {
                    ANode? node = tree.GetNodeFromId(id);
                    if (node != null)
                    {
                        string path = HttpContext.Session.GetString("path")!;
                        if (path.LastIndexOf("/") >= 0)
                            path = path.Remove(path.LastIndexOf('/'));
                        else
                        if (path.LastIndexOf("\\") >= 0)
                            path = path.Remove(path.LastIndexOf('\\'));
                        currentRemoved = currentRemoved + ";" + path + "/" + node.GetPath().Replace("\\", "/");
                        node.Remove();
                    }
                    else
                    {
                        return Content("");
                    }
                }
            }

            HttpContext.Session.SetString("removeNodesFromDirectory", currentRemoved!);

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            string total;
            try
            {
                total = tree.Root!.TotalStr;
            }
            catch
            {
                total = "error";
            }
            return Content(total);
        }

        public ContentResult SaveDefinition()
        {
            if (HttpContext.Session.Get("tree") != null)
            {
                string isNew = HttpContext.Session.GetString("isNew")!;

                byte[] array = HttpContext.Session.Get("tree")!;
                QTree tree = ByteArrayToObject(array);

                Dictionary<string, string> renamed;
                string jsonStringRenamed = HttpContext.Session.GetString("renamed")!;
                if (jsonStringRenamed?.Length == 0)
                {
                    renamed = new Dictionary<string, string>();
                }
                else
                {
                    var fromJsonStringRenamed = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStringRenamed!, new JsonSerializerSettings
                    {
                        TypeNameHandling = TypeNameHandling.All,
                        PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                        MaxDepth = 400
                    });
                    renamed = fromJsonStringRenamed!;
                }

                //Save to folder
                string path = HttpContext.Session.GetString("path")!;
                if (isNew != "true")
                {
                    if (path.LastIndexOf("/") >= 0)
                        path = path.Remove(path.LastIndexOf('/'));
                    else
                    if (path.LastIndexOf("\\") >= 0)
                        path = path.Remove(path.LastIndexOf('\\'));
                }
                string currentRemoved = HttpContext.Session.GetString("removeNodesFromDirectory")!;
                QTree.RemoveNodesFromDirectory(currentRemoved);
                HttpContext.Session.SetString("removeNodesFromDirectory", "");
                QTree.SaveTreeTo(tree.Root!, path, renamed);
                //Reset the entered property
                QTree.ResetEntered(tree.Root!);

                //reset the renamed dictionary
                //TempData["renamed"] = null;
                HttpContext.Session.SetString("renamed", "");
            }
            return Content("");
        }

        //This method is needed to get html in a string
        private async Task<string> RenderPartialViewToString(string viewName, object model)
        {
            if (string.IsNullOrEmpty(viewName))
                viewName = ControllerContext.ActionDescriptor.ActionName;

            ViewData.Model = model;

            await using var writer = new StringWriter();
            ViewEngineResult viewResult =
                _viewEngine.FindView(ControllerContext, viewName, false);

            ViewContext viewContext = new(
                ControllerContext,
                viewResult.View!,
                ViewData,
                TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View!.RenderAsync(viewContext);

            return writer.GetStringBuilder().ToString();
        }

        public async Task<FileResult> SaveQuoteHTML()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            TempData["root"] = tree.Root!;
            Dictionary<string, string> selection = tree.GetSelections();

            var renderedView = await RenderPartialViewToString("QuoteDetails", selection);

            //Do what you want with the renderedView here
            //Removing the Save Quote button from the html file
            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(renderedView);
            var nodes = htmlDoc.DocumentNode.SelectNodes("//input");
            var button = nodes[0];
            button.SetAttributeValue("hidden", "hidden");
            button = nodes[1];
            button.SetAttributeValue("hidden", "hidden");
            var updatedStr = htmlDoc.DocumentNode.OuterHtml;

            return File(Encoding.UTF8.GetBytes(updatedStr), "text/plain", tree.Root!.Name + ".html");
        }
        public FileResult SaveQuoteXML()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            string serializedXMLTree = tree.SerializeToString();

            return File(Encoding.UTF8.GetBytes(serializedXMLTree), "text/plain", tree.Root!.Name + ".xml");
        }

        [HttpGet]
        public ContentResult SaveNodeInfo()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode? node;

            //if the node was renamed add node and old name to session variable (this code has been commented...needs to be fixed and uncommented...have to serialize and save to session)
            string id = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString())["id"]!;
            node = tree.GetNodeFromId(id)!;
            string oldname = node.Name;
            string newname = HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString())["name"]!;

            Dictionary<string, string> renamed;
            string jsonStringRenamed = HttpContext.Session.GetString("renamed")!;
            if (jsonStringRenamed?.Length == 0)
            {
                renamed = new Dictionary<string, string>();
            }
            else
            {
                var fromJsonStringRenamed = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonStringRenamed!, new JsonSerializerSettings
                {
                    TypeNameHandling = TypeNameHandling.All,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    MaxDepth = 400
                });
                renamed = fromJsonStringRenamed!;
            }
            if (oldname.Trim() != newname.Trim() && !renamed.ContainsKey(node.Id))
            {
                renamed.Add(node.Id, oldname.Trim());
                //Implement refactor
                //tree.Refactor(node.References, oldname, newname);
            }

            var toJsonStringRenamed = JsonConvert.SerializeObject(renamed, Newtonsoft.Json.Formatting.Indented, new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                MaxDepth = 400
            });
            HttpContext.Session.SetString("renamed", toJsonStringRenamed);
            //TempData["renamed"] = renamed;

            //before saving node, save curent expression in temp
            string oldExpression = "";
            if (node.Type == NodeType.Math) oldExpression = (node as MathNode)!.Expression;
            if (node.Type == NodeType.Conditional) oldExpression = (node as ConditionalNode)!.Expression;

            //save node
            node = tree.SaveNodeInfo(HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString()));
            if (node == null) return Content("");
            tree.TotalCounter = 0;
            string Expression = "";
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode)!.Expression;
                    EditChildren = (node as MathNode)!.EditChildren;
                    break;
                case NodeType.Text:
                    Expression = (node as TextNode)!.Expression;
                    EditChildren = (node as TextNode)!.EditChildren;
                    break;
                case NodeType.Date:
                    //Expression = ((MathNode)node.Children![0]).Expression + "/" + ((MathNode)node.Children![1]).Expression + "/" +  ((MathNode)node.Children![2]).Expression;
                    EditChildren = (node as DateNode)!.EditChildren;
                    break;
                case NodeType.Today:
                    //Expression = ((TodayNode)node.Children![0]).Expression + "/" + ((TodayNode)node.Children![1]).Expression + "/" +  ((TodayNode)node.Children![2]).Expression;
                    EditChildren = (node as TodayNode)!.EditChildren;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode)!.Expression;
                    EditChildren = (node as ConditionalNode)!.EditChildren;
                    break;
                case NodeType.SumSet:
                    EditChildren = (node as SumSetNode)!.EditChildren;
                    break;
                case NodeType.Reference:
                    Expression = (node as ReferenceNode)!.Expression;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0;
            string dep = "";
            foreach (string n in node.Dependents!) dep = dep + n + ";";
            NodeData nodedata = new(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root!.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.DecimalPlaces.ToString(), node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);

            //Restore old formula if there are errors
            if (node.Type == NodeType.Math && node.HasErrors()) (node as MathNode)!.Expression = oldExpression;
            if (node.Type == NodeType.Conditional && node.HasErrors()) (node as ConditionalNode)!.Expression = oldExpression;

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            string response = JsonConvert.SerializeObject(nodedata, Formatting.Indented);
            return Content(response);
        }

        public void PruneTree(ANode n, int RootExpandedLevels)
        {
            int countDots;
            if (n.Id != "1")
            {
                countDots = n.Id.Split(".".ToCharArray()).Length - 1;
                if (countDots == RootExpandedLevels)
                {
                    n.Children!.Clear();
                    return;
                }
            }
            for (int i = 0; i < n.Children!.Count; i++)
            {
                PruneTree(n.Children[i], RootExpandedLevels);
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

        public JsonResult GetJson()
        {
            if (HttpContext.Session.GetString("tree") != null)
            {
                byte[] array = HttpContext.Session.Get("tree")!;
                QTree tree = ByteArrayToObject(array);

                var settings = new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    TypeNameHandling = TypeNameHandling.Auto,
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects
                };
                if (tree.Root! != null)
                {
                    //this.pruneTree(tree.Root!, tree.Root!.ExpandedLevels);
                    string json = JsonConvert.SerializeObject(tree.Root, settings);
                    //dynamic parsedJson = JObject.Parse(json);
                    //pruneTree2(parsedJson, tree.Root!.ExpandedLevels);
                    return Json(json);
                }
                else
                {
                    return Json(null);
                }
            }
            else
            {
                return Json(null);
            }
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
                {
                    phrase += currChar;
                }
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

        // Convert an object to a byte array
        public static byte[] ObjectToByteArray(QTree obj)
        {
            BinaryFormatter bf = new();
            using var ms = new MemoryStream();
            bf.Serialize(ms, obj);
            return ms.ToArray();
        }

        //Deserialize the tree
        public static QTree ByteArrayToObject(byte[] array)
        {
            BinaryFormatter formater = new();
            using MemoryStream memory_stream = new(array);
            return (formater.Deserialize(memory_stream) as QTree)!;
        }

        [HttpGet]
        public ContentResult CloneNodes(string sourceId, string targetId)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            string[] idsParsed = sourceId.Split(";".ToCharArray());
            List<NodeData> nodeDataList = new();

            foreach (string nodeID in idsParsed)
            {
                if (nodeID != string.Empty)
                {
                    ANode? node = tree.CloneNode(nodeID, targetId);
                    if (node == null) return Content("");

                    string Expression;
                    bool EditChildren = false;
                    switch (node.Type)
                    {
                        case NodeType.Math:
                            Expression = (node as MathNode)!.Expression;
                            EditChildren = (node as MathNode)!.EditChildren;
                            break;
                        case NodeType.Text:
                            Expression = (node as TextNode)!.Expression;
                            EditChildren = (node as TextNode)!.EditChildren;
                            break;
                        case NodeType.Conditional:
                            Expression = (node as ConditionalNode)!.Expression;
                            break;
                        case NodeType.Reference:
                            Expression = (node as ReferenceNode)!.Expression;
                            break;
                        default:
                            Expression = "";
                            break;
                    }
                    bool leaf = node.Children == null || node.Children.Count == 0;
                    string dep = "";
                    foreach (string n in node.Dependents!) dep = dep + n + ";";
                    NodeData nodedata = new(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root!.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.DecimalPlaces.ToString(), node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);
                    nodeDataList.Add(nodedata);
                }
            }

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            string response = JsonConvert.SerializeObject(nodeDataList, Formatting.Indented);
            return Content(response);
        }

        [HttpGet]
        public ContentResult NewNode()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode? node = tree.NewNode(HttpUtility.ParseQueryString(HttpContext.Request.QueryString.ToString()));
            if (node == null) return Content("");
            string Expression;
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode)!.Expression;
                    EditChildren = (node as MathNode)!.EditChildren;
                    break;
                case NodeType.Text:
                    Expression = (node as TextNode)!.Expression;
                    EditChildren = (node as TextNode)!.EditChildren;
                    break;
                case NodeType.Date:
                    Expression = (node as DateNode)!.Expression;
                    EditChildren = (node as DateNode)!.EditChildren;
                    break;
                case NodeType.Today:
                    Expression = (node as TodayNode)!.Expression;
                    EditChildren = (node as TodayNode)!.EditChildren;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode)!.Expression;
                    break;
                case NodeType.Reference:
                    Expression = (node as ReferenceNode)!.Expression;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0;
            string dep = "";
            foreach (string n in node.Dependents!) dep = dep + n + ";";
            NodeData nodedata = new(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root! != null ? tree.Root!.TotalStr : "0", node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.DecimalPlaces.ToString(), node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            string response = JsonConvert.SerializeObject(nodedata, Formatting.Indented);
            return Content(response);
        }

        [HttpGet]
        public ContentResult BuildDependencies()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            Tuple<ANode, ANode>? tuple = tree.SetDependents();

            List<NodeData> nodeDataList = new();
            //Check for circular references
            if (tuple != null)
            {
                ANode node1 = tuple.Item1;
                NodeData nodedata1 = new(node1.Name, node1.Id, "", node1.Url, node1.CheckBox, node1.Type.ToString(), node1.Selected, node1.IsComplete(), tree.Root!.TotalStr, node1.Optional, node1.TotalStr, false, node1.Hidden, node1.ExpandedLevels, "", false, node1.Min.ToString(), node1.Max.ToString(), node1.Discount.ToString(), node1.Order.ToString(), node1.Report, node1.ReportValue, node1.Units, node1.DecimalPlaces.ToString(), node1.Parent != null && node1.Parent.Type == NodeType.Decision, node1.Template, node1.HasErrors(), node1.Error, node1.ReadOnly, node1.DisableCondition, node1.Disabled, node1.DisabledMessage);
                ANode node2 = tuple.Item2;
                NodeData nodedata2 = new(node2.Name, node2.Id, "", node2.Url, node2.CheckBox, node2.Type.ToString(), node2.Selected, node2.IsComplete(), tree.Root!.TotalStr, node2.Optional, node2.TotalStr, false, node2.Hidden, node2.ExpandedLevels, "", false, node2.Min.ToString(), node2.Max.ToString(), node2.Discount.ToString(), node2.Order.ToString(), node2.Report, node2.ReportValue, node2.Units, node2.DecimalPlaces.ToString(), node2.Parent != null && node2.Parent.Type == NodeType.Decision, node2.Template, node2.HasErrors(), node2.Error, node2.ReadOnly, node2.DisableCondition, node2.Disabled, node2.DisabledMessage);
                nodeDataList.Add(nodedata1);
                nodeDataList.Add(nodedata2);
            }
            else
            {
                ANode node1 = tree.Root!;
                NodeData nodedata1 = new(node1.Name, node1.Id, "", node1.Url, node1.CheckBox, node1.Type.ToString(), node1.Selected, node1.IsComplete(), tree.Root!.TotalStr, node1.Optional, node1.TotalStr, false, node1.Hidden, node1.ExpandedLevels, "", false, node1.Min.ToString(), node1.Max.ToString(), node1.Discount.ToString(), node1.Order.ToString(), node1.Report, node1.ReportValue, node1.Units, node1.DecimalPlaces.ToString(), node1.Parent != null && node1.Parent.Type == NodeType.Decision, node1.Template, node1.HasErrors(), node1.Error, node1.ReadOnly, node1.DisableCondition, node1.Disabled, node1.DisabledMessage);
                nodeDataList.Add(nodedata1);
            }

            array = ObjectToByteArray(tree);
            HttpContext.Session.Set("tree", array);

            string response = JsonConvert.SerializeObject(nodeDataList, Formatting.Indented);
            return Content(response);
        }

        public JsonResult JstreeChildNodes(string id)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode node = tree.GetNodeFromId(id.Replace("ckbx_", ""))!;

            List<NodeDatajsTree> nodes = new();
            if (node?.Children != null)
            {
                foreach (ANode child in node.Children)
                {
                    Attributes attr = new(child.Id);
                    nodes.Add(new NodeDatajsTree(child.Name, attr));
                }
            }

            return Json(nodes);
        }

        public ContentResult NodeInfo(string id)
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);

            ANode node = tree.GetNodeFromId(id.Replace("li_ul_", ""))!;

            string Expression = "";
            bool EditChildren = false;
            switch (node.Type)
            {
                case NodeType.Math:
                    Expression = (node as MathNode)!.Expression;
                    EditChildren = (node as MathNode)!.EditChildren;
                    break;
                case NodeType.Text:
                    Expression = (node as TextNode)!.Expression;
                    EditChildren = (node as TextNode)!.EditChildren;
                    break;
                case NodeType.Date:
                    Expression = (node as DateNode)!.Expression;
                    EditChildren = (node as DateNode)!.EditChildren;
                    break;
                case NodeType.Today:
                    Expression = (node as TodayNode)!.Expression;
                    EditChildren = (node as TodayNode)!.EditChildren;
                    break;
                case NodeType.Conditional:
                    Expression = (node as ConditionalNode)!.Expression;
                    EditChildren = (node as ConditionalNode)!.EditChildren;
                    break;
                case NodeType.SumSet:
                    EditChildren = (node as SumSetNode)!.EditChildren;
                    break;
                case NodeType.Reference:
                    Expression = (node as ReferenceNode)!.Expression;
                    break;
                default:
                    Expression = "";
                    break;
            }
            bool leaf = node.Children == null || node.Children.Count == 0;
            string dep = "";
            foreach (string n in node.Dependents!) dep = dep + n + ";";
            NodeData nodedata = new(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root!.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.MinIsSet ? node.Min.ToString() : "", node.MaxIsSet ? node.Max.ToString() : "", node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.DecimalPlaces.ToString(), node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);

            string response = JsonConvert.SerializeObject(nodedata, Formatting.Indented);
            return Content(response);
        }
        [HttpGet]

        public JsonResult GetAllNames()
        {
            byte[] array = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array);
            ArrayList list = new();
            GetNames(tree.Root!, list);
            return Json(list.ToArray());
        }
        public void GetNames(ANode node, ArrayList list)
        {
            if (node != null)
            {
                if (!list.Contains(node.Name)) list.Add(node.Name);
                if (!list.Contains(node.GetPath())) list.Add(node.GetPath());
                foreach (ANode child in node.Children!)
                    GetNames(child, list);
            }
        }

        [HttpPost]
        public ContentResult NodesInfo(List<string> array)
        {
            byte[] array2 = HttpContext.Session.Get("tree")!;
            QTree tree = ByteArrayToObject(array2);

            List<NodeData> nodeDataList = new();

            foreach (string nodeID in array)
            {
                if (nodeID != string.Empty)
                {
                    ANode? node = tree.GetNodeFromId(nodeID);

                    string Expression;
                    bool EditChildren = false;
                    if (node != null)
                    {
                        switch (node.Type)
                        {
                            case NodeType.Math:
                                Expression = (node as MathNode)!.Expression;
                                EditChildren = (node as MathNode)!.EditChildren;
                                break;
                            case NodeType.Text:
                                Expression = (node as TextNode)!.Expression;
                                EditChildren = (node as TextNode)!.EditChildren;
                                break;
                            case NodeType.Date:
                                Expression = (node as DateNode)!.Expression;
                                EditChildren = (node as DateNode)!.EditChildren;
                                break;
                            case NodeType.Today:
                                Expression = (node as TodayNode)!.Expression;
                                EditChildren = (node as TodayNode)!.EditChildren;
                                break;
                            case NodeType.Conditional:
                                Expression = (node as ConditionalNode)!.Expression;
                                break;
                            case NodeType.Reference:
                                Expression = (node as ReferenceNode)!.Expression;
                                break;
                            default:
                                Expression = "";
                                break;
                        }
                        bool leaf = node.Children == null || node.Children.Count == 0;
                        string dep = node.DependentsStr;
                        NodeData nodedata = new(node.Name, node.Id, Expression, node.Url, node.CheckBox, node.Type.ToString(), node.Selected, node.IsComplete(), tree.Root!.TotalStr, node.Optional, node.TotalStr, leaf, node.Hidden, node.ExpandedLevels, dep, EditChildren, node.Min.ToString(), node.Max.ToString(), node.Discount.ToString(), node.Order.ToString(), node.Report, node.ReportValue, node.Units, node.DecimalPlaces.ToString(), node.Parent != null && node.Parent.Type == NodeType.Decision, node.Template, node.HasErrors(), node.Error, node.ReadOnly, node.DisableCondition, node.Disabled, node.DisabledMessage);
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
        public string decimalPlaces;
        public bool parentIsDecision;
        public bool template;
        public bool hasErrors;
        public string error;
        public bool readOnly;
        public bool disabled;
        public string disableCondition;
        public string disabledMessage;

        public NodeData(string Name, string Id, string Expression, string Url, bool Checkbox, string Type, bool Selected, bool Complete, string Total, bool Optional, string Subtotal, bool isLeaf, bool isHidden, int ExpandedLevels, string Dependents, bool EditChildren, string Min, string Max, string Discount, string Order, bool Report, bool ReportValue, string Units, string DecimalPlaces, bool ParentIsDecision, bool Template, bool HasErrors, string Error, bool ReadOnly, string DisableCondition, bool Disabled, string DisabledMessage)
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
            decimalPlaces = DecimalPlaces;
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
        public Attributes attr;

        public NodeDatajsTree()
        {
            data = "";
            attr = new Attributes();
        }
        public NodeDatajsTree(string Data, Attributes Attributes)
        {
            data = Data;
            attr = Attributes;
        }
    }

    class Attributes
    {
        public string id;
        public Attributes()
        {
            id = "";
        }
        public Attributes(string ID)
        {
            id = ID;
        }
    }
}
