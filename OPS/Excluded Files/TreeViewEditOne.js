﻿/* eslint-disable no-self-assign */
/* eslint-disable no-unused-vars */
/* eslint-disable no-undef */
function CheckBoxClickHandler(cb, expand) {
    let checkboxID = cb.id;
    let checkboxIDClean = checkboxID.replace(/ckbx_/g, "");

    //Using this syntax because there are dots (.) in the id of the element which causes problems with jquery.
    let parentliID = $("input[id='" + checkboxID + "']").parent().attr("id");
    let parentliIDclean = parentliID.replace(/li_/g, "");
    let parentulID = $("li[id='" + parentliID + "']").parent().attr("id");

    // Hide/Show siblings if parent is Decision node
    if ($("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision") {
        if (cb.checked && $("input[id='Compact']").is(':checked')) $("li[id='" + parentliID + "']").siblings().hide()
        else {
            $("li[id='" + parentliID + "']").siblings().show();
            $("li[id='" + parentliID + "']").siblings().each(function() {
                let id = this.id;
                RefreshFillers(id.replace(/li_/g, ""), false);
           });
        }
    }

    asynchronous = false;
    //Open the branch, which will get children from server
    if (cb.checked && $("ul[id='li_ul_" + checkboxIDClean + "']").children().length == 0) {
        $.jstree._reference("li[id='li_" + checkboxIDClean + "']").open_node("li[id='li_" + checkboxIDClean + "']");
    }
    else {
        //Collapse the branch if the checkbox is unchecked and compact mode is selected
        if (!cb.checked && $("input[id='Compact']").is(':checked')) $("#container").jstree("close_node", $("li[id='li_" + checkboxIDClean + "']"));

        //Open the branch if the checkbox is checked but the children are already inserted.
        if (cb.checked) $("#container").jstree("open_node", $("li[id='li_" + checkboxIDClean + "']"));
    }
    asynchronous = true;

    //check all the optional parents
    if (cb.checked) {
        //get parent li        
        let parent = $("li[id='" + parentliID + "']").parent("ul").parent("li");
        //search for the first optional unchecked parent            
        while (parent.attr("id") != "li_1" && !($("input[id='ckbx_" + parent.attr("id").replace(/li_/g, "") + "']").length && !$("input[id='ckbx_" + parent.attr("id").replace(/li_/g, "") + "']").is(':checked'))) {
            parent = parent.parent("ul").parent("li");
        }
        if (parent.attr("id") != "li_1") {
            //instead of calling the click event just set the checkbox as checked and call the handler
            $("input[id='ckbx_" + parent.attr("id").replace(/li_/g, "") + "']").attr('checked', true);
            CheckBoxClickHandler(document.getElementById("ckbx_" + parent.attr("id").replace(/li_/g, "")), false);
        }
    }

    //Set the checked/unchecked state in the server tree and get the total. Also update tree nodes.
    let checkboxstate = cb.checked ? "true" : "false";
    $.ajax({
        url: "SetCheckboxState?id=" + checkboxIDClean + "&state=" + checkboxstate,
        type: 'GET',
        dataType: 'json',
        async: false,
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
        },
        success: function (result) {
            //Update the price in the page
            $("#price").text("Total: " + result.total);

            //Merge dependents and branch nodes then update
            let dependents = ";" + $("input[id='dependents_" + parentliIDclean + "']").attr("value");
            let branch = GetBranch(parentliIDclean);
            let branchArray = branch.split(";");
            for (let i = 0; i < branchArray.length; i++) {
                if (dependents.indexOf(";" + branchArray[i] + ";") == -1) dependents = dependents + ";" + branchArray[i];
            }
            //get not optional descendents, this is to update the complete/incomplete images.
            let descendents = $("li[id='" + parentliID + "']").find("li");
            for (let i = 0; i < descendents.length; i++) {
                dependents += ";" + descendents[i].id.replace(/li_/g, "");
            }
            //}

            UpdateNodesFromServer(dependents);
        }
    });   //end ajax

    //Uncheck node siblings when node is checked and parent is Decision, also update descendents to refresh complete/incomplete images.
    if (cb.checked && $("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision") {
        let checkednode = $("li[id='" + parentliID + "']").siblings().children('input:checked');
        if (checkednode.length != 0) {
            let updatenodes = ";" + checkednode[0].id.replace(/ckbx_/g, "");
            let descendents = $("li[id='li_" + checkednode[0].id.replace(/ckbx_/g, "") + "']").find("li").filter(function () {
                return $("input[id='isoptional_" + $(this).attr("id").replace(/li_/g, "") + "']").attr("value") == "false";
            });
            for (let i = 0; i < descendents.length; i++) {
                updatenodes += ";" + descendents[i].id.replace(/li_/g, "");
            }
            //also update the dependents of the unchecked node
            let dependents = ";" + $("input[id='dependents_" + checkednode[0].id.replace(/ckbx_/g, "") + "']").attr("value");
            let dependentsArray = dependents.split(";");
            for (let i = 0; i < dependentsArray.length; i++) {
                if (updatenodes.indexOf(";" + dependentsArray[i] + ";") == -1) updatenodes = updatenodes + ";" + dependentsArray[i];
            }
            updatenodes += dependents;
            asynchronous = true;
            UpdateNodesFromServer(updatenodes);
        }
    }

    //Show the description page in the description section
    if (cb.checked && !descriptionhidden) {
        $("li[id='li_" + checkboxIDClean + "']").children("a").click();
    }

    //Show the parent's description page in the description section if unchecked
    if (!cb.checked && $("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision" && !descriptionhidden) {
        $("li[id='li_" + parentulID.replace(/li_ul_/g, "") + "']").children("a").click();
    }

    //Expand levels
    if (expand)
        if ($("input[id='" + checkboxID + "']").is(':checked')) {
            asynchronous == "false";
            ExpandLevels2(checkboxIDClean, $("input[id='expandedlevels_" + checkboxIDClean + "']").attr("value"));
        }
}

function UpdateParent(id) {

    $.ajax({
        url: "NodeInfo?id=" + id,
        type: 'GET',
        dataType: 'json',
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
        },
        success: function (result) {
            let parentulID = $("li[id='li_" + result.id + "']").parent().attr("id");

            //Add the image for the complete/incomplete status if parent node is not Decision
            if (!result.complete && !(result.optional && !result.selected)) {
                $("li[id='li_" + result.id + "']").children("img").first().show();
            }
            else $("li[id='li_" + result.id + "']").children("img").first().hide();

            //Update node's text
            UpdateNode(result);

            //Recursive call
            if ($("li[id='li_" + result.id + "']").parent().parent().attr("id") != "container")
                UpdateParent(parentulID.replace(/li_ul_/g, ""));
        }
    });   //end ajax
}

function UpdateParentSync(id) {
    let nodeList = '';

    function GetNodeList(node_id) {
        nodeList += node_id + ";";
        let node = "li[id='li_" + node_id + "']";
        let parent;
        if ($(node).parent().parent().attr("id") != "container") {
            parentULID = $(node).parent().attr("id");
            GetNodeList(parentULID.replace(/li_ul_/g, ""))

        }
    }
    GetNodeList(id);
    UpdateNodesFromServer(nodeList);
}

function GetBranch(id) {
    let nodeList = '';

    function GetNodeList(node_id) {
        nodeList += node_id + ";";
        let node = "li[id='li_" + node_id + "']";
        let parentULID;
        if ($(node).parent().parent().attr("id") != "container") {
            parentULID = $(node).parent().attr("id");
            GetNodeList(parentULID.replace(/li_ul_/g, ""))

        }
    }
    GetNodeList(id);
    return nodeList;
}

function BuildDependencies() {
    //Show overlay
    overlay = $('<div></div>').prependTo('body').attr('id', 'overlay');
    $("#overlay").show();

    $.ajax({
        url: "BuildDependencies",
        type: 'GET',
        dataType: 'json',
        async: true,
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
            $(".loading_dependencies").hide();
            $("#overlay").remove();
        },
        success: function (result) {
            if (result.length == 2) {
                alert("Circular reference between nodes: \"" + result[0].name + "\" and \"" + result[1].name + "\"");
            }
            //Update the price in the page
            price = document.getElementById("price");
            $(price).text("Total: " + result[0].total);
            UpdateTreeSync();
        },
    });   //end ajax
}

function getScrollbarWidth(element) {
    return element.offsetWidth - element.clientWidth;
} 

function UpdateNode(data) {
    let node = "li[id='li_" + data.id + "']";
    if ($(node).length) {
        //next line is needed to avoid fillers problem
        $(node).css("white-space", "normal");
        //Adjust the node width
        $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));
        //Refresh node's elements
        let checked;
        let html;
        //If node is optional add a checkbox to child
        $("input[id='ckbx_" + data.id + "']").remove();
        if (data.optional) {
            checked = data.selected == true ? "checked" : "";
            html = "<input type='checkbox' ";
            if (data.disabled) {
                $(node).siblings().show();
                html += "disabled='true'";
            }
            html += " id='ckbx_" + data.id + "' style='padding:0px 0px 0px 0px; margin:0px 0px 0px 2px;' onchange='CheckBoxClickHandler(this, true);' " + checked + "/>";
            $(node).children("ins").after(html);
        }
        //Add the image for the complete/incomplete status
        html = "<img src='../Images/blinkcircle.gif' class='incomplete' style='display: none;' height='14' width='14'>";
        $(node).children(".incomplete").remove();
        $(node).children("ins").after(html);
        if (!data.complete && !(data.optional && !data.selected)) $(node).children(".incomplete").show();
        else $(node).children(".incomplete").hide();

        if ($(node).children('a').length == 0) {
            $(node).append("<a></a>");
            //Add the node url to the <a> inside the node (this doesnt work but needs to be here) 
            let href = "../" + data.url;
            $(node).children('a').attr("href", href);
            $(node).children('a').attr("target", 'details');

        }
        else {
            //Add the node url to the <a> inside the node (this doesnt work but needs to be here) 
            let href = "../" + data.url;
            $(node).children('a').attr("href", href);
            $(node).children('a').attr("target", 'details');
        }
        
        //Check for dark mode
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            // dark mode
            $(node).children('a').attr("style", "color:gray;");
        }

        //Check for light mode
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches) {
            // light mode
            $(node).children('a').attr("style", "color:black;");
        }

        //Set leaves to red color
        if (data.leaf) $("li[id='li_" + data.id + "']").children('a').attr("style", "color:red;");
        //Set hidden to green
        if (data.hidden) $("li[id='li_" + data.id + "']").children('a').attr("style", "color:green");

        //Remove old
        if ($("input[id='nodetype_" + data.id + "']")) $("input[id='nodetype_" + data.id + "']").remove();
        if ($("input[id='ishidden_" + data.id + "']")) $("input[id='ishidden_" + data.id + "']").remove();
        if ($("input[id='isoptional_" + data.id + "']")) $("input[id='isoptional_" + data.id + "']").remove();
        if ($("input[id='dependents_" + data.id + "']")) $("input[id='dependents_" + data.id + "']").remove();
        if ($("input[id='editchildren_" + data.id + "']")) $("input[id='editchildren_" + data.id + "']").remove();
        if ($("input[id='expandedlevels_" + data.id + "']")) $("input[id='expandedlevels_" + data.id + "']").remove();
        //Add the hidden input storing the node type
        html = "<input type='hidden' id='nodetype_" + data.id + "' value='" + data.type + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);
        //Add the hidden input storing the node visibility
        html = "<input type='hidden' id='ishidden_" + data.id + "' value='" + data.hidden + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);
        //Add the hidden input storing if node is optional
        html = "<input type='hidden' id='isoptional_" + data.id + "' value='" + data.optional + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);
        //Add the hidden input storing the node dependents, separated by ;
        html = "<input type='hidden' id='dependents_" + data.id + "' value='" + data.dependents + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);
        html = "<input type='hidden' id='editchildren_" + data.id + "' value='" + data.editChildren + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);
        //Add the hidden input storing the node expanded levels
        html = "<input type='hidden' id='expandedlevels_" + data.id + "' value='" + data.expandedLevels + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);

        //Remove old
        $(node).children('a').empty();
        $(node).children('a').append("<span class='name' id='name_" + data.id + "'>" + data.name + "</span>");
        $(node).children('a').append("<span class='formula' id='formula_" + data.id + "' > " + (data.type != "Decision" && data.type != 'SumSet' ? " &nbsp;[<i>" + data.expression.trim() + "</i>]" : " &nbsp;&nbsp;&nbsp;") + "</span>");
        if (!$('input[id=\'Formulas\']').is(':checked')) $(node).children('a').children('.formula').hide();
        if ($(node).children(".subtotal")) $(node).children(".subtotal").remove();
        if ($(node).children(".filler")) $(node).children(".filler").remove();
        if ($(node).children(".edit")) $(node).children(".edit").remove();
        if ($(node).children(".add")) $(node).children(".add").remove();
        if ($(node).children(".remove")) $(node).children(".remove").remove();
        let newFiller = $("<div class='filler'>&nbsp;</div>");
        let newSubtotal = $("<span class='subtotal' id='subtotal_" + data.id + "'>" + "<font size='1'>" + data.type + "</font>" + "  " + "<font>" + "[" + data.subtotal + "]" + "</font>" + "</span>");

        $(node).children('a').after(newFiller);
        $(node).children('a').after(newSubtotal);
        $(node).children('a').after("<span title='Remove node' id='remove_" + data.id + "' class='remove' ></span>");
        $(node).children('a').after("<span title='Add node' id='add_" + data.id + "' class='add'></span>");
        $(node).children('a').after("<span title='Edit: " + data.name + "' id='edit_" + data.id + "' class='edit'></span>");

        //Add the node expression as a tooltip
        $(node).children('a').attr("title", data.expression.trim());

        $(node).children('a').next().on("click", function (e) {
            e.preventDefault();
            //select the node
            let current_li = "li[id='li_" + data.id + "']";
            //if description section is not hidden show description page for current node
            if (!descriptionhidden)
                $(current_li).children("a").click();
            //$.jstree._reference(current_li).select_node(current_li);
            editedNodeId = data.id;
            //Fill the Order dropdown list
            $("#inodeOrder").empty();
            for (let i = 0; i <= $(node).siblings().length; i++)
                $("#inodeOrder").append("<option  value='" + i + "'>" + i + "</option>");
            FillNodeDialogInfo(editedNodeId);
            editdialog.dialog('option', 'title', 'Edit: "' + data.name + '"');
            editdialog.dialog("open");
            //Save all node names into a global array
            $.ajax({
                url: "getAllNames",
                type: 'GET',
                dataType: 'json',
                cache: false,
                beforeSend: function () {
                },
                complete: function () {
                },
                success: function (result) {
                    //Reset the array
                    allNames.length = 0;
                    for (let i of result) {
                        allNames.push(i);
                    }
                }
            });
        });
        $(node).children('.remove').on("click", function (e) {
            e.preventDefault();
            removeNodeIds = data.id;
            $("#removeNode").children("label").text("Remove \"" + data.name + "\"?");
            removenodedialog.dialog("open");
        });
        $(node).children('.add').on("click", function (e) {
            e.preventDefault();
            //select the node
            let current_li = "li[id='li_" + data.id + "']";
            //if description section is not hidden show description page for current node
            if (!descriptionhidden)
                $(current_li).children("a").click();
            addToNodeId = data.id;
            if ($("input[id='nodetype_" + data.id + "']").attr("value") == "Decision") {
                $("#newinodeOptional").prop("checked", true);
                $("#newinodeOptional").attr("disabled", "disabled");
            }
            else {
                $("#newinodeOptional").prop("checked", false);
                $("#newinodeOptional").attr("disabled", false);
            }
            //Fill the Order dropdown list
            $("#newinodeOrder").empty();
            for (let i = 0; i <= $(node).children("ul").children("li").length; i++)
                $("#newinodeOrder").append("<option  value='" + i + "'>" + i + "</option>");
            newnodedialog.dialog('option', 'title', 'Insert new node into: "' + data.name + '"');
            newnodedialog.dialog("open");
            //Save all node names into a global array
            $.ajax({
                url: "getAllNames",
                type: 'GET',
                dataType: 'json',
                cache: false,
                beforeSend: function () {
                },
                complete: function () {
                },
                success: function (result) {
                    //Reset the array
                    allNames.length = 0;
                    for (let i of result) {
                        allNames.push(i);
                    }
                }
            });
        });
        //Declare some width related variables
        let idWidth, nodeWidth, anchorWidth, subtotalWidth, insWidth, formulaWidth, imageVisibleWidth, checkboxWidth, editWidth, addWidth, removeWidth, nameWidth;
        checkboxWidth = data.optional ? 13 : 0;
        imageVisibleWidth = !data.complete && !(data.optional && !data.selected) ? 16: 0;
        editWidth = data.type != 'Decision' ? $(node).children(".edit").outerWidth(true) : 0;
        addWidth = $(node).children(".add").outerWidth(true);
        removeWidth = $(node).children(".remove").outerWidth(true);
        let formula = $(node).children("a").children(".formula");
        let name = $(node).children("a").children(".name");
        
        $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));

        nodeWidth = $(node).width();
        anchorWidth = $(node).children('a').width();
        idWidth = 0;
        marginLeft = 0;
        subtotalWidth = $(node).children(".subtotal").width();
        insWidth = $(node).children("ins").width();
        if (formula.css("display") == "none") {
            formula.show();
            formulaWidth = formula.width();
            formula.hide();
        }
        else formulaWidth = formula.width();
        nameWidth = name.width();
        if (formula.children("i").length == 0) formulaWidth = 0;
        
        //Set the formula element's width
        if (anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 > nodeWidth) {
            let extraWidth = anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 - nodeWidth;
            if (formulaWidth >= extraWidth + 25)
                formula.width(formulaWidth - extraWidth - 25);
            else {
                formula.width(0);
                name.width(nameWidth - (extraWidth - formulaWidth));
            }
        }
        else {
            formula.width(formulaWidth + 5);
            name.width(nameWidth + 5);
        }

        let scrollbarWidth = getScrollbarWidth(document.getElementById("container"));

        //Set the filler's width
        $(node).children(".filler").css("width", nodeWidth - anchorWidth - subtotalWidth - insWidth - checkboxWidth - imageVisibleWidth - editWidth - addWidth - removeWidth - 30 - scrollbarWidth);
        if (nodeWidth - anchorWidth - subtotalWidth - insWidth - idWidth - marginLeft - checkboxWidth - imageVisibleWidth - editWidth - addWidth - removeWidth - 30 - scrollbarWidth > 0)
            $(node).children(".filler").css("margin-right", subtotalWidth + scrollbarWidth);

        //This will append the id text if not already there
        if ($("li[id='li_" + data.id + "']").children()[0].tagName != "FONT") {
            let offset = $("li[id='li_1']").position().left;
            $("<font width='100%' size='1' class='id'>" + "&nbsp" + data.id + "</font>").insertBefore("li[id='li_" + data.id + "']>ins");
            $(".id").css({ position: 'absolute', left: offset });
            let id_width = $("font[id='id_" + data.Id + "']").width();
            let ins_left = $(node).children("ins").position().left;
            if (id_width > ins_left) {
                $(node).css("margin-left", id_width - ins_left + 13.5);
            }
            else $(node).css("margin-left", 13.5);
        }

        if (!$('input[id=\'Subtotals\']').is(':checked')) $('.subtotal, .filler').hide();
    }
}

function UpdateNodeFromServer(id) {
    $.ajax({
        url: "NodeInfo?id=" + id,
        type: 'GET',
        dataType: 'json',
        async: false,
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
        },
        success: function (result) {
            //Add the image for the complete/incomplete status if parent node is not Decision
            if (!result.complete && !(result.optional && !result.selected)) {
                $("li[id='li_" + result.id + "']").children("img").first().show();
            }
            else $("li[id='li_" + result.id + "']").children("img").first().hide();

            //Update node's text
            UpdateNode(result);
        }
    });   //end ajax
}

function NodeInfo(id) {
    $.ajax({
        url: "NodeInfo?id=" + id,
        type: 'GET',
        dataType: 'json',
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
        },
        success: function (result) {
            //Add the image for the complete/incomplete status if parent node is not Decision
            if (!result.complete && !(result.optional && !result.selected)) {
                $("li[id='li_" + result.id + "']").children("img").first().show();
            }
            else $("li[id='li_" + result.id + "']").children("img").first().hide();

            //Update node's text
            return (result);
        }
    });   //end ajax
}

function UpdateNodesFromServer(ids) {
    //Convert to array
    let array = [];
    array = ids.split(';');
    $.ajax({
        url: "NodesInfo",
        type: 'POST',
        dataType: 'json',
        data: { array: array },
        traditional: true,
        async: asynchronous == "true" ? true : false,
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
        },
        success: function (result) {
            let showSelectors = '';
            let hideSelectors = '';
            for (let i = 0; i < result.length; i++) {
                //Add the image for the complete/incomplete status if parent node is not Decision
                if (!result[i].complete && !(result[i].optional && !result[i].selected)) {
                    showSelectors += "li[id='li_" + result[i].id + "'],";
                }
                else hideSelectors += "li[id='li_" + result[i].id + "'],";

                //Update node's text
                UpdateNode(result[i]);
            }
        }
    });   //end ajax
}

function UpdateDependents(id) {
    //If is an array of id's
    let dependents = '';
    if (typeof id === 'object') {
        dependents = '';
        let dependentsArray;
        for (let i = 0; i < id.length; i++) {
            dependents += ";" + id[i] + ";" + $("input[id='dependents_" + id[i] + "']").attr("value");
        }
        dependentsArray = dependents.split(';');
        dependentsArray = dependentsArray.filter(function (item, index) {
            return dependentsArray.indexOf(item) === index;
        });
        dependents = '';
        for (let i = 0; i < dependentsArray.length; i++) {
            dependents += dependentsArray[i] + ";";
        }
        UpdateNodesFromServer(dependents);
    }
    else {
        dependents = $("input[id='dependents_" + id + "']").attr("value");
        //Updates the current node and its dependents
        UpdateNodesFromServer(id + ';' + dependents);
    }
}

function UpdateTree(id) {
    $.ajax({
        url: "NodeInfo?id=" + id,
        type: 'GET',
        dataType: 'json',
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
        },
        success: function (result) {
            //Update node's text
            UpdateNode(result);

            //Recursive call
            let node = "li[id='li_" + result.id + "']";
            let children = $(node).children('ul').children('li');
            if (children && children.length > 0)
                for (let i = 0; i < children.length; i++) {
                    UpdateTree(children[i].id.replace(/li_/g, ""));
                }
        }
    });   //end ajax

}

function UpdateTreeSync() {
    let nodeList = '';

    function GetNodeList(id) {
        nodeList += id + ";";
        let node = "li[id='li_" + id + "']";
        let children = $(node).children('ul').children('li');
        if (children && children.length > 0)
            for (let i = 0; i < children.length; i++) {
                GetNodeList(children[i].id.replace(/li_/g, ""));
            }
    }
    GetNodeList(1);
    UpdateNodesFromServer(nodeList);
}

function HideNodes(id) {
    if ($("input[id='ishidden_" + id + "']").attr("value") == "true") $("li[id='li_" + id + "']").hide();
    let children = $("li[id='li_" + id + "']").children('ul').children('li');
    if (children && children.length > 0)
        for (let i = 0; i < children.length; i++) {
            HideNodes(children[i].id.replace(/li_/g, ""));
        }
}

function ShowNodes(id) {
    if ($("input[id='ishidden_" + id + "']").attr("value") == "true") {
        $("li[id='li_" + id + "']").css("display", "block");
        RefreshFillers(id, false);
    }
    let children = $("li[id='li_" + id + "']").children('ul').children('li');
    if (children && children.length > 0)
        for (let i = 0; i < children.length; i++) {
            ShowNodes(children[i].id.replace(/li_/g, ""));
        }
}

//this unhides a node and all its ancesters
function UnHideBranch(id) {
    $("li[id='li_" + id + "']").show();
    let parentulID = $("li[id='li_" + id + "']").parent().attr("id");
    //Recursive call
    if ($("li[id='li_" + id + "']").parent().parent().attr("id") != "container")
        UnHideBranch(parentulID.replace(/li_ul_/g, ""));
}

function ToggleSubtotals() {
    if ($('input[id=\'Subtotals\']').is(':checked')) {
        $('.subtotal').show();
        $('.filler').show();
        RefreshFillers(1, true);
    }
    else {
        $('.subtotal').hide();
        $('.filler').hide();
    }
}

function ToggleFormulas() {
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    if ($('input[id=\'Formulas\']').is(':checked')) {
        $('.formula').show();
        RefreshFillers('1', true);
    }
    else {
        $('.formula').hide();
        RefreshFillers('1', true);
    }
}

function ToggleFormulasAsync() {
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    if ($('input[id=\'Formulas\']').is(':checked')) {
        $('.filler').width(0);
        $('.formula').width(0);
        $('.formula').show();
        UpdateTree('1');
    }
    else {
        $('.formula').hide();
        UpdateTree('1');
    }
}


function ToggleHiddenFields() {
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    if ($('input[id=\'HiddenFields\']').is(':checked')) {
        ShowNodes('1');
    }
    else HideNodes('1');
}

function ToggleDescription() {
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    descriptionhidden = !descriptionhidden;
    $("#description").toggle();
    if ($("#container").css("float") == "right") {
        $("#container").css("float", "left");
        $("#container").css("width", "100%");
    }
    else {
        $("#container").css("float", "right");
        $("#container").css("width", "60%");
    }
    $("#container").css("float") == "left" ? $("#hr").css("width", "100%") : $("#hr").css("width", "59.5%");

    RefreshFillers(1, true);

    //Reload page to fix iframe size if decription is shown
    if (!descriptionhidden) {
        let iframe = document.getElementById('Iframe15');
        let innerDoc = iframe.contentDocument || iframe.contentWindow.document;
        innerDoc.location = innerDoc.location;
    }
}

function ToggleDescriptionAsync() {
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    descriptionhidden = !descriptionhidden;
    $("#description").toggle();
    if ($("#container").css("float") == "right") {
        $("#container").css("float", "left");
        $("#container").css("width", "100%");
    }
    else {
        $("#container").css("float", "right");
        $("#container").css("width", "60%");
    }
    $("#container").css("float") == "left" ? $("#hr").css("width", "100%") : $("#hr").css("width", "59.5%");

    $('.filler').width(0);
    $('.formula').width(0);
    UpdateTree('1');

    //Reload page to fix iframe size if decription is shown
    if (!descriptionhidden) {
        let iframe = document.getElementById('Iframe15');
        let innerDoc = iframe.contentDocument || iframe.contentWindow.document;
        innerDoc.location = innerDoc.location;
    }
}


function ToggleCompact() {
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    if (!$('input[id=\'Compact\']').is(':checked')) {
        let nodes = $("#container li").filter(function () {
            return $(this).css('display') == 'none' && $("input[id='ishidden_" + $(this).attr("id").replace(/li_/g, "") + "']").attr("value") == "false";
        });
        nodes.show();
        for (let i = 0; i < nodes.length; i++) {
            RefreshFillers(nodes[i].id.replace(/li_/g, ""), false);
        }
    }
    else {
        let nodes = $("#container li").filter(function () {
            let parentulID = $(this).parent().attr("id");
            return parentulID != undefined && $("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision" &&
                !($(this).children(':checkbox').is(':checked')) &&
                $(this).siblings().children('input:checked').length == 1;
        });
        nodes.hide();
    }
}

function ToggleId() {

    if ($('input[id=\'Id\']').is(':checked')) {
        ShowIds();
    }
    else HideIds();
}

function ShowIds() {
    $(".id").show();
}
function HideIds() {
    $(".id").hide();
}

//Recursive function to refresh the fillers's width
function EnlargeFillers(id) {
    let node = "li[id='li_" + id + "']";
    $(node).children(".filler").css("width", $(node).children(".filler").width() + $(node).children("a").children(".formula").width());

    let children = $(node).children('ul').children('li');
    if (children && children.length > 0)
        for (let i = 0; i < children.length; i++) {
            EnlargeFillers(children[i].id.replace(/li_/g, ""));
        }
}
//Recursive function to refresh the fillers's width
function ShortenFillers(id) {
    let node = "li[id='li_" + id + "']";
    $(node).children(".filler").css("width", $(node).children(".filler").width() - $(node).children("a").children(".formula").width());

    let children = $(node).children('ul').children('li');
    if (children && children.length > 0)
        for (let i = 0; i < children.length; i++) {
            ShortenFillers(children[i].id.replace(/li_/g, ""));
        }
}

function Refill() {
    RefreshFillers(1, true);
}

function RefreshFillers(id, recursive) {
    let node = "li[id='li_" + id + "']";
    if ($(node).length) {
        //Adjust the node width
        $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));

        let nameText = $(node).children('a').children(".name").text();
        let formulaText = $(node).children('a').children(".formula").children("i").text();
        let subtotalType = $(node).children(".subtotal").children("font").eq(0).text();
        let subtotalText = $(node).children(".subtotal").children("font").eq(1).text();

        //Refresh node's elements
        $(node).children('a').empty();
        $(node).children('a').append("<span class='name' id='name_" + id + "'>" + nameText + "</span>");
        $(node).children('a').append("<span class='formula' id='formula_" + id + "' >" + ($("input[id='nodetype_" + id + "']").attr("value") != "Decision" 
                    && $("input[id='nodetype_" + id + "']").attr("value") != "SumSet" ? " &nbsp;[<i>" + formulaText.trim() + "</i>]" : "") + "</span>");
        if (!$('input[id=\'Formulas\']').is(':checked')) $(node).children('a').children('.formula').hide();
        $(node).children(".subtotal").remove();
        $(node).children(".filler").remove();
        let newFiller = $("<div class='filler'>&nbsp;</div>");
        let newSubtotal = $("<span class='subtotal' id='subtotal_" + id + "'>" + "<font size='1'>" + subtotalType + "</font>" + " " + "<font>" + subtotalText + "</font>" + "</span>");
        $(node).children('a').after(newFiller);
        $(node).children('a').after(newSubtotal);

         //Check for dark mode
         if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            // dark mode
            if ($(node).children('a').css("color") != "rgb(255, 0, 0)" && $(node).children('a').css("color") != "rgb(0, 128, 0)") 
                $(node).children('a').attr("style", "color:gray;");
        }

         //Check for light mode
         if (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches) {
            // dark mode
            if ($(node).children('a').css("color") != "rgb(255, 0, 0)" && $(node).children('a').css("color") != "rgb(0, 128, 0)") 
                $(node).children('a').attr("style", "color:black;");
        }

        //Declare some width related variables
        let nodeWidth, anchorWidth, subtotalWidth, insWidth, formulaWidth, imageVisibleWidth, checkboxWidth, editWidth, addWidth, removeWidth, nameWidth;
        checkboxWidth = $("input[id='ckbx_" + id + "']").length ? 13 : 0;
        imageVisibleWidth = $(node).children("img").first().is(":visible") ? 16 : 0;
        editWidth = $(node).children(".edit").length ? $(node).children(".edit").outerWidth(true) : 0;
        addWidth = $(node).children(".add").length ? $(node).children(".add").outerWidth(true) : 0;
        removeWidth = $(node).children(".remove").length ? $(node).children(".remove").outerWidth(true) : 0;
        let formula = $(node).children("a").children(".formula");
        let name = $(node).children("a").children(".name");

        $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));
        nodeWidth = $(node).width();
        anchorWidth = $(node).children('a').width();
        subtotalWidth = $(node).children(".subtotal").width();
        insWidth = $(node).children("ins").width();
        if (formula.css("display") == "none") {
            formula.show();
            formulaWidth = formula.width();
            formula.hide();
        }
        else formulaWidth = formula.width();
        nameWidth = name.width();

        //Set the formula element's width
        if (anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 > nodeWidth) {
            let extraWidth = anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 - nodeWidth;
            if (formulaWidth >= extraWidth + 25)
                formula.width(formulaWidth - extraWidth - 25);
            else {
                formula.width(0);
                name.width(nameWidth - (extraWidth - formulaWidth));
            }
        }
        else {
            formula.width(formulaWidth + 5);
            name.width(nameWidth + 5);
        }

        let scrollbarWidth =  getScrollbarWidth(document.getElementById("container"));
        $(node).children(".filler").css("width", nodeWidth - anchorWidth - subtotalWidth - insWidth - checkboxWidth - imageVisibleWidth - editWidth - addWidth - removeWidth - 30 - scrollbarWidth);
        $(node).children(".filler").css("margin-right", subtotalWidth + scrollbarWidth);
        //Recursive call
        if (recursive == true) {
            let children = $(node).children('ul').children('li');
            if (children && children.length > 0)
                for (let i = 0; i < children.length; i++) {
                    RefreshFillers(children[i].id.replace(/li_/g, ""), true);
                }
        }
    }

    if (!$('input[id=\'Subtotals\']').is(':checked')) $('.subtotal, .filler').hide();
}

function OpenNode(node) {
    let split = node.split('>');
    let current = "li[id='li_1']";
    for (let i = 0; i < split.length; i++) {
        //1:Open the node
        top.asynchronous = false;
        $.jstree._reference(current).open_node(current, function () {

            if (i == split.length - 1) {
                $.jstree._reference("li[id='li_<%=Model.id%>']").deselect_node("li[id='li_<%=Model.id%>']");
                $.jstree._reference(current).select_node(current);
                $('html').scrollTop($(current).offset().top);
            }
            else {
                //2:Get next node
                let children = $(current).children('ul').children('li');
                for (let j = 0; j < children.length; j++) {
                    if ($(children[j]).children('a').children('font:first').text() == split[i + 1]) {
                        current = "li[id='" + $(children[j]).attr('id') + "']";
                        break;
                    }
                }
            }
        });
    }
    top.asynchronous = true;
}

//Method used to expand the levels accordingly to expandedLevels node properties
function ExpandLevels(node, levels) {
    if (levels != 0) {
        asynchronous = false;
        $.jstree._reference("li[id='li_" + node + "']").open_node("li[id='li_" + node + "']", function () {

            let children = $("li[id='li_" + node + "']").children('ul').children('li');
            if (children && children.length > 0)
                for (let i = 0; i < children.length; i++) {
                    ExpandLevels(children[i].id.replace(/li_/g, ""), levels - 1);
                }

        });
        asynchronous = true;
    }
}
// this one works better with no lag due to sync ajax call
function ExpandLevels2(node, levels) {
    expandingLevels = true;
    if (levels != 0) {
        let children_ul = $("li[id='li_" + node + "']").children("ul");
        if (children_ul.length == 0 || children_ul.children("li").length == 0) {
            $.ajax({
                url: "ChildNodes?id=" + node,
                type: 'GET',
                dataType: "json",
                cache: false,
                beforeSend: function () {

                },
                complete: function (result) {
                    //Expand levels
                    let jsonObject = jQuery.parseJSON(result.responseText);
                    for (let i = 0; i < jsonObject.length; i++) {
                        ExpandLevels2(jsonObject[i].id, levels - 1);
                    }
                },
                success: function (result) {
                    Assemble(result, "li_" + node);
                    //Open the node
                    $("#container").jstree("open_node", $("li[id='li_" + node + "']"));
                }
            });   //end ajax
        }
        else {
            //Open the node
            $("#container").jstree("open_node", $("li[id='li_" + node + "']"));
            RefreshFillers(node, true);
            let children = $("li[id='li_" + node + "']").children('ul').children('li');
            for (let j = 0; j < children.length; j++) {
                ExpandLevels2(children[j].id.replace(/li_/g, ""), levels - 1);
            }
        }
    }
}

function SetUnits() {
    $("#inodeUnitsText").val($("#inodeUnits").val());
    $("#newinodeUnitsText").val($("#newinodeUnits").val());
}

//Fill the edit node dialog info
function FillNodeDialogInfo(id) {
    asynchronous = false;
    $.ajax({
        url: "NodeInfo?id=" + id,
        type: 'GET',
        dataType: 'json',
        async: false,
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
        },
        success: function (result) {
            $("#inodeName").val(result.name);
            $("#inodeType").val(result.type);
            if (result.type == "Decision" || result.type == "SumSet"){
                $("#inodeExpression").attr('disabled', 'disabled');
                $("#inodeExpression").val("");
            }
            else
            if (result.type == "Date" || result.type == "Today"){
                $("#inodeExpression").attr('disabled', 'disabled');
                $("#inodeExpression").val(result.expression);
            }
            else {
                $("#inodeExpression").removeAttr('disabled');
                $("#inodeExpression").val(result.expression);
            }  
            $("#inodeExpandedLevels").val(result.expandedLevels);
            $("#inodeOrder").val(result.order);
            $("#inodeMin").val(result.min);
            $("#inodeMax").val(result.max);
            $("#inodeDiscount").val(result.discount);
            $("#inodeHidden").prop("checked", result.hidden);
            $("#inodeOptional").prop("checked", result.optional);
            $("#inodeDisable").val(result.disableCondition);
            $("#inodeDisabledMessage").val(result.disabledMessage);
            if (result.parentIsDecision) $("#inodeOptional").prop('disabled', true);
            else $("#inodeOptional").prop('disabled', false);
            $("#inodeEditChildren").prop("checked", result.editChildren);
            $("#inodeReport").prop("checked", result.report);
            $("#inodeReportValue").prop("checked", result.reportValue);
            $("#inodeUnits").val(result.units);
            $("#inodeDecimalPlaces").val(result.decimalPlaces);
            $("#inodeUnitsText").val(result.units);
            $("#inodeTemplate").prop("checked", result.template);
            $("#inodeReadOnly").prop("checked", result.readOnly);
        }
    });   //end ajax
    asynchronous = true;
}

function SaveNodeInfo() {
    //Post values to server
    let name = $("#inodeName").val();
    let type = $("#inodeType").val();
    let expression = $("#inodeExpression").val();
    let expandedLevels = $("#inodeExpandedLevels").val();
    let order = $("#inodeOrder").val();
    let min = $("#inodeMin").val();
    let max = $("#inodeMax").val();
    let discount = $("#inodeDiscount").val();
    let hidden = $("#inodeHidden").is(":checked");
    let optional = $("#inodeOptional").is(":checked");
    let disable = $("#inodeDisable").val();
    let disabledMessage = $("#inodeDisabledMessage").val();
    let editChildren = $("#inodeEditChildren").is(":checked");
    let report = $("#inodeReport").is(":checked");
    let reportValue = $("#inodeReportValue").is(":checked");
    let units = $("#inodeUnitsText").val();
    let decimalPlaces = $("#inodeDecimalPlaces").val();
    let template = $("#inodeTemplate").is(":checked");
    let readOnly = $("#inodeReadOnly").is(":checked");

    let specialChars = "*/\\+-|[]?&!()><=#{};:,_";
    let check = function (string) {
        for (i = 0; i < specialChars.length; i++) {
            if (string.indexOf(specialChars[i]) > -1) {
                return true
            }
        }
        return false;
    };

    if (check(name) == false) {
        let queryString =
            "id=" + editedNodeId
            + "&name=" + name
            + "&type=" + type
            + "&expression=" + encodeURIComponent(expression)
            + "&expandedLevels=" + expandedLevels
            + "&order=" + order
            + "&min=" + min
            + "&max=" + max
            + "&discount=" + discount
            + "&hidden=" + hidden
            + "&optional=" + optional
            + "&disable=" + encodeURIComponent(disable)
            + "&disabledMessage=" + encodeURIComponent(disabledMessage)
            + "&editChildren=" + editChildren
            + "&report=" + report
            + "&reportValue=" + reportValue
            + "&units=" + encodeURI(units)
            + "&decimalPlaces=" + decimalPlaces
            + "&template=" + template
            + "&readOnly=" + readOnly
            ;
        let data = $('#inodeName').serialize();
        //Show overlay
        overlay = $('<div></div>').prependTo('body').attr('id', 'overlay');
        $("#overlay").show();
        $(".loading").show();
        //Get total price
        $.ajax({
            url: "SaveNodeInfo?" + queryString,
            type: 'GET',
            data: data,
            dataType: 'json',
            async: false,
            timeout: 120000,
            cache: false,
            beforeSend: function () {
            },
            error: function () {
                alert("Couldn't save node. Either the name is empty or a node with the same name already exists.");
                $(".loading_dependencies").hide();
                $("#overlay").remove();
                editdialog.dialog("close");
            },
            complete: function () {
            },
            success: function (result) {
                //this is needed to refresh the order of nodes in case the order property was modified
                if (editedNodeId != 1) {
                    let parentId = $("li[id='li_" + editedNodeId + "']").parent().attr("id").replace(/li_ul_/g, "");
                    $.jstree._reference("li[id='li_" + parentId + "']").move_node("li[id='li_" + editedNodeId + "']", "li[id='li_" + parentId + "']", parseInt(result.order));
                }
                //Update the price in the page
                price = document.getElementById("price");
                $(price).text("Total: " + result.total);

                if (result.hasErrors) {
                    alert(result.error);
                    UpdateTreeSync();
                    $(".loading_dependencies").hide();
                    $("#overlay").remove();
                }
                else {
                    //Instead of updating just dependents, update the whole tree to refresh dependent list for each node on client.
                    UpdateTreeSync();
                    $(".loading_dependencies").hide();
                    $("#overlay").remove();
                    editdialog.dialog("close");
                }
            },

        });   //end ajax
        top.asynchronous = true;
    }

    else {
        alert('The name contains illegal characters.');
    }
}

function NewNode() {
    //Post values to server
    let name = $("#newinodeName").val();
    let type = $("#newinodeType").val();
    let expression = $("#newinodeExpression").val();
    let expandedLevels = $("#newinodeExpandedLevels").val();
    let order = $("#newinodeOrder").val();
    let min = $("#newinodeMin").val();
    let max = $("#newinodeMax").val();
    let discount = $("#newinodeDiscount").val();
    let hidden = $("#newinodeHidden").is(":checked");
    let optional = $("#newinodeOptional").is(":checked");
    let disable = $("#newinodeDisable").val();
    let disabledMessage = $("#newinodeDisabledMessage").val();
    let editChildren = $("#newinodeEditChildren").is(":checked");
    let report = $("#newinodeReport").is(":checked");
    let reportValue = $("#newinodeReportValue").is(":checked");
    let units = $("#newinodeUnitsText").val();
    let decimalPlaces = $("#newinodeDecimalPlaces").val();
    let template = $("#newinodeTemplate").is(":checked");
    let readOnly = $("#newinodeReadOnly").is(":checked");

    let specialChars = "*/\\+-|[]?&!()><=#{};:,_";
    let check = function (string) {
        for (i = 0; i < specialChars.length; i++) {
            if (string.indexOf(specialChars[i]) > -1) {
                return true
            }
        }
        return false;
    };

    if (check(name) == false) {
        let queryString =
            "id=" + addToNodeId
            + "&name=" + name
            + "&type=" + type
            + "&expression=" + encodeURIComponent(expression)
            + "&expandedLevels=" + expandedLevels
            + "&order=" + order
            + "&min=" + min
            + "&max=" + max
            + "&discount=" + discount
            + "&hidden=" + hidden
            + "&optional=" + optional
            + "&disable=" + encodeURIComponent(disable)
            + "&disabledMessage=" + encodeURIComponent(disabledMessage)
            + "&editChildren=" + editChildren
            + "&report=" + report
            + "&reportValue=" + reportValue
            + "&units=" + encodeURI(units)
            + "&decimalPlaces=" + decimalPlaces
            + "&template=" + template
            + "&readOnly=" + readOnly
            ;
        let data = $('#inodeName').serialize();

        //Show overlay
        overlay = $('<div></div>').prependTo('body').attr('id', 'overlay');
        $("#overlay").show();
        $(".loading").show();
        //Get total price
        $.ajax({
            url: "NewNode?" + queryString,
            type: 'GET',
            data: data,
            dataType: 'json',
            async: true,
            cache: false,
            timeout: 2 * 60 * 60 * 1000, //2 hours
            beforeSend: function () {
            },
            complete: function () {
                UpdateTreeSync();
                $(".loading_dependencies").hide();
                $("#overlay").remove();
                creatingNewNode = false;
            },
            error: function () {
                alert("Couldn't insert the new node.");
                $(".loading_dependencies").hide();
                $("#overlay").remove();
                creatingNewNode = false;
            },
            success: function (result) {
                
                if (result.hasErrors) {
                    alert(result.error);
                }
                else 
                if (result != null){
                    //create new node, if it is the root it is already created so just update
                    creatingNewNode = true;
                    if (addToNodeId != "") {
                        $("#container").jstree("create", null, parseInt(result.order), { "data": " ", "attr": { "class": "jstree-closed", "id": "li_" + result.id } }, false, true);
                        //remove the anchor so it will be re-created with UpdateNode()
                        $("li[id='li_" + result.id + "']").children("a").remove();

                        //Set the id for the automatically generated ul
                        if ($("li[id='li_" + result.id + "']").parent().attr("id") == undefined) {
                            $("li[id='li_" + result.id + "']").parent().attr("id", "li_ul_" + addToNodeId);
                        }
                        UpdateNode(result);
                    }
                    else {
                        $("#container").jstree("create", null, parseInt(result.order), { "data": " ", "attr": { "class": "jstree-closed", "id": "li_" + result.id } }, false, true);
                        UpdateNodeFromServer("1");
                    }
                    // Clean the fields and close
                    $("#newinodeName").val("");
                    $("#newinodeType").val("Math");
                    $("#newinodeExpression").val("");
                    $("#newinodeExpandedLevels").val("");
                    $("#newinodeOrder").val("");
                    $("#newinodeMin").val("");
                    $("#newinodeMax").val("");
                    $("#newinodeDiscount").val("");
                    $("#newinodeHidden").attr('checked', false);
                    $("#newinodeOptional").attr('checked', false);
                    $("#newinodeEditChildren").attr('checked', false);
                    $("#newinodeReport").attr('checked', false);
                    $("#newinodeReportValue").attr('checked', false);
                    $("#newinodeUnits").val("");
                    $("#newinodeDecimalPlaces").val("");
                    newnodedialog.dialog("close");
                }
            }

        });   //end ajax
    }
    else {
        alert('The name contains illegal characters.');
    }
}
function removeNodes() {
    $.ajax({
        url: "removeNodes?ids=" + removeNodeIds,
        type: 'GET',
        async: false,
        dataType: "text",
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
            removenodedialog.dialog("close");
        },
        success: function (result) {
            if (result != ""){
                //Update the price in the page
                price = document.getElementById("price");
                $(price).text("Total: " + result);
                let ids = removeNodeIds.split(";");
                for (let i = 0; i < ids.length; i++) {
                    if (ids[i] != "") {
                        let parentulID = $("li[id='li_" + ids[i] + "']").parent().attr("id");
                        //Show siblings if parent is Decision node
                        if ($("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision")
                            $("li[id='li_" + ids[i] + "']").siblings().show();
                        $("li[id='li_" + ids[i] + "']").remove();
                    }
                }
                UpdateTree(1);
            }
            else alert ("Some nodes could not be removed.");
        }

    });   //end ajax
}

//Assemble the tree
function Assemble(result, id) {
    let li_id = id;
    //Get children from server if not already inserted
    let children_ul = $(id).children("ul");

    //Select the node
    $("#container").jstree("select_node", $("li[id='" + li_id + "']"));
    //Insert children
    for (let i = 0; i < result.length; i++) {
        $("#container").jstree("create", null, "last", { "data": " ", "attr": { "class": "jstree-closed", "id": "li_" + result[i].id } }, false, true);
        //Insert content into anchor
        UpdateNode(result[i]);

        //If hidden then hide
        if (result[i].hidden == true && !($('input[id=\'HiddenFields\']').is(':checked'))) $("li[id='li_" + result[i].id + "']").hide();
        //Add the node url to the <a> inside the node (this doesnt work but needs to be here) 
        let href = result[i].url;

        $("li[id='li_" + result[i].id + "']").children('a').attr("href", href);
        $("li[id='li_" + result[i].id + "']").children('a').attr("target", 'details');

        //Add the node expression as a tooltip
        $("li[id='li_" + result[i].id + "']").children('a').attr("title", result[i].expression);
    }
    //Set the id for the automatically generated ul
    $("li[id='" + li_id + "']").children('ul').attr("id", "li_ul_" + li_id.replace(/li_/g, ""));
    //Deselect the node
    $("#container").jstree("deselect_node", $("li[id='" + li_id + "']"));
}

function RenderTree(tree) {
    //Select parent if is not the root
    let parentId = tree.Id.substr(0, tree.Id.lastIndexOf("."));
    if (tree.Id != 1)
        $("#container").jstree("select_node", $("li[id='" + "li_" + parentId + "']"));
    $("#container").jstree("create", null, "last", { "data": " ", "attr": { "class": "jstree-closed", "id": "li_" + tree.Id } }, false, true);

    //////////////////////////////////////Update node/////////////////////////////////////////////////////////////////////////////////
    let node = "li[id='li_" + tree.Id + "']";
    if ($(node).length) {
        //next line is needed to avoid fillers problem
        $(node).css("white-space", "normal");
        //Adjust the node width
        $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));
        //Refresh node's elements
        let checked;
        let html;
        //If node is optional add a checkbox to child
        $("input[id='ckbx_" + tree.Id + "']").remove();
        if (tree.Optional) {
            checked = tree.Selected == true ? "checked" : "";
            html = "<input type='checkbox' ";
            if (tree.Disabled) {
                $(node).siblings().show();
                html += "disabled='true'";
            }
            html += " id='ckbx_" + tree.Id + "' style='padding:0px 0px 0px 0px; margin:0px 0px 0px 2px;' onchange='CheckBoxClickHandler(this, true);' " + checked + "/>";
            $(node).children("ins").after(html);
        }
        //Add the image for the complete/incomplete status
        html = "<img src='../Images/blinkcircle.gif' class='incomplete' style='display: none;' height='14' width='14'>";
        $(node).children(".incomplete").remove();
        $(node).children("ins").after(html);
        if (!tree.Complete && !(tree.Optional && !tree.Selected)) $(node).children(".incomplete").show();
        else $(node).children(".incomplete").hide();

        let href = "../" + tree.Url;
        $(node).children('a').attr("href", href);
        $(node).children('a').attr("target", 'details');

        //Check for dark mode
         if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            // dark mode
            if ($(node).children('a').css("color") != "rgb(255, 0, 0)" && $(node).children('a').css("color") != "rgb(0, 128, 0)") 
                $(node).children('a').attr("style", "color:gray;");
        }

         //Check for light mode
         if (window.matchMedia && window.matchMedia('(prefers-color-scheme: light)').matches) {
            // dark mode
            if ($(node).children('a').css("color") != "rgb(255, 0, 0)" && $(node).children('a').css("color") != "rgb(0, 128, 0)") 
                $(node).children('a').attr("style", "color:black;");
        }
        //Set leaves to red color
        if (tree.Leaf) $("li[id='li_" + tree.Id + "']").children('a').attr("style", "color:red;");
        //Set hidden to green
        if (tree.Hidden) $("li[id='li_" + tree.Id + "']").children('a').attr("style", "color:green");
        //Add the formula
        let expression = "";
        if (tree.TypeStr != 'Decision' && tree.TypeStr != 'SumSet' && tree.TypeStr != 'Text')
            expression = "&nbsp;[<i>" + tree.Expression + "</i>]";
        else
        if (tree.TypeStr == 'Text')
            expression = "&nbsp;[<i>" + tree.Expression + "</i>]";
        else expression = " &nbsp;&nbsp;&nbsp;";
        //Add the node expression as a tooltip
        $(node).children('a').attr("title", tree.Expression);
        //Add the hidden input storing the node type
        html = "<input type='hidden' id='nodetype_" + tree.Id + "' value='" + tree.TypeStr + "'/> ";
        $("li[id='li_" + tree.Id + "']").append(html);
        //Add the hidden input storing the node visibility
        html = "<input type='hidden' id='ishidden_" + tree.Id + "' value='" + tree.Hidden + "'/> ";
        $("li[id='li_" + tree.Id + "']").append(html);
        //Add the hidden input storing if node is optional
        html = "<input type='hidden' id='isoptional_" + tree.Id + "' value='" + tree.Optional + "'/> ";
        $("li[id='li_" + tree.Id + "']").append(html);
        //Add the hidden input storing the node dependents, separated by ;
        html = "<input type='hidden' id='dependents_" + tree.Id + "' value='" + tree.DependentsStr + "'/> ";
        $("li[id='li_" + tree.Id + "']").append(html);
        html = "<input type='hidden' id='editchildren_" + tree.Id + "' value='" + tree.EditChildren + "'/> ";
        $("li[id='li_" + tree.Id + "']").append(html);
        //Add the hidden input storing the node expanded levels
        html = "<input type='hidden' id='expandedlevels_" + tree.Id + "' value='" + tree.ExpandedLevels + "'/> ";
        $("li[id='li_" + tree.Id + "']").append(html);

        //Remove old
        $(node).children('a').append("<span class='name' id='name_" + tree.Id + "'>" + tree.Name + "</span>");
        $(node).children('a').append("<span class='formula' id='formula_" + tree.Id + "' > "+ expression.trim() + "</span>");
        if (!$('input[id=\'Formulas\']').is(':checked')) $(node).children('a').children('.formula').hide();
        $(node).children(".subtotal").remove();
        $(node).children(".filler").remove();
        $(node).children(".edit").remove();
        $(node).children(".add").remove();
        $(node).children(".remove").remove();
        let newFiller = $("<div class='filler'>&nbsp;</div>");
        let newSubtotal = $("<span class='subtotal' id='subtotal_" + tree.Id + "'>" + "<font size='1'>" + tree.TypeStr + "</font>" + "  " + "<font>" + "[" + tree.TotalStr + "]" + "</font>" + "</span>");

        $(node).children('a').after(newFiller);
        $(node).children('a').after(newSubtotal);
        $(node).children('a').after("<span title='Remove node' id='remove_" + tree.Id + "' class='remove' ></span>");
        $(node).children('a').after("<span title='Add node' id='add_" + tree.Id + "' class='add'></span>");
        $(node).children('a').after("<span title='Edit: " + tree.Name + "' id='edit_" + tree.Id + "' class='edit'></span>");

        $(node).children('a').next().on("click", function (e) {
            e.preventDefault();
            //select the node
            let current_li = "li[id='li_" + tree.Id + "']";
            //if description section is not hidden show description page for current node
            if (!descriptionhidden)
                $(current_li).children("a").click();
            editedNodeId = tree.Id;
            //Fill the Order dropdown list
            $("#inodeOrder").empty();
            for (let i = 0; i <= $(node).siblings().length; i++)
                $("#inodeOrder").append("<option  value='" + i + "'>" + i + "</option>");
            FillNodeDialogInfo(editedNodeId);
            editdialog.dialog('option', 'title', 'Edit: "' + tree.Name + '"');
            editdialog.dialog("open");
            //Save all node names into a global array
            $.ajax({
                url: "getAllNames",
                type: 'GET',
                dataType: 'json',
                cache: false,
                beforeSend: function () {
                },
                complete: function () {
                },
                success: function (result) {
                    //Reset the array
                    allNames.length = 0;
                    for (let i of result) {
                        allNames.push(i);
                    }
                }
            });
        });
        $(node).children('.remove').on("click", function (e) {
            e.preventDefault();
            removeNodeIds = tree.Id;
            $("#removeNode").children("label").text("Remove \"" + tree.Name + "\"?");
            removenodedialog.dialog("open");
        });
        $(node).children('.add').on("click", function (e) {
            e.preventDefault();
            //select the node
            let current_li = "li[id='li_" + tree.Id + "']";
            //if description section is not hidden show description page for current node
            if (!descriptionhidden)
                $(current_li).children("a").click();
            addToNodeId = tree.Id;
            if ($("input[id='nodetype_" + tree.Id + "']").attr("value") == "Decision") {
                $("#newinodeOptional").prop("checked", true);
                $("#newinodeOptional").attr("disabled", "disabled");
            }
            else {
                $("#newinodeOptional").prop("checked", false);
                $("#newinodeOptional").attr("disabled", false);
            }
            //Fill the Order dropdown list
            $("#newinodeOrder").empty();
            for (let i = 0; i <= $(node).children("ul").children("li").length; i++)
                $("#newinodeOrder").append("<option  value='" + i + "'>" + i + "</option>");
            newnodedialog.dialog('option', 'title', 'Insert new node into: "' + tree.Name + '"');
            newnodedialog.dialog("open");
            //Save all node names into a global array
            $.ajax({
                url: "getAllNames",
                type: 'GET',
                dataType: 'json',
                cache: false,
                beforeSend: function () {
                },
                complete: function () {
                },
                success: function (result) {
                    //Reset the array
                    allNames.length = 0;
                    for (let i of result) {
                        allNames.push(i);
                    }
                }
            });
        });
        //Declare some width related variables
        let nodeWidth, anchorWidth, subtotalWidth, insWidth, formulaWidth, imageVisibleWidth, checkboxWidth, editWidth, addWidth, removeWidth, nameWidth;
        checkboxWidth = $("input[id='ckbx_" + tree.Id + "']").length ? 13 : 0;
        imageVisibleWidth = $(node).children("img").first().is(":visible") ? 16 : 0;
        editWidth = tree.TypeStr != 'Decision' ? $(node).children(".edit").outerWidth(true) : 0;
        addWidth = $(node).children(".add").outerWidth(true);
        removeWidth = $(node).children(".remove").outerWidth(true);

        let formula = $(node).children("a").children(".formula");
        let name = $(node).children("a").children(".name");
        //If node is hidden then show it to get correct widths, then hide it again

        $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));
        nodeWidth = $(node).width();
        anchorWidth = $(node).children('a').width();
        subtotalWidth = $(node).children(".subtotal").width();
        insWidth = $(node).children("ins").width();
        if (formula.css("display") == "none") {
            formula.show();
            formulaWidth = formula.width();
            formula.hide();
        }
        else formulaWidth = formula.width();
        nameWidth = name.width();
        if (formula.children("i").length == 0) formulaWidth = 0;

        //Set the formula element's width
        if (anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 > nodeWidth) {
            let extraWidth = anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 - nodeWidth;
            if (formulaWidth >= extraWidth + 25)
                formula.width(formulaWidth - extraWidth - 25);
            else {
                formula.width(0);
                name.width(nameWidth - (extraWidth - formulaWidth));
            }
        }
        else {
            formula.width(formulaWidth + 5);
            name.width(nameWidth + 5);
        }

        let scrollbarWidth = getScrollbarWidth(document.getElementById("container"));
        $(node).children(".filler").css("width", nodeWidth - anchorWidth - subtotalWidth - insWidth - checkboxWidth - imageVisibleWidth - editWidth - addWidth - removeWidth - 30 - scrollbarWidth);
        $(node).children(".filler").css("margin-right", subtotalWidth + scrollbarWidth);

        //This will append the id text (THIS CODE WILL EVENTUALY REPLACE THE CODE ABOVE
        let offset = $("li[id='li_1']").position().left;
        $("<font width='100%' size='1' class='id' id = 'id_" + tree.Id + "'>" + "&nbsp" + tree.Id + "</font>").insertBefore("li[id='li_" + tree.Id + "']>ins");
        $(".id").css({ position: 'absolute', left: offset });
        let id_width = $("font[id='id_" + tree.Id + "']").width();
        let ins_left = $(node).children("ins").position().left;
        if (id_width > ins_left) {
            $(node).css("margin-left", id_width - ins_left + 13.5);
        }
        else {
            $(node).css("margin-left", 13.5);
        }

        if (!$('input[id=\'Subtotals\']').is(':checked')) $('.subtotal, .filler').hide();

        //Set the id for the automatically generated ul
        if ($(node).children("ul").length == 0) {
            $(node).append("<ul id='" + "li_ul_" + tree.Id + "' style='display:inline'>" + "</ul>");
        }
    }
    ///////////////////////////////////////End Updating Node//////////////////////////////////////////////////////////////////////////
    //Recursive call
    if (tree._Children != null) {
        for (let i = 0; i < tree._Children.length; i++) {
            if(tree._Children[i].Id == undefined) continue;
            RenderTree(tree._Children[i]);
        }
    }
    $("#container").jstree("deselect_node", $("li[id='" + "li_" + parentId + "']"));
}

function SaveDefinition(){
    //Show overlay
    overlay = $('<div></div>').prependTo('body').attr('id', 'overlay');
    $("#overlay").show();
    $(".loading").show();

    $.ajax({
        url: "SaveDefinition",
        type: 'GET',
        dataType: "json",
        cache: false,
        beforeSend: function () {

        },
        complete: function () {
            $(".loading_dependencies").hide();
            $("#overlay").remove();
        },
        success: function () {
            
        }
    });   //end ajax
}

//This function was replaced by conditional serialization in the server side
function PruneTree(tree,Root) {
    let countDots = tree.Id.split('.').length - 1;
    if (tree.Id != '1' && countDots == Root.ExpandedLevels) {
        tree._Children = null;
        return;
    }
    else {
        for (let i = 0; i < tree._Children.length; i++){
            PruneTree(tree._Children[i], Root);
        }
    } 
}

// Decompress an LZW-encoded string
function lzw_decode(s) {
    let dict = {};
    let data = (s + "").split("");
    let currChar = data[0];
    let oldPhrase = currChar;
    let out = [currChar];
    let code = 256;
    let phrase;
    for (let i = 1; i < data.length; i++) {
        let currCode = data[i].charCodeAt(0);
        if (currCode < 256) {
            phrase = data[i];
        }
        else {
            phrase = dict[currCode] ? dict[currCode] : (oldPhrase + currChar);
        }
        out.push(phrase);
        currChar = phrase.charAt(0);
        dict[code] = oldPhrase + currChar;
        code++;
        oldPhrase = phrase;
    }
    return out.join("");
} 

function replaceLastOccurrence(inputString, searchText, replacementText) {
    const lastIndex = inputString.lastIndexOf(searchText);
  
    if (lastIndex === -1) {
      // If the searchText is not found in the inputString, return the original string.
      return inputString;
    }
  
    const firstPart = inputString.slice(0, lastIndex);
    const secondPart = inputString.slice(lastIndex + searchText.length);
  
    return firstPart + replacementText + secondPart;
  }

$(function () {
    descriptionhidden = false;
    ajaxDone = false;
    asynchronous = true;
    mobile = false;
    copyNodesId = "";
    treeIsLoaded = false;
    expandingLevels = false;
    creatingNewNode = false;
    allNames = [];
    dialogHeight = window.innerHeight * 98 / 100;
    // TO CREATE AN INSTANCE
    // select the tree container using jQuery
    $("#container")
        // call .jstree with the options object
        .jstree({
            // the plugins array allows you to configure the active plugins on this instance
            "plugins": ["themes", "html_data", "ui", "crrm", "contextmenu"],
            "contextmenu": {
                "items": function ($node) {
                    return {
                        "Edit": {
                            "label": "Edit",
                            "action": function (obj) {
                                nodeId = obj.attr("id").replace(/li_/g, "");
                                $("span[id='edit_" + nodeId + "']").click();
                            }
                        },
                        "Add": {
                            "label": "Add",
                            "action": function (obj) {
                                nodeId = obj.attr("id").replace(/li_/g, "");
                                $("span[id='add_" + nodeId + "']").click();
                            }
                        },
                        "Remove": {
                            "label": "Remove",
                            "action": function (obj) {
                                removeNodeIds = "";
                                let selectedNodes = $("#container").jstree("get_selected");
                                if (selectedNodes.length > 1) {
                                    for (let i = 0; i < selectedNodes.length; i++) {
                                        removeNodeIds += selectedNodes[i].id.replace(/li_/g, "") + ";";
                                    }
                                    $("#removeNode").children("label").text("Remove multiple nodes?");
                                    removenodedialog.dialog("open");
                                }
                                else {
                                    nodeId = obj.attr("id").replace(/li_/g, "");
                                    $("span[id='remove_" + nodeId + "']").click();
                                }
                            }
                        },
                        "Copy": {
                            "label": "Copy",
                            "action": function (obj) {
                                copyNodesId = "";
                                let selectedNodes = $("#container").jstree("get_selected");
                                this.copy(selectedNodes);
                                if (selectedNodes.length <= 1) copyNodesId = obj.attr("id").replace(/li_/g, "");
                                else
                                    for (let i = 0; i < selectedNodes.length; i++) {
                                        copyNodesId += selectedNodes[i].id.replace(/li_/g, "") + ";";
                                    }
                            }
                        },
                        "Paste": {
                            "label": "Paste",
                            "action": function (obj) {
                                let targetNodeId = obj.attr("id").replace(/li_/g, "");
                                //Clone node with id in server side
                                if (copyNodesId != null && copyNodesId != ""){
                                    //Show overlay
                                    overlay = $('<div></div>').prependTo('body').attr('id', 'overlay');
                                    $("#overlay").show();
                                    $(".loading").show();
                                    $.ajax({
                                        url: "cloneNodes?sourceId=" + copyNodesId + "&targetId=" + targetNodeId,
                                        type: 'GET',
                                        async: true,
                                        dataType: 'json',
                                        cache: false,
                                        beforeSend: function () {
                                        },
                                        complete: function () {
                                            $(".loading_dependencies").hide();
                                            $("#overlay").remove();
                                            creatingNewNode = false;
                                        },
                                        error: function () {
                                            alert("An error occurred while copying the node.");
                                            $(".loading_dependencies").hide();
                                            $("#overlay").remove();
                                            creatingNewNode = false;
                                        },
                                        success: function (result) {
                                            for (let i = 0; i < result.length; i++) {
                                                creatingNewNode = true;
                                                $("#container").jstree("create", obj, parseInt(result[i].order), { "data": " ", "attr": { "class": "jstree-closed", "id": "li_" + result[i].id } }, false, true);
                                                //remove the anchor so it will be re-created with UpdateNode()
                                                $("li[id='li_" + result[i].id + "']").children("a").remove();

                                                //Set the id for the automatically generated ul
                                                if ($("li[id='li_" + result[i].id + "']").parent().attr("id") == undefined) {
                                                    $("li[id='li_" + result[i].id + "']").parent().attr("id", "li_ul_" + targetNodeId);
                                                }

                                                UpdateNode(result[i]);
                                            }
                                            UpdateTreeSync();
                                            //deselect all
                                            $("#container").jstree("deselect_all");
                                            //Update the price in the page
                                            price = document.getElementById("price");
                                            $(price).text("Total: " + result[0].total);
                                        },

                                    });   //end ajax
                                }
                            }
                        }
                    };
                }
            },
            "themes": { theme: "default", dots: false, icons: false/*, url: "../Content/themes/jsTree/default/style.css"*/ }
            // each plugin you have included can have its own config object
            // it makes sense to configure a plugin only if overriding the defaults
        })
        // EVENTS
        // each instance triggers its own events - to process those listen on the container
        // all events are in the .jstree namespace
        // so listen for function_name. jstree - you can function names from the docs
        .bind("loaded.jstree", function (event, data) {
            // Open the node levels    
            ExpLevels = 0;
            $.ajax({
                url: "getJson",
                type: 'GET',
                dataType: 'json',
                cache: false,
                beforeSend: function () {
                },
                complete: function () {
                },
                success: function (result) {
                    //ExpLevels = result.expandedLevels;
                    if (result != null) {
                        let jsonObject = JSON.parse(result);
                        PruneTree(jsonObject, jsonObject);
                        RenderTree(jsonObject);
                        //This is to fix bug...not very elegant though
                        RefreshFillers(1, false);
                        treeIsLoaded = true;                      
                    }
                    else {
                        //Create root node
                        addToNodeId = "";
                        //Fill the Order dropdown list
                        $("#newinodeOrder").empty();
                        $("#newinodeOrder").append("<option  value='" + 0 + "'>" + 0 + "</option>");
                        newnodedialog.dialog('option', 'title', 'Insert first node');
                        newnodedialog.dialog("open");
                        //if the user clicks the top right corner x button before inserting the first node redirect to Admin page.
                        newnodedialog.closest('div.ui-dialog')
                            .find('button.ui-dialog-titlebar-close')
                            .click(function (e) {
                                if (!$("#li_1").children("a").length) document.location = "../Home/Index";
                                e.preventDefault();
                            });
                    }
                    //Resize description
                    descriptionOffsetTop = $("#description").offset().top;
                    $("#Iframe15").height($(window).height() - descriptionOffsetTop - 70);
                    $("#container").height($(window).height() - descriptionOffsetTop - 40);
                }
            });   //end ajax
            // you get two params - event & data - check the core docs for a detailed description
        });
    // INSTANCES
    // 1) you can call most functions just by selecting the container and calling .jstree('func','
    //setTimeout(function () { $("#container").jstree("set_focus"); }, 500);
    // with the methods below you can call even private functions (prefixed with `_`)
    // 2) you can get the focused instance using `$.jstree._focused(). 
    //setTimeout(function () { $.jstree._focused().select_node("#li_1"); }, 1000);
    // 3) you can use $.jstree._reference - just pass the container, a node inside it, or a selector
    //setTimeout(function () { $.jstree._reference("#li_1").close_node("#li_1"); }, 1500);
    // 4) when you are working with an event you can use a shortcut
    $("#container").bind("open_node.jstree", function (e, data) {
        let li_id = data.rslt.obj.attr('id');
        //Get children from server if not already inserted
        let children_ul = data.rslt.obj.find('ul');
        //Hide the node if is hidden and the show hidden checkbox is unchecked.....This is needed because when the open node event is called it will unhide children nodes
        if (!$('input[id=\'HiddenFields\']').is(':checked')) HideNodes(li_id.replace(/li_/g, ""), '');
        show_all = document.location.href.search('true') != -1 ? "true" : "false";

        if (children_ul.length == 0 || $(children_ul).children("li").length == 0) {
            //If asynchronous star async ajax, else start sync ajax
            if (asynchronous) {
                $.ajax({
                    url: "ChildNodes?id=" + li_id,
                    type: 'GET',
                    dataType: "json",
                    cache: false,
                    beforeSend: function () {

                    },
                    complete: function (result) {
                    },
                    success: function (result) {
                        Assemble(result, li_id);
                    }
                });   //end ajax
            }
            else {
                $.ajax({
                    url: "ChildNodes?id=" + li_id,
                    type: 'GET',
                    dataType: "json",
                    async: false,
                    cache: false,
                    beforeSend: function () {

                    },
                    complete: function (result) {
                    },
                    success: function (result) {
                        Assemble(result, li_id);
                    }
                });   //end ajax
            }
        }
        else {
            //This code is to fix the wrong node elements dimension when a node is hidden and ToggleFormulas is called
            if (treeIsLoaded && !expandingLevels && !creatingNewNode) {
                RefreshFillers(li_id.replace(/li_/g, ""), true);
            }

        }
    });
    $('#container').bind('select_node.jstree', function (e, data) {
        if (data.args[0].href)
            document.getElementById('Iframe15').src = data.args[0].href;
    });

    //If client is mobile....
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        // some code..
        mobile = true;
        //ToggleDescription();
        //$("#Formulas,#FormulaText, #Description, #DescriptionText").hide();
    }

    function split( val ) {
        return val.split(/[()*/%+-?&!><=:,]+/g);
      }
      function extractLast( term ) {
        return split( term ).pop().trimLeft ().trimRight ();
      }
   
    $( "textarea" )
    // don't navigate away from the field on tab when selecting an item
    .on( "keydown", function( event ) {
        if ( event.keyCode === $.ui.keyCode.TAB &&
            $( this ).autocomplete( "instance" ).menu.active ) {
        event.preventDefault();
        }
    })
    .autocomplete({
        minLength: 0,
        source: function( request, response ) {
        // delegate back to autocomplete, but extract the last term
        response( $.ui.autocomplete.filter(
            allNames, extractLast( request.term ) ) );
        },
        focus: function() {
        // prevent value inserted on focus
        return false;
        },
        select: function( event, ui ) {
        let terms = split( this.value );
        this.value = replaceLastOccurrence(this.value,terms[terms.length -1], '');

        if (ui.item.value[0] != " ") ui.item.value = " " + ui.item.value;
        if (ui.item.value[ui.item.value.length - 1] != " ") ui.item.value = ui.item.value + " ";
        this.value = this.value + ui.item.value;
        return false;
        }
    });

    //Resize fillers on window resize
    $(window).on("resize", function () {
        let isDialogOpen = editdialog.dialog("isOpen");
        let isNewNodeDialogOpen = newnodedialog.dialog("isOpen");
        let isDescriptionDialogOpen = dialog.dialog("isOpen");

        dialogHeight = window.innerHeight * 98 / 100;

        editdialog.dialog("close");
        newnodedialog.dialog("close");
        dialog.dialog("close");

         // code to show the edit formula dialog
        editdialog = $("#inodeInfo").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: "auto",
        height: "auto",
        maxHeight: dialogHeight,
        title: "Edit node",
        close: function () {
            $(".loading_dependencies").hide();
            $("#overlay").remove();
        },
        buttons:
        {
            "Save": function () {
                SaveNodeInfo();
                return false;
            }
        }
        }).css({overflow:"auto"});

         // code to show the edit formula dialog
        newnodedialog = $("#inewnodeInfo").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: "auto",
        height: "auto",
        maxHeight: dialogHeight,
        title: "Insert new node",
        close: function () {
        },
        buttons:
        {
            "Save": function () {
                NewNode();
            }
        }
        }).css({overflow:"auto"});

        // code to show the description dialog
        dialog = $("<div></div>").append(iframe).appendTo("body").dialog({
            autoOpen: false,
            modal: true,
            resizable: false,
            width: "auto",
            height: "auto",
            maxHeight: dialogHeight,
            title: "Description",
            close: function () {
                iframe.attr("src", "about:blank");
            }
        }).css({overflow:"auto"});

        if (isDialogOpen) editdialog.dialog("open");
        if (isNewNodeDialogOpen) newnodedialog.dialog("open");

        //If window width is less than 1024 then hide the description
        if ($(window).width() < "1024" && $("input[id='Description']").is(':checked')) {
            descriptionOffsetTop = $("#description").offset().top;
            $("input[id='Description']").click();
        }
        //If window width is greater than 1024 then show the description
        if ($(window).width() >= "1024" && !$("input[id='Description']").is(':checked')) {
            $("input[id='Description']").click();
            descriptionOffsetTop = $("#description").offset().top;
        }
        else if ($("input[id='Description']").is(':checked')) descriptionOffsetTop = $("#description").offset().top;

        if ($("#description").is(":hidden")) {
            if (isDescriptionDialogOpen) {
                    let src = document.getElementById("Iframe15").contentWindow.location.href;
                    let width = !mobile ? $("#hr").width() * 40 / 100 : "100%";
                    let height = width;
                    iframe.attr({
                        width: +width,
                        height: +height,
                        src: src
                    });
                    dialog.dialog("open");
            }
        }

        Refill();
        //Resize description and tree
        containerOffsetTop = $("#container").offset().top;
        $("#Iframe15").height($(window).height() - descriptionOffsetTop - 70);
        $("#container").height($(window).height() - containerOffsetTop);
    });

    // code to show dialog box
    let iframe = $('<iframe style="border:0;" frameborder="0" marginwidth="0" marginheight="0" allowfullscreen></iframe>');
    dialog = $("<div></div>").append(iframe).appendTo("body").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: "auto",
        height: "auto",
        maxHeight: dialogHeight,
        title: "Description",
        close: function () {
            iframe.attr("src", "about:blank");
        }
    }).css({overflow:"auto"});
    $("#Iframe15").on("load", function (e) {
        if (descriptionhidden) {
            e.preventDefault();
            let src = document.getElementById("Iframe15").contentWindow.location.href;
            let width = !mobile ? $("#hr").width() * 40 / 100 : "100%";
            let height = width;
            iframe.attr({
                width: +width,
                height: +height,
                src: src
            });
            dialog.dialog("open");
        }
    });

    // code to show the edit formula dialog
    editdialog = $("#inodeInfo").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: "auto",
        height: "auto",
        maxHeight: dialogHeight,
        title: "Edit node",
        close: function () {
            $(".loading_dependencies").hide();
            $("#overlay").remove();
        },
        buttons:
        {
            "Save": function () {
                SaveNodeInfo();
                return false;
            }
        }
    }).css({overflow:"auto"});

    // code to show the edit formula dialog
    newnodedialog = $("#inewnodeInfo").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: "auto",
        height: "auto",
        maxHeight: dialogHeight,
        title: "Insert new node",
        close: function () {
        },
        buttons:
        {
            "Save": function () {
                NewNode();
            }
        }
    }).css({overflow:"auto"});

    removenodedialog = $("#removeNode").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: "auto",
        height: "auto",
        close: function () {
        },
        buttons:
        {
            "Remove": function () {
                removeNodes();
            }
        }
    });

    //If window width is less than 1024 then hide the description
    if ($(window).width() < "1024")
        $("input[id='Description']").click();

    //Disable context menu
    document.addEventListener("contextmenu", function(e){
    e.preventDefault();
    }, false);

});