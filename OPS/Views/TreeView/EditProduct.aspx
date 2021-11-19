﻿<%@ Import Namespace="QuoteTree" %>

<%@ Page Title="Analysis Module" Language="C#" MasterPageFile="~/Views/Shared/Site.Master" Inherits="System.Web.Mvc.ViewPage<ANode>" %>

<asp:Content ID="Content1" ContentPlaceHolderID="TitleContent" runat="server">
    Edit Product
</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="FeaturedContent" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">

 <script src="../Scripts/TreeViewEditOne.js"></script>
    <style>
        html {
            overflow-y: scroll;
        }
        #description {
            float: left;
            width: 40%;
            background: #cccccc;
        }

        #container {
            float: right;
            width: 60%;
            font-family: Verdana;
        }
		.jstree a {
			line-height:18px;
		}
        .filler {
            display: inline-block;
            float:right;  
            clear: both;
            height: 90%;  
            /*background-repeat:repeat-x ;*/
            background:url('../Images/dot.gif') repeat-x  center;
        }
        .filler:hover {
            background:url('../Images/dot-bold.gif') repeat-x  center;
        }
        .build{
            display: inline-block;
            height:20px;
			width:20px;
            cursor: pointer;
            background: url('../Images/link_anchor-128.png') center no-repeat;
			background-size: 16px 16px;
        }
        .build:hover {
            background-color: #f6a828;
		}
		.remove {
			display: inline-block;
            cursor: pointer; 
            margin-left:6px;
			height:12px;
			width:12px;
			background: url('../Images/badge_minus.png') center;
			background-size: 12px 12px;
		}
		.remove:hover {
			display: inline-block;
			height:12px;
			width:12px;
			background: url('../Images/badge_minus_orange.png') center;
			background-size: 12px 12px;
		}
		.add {
			display: inline-block;
            cursor: pointer; 
            margin-left:6px;
			height:12px;
			width:12px;
			background: url('../Images/badge_plus.png') center;
			background-size: 12px 12px;
		}
		.add:hover {
			display: inline-block;
			height:12px;
			width:12px;
			background: url('../Images/badge_plus_orange.png') center;
			background-size: 12px 12px;
		}
		.edit {
			display: inline-block;
            cursor: pointer; 
            margin-left:6px;
			height:12px;
			width:12px;
			background: url('../Images/pencil.png') center;
			background-size: 12px 12px;
		}
		.edit:hover {
			display: inline-block;
			height:12px;
			width:12px;
			background: url('../Images/pencil_orange.png') center;
			background-size: 12px 12px;
		}
        .formula {
            position:relative;
            -o-text-overflow: ellipsis;
            text-overflow: ellipsis; 
            display:inline-block;
            vertical-align:bottom;
            white-space:nowrap;
            overflow:hidden;
            color:blue;

        }
        .name {
            position:relative;
            -o-text-overflow: ellipsis;
            text-overflow: ellipsis; 
            /*display:inline-block;*/
            vertical-align:bottom;
            white-space:nowrap;
            overflow:hidden;
        }
        .subtotal {
            position:absolute;
            right:0;
            color:darkblue;
        }
        label {            
             font-size: 12px;
		 }
		#overlay {
		    background-color: #aaaaaa;
		    z-index: 999;
		    position: fixed;
		    left: 0;
		    top: 0;
		    width: 100%;
		    height: 100%;
		    display: none;
		    opacity: .3;
		}
		#offline_overlay {
		    background-color: #aaaaaa;
		    z-index: 999;
		    position: fixed;
		    left: 0;
		    top: 0;
		    width: 100%;
		    height: 100%;
		    display: none;
		    opacity: .3;
		}
        html, body {margin: 0; height: 100%; overflow: hidden}    
    </style>
    <style>
        .ui-tooltip {
            opacity:1;
            box-shadow: 0 0 0px orange;
            word-wrap: break-word;
            background: #FDEBD0;
        }	 
    </style>
     <div style="border:1px solid #f6a828; height:20px; width:100%; display:inline-block; text-align:center; background-color:#C0C0C0;">
     	<label style="display:inline; color:#666666; background-color:#C0C0C0; font-size: 15px">Product testing area</label>
     	<div style="position:absolute;right:0; display:inline-block;">
     		<div style="display:inline-block;vertical-align:middle;"> 
	 	     	<img align="left" title='Working...' style='display:none;' class='loading' src='../Images/ajax-bar.gif' alt='Working...'>&nbsp;
     		</div>
            <div style="display:inline-block;vertical-align:middle;"> 
	 	     	<span class="build" title='Build dependencies' onclick="javascript: BuildDependencies();"></span>&nbsp;
     		</div>
	        <a href="SaveProduct?id=<%=ViewBag.Id%>" style="color: black; text-decoration: none; display:inline;">Save product</a>
	     	<a href="../MyProducts/Index/1" style="color: black; text-decoration: none; display:inline;">Discard changes</a>
		</div>
     </div>
     <br/>
     <br/>
    <div id="description" class="follow-scroll" style="text-align: center">
        <div style="line-height:15px; height: 15px; vertical-align: middle; background-color: #666666; padding: 0px 0px 0px 0px; font-size:14px; font-weight: bold; color: white; text-align: center;">
    	    <img style="padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/description.jpg" height="15"/>
            <img style="float: left; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/top left gray.jpg" height="15" width="16" />
            <img style="float: right; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/top right gray.jpg" height="15" width="16" />
        </div>
        <div style="margin-top:16px; margin-left: 16px; margin-right:16px;text-align: center; background-color: white; position: relative">
            <div style="text-align: center; background-color: white; width: 100%; position:absolute; top:0">
                <img style="float: left; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/top left white.jpg" height="15" width="16" />
                <img style="float: right; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/top right white.jpg" height="15" width="16" />
            </div>
            <br />
            <%string url = ""; if (Model != null) url = Model.Url.Contains("ChangeTreeValue")? Model.Url : "Description?id=1";%>
            <iframe style="overflow: scroll; width: 100%; border-width: 0px; background-color: white;" id="Iframe15" name="details" src="<%=url%>"></iframe>
            <div style="text-align: center; background-color: white; width: 100%; position:absolute; bottom:0">
                <img style="float: left; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/bottom left white.jpg" height="15" width="16" />
                <img style="float: right; padding: 0px 0px 0px 0px" src="<%=Url.Content("~/")%>Images/bottom right white.jpg" height="15" width="16" />
            </div>
        </div>
        <span style="font-size: 7pt; color: #999999; padding: 0px 0px 0px 0px; height: 15px; border-width: 0px">PATENT PENDING</span>
        <br />
    </div>
     
    <span id="price" style="float: right"> <% if (Model!= null){%> Price: <%=Model.TotalStr%> <%}%></span>
    <span id="space" style="float: right">&nbsp;</span>
    <a href="QuoteDetails" target="details" style="float: right; color: black">Details</a>

    <div style="white-space: nowrap;display: inline-block;color:green;">
        &nbsp; &nbsp;<input type="checkbox" id="HiddenFields" name="HiddenFields" checked="checked" onchange="javascript: ToggleHiddenFields();"><label style="display: inline-block; " onclick="javascript:$('#HiddenFields').click();">&nbsp; Hidden</label>
    </div>
    <div style="white-space: nowrap;display: inline-block;color:blue;">
        &nbsp;<input type="checkbox" id="Formulas" name="Formulas" checked="checked" onchange="javascript: ToggleFormulas();"><label id="FormulaText" style="display: inline-block; " onclick="javascript:$('#Formulas').click();">&nbsp;Formulas</label>
        
    </div>
    <div style="white-space: nowrap;display: none;color:darkblue;">
        &nbsp;<input type="checkbox" id="Subtotals" name="Subtotals" checked="checked" onchange="javascript: ToggleSubtotals();"><label style="display: inline-block; " onclick="javascript:$('#Subtotals').click();">&nbsp;Subtotals</label>
    </div>  
    <div style="white-space: nowrap;display: inline-block">
        &nbsp;<input type="checkbox" id="Description" name="Description" checked="checked" onchange="javascript: ToggleDescription();"><label id="DescriptionText" style="display: inline-block; " onclick="javascript:$('#Description').click();">&nbsp;Description</label>
    </div> 
    <div style="white-space: nowrap;display: inline-block">
        &nbsp;<input type="checkbox" id="Compact" name="Compact" onchange="javascript: ToggleCompact();"><label id="CompactText" style="display: inline-block; " onclick="javascript:$('#Compact').click();">&nbsp;Cmpct</label>
    </div>   
    <div style="white-space: nowrap;display: none">
        &nbsp;<input type="checkbox" id="Id" name="Id" checked="checked" onchange="javascript: ToggleId();"><label id="Id" style="display: inline-block; " onclick="javascript:$('#Id').click();">&nbsp;Id</label>
    </div>
    <div id="hr" style="float:right; width:59.5%"><HR></div>
    <div id="container" style="height:100%; overflow-x:hidden; overflow-y:scroll;margin:0px 0px 0px 0px;position:relative;">
        <ul id="treeview" style="display:block">              
        </ul>
    </div>  
	
    <div id="inodeInfo" style="display:none">
		<table class="table1class" id="inodeInfoTable" style="text-align:right; margin: auto; font-size:small; padding:0; margin:0; border-collapse:collapse;">
			<tr>
				<td><label>Name</label></td>
				<td style="text-align:left;"><input type="text" id="inodeName"></td>
			</tr>
			<tr>
				<td> <label>Type</label></td>
				<td  style="text-align:left;">
					<select id="inodeType" disabled="disabled">
					  <option value="Math">Math</option>
					  <option value="Decision">Decision</option>
					  <option value="Range">Range</option>
					  <option value="SumSet">Sum Set</option>
					  <option value="Conditional">Conditional</option>
					  <option value="ConditionalRules">Conditional Rules</option>
                      <option value="Reference">Reference</option>
                      <option value="Date">Date</option>
                      <option value="Today">Today</option>
					</select>
				</td>
			</tr>
			<tr>
				<td> <label>Units</label></td>
				<td  style="text-align:left;">
					<select id="inodeUnits" onchange="SetUnits();">
                      <option value=""></option>
					  <option value="$">Currency</option>
					  <option value="In">Inches</option>
					  <option value="Sq In">Square Inches</option>
					  <option value="Ft">Feet</option>
					  <option value="Sq Ft">Square Feet</option>
					</select>
                    <input id="inodeUnitsText" type="text" />
				</td>
			</tr>
			<tr>
				<td><label>Expression</label></td>
				<td  style="text-align:left;"><textarea rows="2" id="inodeExpression"></textarea></td>
			</tr>
			<tr>
				<td><label>Expanded Levels</label></td>
				<td  style="text-align:left;"><input type="text" id="inodeExpandedLevels"></td>
			</tr>
            <tr>
				<td> <label>Order</label></td>
				<td  style="text-align:left;">
					<select id="inodeOrder">
					</select>
				</td>
			</tr>
			<tr>
				<td><label>Min</label></td>
				<td  style="text-align:left;"><input type="text" id="inodeMin"></td>
			</tr>
			<tr>
				<td><label>Max</label></td>
				<td  style="text-align:left;"><input type="text" id="inodeMax"></td>
			</tr>
			<tr>
				<td><label>Discount (%)</label></td>
				<td  style="text-align:left;"><input type="text" id="inodeDiscount"></td>
			</tr>
            <tr>
				<td><label>Disable Condition</label></td>
                <td  style="text-align:left;"><textarea rows="2" id="inodeDisable"></textarea></td>
			</tr>
            <tr>
				<td><label>Disabled Message</label></td>
                <td  style="text-align:left;"><textarea rows="2" id="inodeDisabledMessage"></textarea></td>
			</tr>
			<tr>
				<td style="text-align:center; border:solid thin; border-color:#C0C0C0;" colspan="2">
                        &nbsp;&nbsp;
                        <label style="display:inline">Optional &nbsp;</label><input style="display:inline" type="checkbox" id="inodeOptional">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Hidden &nbsp;</label><input style="display:inline" type="checkbox" id="inodeHidden">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Edit Children &nbsp;</label><input style="display:inline" type="checkbox" id="inodeEditChildren">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Report &nbsp;</label><input style="display:inline" type="checkbox" id="inodeReport">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Report Value &nbsp;</label><input style="display:inline" type="checkbox" id="inodeReportValue">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Template &nbsp;</label><input style="display:inline" type="checkbox" id="inodeTemplate">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Read Only &nbsp;</label><input style="display:inline" type="checkbox" id="inodeReadOnly">
				</td>
			</tr>
		</table>
    </div>
    <div id="inewnodeInfo" style="display:none">
		<table class="table2class" style="text-align:right; margin: auto">
			<tr>
				<td><label>Name</label></td>
				<td  style="text-align:left;"><input type="text" id="newinodeName"></td>
			</tr>
			<tr>
				<td> <label>Type</label></td>
				<td  style="text-align:left;">
					<select id="newinodeType">
					  <option value="Math">Math</option>
					  <option value="Decision">Decision</option>
					  <option value="Range">Range</option>
					  <option value="SumSet">Sum Set</option>
					  <option value="Conditional">Conditional</option>
					  <option value="ConditionalRules">Conditional Rules</option>
                      <option value="Reference">Reference</option>
                      <option value="Date">Date</option>
                      <option value="Today">Today</option>
					</select>
				</td>
			</tr>
			<tr>
				<td> <label>Units</label></td>
				<td  style="text-align:left;">
					<select id="newinodeUnits" onchange="SetUnits();">
                      <option value=""></option>
					  <option value="$">Currency</option>
					  <option value="In">Inches</option>
					  <option value="Sq In">Square Inches</option>
					  <option value="Ft">Feet</option>
					  <option value="Sq Ft">Square Feet</option>
					</select>
                    <input id="newinodeUnitsText" type="text" />
				</td>
			</tr>
			<tr>

			<tr>
				<td><label>Expression</label></td>
				<td  style="text-align:left;"><textarea rows="2" id="newinodeExpression"></textarea></td>
			</tr>
			<tr>
				<td><label>Expanded Levels</label></td>
				<td  style="text-align:left;"><input type="text" id="newinodeExpandedLevels"></td>
			</tr>
             <tr>
				<td> <label>Order</label></td>
				<td  style="text-align:left;">
					<select id="newinodeOrder">
					</select>
				</td>
			</tr>
			<tr>
				<td><label>Min</label></td>
				<td  style="text-align:left;"><input type="text" id="newinodeMin"></td>
			</tr>
			<tr>
				<td><label>Max</label></td>
				<td  style="text-align:left;"><input type="text" id="newinodeMax"></td>
			</tr>
			<tr>
				<td><label>Discount (%)</label></td>
				<td  style="text-align:left;"><input type="text" id="newinodeDiscount"></td>
			</tr>
            <tr>
				<td><label>Disable Condition</label></td>
                <td  style="text-align:left;"><textarea rows="2" id="newinodeDisable"></textarea></td>
			</tr>
            <tr>
				<td><label>Disabled Message</label></td>
                <td  style="text-align:left;"><textarea rows="2" id="newinodeDisabledMessage"></textarea></td>
			</tr>
            <tr>
                <td style="text-align:center; border:solid thin; border-color:#C0C0C0;" colspan="2">
                        &nbsp;&nbsp;
                        <label style="display:inline">Optional &nbsp;</label><input style="display:inline" type="checkbox" id="newinodeOptional">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Hidden &nbsp;</label><input style="display:inline" type="checkbox" id="newinodeHidden">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Edit Children &nbsp;</label><input style="display:inline" type="checkbox" id="newinodeEditChildren">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Report &nbsp;</label><input style="display:inline" type="checkbox" id="newinodeReport">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Report Value &nbsp;</label><input style="display:inline" type="checkbox" id="newinodeReportValue">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Template &nbsp;</label><input style="display:inline" type="checkbox" id="newinodeTemplate">&nbsp;&nbsp;&nbsp;
                        <label style="display:inline">Read Only &nbsp;</label><input style="display:inline" type="checkbox" id="newinodeReadOnly">
				</td>
            </tr>
		</table>
    </div>
     <div id="removeNode" style="display:none">
     	<label>Delete node?</label>
     </div>

     <script>
     	$(function(){
     		Offline.on('down', function(){
     			$('#container *').prop('disabled',true).css( 'pointer-events', 'none' );
     		});
			Offline.on('up', function(){
				$('#container *').prop('disabled',false).css( 'pointer-events', 'auto' );
			});   		
     	});
     </script>
</asp:Content>
