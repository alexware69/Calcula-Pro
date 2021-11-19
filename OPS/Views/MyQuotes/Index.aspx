<%@ Import Namespace="OnlinePriceSystem.Controllers" %>
<%@ Import Namespace="Pager" %>
<%@ Page Title="My Quotes" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<PagedList<QuoteUtil>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    My Quotes
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div style="border:1px solid #f6a828; width:100%; display:inline-block; text-align:center; background-color:#C0C0C0;">
 		<label style="display:inline; color:#666666; background-color:#C0C0C0;">My Quotes</label>
    </div>
    <br/>
    <br/>
     Total pages:&nbsp;<%:Model.TotalPages%>&nbsp;&nbsp;

    <%if (!(Model.CurrentPage - Model.OffSet < 1))
      { %>
      	<%: Html.ActionLink("⇤", "Index", new { id = 1 })%>
        <%: Html.ActionLink("←", "Index", new { id = Model.CurrentPage - Model.OffSet })%>
    <%} %>
    <% Html.RenderPartial("Pages", Model); %>

    <%if (!(Model.CurrentPage + Model.OffSet > Model.TotalPages))
      { %>
        <%: Html.ActionLink("→", "Index", new { id =  Model.CurrentPage + Model.OffSet })%>
        <%: Html.ActionLink("⇥", "Index", new { id = Model.TotalPages })%>
    <%} %>
    <table id="MyQuotes" width="100%">
        <tr>
            <th>Product</th>
            <th>Quote #</th>
            <th>Revision</th>
            <th>Store</th>
            <th>Date</th>
            <th>Total</th>
            <th></th>
        </tr>
    <%foreach ( var quoteUtil in Model.CurrentPageList){ %>
        <tr>
          <td><a href="<%=Url.Content("~/")%>TreeView/LoadQuote?id=<%=quoteUtil.id%>"><%=quoteUtil.product_name%></a></td>
          <td><%=quoteUtil.id%></td>
          <td style="text-align:center;"><%=quoteUtil.revision!= null?quoteUtil.revision.ToString():"--"%></td>
          <td><%=quoteUtil.store%></td>
          <td><%=(DateTime.Parse(quoteUtil.date)).ToShortDateString()%></td>
          <td><%=quoteUtil.total.ToString("C")%></td>
          <td style="padding: 1em 1em 1em 1em; text-align:center;"><img src='../../Images/buy-128.png' height='18' width='18'></td>
        </tr>
    <%} %>
    </table>

    <script>
	(function ($) {
		$(function() {
	    $( "input[type=submit], button" )
	      .button();
        $( "input[type=button], button" )
	      .button();
	  });

	    $.fn.styleTable = function (options) {
	        var defaults = {
	            css: 'ui-styled-table'
	        };
	        options = $.extend(defaults, options);

	        return this.each(function () {
	            $this = $(this);
	            $this.addClass(options.css);

	           
	           	
	            $this.find("th").addClass("ui-state-default");
	            $this.find("td").addClass("ui-widget-content");
	            $this.find("tr:last-child").addClass("last-child");
	        });
	    };
	})(jQuery);

    $(document).ready(function () {
        $("#MyQuotes").styleTable();
    });
    </script>
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptsSection" runat="server">
</asp:Content>
