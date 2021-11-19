<%@ Import Namespace="System.Collections.Generic"%> 
<%@ Import Namespace="OnlinePriceSystem.Controllers"%> 
<%@ Import Namespace="OnlinePriceSystem.Models"%> 
<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<List<StoreProductsUtil>>" %>

<asp:Content ID="indexTitle" ContentPlaceHolderID="TitleContent" runat="server">
    Home Page - Online Price System
</asp:Content>

<asp:Content ID="indexFeatured" ContentPlaceHolderID="FeaturedContent" runat="server">
    <section class="featured">
        <div class="content-wrapper">
            <hgroup class="title">
                <h1 style="padding:10px">Stores</h1>
            </hgroup>
        </div>
    </section>
</asp:Content>

<asp:Content ID="indexContent" ContentPlaceHolderID="MainContent" runat="server">
    <br />
    <ul id="tiles" style="list-style-type: none;">
    <%foreach (StoreProductsUtil spu in Model){ %>
        <%for (int i = 0; i < 5; i ++) { %>
	    <li class="myElements">
		    <div style="width:200px; border: 1.5px solid;border-color:orange;box-shadow: 10px 10px 5px #888888;">
		    	<h3 align="center" class="ui-state-default" style="padding:10px; border-left:0px; border-right:0px"><%=spu.Store.name%></h3>
		    	<br/>
		    	<ul>
			    	<%foreach (ProductUtility prod in spu.Products){ %>
			    	 <li> 
			    	 <a href="<%=Url.Content("~/")%>TreeView/NewQuote?id=<%=prod.id%>"><%=prod.name%></a> 
			    	 </li>  		
			    	 <br/>
			    	 <%} %>
		    	 </ul>
		    </div>
		</li>
        <%} %>
    <%} %>
    </ul>
  
    <script type="text/javascript">
    	//$('#tiles li').wookmark();
    	$('.myElements').wookmark({
		      align: 'left',
		      autoResize: true,
		      comparator: null,
		      container: $('.main-content'),
		      direction: undefined,
		      ignoreInactiveItems: true,
		      itemWidth: "15%",
		      fillEmptySpace: true,
		      flexibleWidth: 0,
		      offset: 90,
		      onLayoutChanged: undefined,
		      outerOffset: 20,
		      possibleFilters: [],
		      resizeDelay: 50,
		      verticalOffset: 20
	    });
    </script>
</asp:Content>
