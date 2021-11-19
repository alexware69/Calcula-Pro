<%@ Import Namespace="QuoteTree" %>

<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ANode>" %>

<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page - My ASP.NET MVC Application
</asp:Content>

<asp:Content ID="indexFeatured" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <script src="../Scripts/TreeView.js"></script>
    <style>
        #description {
            float: left;
            width: 40%;
            background: #d3d3d3;
        }

        #container {
            float: right;
            width: 60%;
            /*background: #c9c;*/
        }
    </style>


    <div id="description" style="text-align: center">
        <div style="height: 15px; vertical-align: central; background-color: #666666; padding: 0px 0px 0px 0px; font-size: xx-small; font-weight: bold; color: white; text-align: center">
            DESCRIPTION
            <img style="float: left; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/top_left_gray.gif" height="15" width="16" />
            <img style="float: right; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/top_right_gray.gif" height="15" width="16" />
        </div>
        <br />

        <div style="margin: 0 auto; text-align: center; width: 90%; background-color: white">
            <img style="float: left; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/top_left_white.gif" height="15" width="16" />
            <img style="float: right; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/top_right_white.gif" height="15" width="16" />
            <iframe scrolling="no" style="overflow: hidden; width: 100%; border-width: 0px; background-color: white;" id="Iframe1" name="details" src="<%=Url.Content("~/")%>Products/homepage.htm"></iframe>
            <div style="text-align: center; background-color: white; width: 100%">
                <img style="float: left; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/bottom_left_white.gif" height="15" width="16" />
                <img style="float: right; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/bottom_right_white.gif" height="15" width="16" />
                <table>
                    <tr>
                        <td style="background-color: white; width: 100%; height: 15px; padding: 0px 0px 0px 0px; border-width: 0px"></td>
                    </tr>
                </table>
            </div>
        </div>
        <span style="font-size: 7pt; color: #999999; padding: 0px 0px 0px 0px; height: 15px; border-width: 0px">PATENT PENDING</span>
        <br />
    </div>
       
    <span id="price" style="float: right">Price: <%=Model.Total().ToString("C")%></span>
    <span id="Span2" style="float: right">&nbsp;</span>
    <a href="QuoteDetails" target="details" style="float: right; color: blue">View Quote Details</a>
    <span id="Span1" style="float: right">&nbsp;</span>
    <%if(Request.QueryString["id"]!=null){ %>
    <a href="LoadTreeViewAModule?id=<%=Request.QueryString["id"].ToString()%>" style="color: blue;float: right">Analysis Module</a>
    <%} %>
    <br />
    <HR size="1" />
    <div id="container">
        <ul id="treeview">
            <li class="jstree-closed" id='<%="li_" + Model.Id %>'>   
                <%if (!Model.IsComplete()) { %>  
                    <img src='../Images/attention.png'>
                <%} %>         
                <%else { %>
                    <img src='../Images/attention.png' style='display: none;'>
                <%} %>
                <a href="<%=Url.Content("~/") + Model.Url%>" onclick="javascript: window.open('<%=Url.Content("~/") + Model.Url %>','details')"><%=Model.Name%></a>
                <input type="hidden" id='<%="nodetype_" + Model.Id %>' value="<%=Model.Type.ToString()%>">
            </li>
        </ul>
    </div>
</asp:Content>
