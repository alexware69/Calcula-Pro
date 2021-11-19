<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    ReloadProducts
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

	<div style="border:1.5px solid #f6a828; width:100%; display:inline-block; text-align:center; background-color:#C0C0C0;">
 		<label style="display:inline; color:#666666; background-color:#C0C0C0;">Edit Products</label>
    </div>
    <br/>
    <br/>
    <ul>
    <%for (int i = 0; i < Model.Length; i++) {%>
    <%string[] temp = Model[i].Split("/".ToCharArray()); %>
    <li>
    <div style="white-space: nowrap;display: inline-block;">
		<a href= '<%=ResolveUrl("~/TreeView/TestTreeViewAModule")%>?product=<%=temp[temp.Length - 1]%>'> <%=temp[temp.Length - 1]%></a> 
    </div>
    </li>
    <br />
    <%}%>
    </ul>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptsSection" runat="server">
</asp:Content>
