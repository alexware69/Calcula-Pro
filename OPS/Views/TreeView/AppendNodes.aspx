﻿<%@ Import Namespace="QuoteTree"%> 
<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<SumSetNode>" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <meta HTTP-EQUIV="CACHE-CONTROL" CONTENT="NO-CACHE">
    <meta HTTP-EQUIV="PRAGMA" CONTENT="NO-CACHE">
    <script src="../Scripts/iframesize.js"></script>
    <title>Append Nodes</title>
    <script src="../Scripts/jquery-1.7.1.min.js"></script>
 
    <script>

        var iframeids = ["Iframe2"];
        var iframehide = "yes";
        var getFFVersion = navigator.userAgent.substring(navigator.userAgent.indexOf("Firefox")).split("/")[1];
        var FFextraHeight = parseFloat(getFFVersion) >= 0.1 ? 16 : 0;
        function resizeCaller() {
            var dyniframe = new Array();
            for (i = 0; i < iframeids.length; i++) {
                if (document.getElementById) resizeIframe(iframeids[i]);
                if ((document.all || document.getElementById) && iframehide == "no") {
                    var tempobj = document.all ? document.all[iframeids[i]] : document.getElementById(iframeids[i]);
                    tempobj.style.display = "block";
                }
            }
        }
        function resizeIframeByElement(frame) {
            var currentfr = frame;
            if (currentfr) {
                currentfr.style.display = "block";
                if (currentfr.contentDocument && currentfr.contentDocument.body.offsetHeight) currentfr.height = currentfr.contentDocument.body.offsetHeight;
                else if (currentfr.Document && currentfr.Document.body.scrollHeight) currentfr.height = document.getElementById('ctl00_ContentPlaceHolder1_DescriptionFrame').Document.body.scrollHeight + 5;
            }
        }
        function resizeIframe(frameid) {
            var currentfr = document.getElementById(frameid);
            if (currentfr) {
                currentfr.style.display = "block";
                if (currentfr.contentDocument && currentfr.contentDocument.body.offsetHeight) currentfr.height = currentfr.contentDocument.body.offsetHeight + FFextraHeight;
                else if (currentfr.Document && currentfr.Document.body.scrollHeight) currentfr.height = currentfr.Document.body.scrollHeight;
                if (currentfr.addEventListener) currentfr.addEventListener("load", readjustIframe, false);
                else if (currentfr.attachEvent) {
                    currentfr.detachEvent("onload", readjustIframe);
                    currentfr.attachEvent("onload", readjustIframe);
                }
            }
            //var dif = document.body.scrollHeight - window.frameElement.height;
            //window.frameElement.height = document.body.scrollHeight - 22;
        }

        function resizeIframeParent(frameid) {
            var currentfr = window.parent.document.getElementById(frameid);
            if (currentfr) {
                currentfr.style.display = "block";
                if (currentfr.contentDocument && currentfr.contentDocument.body.offsetHeight) currentfr.height = currentfr.contentDocument.body.offsetHeight + FFextraHeight;
                else if (currentfr.Document && currentfr.Document.body.scrollHeight) currentfr.height = currentfr.Document.body.scrollHeight;
                if (currentfr.addEventListener) currentfr.addEventListener("load", readjustIframe, false);
                else if (currentfr.attachEvent) {
                    currentfr.detachEvent("onload", readjustIframe);
                    currentfr.attachEvent("onload", readjustIframe);
                }
            }
            //var dif = document.body.scrollHeight - window.frameElement.height;
            //window.frameElement.height = document.body.scrollHeight - 22;
        }
        function readjustIframe(loadevt) {
            var crossevt = (window.event) ? event : loadevt;
            var iframeroot = (crossevt.currentTarget) ? crossevt.currentTarget : crossevt.srcElement;
            if (iframeroot) resizeIframe(iframeroot.id);
        }
        function loadintoIframe(iframeid, url) {
            if (document.getElementById) document.getElementById(iframeid).src = url;
        }
        if (window.addEventListener) window.addEventListener("load", resizeCaller, false);
        else if (window.attachEvent) window.attachEvent("onload", resizeCaller);
        else window.onload = resizeCaller;
        //resize frame when resizing the window
        if (window.addEventListener) window.addEventListener("resize", resizeCaller, false);
        else if (window.attachEvent) window.attachEvent("onresize", resizeCaller);
        else window.onresize = resizeCaller;


        function maximizeWindow() {
            var offset = (navigator.userAgent.indexOf("Mac") != -1 ||
                          navigator.userAgent.indexOf("Gecko") != -1 ||
                          navigator.appName.indexOf("Netscape") != -1) ? 0 : 4;
            window.moveTo(-offset, -offset);
            window.resizeTo(screen.availWidth + (2 * offset),
                           screen.availHeight + (2 * offset));
        }

</script>
    <style>
        li { background: FloralWhite; }
        li:nth-child(odd) { background: PowderBlue; }
    </style>
</head>
<body onunload="">
<% using (Html.BeginForm("ChangeTreeValue", "TreeView", FormMethod.Get, new { id = "formID" }))
    {%>
        <div style="text-align:center">
            <span style="color:red; font-size:medium; font-style:italic">
                <%=Model.Parent!=null? Model.Parent.Name + " > " + Model.Name : Model.Name %>
            </span>
            <br />
            <br />
            <%if (Model.Disabled){ %>
            <label style="color:red">*Disabled* &nbsp;</label> <label style="font-size:small"> <%=Model.DisabledMessage%></label>
            <br />
            <%} %>
            <table style="text-align:right; margin: auto">
                 <%foreach(ANode node in Model.Children){ %>
                    <% if (node.Template) {%>
                    <tr>
                        <td><input type="button" id="<%=node.Id%>" class="input" onclick="AppendNode(this.id);" value="Add" /></td>
                        <td><%=node.Name%></td>                        
                    </tr> 
                    <%} %>                 
                <%} %>
            </table>
        </div>
    <%} %>
    <br />
    <iframe scrolling="no" style="overflow: hidden; width: 100%; border-width: 0px" id="Iframe2" src="<%= System.IO.File.Exists(Server.MapPath(Url.Content("~/") + ConfigurationManager.AppSettings["productsURLroot"] + "/" + Session["store_name"].ToString() + "/" + Model.GetPath().Replace('\\', '/') + "/homepage.htm"))? Url.Content("~/") + ConfigurationManager.AppSettings["productsURLroot"] + "/" + Session["store_name"].ToString() +  "/" + Model.GetPath().Replace('\\', '/') + "/homepage.htm": Url.Content("~/") +"NoInfo.html"%>"></iframe>

    <div>
        <label onclick="$('#dependents').toggle();resizeIframeParent('Iframe1');" style="color:blue"> <U>DEPENDENTS</U></label>
        <ol class="list_dependents" id="dependents">
            <%foreach (ANode dependent in Model.Dependents) {%>
            <li><a title="<%=dependent.GetPath()%>" onclick="OpenNode('<%=dependent.GetPath().Replace("\\",">")%>'); UnHideBranch('<%=dependent.Id%>');" href="<%=dependent.Url.Contains("ChangeTreeValue")? Url.Content("~/" + dependent.Url):"Description?id=" + dependent.Id%>"><%=dependent.Name%></a><label style="font-size:small" > (<%=dependent.GetPath()%>) </label></li>
            <%} %>
        </ol>
        <br />
    </div>
    <div>
        <label onclick="$('#references').toggle();resizeIframeParent('Iframe1');" style="color:blue"> <U>REFERENCES</U></label>
        <ol class="list_references" id="references">
            <%foreach (ANode reference in Model.References) {%>
            <li><a title="<%=reference.GetPath()%>" onclick="OpenNode('<%=reference.GetPath().Replace("\\",">")%>'); UnHideBranch('<%=reference.Id%>');" href="<%=reference.Url.Contains("ChangeTreeValue")? Url.Content("~/" + reference.Url):"Description?id=" + reference.Id%>"><%=reference.Name%></a><label style="font-size:small" > (<%=reference.GetPath()%>) </label></li>
            <%} %>
        </ol>
        <br />
        <br />
    </div>
    <script>
        $(function () {
            $(".list_dependents li").sort(asc_sort).appendTo('.list_dependents');
            $(".list_references li").sort(asc_sort).appendTo('.list_references');
            $("#dependents").hide();
            $("#references").hide();
            resizeIframeParent('Iframe1');
            $('#formID').keypress(function (e) {
                if (e.which == 13) {
                    e.preventDefault();
                    $('.input').click();
                }
            });
        });
        function AppendNode(source) {
            //Post values to server
            var data = $('#formID').serialize();
            //First open the node
            top.asynchronous = false;
            if ($("ul[id='li_ul_<%=Model.Id%>']").children().length == 0)
                parent.$.jstree._reference("li[id='li_<%=Model.Id%>']").open_node("li[id='li_<%=Model.Id%>']");
            top.asynchronous = true;

            $.ajax({
                url: "AppendNode?sourceId=" + source + "&targetId=<%=Model.Id%>",
                type: 'GET',
                data: data,
                dataType: 'json',
                async: true,
                cache: false,
                beforeSend: function () {
                },
                complete: function () {
                    top.dialog.dialog("close");
                },
                success: function (result) {
                    if (result == "_SessionTimeout_") {
                        parent.document.location = "/SessionTimeOut.html";
                        return false;
                    }
                    //select the node
                    parent.$.jstree._reference("li[id='li_<%=Model.Id%>']").select_node("li[id='li_<%=Model.Id%>']");
                    //Insert the new node
                    parent.$("#container").jstree("create", null, parseInt(result.order), { "data": " ", "attr": { "class": "jstree-closed", "id": "li_" + result.id } }, false, true);
                    //remove the anchor so it will be re-created with UpdateNode()
                    parent.$("li[id='li_" + result.id + "']").children("a").remove();

                    //Set the id for the automatically generated ul
                    if (parent.$("li[id='li_" + result.id + "']").parent().attr("id") == undefined) {
                        parent.$("li[id='li_" + result.id + "']").parent().attr("id", "li_ul_<%=Model.Id%>");
                    }
                    parent.UpdateNode(result);
                    
                    //Update the price in the page
                    top.asynchronous = false;
                    var price = window.parent.document.getElementById("price");
                    $(price).text("Price: " + result.total);
                    if ($("input[id='editchildren_<%=Model.Id%>']", parent.document).attr("value") == "true") {
                        //Open the node
                        parent.$.jstree._reference("li[id='li_<%=Model.Id%>']").open_node("li[id='li_<%=Model.Id%>']");
                        var array = [];
                        var children = $("li[id='li_<%=Model.Id%>']", parent.document).children('ul').children('li');
                        for (var i = 0; i < children.length; i++) {
                            array[i] = children[i].id.replace(/li_/g, "");
                        }
                        parent.UpdateDependents(array);
                    }
                    else {
                        parent.UpdateDependents('<%=Model.Id%>');
                    }
                    top.asynchronous = true;

                },
                error: function (result) {
                    alert("Error updating price from server!");
                }
            });   //end ajax
        }


        // accending sort
        function asc_sort(a, b) {
            return ($(b).text()) < ($(a).text()) ? 1 : -1;
        }

        // decending sort
        function dec_sort(a, b) {
            return ($(b).text()) > ($(a).text()) ? 1 : -1;
        }

        //Receives a node's full path and opens the node
        function OpenNode(node) {
            var split = node.split('>');
            var current = "li[id='li_1']";
            top.asynchronous = false;
            for (var i = 0; i < split.length; i++) {
                //1:Open the node
                parent.$.jstree._reference(current).open_node(current, function () {

                    if (i == split.length - 1) {
                        parent.$.jstree._reference("li[id='li_<%=Model.Id%>']").deselect_all();
                        parent.$.jstree._reference(current).select_node(current);
                        $('html', parent.document).scrollTop($(current, parent.document).offset().top);
                    }
                    else {
                        //2:Get next node
                        var children = $(current, parent.document).children('ul').children('li');
                        for (var j = 0; j < children.length; j++) {
                            if ($(children[j]).children('a').children('span:first').text() == split[i + 1]) {
                                current = "li[id='" + $(children[j]).attr('id') + "']";
                                break;
                            }
                        }
                    }
                });
            }
            top.asynchronous = true;
        }

        function UnHideBranch(id) {
            $("li[id='li_" + id + "']", parent.document).show();
            parent.RefreshFillers(id,false);
            var parentulID = $("li[id='li_" + id + "']", parent.document).parent().attr("id");
            //Recursive call
            if ($("li[id='li_" + id + "']", parent.document).parent().parent().attr("id") != "container")
                UnHideBranch(parentulID.replace(/li_ul_/g, ""));
        }
    </script>
</body>
</html>
