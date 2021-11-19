﻿<%@ Import Namespace="OnlinePriceSystem.Models" %>
<%@ Import Namespace="OnlinePriceSystem.Controllers" %>
<%@ Import Namespace="Pager" %>
<%@ Page Title="Admin" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<PagedList<OnlinePriceSystem.Controllers.ProductUtil>>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    My Products
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <style>
        #overlay {
		    background-color: #aaaaaa;
		    z-index: 999;
		    position: absolute;
		    left: 0;
		    top: 0;
		    width: 100%;
		    height: 100%;
		    display: none;
		    opacity: .3;
		}
    </style>
	<div style="border:1px solid #f6a828; width:100%; display:inline-block; text-align:center; background-color:#C0C0C0;">
     	<label style="display:inline; color:#666666; background-color:#C0C0C0;">My Products</label>
        <div style="position:absolute;right:0; display:inline-block;">
     		<div style="display:inline-block;vertical-align:middle;"> 
	 	     	<img align="left" title='Working...' style='display:none;' class='loading' src='../Images/ajax-bar.gif' alt='Working...'> &nbsp;
     		</div>
		</div>
     </div>
 	<br/>
	<br/>
	<div style="display:inline-block;white-space: nowrap; float:right">
	    <% using (Html.BeginForm("Upload", "MyProducts", FormMethod.Post, new { enctype = "multipart/form-data", style="display:inline-block" }))
	    {%>
	    <label style="display:inline-block; border:0px; font-weight: lighter; background: white;" class="ui-state-default">Upload product</label>
	    <input style="display:inline-block" type="file" name="file" />
	    <input style="display:inline-block" type="submit" value="OK" />
	    <%}%>
	</div>
    <% using (Html.BeginForm(null, null, FormMethod.Get, new { id = "productsform" }))
      {%>
        <input type="button" id="toggle" value="Toggle Active"/>
		<input type="button" id="reload" value="Reload"/>
		<input type="button" id="download" value="Download"/>
		<input type="button" value="Delete"/>
		<input type="button" value="New" onclick="javascript:document.location='../../TreeView/Edit?product=new'"/>	
        <br />
        <br />
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
	    <table id="Products" width="100%">	 	    
    	    <tr>
    		    <th style="font-size:medium; border-style:solid none solid solid;"> Product</th>
    		    <th style="font-size:medium; border-style:solid none solid none;"> Created</th>
    		    <th style="font-size:medium; border-style:solid none solid none;"> Created by</th>
    		    <th style="font-size:medium; border-style:solid none solid none;"> Modified</th>
    		    <th style="font-size:medium; border-style:solid none solid none;"> Modified by</th>
    		    <th style="font-size:medium; border-style:solid none solid none;"> Active</th>
    		    <th style="font-size:medium; border-style:solid solid solid none;"> Size</th>
    	    </tr>
    	    <%foreach (var product in Model.CurrentPageList)
           { %>
    	    <tr>
    		    <td style="padding: 1em 1em 1em 1em; white-space: nowrap; border-style:solid none solid solid;">   			
    			    <input type="checkbox" name="<%=product.name%>" value="<%=Server.MapPath ("~/Products/" + ViewBag.store_name + "/") + product.name%>">
    			    <a href= '<%=ResolveUrl("~/TreeView/Edit")%>?product=<%= product.id%>'> <%= product.name%></a> 
    		    </td>
			    <td style="border-style:solid none solid none;">
    			    <%= (DateTime.Parse(product.created)).ToShortDateString()%>
			    </td>
			    <td style="border-style:solid none solid none;">
    			    <%= product.created_by%>
			    </td>
			    <td style="border-style:solid none solid none;">
    			    <%= (DateTime.Parse(product.modified)).ToShortDateString()%>
			    </td>
			    <td style="border-style:solid none solid none;">
    			    <%= product.modified_by%>
			    </td>
			    <td style="padding: 0; text-align:center; border-style:solid none solid none;">
    			    <% if (product.active.ToString()== "True") {%>
					    <img src='<%=Url.Content("~/")%>Images/check-mark-8-512.png' height='25' width='25'>
    			    <%}%>
			    </td>
			    <td style="border-style:solid solid solid none;">
    			    <%= product.size.ToString()%> B
			    </td>
    	    </tr>
    	    <%}%>
        </table>   
    <%}%>
     
    
<script type="text/javascript">
  	 $(function() {
	    $( "input[type=submit], button" )
	      .button();
        $( "input[type=button], button" )
	      .button();
	  });

	(function ($) {
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
        $("#Products").styleTable();

        //Toggle active
    	$('#toggle').click( function() {
			    $.ajax({
		        url: '/MyProducts/ToggleActive',
		        type: 'GET',
		        cache: false,
		        data: $('#productsform').serialize(),
		        success: function(data) {	 
		            document.location = "/MyProducts/Index/1";
             	}
	    	});
		});

		//Reload products
    	$('#reload').click( function() {
    	        //Show overlay
    	        overlay = $('<div></div>').prependTo('body').attr('id', 'overlay');
    	        $("#overlay").show();
    	        $(".loading").show();
			    $.ajax({
		        url: '/MyProducts/ReloadProducts',
		        type: 'GET',
		        cache: false,
		        data: $('#productsform').serialize(),
		        success: function(data) {	 
		            $(".loading").hide();
		            $("#overlay").remove();
		            document.location = "/MyProducts/Index/1";
             	}
	    	});
    	});
        
        //Download products
    	$('#download').click(function () {
    	    document.location = "/MyProducts/Download?" + $('#productsform').serialize();

    	});
    
    });



</script>

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>

<asp:Content ID="Content4" ContentPlaceHolderID="ScriptsSection" runat="server">
</asp:Content>
