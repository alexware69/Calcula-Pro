<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<dynamic>" %>
<%@ Import Namespace="Pager" %>
<%@ Import Namespace="OnlinePriceSystem.Controllers" %>

<%int OffSet = Model.OffSet;%>
 <% for (int i = Model.CurrentPage + OffSet > Model.TotalPages? Model.CurrentPage - (OffSet*2-(Model.TotalPages - Model.CurrentPage)): Model.CurrentPage - OffSet; i <= Model.CurrentPage + OffSet; i++)
       {%>
       <%if (i > 0 && i <= Model.TotalPages)
         { %>
           <%if (i == Model.CurrentPage)
             { %>
                <strong> <%= Html.ActionLink(i.ToString(), "Index", new { id = i })%> </strong>
           <%} %>
           <%else
           { %>
               <%= Html.ActionLink(i.ToString(), "Index", new { id = i })%> 
           <%} %>
       <%} %>


       <%if (i <= 0) OffSet++; %>
      

    <%} %>