<%@ Page Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage" %>

<asp:Content ID="aboutTitle" ContentPlaceHolderID="TitleContent" runat="server">
    About - My ASP.NET MVC Application
</asp:Content>

<asp:Content ID="aboutContent" ContentPlaceHolderID="MainContent" runat="server">
    <hgroup class="title">
        <h2>Features of the Online Price System</h2>

    </hgroup>

    <article>
    <br/>
	   <ul>
	    <li> Is web-based.</li>

	    <li>Near instant intelligent updates in web page.</li>
	   
	    <li>Product definitions are independent of product quotes and orders, changes to product definitions won’t affect existing quotes.</li>

	    <li>Product definitions are a hierarchy of folders, which contain the details, formulas, images and descriptions.</li>

	    <li>Folder nature of product definitions makes teamwork easy and natural, as well as making possible to backup and storing definitions offline.</li>

	    <li>Readable formulas are made of words.</li>

	    <li>Supports most common math expressions and logic conditions.</li>

	    <li>Multiple same-name fields are supported (compare to Excel single-name naming of cells)</li>

        <li>Shows subtotals and node values with respective units.</li>

	    <li>Create and modify product definitions via web.</li>

	    <li>Upload and download existing product definitions.</li>

	    <li>Create and analyze price quotes of complex products.</li>

	    <li>Calculations are server-based, which reduces the load on the client computer and keeps formulas and logic safe from viewing and modifying.</li>

        <li>Re-quoting based on the original formulas.</li>

	    <li>Shows dependencies and references.</li>

	    <li>Is fast!</li>
	   </ul>
    </article>

    <aside>
        <h3>Aside Title</h3>
        <p>
            Use this area to provide additional information.
        </p>
        <ul>
            <li><%: Html.ActionLink("Home", "Index", "Home") %></li>
            <li><%: Html.ActionLink("About", "About", "Home") %></li>
            <li><%: Html.ActionLink("Contact", "Contact", "Home") %></li>
        </ul>
    </aside>
</asp:Content>