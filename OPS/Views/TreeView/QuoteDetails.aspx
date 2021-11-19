<%@ Import Namespace="QuoteTree"%> 
<%@ Import Namespace="System.Collections.Generic"%> 
<%@ Page Language="C#" Inherits="System.Web.Mvc.ViewPage<Dictionary<string,string>>" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <meta name="viewport" content="width=device-width" />
    <title>QuoteDetails</title>
    <script src="../Scripts/jquery-1.7.1.min.js"></script>
    <script>
        <%--$().ready(function () {
            //$("#Content").html("<%=ViewData["html"].ToString()%>");
        });--%>
        
        function Save() {
            window.parent.window.location = "SaveQuote";
        }
    </script>
</head>
<body>
    <%if (ViewData["error"] != null && ViewData["error"] != ""){ %>
    <label><%=ViewData["error"]%></label>
    <br />
    <br />
    <%} else {%>
    <div style="text-align:center">
        <input type="submit" value="Save Quote" onclick="Save()" <%= (!(Session["tree"] as QTree).Root.IsComplete()) || (Session["tree"] as QTree).Root.TotalStr == "error" ? "disabled": ""%>/>
    </div>
    <br />
    <div id="Content" style="text-wrap:normal; overflow-wrap:break-word; width:100%">
        <table style="width:100%; border-collapse:collapse;">
            <%
            foreach (KeyValuePair<string,string> entry in Model)
            {%>
                <tr>
                    <td style="padding-bottom:5px; margin:0px; padding-top:5px; border-bottom:dotted thin; border-top:dotted thin"><%=entry.Key%>.</td>
                    <td style="padding-bottom:5px; margin:0px; padding-top:5px; border-bottom:solid thin;border-top:solid thin; border-left:dotted thin">
                        <%foreach (char c in entry.Value){
                                if (c == ' ')
                                { %>
                                &#8202;
                                <%}
                                else break;
                            %>
                        <%} %>
                        <%=entry.Value%>
                    </td>
                </tr>
            <%}%>            
        </table>
        <br />
        <br />
    </div>    
    <%} %>
</body>
</html>
