<%@ Page Title="" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<dynamic>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    ReloadProducts
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="border:1.5px solid #f6a828; width:100%; display:inline-block; text-align:center; background-color:#C0C0C0;">
 		<label style="display:inline; color:#666666; background-color:#C0C0C0;">Reload Products</label>
    </div>
    <br/>
    <br/>
    <% using (Html.BeginForm())
      {%>
        <%for (int i = 0; i < Model.Length; i++) {%>
        <%string[] temp = Model[i].Split("/".ToCharArray()); %>
        <div style="white-space: nowrap;display: inline-block;">
            <input type="checkbox" name="<%=temp[temp.Length - 1]%>" value="<%=Model[i]%>" /><label style="display: inline-block; font:inherit">&nbsp;<%=temp[temp.Length - 1]%></label>
        </div>
        <br />
        <%}%>
        <input type="submit" value="Reload"/>
    <%}%>

    <script>
	  $(function() {
	    $( "input[type=submit], button" )
	      .button();
	  });
  </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptsSection" runat="server">
</asp:Content>
