function CheckBoxClickHandler(cb, expand) {
    var checkboxID = cb.id;
    var checkboxIDClean = checkboxID.replace(/ckbx_/g, "");

    //Using this syntax because there are dots (.) in the id of the element which causes problems with jquery.
    var parentliID = $("input[id='" + checkboxID + "']").parent().attr("id");
    var parentliIDclean = parentliID.replace(/li_/g, "");
    var parentulID = $("li[id='" + parentliID + "']").parent().attr("id");

    // Hide/Show siblings if parent is Decision node
    if ($("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision") {
        if (cb.checked && $("input[id='Compact']").is(':checked')) $("li[id='" + parentliID + "']").siblings().hide()
        else $("li[id='" + parentliID + "']").siblings().show();
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
        var parent = $("li[id='" + parentliID + "']").parent("ul").parent("li");
        //search for the first optional unchecked parent            
        while (parent.attr("id") != "li_1" && !($("input[id='ckbx_" + parent.attr("id").replace(/li_/g, "") + "']").length && !$("input[id='ckbx_" + parent.attr("id").replace(/li_/g, "") + "']").is(':checked'))) {
            parent = parent.parent("ul").parent("li");
        }
        if (parent.attr("id") != "li_1") {
            //$("input[id='ckbx_" + parent.attr("id").replace(/li_/g, "") + "']").click();
            //instead of calling the click event just set the checkbox as checked and call the handler
            $("input[id='ckbx_" + parent.attr("id").replace(/li_/g, "") + "']").attr('checked', true);
            CheckBoxClickHandler(document.getElementById("ckbx_" + parent.attr("id").replace(/li_/g, "")), false);
        }
    }

    //Show the description page in the description section
    if (cb.checked && !descriptionhidden) {
        $("li[id='li_" + checkboxIDClean + "']").children("a").click();
    }

    //Show the parent's description page in the description section if unchecked
    if (!cb.checked && $("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision" && !descriptionhidden) {
        //href = $("li[id='li_" + parentulID.replace(/li_ul_/g, "") + "']").children("a").attr("href");
        //window.open(href, 'details');
        $("li[id='li_" + parentulID.replace(/li_ul_/g, "") + "']").children("a").click();
    }

    //Set the checked/unchecked state in the server tree and get the total. Also update tree nodes.
    var checkboxstate = cb.checked ? "true" : "false";
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
            if (result == "_SessionTimeout_") {
                document.location = "../SessionTimeOut.html";
                return false;
            }
            //Update the price in the page
            $("#price").text("Price: " + result.total);

            //this will update the branch, needed to update the complete/incomplete images
            //UpdateParentSync(parentliIDclean);
            //This will updated current node and dependents
            //UpdateDependents(parentliIDclean);

            //Merge dependents and branch nodes then update
            var dependents = ";" + $("input[id='dependents_" + parentliIDclean + "']").attr("value");
            var branch = GetBranch(parentliIDclean);
            var branchArray = branch.split(";");
            for (var i = 0; i < branchArray.length; i++) {
                if (dependents.indexOf(";" + branchArray[i] + ";") == -1) dependents = dependents + ";" + branchArray[i];
            }
            //get not optional descendents, this is to update the complete/incomplete images.
            //if ($("input[id='nodetype_" + parentliID.replace(/li_/g, "") + "']").attr("value") != "Decision") {
            var descendents = $("li[id='" + parentliID + "']").find("li");
            for (var i = 0; i < descendents.length; i++) {
                dependents += ";" + descendents[i].id.replace(/li_/g, "");
            }
            //}

            UpdateNodesFromServer(dependents);
            //UpdateTreeSync();

            //UpdateTreeSync();
        }
    });   //end ajax

    //Uncheck node siblings when node is checked and parent is Decision, also update descendents to refresh complete/incomplete images.
    if (cb.checked && $("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision") {
        var checkednode = $("li[id='" + parentliID + "']").siblings().children('input:checked');
        if (checkednode.length != 0) {
            var updatenodes = ";" + checkednode[0].id.replace(/ckbx_/g, "");
            //UpdateNodeFromServer(checkednode[0].id.replace(/ckbx_/g, ""));
            var descendents = $("li[id='li_" + checkednode[0].id.replace(/ckbx_/g, "") + "']").find("li").filter(function () {
                return $("input[id='isoptional_" + $(this).attr("id").replace(/li_/g, "") + "']").attr("value") == "false";
            });
            for (var i = 0; i < descendents.length; i++) {
                updatenodes += ";" + descendents[i].id.replace(/li_/g, "");
            }
            //also update the dependents of the unchecked node
            var dependents = ";" + $("input[id='dependents_" + checkednode[0].id.replace(/ckbx_/g, "") + "']").attr("value");
            var dependentsArray = dependents.split(";");
            for (var i = 0; i < dependentsArray.length; i++) {
                if (updatenodes.indexOf(";" + dependentsArray[i] + ";") == -1) updatenodes = updatenodes + ";" + dependentsArray[i];
            }
            updatenodes += dependents;
            asynchronous = true;
            UpdateNodesFromServer(updatenodes);
            //UpdateTreeSync();
        }
        //$("li[id='" + parentliID + "']").siblings().children(':checkbox').attr('checked', false);
    }

    //Expand levels
    if (expand)
        if ($("input[id='" + checkboxID + "']").is(':checked')) {
            asynchronous == "false";
            ExpandLevels2(checkboxIDClean, $("input[id='expandedlevels_" + checkboxIDClean + "']").attr("value"));
            //expandingLevels = false;
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
            if (result == "_SessionTimeout_") {
                document.location = "../SessionTimeOut.html";
                return false;
            }
            var parentulID = $("li[id='li_" + result.id + "']").parent().attr("id");

            //Add the image for the complete/incomplete status if parent node is not Decision
            if (!result.complete && !(result.optional && !result.selected)) {
                $("li[id='li_" + result.id + "']").children("img").first().show();
            }
            else $("li[id='li_" + result.id + "']").children("img").first().hide();

            //If node is checked and complete and parent is Decision then hide parent's image
            //if (cb.checked && result.complete && $("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision") {
            //    $("li[id='li_" + parentulID.replace(/li_ul_/g, "") + "']").children("img").hide();
            //}

            //Update node's text
            UpdateNode(result);

            //Recursive call
            if ($("li[id='li_" + result.id + "']").parent().parent().attr("id") != "container")
                UpdateParent(parentulID.replace(/li_ul_/g, ""));
        }
    });   //end ajax
}

function UpdateParentSync(id) {
    var nodeList = '';

    function GetNodeList(node_id) {
        nodeList += node_id + ";";
        var node = "li[id='li_" + node_id + "']";
        var parent;
        if ($(node).parent().parent().attr("id") != "container") {
            parentULID = $(node).parent().attr("id");
            GetNodeList(parentULID.replace(/li_ul_/g, ""))

        }
    }
    GetNodeList(id);
    UpdateNodesFromServer(nodeList);
}

function GetBranch(id) {
    var nodeList = '';

    function GetNodeList(node_id) {
        nodeList += node_id + ";";
        var node = "li[id='li_" + node_id + "']";
        var parentULID;
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
    $(".loading").show();

    $.ajax({
        url: "BuildDependencies",
        type: 'GET',
        dataType: 'json',
        async: true,
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
            $(".loading").hide();
            $("#overlay").remove();
        },
        success: function (result) {
            if (result == "_SessionTimeout_") {
                document.location = "../SessionTimeOut.html";
                return false;
            }
            if (result.length == 2) {
                alert("Circular reference between nodes: \"" + result[0].name + "\" and \"" + result[1].name + "\"");
            }
            //Update the price in the page
            price = document.getElementById("price");
            $(price).text("Price: " + result[0].total);
            UpdateTreeSync();
        },
    });   //end ajax
}

function getScrollbarWidth(element) {
    return element.offsetWidth - element.clientWidth;
} 

function UpdateNode(data) {
    var node = "li[id='li_" + data.id + "']";
    if ($(node).length) {
        //next line is needed to avoid fillers problem
        $(node).css("white-space", "normal");
        //Adjust the node width
        /*if (!$("input[id='Description']").is(':checked'))
            $(node).width($(".content-wrapper").width() - ($(node).offset().left - $("#container").offset().left));
        else*/ $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));
        //Refresh node's elements
        var checked;
        var html;
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
        html = "<img src='../Images/attention.png' class='incomplete' style='display: none;' height='13' width='13'>";
        $(node).children(".incomplete").remove();
        $(node).children("ins").after(html);
        if (!data.complete && !(data.optional && !data.selected)) $(node).children(".incomplete").show();
        else $(node).children(".incomplete").hide();

        if ($(node).children('a').length == 0) {
            $(node).append("<a></a>");
            //Add the node url to the <a> inside the node (this doesnt work but needs to be here) 
            var href = "../" + data.url + "&nocache=" + new Date().getTime();
            $(node).children('a').attr("href", href);
            $(node).children('a').attr("target", 'details');
            //Set the <a> onclick to open the url in a new window
            //if (true/*!mobile*/) {
            //    $(node).children('a').click(function() {
            //        window.open(href, 'details');
            //      });
                //$(node).children('a').attr("onclick", "javascript: window.open('" + href + "', 'details')");
            //}
            //else {
            //    $(node).children('a').on("click", function (e) {
            //        e.preventDefault();
            //        var width = "100%";
            //        var height = width;
            //        iframe.attr({
            //            width: +width,
            //            height: +height,
            //            src: this.href
            //        });
            //        dialog.dialog("open");
            //    });
            //}
        }
        //Add the node url to the <a> inside the node (this doesnt work but needs to be here) 
        var href = "../" + data.url + "&nocache=" + new Date().getTime();
        $(node).children('a').attr("href", href);
        $(node).children('a').attr("target", 'details');
        //Set the <a> onclick to open the url in a new window
       // $(node).children('a').click(function() {
       //     window.open(href, 'details');
        //  });
        //$(node).children('a').attr("onclick", "javascript: window.open('" + href + "', 'details')");

        //Set leaves to red color
        if (data.leaf) $("li[id='li_" + data.id + "']").children('a').attr("style", "color:red;");
        else $("li[id='li_" + data.id + "']").children('a').attr("style", "color:black;");
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
        html = "<input type='hidden' id='dependents_" + data.id + "' value='" + data.DependentsStr + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);
        html = "<input type='hidden' id='editchildren_" + data.id + "' value='" + data.editChildren + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);
        //Add the hidden input storing the node expanded levels
        html = "<input type='hidden' id='expandedlevels_" + data.id + "' value='" + data.expandedLevels + "'/> ";
        $("li[id='li_" + data.id + "']").append(html);

        //Remove old
        $(node).children('a').empty();
        $(node).children('a').append("<span class='name' id='name_" + data.id + "'>" + data.name + "</span>");
        $(node).children('a').append("<span class='formula' id='formula_" + data.id + "' > " + (data.type != "Decision" && data.type != 'SumSet' && data.type != 'Date' && data.type != 'Today' ? " &nbsp;[<i>" + data.expression + "</i>]" : " &nbsp;&nbsp;&nbsp;") + "</span>");
        if (!$('input[id=\'Formulas\']').is(':checked')) $(node).children('a').children('.formula').hide();
        if ($(node).children(".subtotal")) $(node).children(".subtotal").remove();
        if ($(node).children(".filler")) $(node).children(".filler").remove();
        if ($(node).children(".edit")) $(node).children(".edit").remove();
        if ($(node).children(".add")) $(node).children(".add").remove();
        if ($(node).children(".remove")) $(node).children(".remove").remove();
        var newFiller = $("<div class='filler'>&nbsp;</div>");
        //var newSubtotal = $("<span class='subtotal' id='subtotal_" + data.id + "'>" + "[" + data.subtotal + "]" + "</span>");
        var newSubtotal = $("<span class='subtotal' id='subtotal_" + data.id + "'>" + "<font size='1'>" + data.type + "</font>" + "  " + "<font>" + "[" + data.subtotal + "]" + "</font>" + "</span>");

        $(node).children('a').after(newFiller);
        $(node).children('a').after(newSubtotal);
        $(node).children('a').after("<span title='Remove node' id='remove_" + data.id + "' class='remove' ></span>");
        $(node).children('a').after("<span title='Add node' id='add_" + data.id + "' class='add'></span>");
        $(node).children('a').after("<span title='Edit: " + data.name + "' id='edit_" + data.id + "' class='edit'></span>");


        var formula_value = data.expression;
        $(node).children('a').next().on("click", function (e) {
            e.preventDefault();
            //select the node
            var current_li = "li[id='li_" + data.id + "']";
            //if description section is not hidden show description page for current node
            if (!descriptionhidden)
                $(current_li).children("a").click();
            //$.jstree._reference(current_li).select_node(current_li);
            editedNodeId = data.id;
            //Fill the Order dropdown list
            $("#inodeOrder").empty();
            for (var i = 0; i <= $(node).siblings().length; i++)
                $("#inodeOrder").append("<option  value='" + i + "'>" + i + "</option>");
            FillNodeDialogInfo(editedNodeId);
            editdialog.dialog('option', 'title', 'Edit: \"' + data.name + '\"');
            editdialog.dialog("open");
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
            var current_li = "li[id='li_" + data.id + "']";
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
            for (var i = 0; i <= $(node).children("ul").children("li").length + 1; i++)
                $("#newinodeOrder").append("<option  value='" + i + "'>" + i + "</option>");
            newnodedialog.dialog('option', 'title', 'Insert new node into: \"' + data.name + '\"');
            newnodedialog.dialog("open");
        });
        //Declare some width related variables
        var nodeWidth, anchorWidth, subtotalWidth, insWidth, formulaWidth, imageVisibleWidth, checkboxWidth, editWitdh, addWidth, removeWidth, nameWidth;
        checkboxWidth = data.optional ? 13 : 0;
        imageVisibleWidth = !data.complete && !(data.optional && !data.selected) ? 16: 0;
        editWidth = data.type != 'Decision' ? $(node).children(".edit").outerWidth(true) : 0;
        addWidth = $(node).children(".add").outerWidth(true);
        removeWidth = $(node).children(".remove").outerWidth(true);
        var formula = $(node).children("a").children(".formula");
        var name = $(node).children("a").children(".name");
        
        //If node is hidden then show it to get correct widths, then hide it again
        //var hideParentUL = false;
        //var hideNode = false;
        //this step is needed to restore the node visibility that was temporarily changed to get correct widths
        //if ($(node).parent("ul").css("display") == "none") {
        //    hideParentUL = true;
        //}
        //if ($(node).css("display") == "none") {
        //    hideNode = true;
        //}

        //var hiddenUL = $('ul').filter(function () {
        //    return $(this).css('display') == 'none';
        //});
        //var hiddenLI = $('li').filter(function () {
        //    return $(this).css('display') == 'none';
        //});

        //show nodes and ul
        //hiddenUL.show();
        //hiddenLI.show();
        //$(node).parent("ul").show();
        //$(node).show();

        /*if (!$("input[id='Description']").is(':checked'))
            $(node).width($(".content-wrapper").width() - ($(node).offset().left - $("#container").offset().left));
        else*/ $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));

        nodeWidth = $(node).width();
        //nodeWidth = $("#container").width() - $(node).position().left;
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
        //if (hideParentUL) $(node).parent("ul").hide();
        //if (hideNode) $(node).hide();
        //hiddenUL.hide();
        //hiddenLI.hide();

        //If mobile remove the filler and the formula
        //if (mobile) {
        //    // some code..
        //    $(node).children(".filler").remove();
        //    $(node).children("a").children(".formula").hide();
        //    $(node).children(".name").width("100%");
        //}
        //else {
        //Set the formula element's width
        if (anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 > nodeWidth) {
            var extraWidth = anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 - nodeWidth;
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

        //detect OS and Set the filler's width
        var OSName = "Unknown OS";
        if (navigator.appVersion.indexOf("Win") != -1) OSName = "Windows";
        if (navigator.appVersion.indexOf("Mac") != -1) OSName = "MacOS";
        if (navigator.appVersion.indexOf("X11") != -1) OSName = "UNIX";
        if (navigator.appVersion.indexOf("Linux") != -1) OSName = "Linux";
        var scrollbarWidth = /*OSName == "MacOS" ? 0 :*/ getScrollbarWidth(document.getElementById("container"));

        //Set the filler's width
        $(node).children(".filler").css("width", nodeWidth - anchorWidth - subtotalWidth - insWidth - checkboxWidth - imageVisibleWidth - editWidth - addWidth - removeWidth - 30 - scrollbarWidth);
        if (nodeWidth - anchorWidth - subtotalWidth - insWidth - idWidth - marginLeft - checkboxWidth - imageVisibleWidth - editWidth - addWidth - removeWidth - 30 - scrollbarWidth > 0)
            $(node).children(".filler").css("margin-right", subtotalWidth + scrollbarWidth);

        //This will append the id text if not already there
        if ($("li[id='li_" + data.id + "']").children()[0].tagName != "FONT") {
            var offset = $("li[id='li_1']").position().left;
            $("<font width='100%' size='1' class='id'>" + "&nbsp" + data.id + "</font>").insertBefore("li[id='li_" + data.id + "']>ins");
            $(".id").css({ position: 'absolute', left: offset });
            var id_width = $("font[id='id_" + data.Id + "']").width();
            var ins_left = $(node).children("ins").position().left;
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
            if (result == "_SessionTimeout_") {
                document.location = "../SessionTimeOut.html";
                return false;
            }
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
            if (result == "_SessionTimeout_") {
                document.location = "../SessionTimeOut.html";
                return false;
            }
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
    var array = [];
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
            if (result == "_SessionTimeout_") {
                document.location = "../SessionTimeOut.html";
                return false;
            }
            var showSelectors = '';
            var hideSelectors = '';
            for (var i = 0; i < result.length; i++) {
                //Add the image for the complete/incomplete status if parent node is not Decision
                if (!result[i].complete && !(result[i].optional && !result[i].selected)) {
                    showSelectors += "li[id='li_" + result[i].id + "'],";
                }
                else hideSelectors += "li[id='li_" + result[i].id + "'],";

                //Update node's text
                UpdateNode(result[i]);
            }
            //$(showSelectors).children("img").first().show();
            //$(hideSelectors).children("img").first().hide();
        }
    });   //end ajax
}

function UpdateDependents(id) {
    //If is an array of id's
    if (typeof id === 'object') {
        var dependents = '';
        var dependentsArray;
        for (var i = 0; i < id.length; i++) {
            dependents += ";" + id[i] + ";" + $("input[id='dependents_" + id[i] + "']").attr("value");
        }
        dependentsArray = dependents.split(';');
        dependentsArray = dependentsArray.filter(function (item, index) {
            return dependentsArray.indexOf(item) === index;
        });
        dependents = '';
        for (var i = 0; i < dependentsArray.length; i++) {
            dependents += dependentsArray[i] + ";";
        }
        UpdateNodesFromServer(dependents);
        //UpdateTreeSync();
    }
    else {
        var dependents = $("input[id='dependents_" + id + "']").attr("value");
        //Updates the current node and its dependents
        UpdateNodesFromServer(id + ';' + dependents);
        //UpdateTreeSync();
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
            if (result == "_SessionTimeout_") {
                document.location = "/SessionTimeOut.html";
                return false;
            }

            //Add the image for the complete/incomplete status if parent node is not Decision
            //                if (!result.complete && !(result.optional && !result.selected)) {
            //                    $("li[id='li_" + result.id + "']").children("img").first().show();
            //                }
            //                else $("li[id='li_" + result.id + "']").children("img").first().hide();

            //Update node's text
            UpdateNode(result);

            //Recursive call
            var node = "li[id='li_" + result.id + "']";
            var children = $(node).children('ul').children('li');
            if (children && children.length > 0)
                for (var i = 0; i < children.length; i++) {
                    UpdateTree(children[i].id.replace(/li_/g, ""));
                }
        }
    });   //end ajax

}

function UpdateTreeSync() {
    var nodeList = '';

    function GetNodeList(id) {
        nodeList += id + ";";
        var node = "li[id='li_" + id + "']";
        var children = $(node).children('ul').children('li');
        if (children && children.length > 0)
            for (var i = 0; i < children.length; i++) {
                GetNodeList(children[i].id.replace(/li_/g, ""));
            }
    }
    GetNodeList(1);
    UpdateNodesFromServer(nodeList);
}

function HideNodes(id) {
    if ($("input[id='ishidden_" + id + "']").attr("value") == "true") $("li[id='li_" + id + "']").hide();
    var children = $("li[id='li_" + id + "']").children('ul').children('li');
    if (children && children.length > 0)
        for (var i = 0; i < children.length; i++) {
            HideNodes(children[i].id.replace(/li_/g, ""));
        }
}

function ShowNodes(id) {
    if ($("input[id='ishidden_" + id + "']").attr("value") == "true") {
        $("li[id='li_" + id + "']").css("display", "block");
        RefreshFillers(id, false);
    }
    var children = $("li[id='li_" + id + "']").children('ul').children('li');
    if (children && children.length > 0)
        for (var i = 0; i < children.length; i++) {
            ShowNodes(children[i].id.replace(/li_/g, ""));
        }
}

//this unhides a node and all its ancesters
function UnHideBranch(id) {
    $("li[id='li_" + id + "']").show();
    var parentulID = $("li[id='li_" + id + "']").parent().attr("id");
    //Recursive call
    if ($("li[id='li_" + id + "']").parent().parent().attr("id") != "container")
        UnHideBranch(parentulID.replace(/li_ul_/g, ""));
}

function ToggleSubtotals() {
    // $('input[id=\'Subtotals\']').is(':checked') ? $('.subtotal').show() : $('.subtotal').hide();
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
    //$('input[id=\'Formulas\']').is(':checked') ? $('.formula').show() : $('.formula').hide();
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    if ($('input[id=\'Formulas\']').is(':checked')) {
        $('.formula').show();
        RefreshFillers('1', true);
    }
    else {
        //EnlargeFillers('1');
        $('.formula').hide();
        RefreshFillers('1', true);
    }
}

function ToggleFormulasAsync() {
    //$('input[id=\'Formulas\']').is(':checked') ? $('.formula').show() : $('.formula').hide();
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    if ($('input[id=\'Formulas\']').is(':checked')) {
        $('.filler').width(0);
        $('.formula').width(0);
        $('.formula').show();
        //RefreshFillers('1', true);        
        UpdateTree('1');
    }
    else {
        $('.formula').hide();
        //RefreshFillers('1', true);
        UpdateTree('1');
    }
}


function ToggleHiddenFields() {
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    if ($('input[id=\'HiddenFields\']').is(':checked')) {
        ShowNodes('1');
        //Refill();
        //$("li,ul").show();
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
   // $("#container").css("float") == "right" ? $("#container").css("float", "left") : $("#container").css("float", "right");
    $("#container").css("float") == "left" ? $("#hr").css("width", "100%") : $("#hr").css("width", "59.5%");

    //setTimeout(Refill, 700);
    RefreshFillers(1, true);
    //setTimeout(UpdateTreeSync, 2000);
    //UpdateTree(1);
    //UpdateTreeSync();

    //Reload page to fix iframe size if decription is shown
    if (!descriptionhidden) {
        var iframe = document.getElementById('Iframe15');
        var innerDoc = iframe.contentDocument || iframe.contentWindow.document;
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
    //$("#container").css("float") == "right" ? $("#container").css("float", "left") : $("#container").css("float", "right");
    $("#container").css("float") == "left" ? $("#hr").css("width", "100%") : $("#hr").css("width", "59.5%");

    //setTimeout(Refill, 700);
    $('.filler').width(0);
    $('.formula').width(0);
    //RefreshFillers('1', true);        
    UpdateTree('1');
    //RefreshFillers(1, true);
    //setTimeout(UpdateTreeSync, 2000);
    //UpdateTree(1);
    //UpdateTreeSync();

    //Reload page to fix iframe size if decription is shown
    if (!descriptionhidden) {
        var iframe = document.getElementById('Iframe15');
        var innerDoc = iframe.contentDocument || iframe.contentWindow.document;
        innerDoc.location = innerDoc.location;
    }
}


function ToggleCompact() {
    //This is needed to reset the expandingLevels variable
    expandingLevels = false;
    if (!$('input[id=\'Compact\']').is(':checked')) {
        var nodes = $("#container li").filter(function () {
            return $(this).css('display') == 'none' && $("input[id='ishidden_" + $(this).attr("id").replace(/li_/g, "") + "']").attr("value") == "false";
        });
        nodes.show();
        for (var i = 0; i < nodes.length; i++) {
            RefreshFillers(nodes[i].id.replace(/li_/g, ""), false);
        }
    }
    else {
        var nodes = $("#container li").filter(function () {
            var parentulID = $(this).parent().attr("id");
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
        //Refill();
        //$("li,ul").show();
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
    var node = "li[id='li_" + id + "']";
    $(node).children(".filler").css("width", $(node).children(".filler").width() + $(node).children("a").children(".formula").width());

    var children = $(node).children('ul').children('li');
    if (children && children.length > 0)
        for (var i = 0; i < children.length; i++) {
            EnlargeFillers(children[i].id.replace(/li_/g, ""));
        }
}
//Recursive function to refresh the fillers's width
function ShortenFillers(id) {
    var node = "li[id='li_" + id + "']";
    $(node).children(".filler").css("width", $(node).children(".filler").width() - $(node).children("a").children(".formula").width());

    var children = $(node).children('ul').children('li');
    if (children && children.length > 0)
        for (var i = 0; i < children.length; i++) {
            ShortenFillers(children[i].id.replace(/li_/g, ""));
        }
}

function Refill() {
    RefreshFillers(1, true);
}

function RefreshFillers(id, recursive) {
    var node = "li[id='li_" + id + "']";
    if ($(node).length) {
        //var hideParentUL = false;
        //var hideNode = false;
        //this step is needed to restore the node visibility that will be temporarily changed to get correct widths
        //if ($(node).parent("ul").css("display") == "none") {
        //    hideParentUL = true;
        //}
        //if ($(node).css("display") == "none") {
        //    hideNode = true;
        //}
        //var hiddenUL = $('ul').filter(function () {
        //    return $(this).css('display') == 'none';
        //});
        //var hiddenLI = $('li').filter(function () {
        //    return $(this).css('display') == 'none';
        //});
        ////show nodes and ul
        //hiddenUL.show();
        //hiddenLI.show();
        //$(node).parent("ul").show();
        //$(node).show();
        //Adjust the node width
        var padding_left = 0;
        /*if (!$("input[id='Description']").is(':checked'))
            $(node).width($(".content-wrapper").width() - padding_left - ($(node).offset().left - $("#container").offset().left));
        else*/ $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));

        var nameText = $(node).children('a').children(".name").text();
        var formulaText = $(node).children('a').children(".formula").children("i").text();
        var subtotalType = $(node).children(".subtotal").children("font").eq(0).text();
        var subtotalText = $(node).children(".subtotal").children("font").eq(1).text();

        //Refresh node's elements
        $(node).children('a').empty();
        $(node).children('a').append("<span class='name' id='name_" + id + "'>" + nameText + "</span>");
        //$(node).children('a').append("<span class='formula' id='formula_" + id + "' > &nbsp;[<i>" + formulaText.trim() + "</i>]</span>");
        $(node).children('a').append("<span class='formula' id='formula_" + id + "' >" + ($("input[id='nodetype_" + id + "']").attr("value") != "Decision" && $("input[id='nodetype_" + id + "']").attr("value") != "Date" && $("input[id='nodetype_" + id + "']").attr("value") != "Today"? " &nbsp;[<i>" + formulaText.trim() + "</i>]" : "") + "</span>");
        if (!$('input[id=\'Formulas\']').is(':checked')) $(node).children('a').children('.formula').hide();
        $(node).children(".subtotal").remove();
        $(node).children(".filler").remove();
        var newFiller = $("<div class='filler'>&nbsp;</div>");
        var newSubtotal = $("<span class='subtotal' id='subtotal_" + id + "'>" + "<font size='1'>" + subtotalType + "</font>" + " " + "<font>" + subtotalText + "</font>" + "</span>");
        $(node).children('a').after(newFiller);
        $(node).children('a').after(newSubtotal);

        //Declare some width related variables
        var nodeWidth, anchorWidth, subtotalWidth, insWidth, formulaWidth, imageVisibleWidth, checkboxWidth, editWidth, addWidth, removeWidth, nameWidth;
        checkboxWidth = $("input[id='ckbx_" + id + "']").length ? 13 : 0;
        imageVisibleWidth = $(node).children("img").first().is(":visible") ? 16 : 0;
        editWidth = $(node).children(".edit").length ? $(node).children(".edit").outerWidth(true) : 0;
        addWidth = $(node).children(".add").length ? $(node).children(".add").outerWidth(true) : 0;
        removeWidth = $(node).children(".remove").length ? $(node).children(".remove").outerWidth(true) : 0;
        var formula = $(node).children("a").children(".formula");
        var name = $(node).children("a").children(".name");

        //I node is hidden then show it to get correct widths, then hide it again
        //            if ($(node).parent("ul").is(":visible") && $(node).is(":visible")) {
        //                nodeWidth = $(node).width();
        //                anchorWidth = $(node).children('a').width();
        //                subtotalWidth = $(node).children(".subtotal").width();
        //                insWidth = $(node).children("ins").width();
        //                if (!formula.is(":visible")) {
        //                    formula.show();
        //                    formulaWidth = formula.width();
        //                    formula.hide();
        //                }
        //                else formulaWidth = formula.width();
        //            }
        //            else {


        /*if (!$("input[id='Description']").is(':checked'))
            $(node).width($(".content-wrapper").width() - ($(node).offset().left - $("#container").offset().left));
        else*/ $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));
        nodeWidth = $(node).width();
        //nodeWidth = $("#container").width() - $(node).position().left;
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

        
        //}
        //Set the formula element's width
        if (anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 > nodeWidth) {
            var extraWidth = anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 - nodeWidth;
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

        //Hide the hidden nodes
        //if (hideParentUL) $(node).parent("ul").hide();
        //if (hideNode) $(node).hide();
        //hiddenUL.hide();
        //hiddenLI.hide();

        //detect OS and Set the filler's width
        var OSName = "Unknown OS";
        if (navigator.appVersion.indexOf("Win") != -1) OSName = "Windows";
        if (navigator.appVersion.indexOf("Mac") != -1) OSName = "MacOS";
        if (navigator.appVersion.indexOf("X11") != -1) OSName = "UNIX";
        if (navigator.appVersion.indexOf("Linux") != -1) OSName = "Linux";
        var scrollbarWidth = /*OSName == "MacOS" ? 0 :*/ getScrollbarWidth(document.getElementById("container"));
        $(node).children(".filler").css("width", nodeWidth - anchorWidth - subtotalWidth - insWidth - checkboxWidth - imageVisibleWidth - editWidth - addWidth - removeWidth - 30 - scrollbarWidth);
        $(node).children(".filler").css("margin-right", subtotalWidth + scrollbarWidth);
        //Recursive call
        if (recursive = true) {
            var children = $(node).children('ul').children('li');
            if (children && children.length > 0)
                for (var i = 0; i < children.length; i++) {
                    RefreshFillers(children[i].id.replace(/li_/g, ""), true);
                }
        }
    }
    //This will append the id text
    //var offset = $("li[id='li_1']").position().left;
    //$(".id").css({ position: 'absolute', left: offset });

    if (!$('input[id=\'Subtotals\']').is(':checked')) $('.subtotal, .filler').hide();
    //if (mobile) $('.filler, .formula').hide();
}

function OpenNode(node) {
    var split = node.split('>');
    var current = "li[id='li_1']";
    for (var i = 0; i < split.length; i++) {
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
                var children = $(current).children('ul').children('li');
                for (var j = 0; j < children.length; j++) {
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

            var children = $("li[id='li_" + node + "']").children('ul').children('li');
            if (children && children.length > 0)
                for (var i = 0; i < children.length; i++) {
                    //$.jstree._reference("#" + children[i].id).open_node("#li_" + node);
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
        var children_ul = $("li[id='li_" + node + "']").children("ul");
        if (children_ul.length == 0 || children_ul.children("li").length == 0) {
            $.ajax({
                url: "ChildNodes?id=" + node,
                type: 'GET',
                dataType: "json",
                cache: false,
                //async: asynchronous == "true" ? true : false,
                beforeSend: function () {

                },
                complete: function (result) {
                    //Expand levels
                    var jsonObject = jQuery.parseJSON(result.responseText);
                    for (var i = 0; i < jsonObject.length; i++) {
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
            var children = $("li[id='li_" + node + "']").children('ul').children('li');
            for (var j = 0; j < children.length; j++) {
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
    //var nodeInfo = NodeInfo(id);
    //$("inodeName").attr("value",nodeInfo.name);
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
            if (result == "_SessionTimeout_") {
                document.location = "../SessionTimeOut.html";
                return false;
            }

            $("#inodeName").val(result.name);
            $("#inodeType").val(result.type);
            $("#inodeExpression").val(result.expression);
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
            $("#inodeUnitsText").val(result.units);
            $("#inodeTemplate").prop("checked", result.template);
            $("#inodeReadOnly").prop("checked", result.readOnly);
        }
    });   //end ajax
    asynchronous = true;
}

function SaveNodeInfo() {
    //Post values to server
    //editdialog.parent().appendTo("#nodeInfoForm");
    var name = $("#inodeName").val();
    var type = $("#inodeType").val();
    var expression = $("#inodeExpression").val();
    var expandedLevels = $("#inodeExpandedLevels").val();
    var order = $("#inodeOrder").val();
    var min = $("#inodeMin").val();
    var max = $("#inodeMax").val();
    var discount = $("#inodeDiscount").val();
    var hidden = $("#inodeHidden").is(":checked");
    var optional = $("#inodeOptional").is(":checked");
    var disable = $("#inodeDisable").val();
    var disabledMessage = $("#inodeDisabledMessage").val();
    var editChildren = $("#inodeEditChildren").is(":checked");
    var report = $("#inodeReport").is(":checked");
    var reportValue = $("#inodeReportValue").is(":checked");
    var units = $("#inodeUnitsText").val();
    var template = $("#inodeTemplate").is(":checked");
    var readOnly = $("#inodeReadOnly").is(":checked");

    var specialChars = "*/\+-|[]?&!()><=#{};:";
    var check = function (string) {
        for (i = 0; i < specialChars.length; i++) {
            if (string.indexOf(specialChars[i]) > -1) {
                return true
            }
        }
        return false;
    };

    if (check(name) == false) {
        var queryString =
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
            + "&template=" + template
            + "&readOnly=" + readOnly
            ;
        var data = $('#inodeName').serialize();
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
            complete: function () {
            },
            success: function (result) {
                if (result == "_SessionTimeout_") {
                    document.location = "../SessionTimeOut.html";
                    return false;
                }
                //this is needed to refresh the order of nodes in case the order property was modified
                if (editedNodeId != 1) {
                    var parentId = $("li[id='li_" + editedNodeId + "']").parent().attr("id").replace(/li_ul_/g, "");
                    $.jstree._reference("li[id='li_" + parentId + "']").move_node("li[id='li_" + editedNodeId + "']", "li[id='li_" + parentId + "']", parseInt(result.order));
                }
                //Update the price in the page
                price = document.getElementById("price");
                $(price).text("Price: " + result.total);

                if (result.hasErrors) {
                    alert(result.error);
                    UpdateTreeSync();
                    $(".loading").hide();
                    $("#overlay").remove();
                }
                else {
                    //Instead of updating just dependents, update the whole tree to refresh dependent list for each node on client.
                    UpdateTreeSync();
                    $(".loading").hide();
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
    //editdialog.parent().appendTo("#nodeInfoForm");
    var name = $("#newinodeName").val();
    var type = $("#newinodeType").val();
    var expression = $("#newinodeExpression").val();
    var expandedLevels = $("#newinodeExpandedLevels").val();
    var order = $("#newinodeOrder").val();
    var min = $("#newinodeMin").val();
    var max = $("#newinodeMax").val();
    var discount = $("#newinodeDiscount").val();
    var hidden = $("#newinodeHidden").is(":checked");
    var optional = $("#newinodeOptional").is(":checked");
    var disable = $("#newinodeDisable").val();
    var disabledMessage = $("#newinodeDisabledMessage").val();
    var editChildren = $("#newinodeEditChildren").is(":checked");
    var report = $("#newinodeReport").is(":checked");
    var reportValue = $("#newinodeReportValue").is(":checked");
    var units = $("#newinodeUnitsText").val();
    var template = $("#newinodeTemplate").is(":checked");
    var readOnly = $("#newinodeReadOnly").is(":checked");

    var specialChars = "*/\\+-|[]?&!()><=#{}";
    var check = function (string) {
        for (i = 0; i < specialChars.length; i++) {
            if (string.indexOf(specialChars[i]) > -1) {
                return true
            }
        }
        return false;
    };

    if (check(name) == false) {
        var queryString =
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
            + "&template=" + template
            + "&readOnly=" + readOnly
            ;
        var data = $('#inodeName').serialize();

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
                //set cursor to wait and disable inputs
                //$("#inewnodeInfo").css("cursor", "wait");
                //$('#inewnodeInfo *').prop('disabled', true).css('pointer-events', 'none');
            },
            complete: function () {
                UpdateTreeSync();
                //UpdateDependents(editedNodeId);
                //restore cursor and inputs
                //$("#inewnodeInfo").css("cursor", "auto");
                //$('#inewnodeInfo *').prop('disabled', false).css('pointer-events', 'auto');
                $(".loading").hide();
                $("#overlay").remove();
                creatingNewNode = false;
            },
            success: function (result) {
                if (result == "_SessionTimeout_") {
                    document.location = "../SessionTimeOut.html";
                    return false;
                }
                if (result.hasErrors) {
                    alert(result.error);
                }
                else {
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
        cache: false,
        beforeSend: function () {
        },
        complete: function () {
            removenodedialog.dialog("close");
        },
        success: function (result) {
            if (result == "_SessionTimeout_") {
                document.location = "../SessionTimeOut.html";
                return false;
            }
            //Update the price in the page
            //top.asynchronous = false;
            price = document.getElementById("price");
            $(price).text("Price: " + result);
            //top.asynchronous = true;
            var ids = removeNodeIds.split(";");
            for (var i = 0; i < ids.length; i++) {
                if (ids[i] != "") {
                    var parentulID = $("li[id='li_" + ids[i] + "']").parent().attr("id");
                    //Show siblings if parent is Decision node
                    if ($("input[id='nodetype_" + parentulID.replace(/li_ul_/g, "") + "']").attr("value") == "Decision")
                        $("li[id='li_" + ids[i] + "']").siblings().show();
                    $("li[id='li_" + ids[i] + "']").remove();
                }
            }
            UpdateTree(1);
        },

    });   //end ajax
}

//Assemble the tree
function Assemble(result, id) {
    if (result == "_SessionTimeout_") {
        document.location = "../SessionTimeOut.html";
        return false;
    }
    var li_id = id;
    //Get children from server if not already inserted
    var children_ul = $(id).children("ul");

    //Update the first node
    //UpdateNodeFromServer(li_id.replace(/li_/g, ""));

    //Select the node
    $("#container").jstree("select_node", $("li[id='" + li_id + "']"));
    //Insert children
    for (var i = 0; i < result.length; i++) {
        $("#container").jstree("create", null, "last", { "data": " ", "attr": { "class": "jstree-closed", "id": "li_" + result[i].id } }, false, true);
        //"data": result[i].name + (result[i].type != 'Decision' ? (' [' + result[i].expression + ']') : '') + '=[' + result[i].subtotal + ']'
        //Insert content into anchor
        UpdateNode(result[i]);

        //Padd the anchor
        //$("li[id='li_" + result[i].id + "']").children('a').attr("style", "padding: 5px;");
        //If hidden then hide
        if (result[i].hidden == true && !($('input[id=\'HiddenFields\']').is(':checked'))) $("li[id='li_" + result[i].id + "']").hide();
        //Add the node url to the <a> inside the node (this doesnt work but needs to be here) 
        var href = result[i].url + "&nocache=" + new Date().getTime();

        $("li[id='li_" + result[i].id + "']").children('a').attr("href", href);
        $("li[id='li_" + result[i].id + "']").children('a').attr("target", 'details');
        //Set the <a> onclick to open the url in a new window
        //if (true /*!mobile*/) {
        //    $("li[id='li_" + result[i].id + "']").children('a').click(function() {
        //        window.open(href, 'details');
        //      });

            //$("li[id='li_" + result[i].id + "']").children('a').attr("onclick", "javascript: window.open('" + href + "', 'details')");
       // }
        //else {
        //    $("li[id='li_" + result[i].id + "']").children('a').on("click", function (e) {
        //        e.preventDefault();
        //        var width = "100%";
        //        var height = width;
        //        iframe.attr({
        //            width: +width,
        //            height: +height,
        //            src: this.href
        //        });
        //        dialog.dialog("open");
        //    });
        //}
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
    var parentId = tree.Id.substr(0, tree.Id.lastIndexOf("."));
    if (tree.Id != 1)
        $("#container").jstree("select_node", $("li[id='" + "li_" + parentId + "']"));
    $("#container").jstree("create", null, "last", { "data": " ", "attr": { "class": "jstree-closed", "id": "li_" + tree.Id } }, false, true);

    //////////////////////////////////////Update node/////////////////////////////////////////////////////////////////////////////////
    var node = "li[id='li_" + tree.Id + "']";
    if ($(node).length) {
        //next line is needed to avoid fillers problem
        $(node).css("white-space", "normal");
        //Adjust the node width
        /*if (!$("input[id='Description']").is(':checked'))
            $(node).width($(".content-wrapper").width() - ($(node).offset().left - $("#container").offset().left));
        else*/ $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));
        //Refresh node's elements
        var checked;
        var html;
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
        html = "<img src='../Images/attention.png' class='incomplete' style='display: none;' height='13' width='13'>";
        $(node).children(".incomplete").remove();
        $(node).children("ins").after(html);
        if (!tree.Complete && !(tree.Optional && !tree.Selected)) $(node).children(".incomplete").show();
        else $(node).children(".incomplete").hide();

        //if ($(node).children('a').length == 0) {
        //    $(node).append("<a></a>");
            //Add the node url to the <a> inside the node (this doesnt work but needs to be here) 
            var href = "../" + tree.Url + "&nocache=" + new Date().getTime();
            $(node).children('a').attr("href", href);
            $(node).children('a').attr("target", 'details');
            //Set the <a> onclick to open the url in a new window
            //if (true/*!mobile*/) {
            //    $(node).children('a').click(function() {
             //       window.open(href, 'details');
            //      });

                //$(node).children('a').attr("onclick", "javascript: window.open('" + href + "', 'details')");
            //}
            //else {
            //    $(node).children('a').on("click", function (e) {
            //        e.preventDefault();
            //        var width = "100%";
            //        var height = width;
            //        iframe.attr({
            //            width: +width,
            //            height: +height,
            //            src: this.href
            //        });
            //        dialog.dialog("open");
            //    });
            //}
        //}

        //Set leaves to red color
        if (tree.Leaf) $("li[id='li_" + tree.Id + "']").children('a').attr("style", "color:red;");
        else $("li[id='li_" + tree.Id + "']").children('a').attr("style", "color:black;");
        //Set hidden to green
        if (tree.Hidden) $("li[id='li_" + tree.Id + "']").children('a').attr("style", "color:green");
        //Add the node expression as a tooltip
        if (tree.TypeStr != 'Decision' && tree.TypeStr != 'SumSet' && tree.TypeStr != 'ConditionalRules' && tree.TypeStr != 'Date' && tree.TypeStr != 'Today')
            expression = tree.Formula;
        else
            if (tree.TypeStr == 'ConditionalRules')
                expression = tree.Expression;
            else expression = "";
        //Add the node expression as a tooltip
        $(node).children('a').attr("title", expression);
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
        var expression = "";
        if (tree.TypeStr != 'Decision' && tree.TypeStr != 'SumSet' && tree.TypeStr != 'ConditionalRules' && tree.TypeStr != 'Date' && tree.TypeStr != 'Today')
            expression = "&nbsp;[<i>" + tree.Formula + "</i>]";
        else
            if (tree.TypeStr == 'ConditionalRules')
                expression = "&nbsp;[<i>" + tree.Expression + "</i>]";
            else expression = " &nbsp;&nbsp;&nbsp;";
        $(node).children('a').append("<span class='formula' id='formula_" + tree.Id + "' > "+ expression + "</span>");
        if (!$('input[id=\'Formulas\']').is(':checked')) $(node).children('a').children('.formula').hide();
        $(node).children(".subtotal").remove();
        $(node).children(".filler").remove();
        $(node).children(".edit").remove();
        $(node).children(".add").remove();
        $(node).children(".remove").remove();
        var newFiller = $("<div class='filler'>&nbsp;</div>");
        var newSubtotal = $("<span class='subtotal' id='subtotal_" + tree.Id + "'>" + "<font size='1'>" + tree.TypeStr + "</font>" + "  " + "<font>" + "[" + tree.TotalStr + "]" + "</font>" + "</span>");

        $(node).children('a').after(newFiller);
        $(node).children('a').after(newSubtotal);
        $(node).children('a').after("<span title='Remove node' id='remove_" + tree.Id + "' class='remove' ></span>");
        $(node).children('a').after("<span title='Add node' id='add_" + tree.Id + "' class='add'></span>");
        $(node).children('a').after("<span title='Edit: " + tree.Name + "' id='edit_" + tree.Id + "' class='edit'></span>");


        var formula_value = tree.TypeStr != "Decision" && tree.TypeStr != 'SumSet' && tree.TypeStr != 'Date' && tree.TypeStr != 'Today' ? tree.Formula : "";
        $(node).children('a').next().on("click", function (e) {
            e.preventDefault();
            //select the node
            var current_li = "li[id='li_" + tree.Id + "']";
            //if description section is not hidden show description page for current node
            if (!descriptionhidden)
                $(current_li).children("a").click();
            editedNodeId = tree.Id;
            //Fill the Order dropdown list
            $("#inodeOrder").empty();
            for (var i = 0; i <= $(node).siblings().length; i++)
                $("#inodeOrder").append("<option  value='" + i + "'>" + i + "</option>");
            FillNodeDialogInfo(editedNodeId);
            editdialog.dialog('option', 'title', 'Edit: \"' + tree.Name + '\"');
            editdialog.dialog("open");
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
            var current_li = "li[id='li_" + tree.Id + "']";
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
            for (var i = 0; i <= $(node).children("ul").children("li").length + 1; i++)
                $("#newinodeOrder").append("<option  value='" + i + "'>" + i + "</option>");
            newnodedialog.dialog('option', 'title', 'Insert new node into: \"' + tree.Name + '\"');
            newnodedialog.dialog("open");
        });
        //Declare some width related variables
        var nodeWidth, anchorWidth, subtotalWidth, insWidth, formulaWidth, imageVisibleWidth, checkboxWidth, editWitdh, addWidth, removeWidth, nameWidth;
        checkboxWidth = $("input[id='ckbx_" + tree.Id + "']").length ? 13 : 0;
        imageVisibleWidth = $(node).children("img").first().is(":visible") ? 16 : 0;
        editWidth = tree.TypeStr != 'Decision' ? $(node).children(".edit").outerWidth(true) : 0;
        addWidth = $(node).children(".add").outerWidth(true);
        removeWidth = $(node).children(".remove").outerWidth(true);

        var formula = $(node).children("a").children(".formula");
        var name = $(node).children("a").children(".name");
        //If node is hidden then show it to get correct widths, then hide it again
        var hideParentUL = false;
        var hideNode = false;
        //this step is needed to restore the node visibility that was temporarily changed to get correct widths
        //if ($(node).parent("ul").css("display") == "none") {
        //    hideParentUL = true;
        //}
        //if ($(node).css("display") == "none") {
        //    hideNode = true;
        //}

        //var hiddenUL = $('ul').filter(function () {
        //    return $(this).css('display') == 'none';
        //});
        //var hiddenLI = $('li').filter(function () {
        //    return $(this).css('display') == 'none';
        //});

        ////show nodes and ul
        //hiddenUL.show();
        //hiddenLI.show();
        //$(node).parent("ul").show();
        //$(node).show();

        /*if (!$("input[id='Description']").is(':checked'))
            $(node).width($(".content-wrapper").width() - ($(node).offset().left - $("#container").offset().left));
        else*/ $(node).width($("#container").width() - ($(node).offset().left - $("#container").offset().left));
        nodeWidth = $(node).width();
        //nodeWidth = $("#container").width() - $(node).position().left;
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
        //if (hideParentUL) $(node).parent("ul").hide();
        //if (hideNode) $(node).hide();
        //hiddenUL.hide();
        //hiddenLI.hide();

        //If mobile remove the filler and the formula
        //if (mobile) {
        //    // some code..
        //    $(node).children(".filler").remove();
        //    $(node).children("a").children(".formula").hide();
        //    $(node).children(".name").width("100%");
        //}
        //else {
        //Set the formula element's width
        if (anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 > nodeWidth) {
            var extraWidth = anchorWidth + subtotalWidth + insWidth + checkboxWidth + imageVisibleWidth + editWidth + addWidth + removeWidth + 25 - nodeWidth;
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
        //detect OS and Set the filler's width
        var OSName = "Unknown OS";
        if (navigator.appVersion.indexOf("Win") != -1) OSName = "Windows";
        if (navigator.appVersion.indexOf("Mac") != -1) OSName = "MacOS";
        if (navigator.appVersion.indexOf("X11") != -1) OSName = "UNIX";
        if (navigator.appVersion.indexOf("Linux") != -1) OSName = "Linux";
        var scrollbarWidth = /*OSName == "MacOS" ? 0 :*/ getScrollbarWidth(document.getElementById("container"));
        $(node).children(".filler").css("width", nodeWidth - anchorWidth - subtotalWidth - insWidth - checkboxWidth - imageVisibleWidth - editWidth - addWidth - removeWidth - 30 - scrollbarWidth);
        $(node).children(".filler").css("margin-right", subtotalWidth + scrollbarWidth);

        //This will append the id text
        //var offset = $("li[id='li_1']").position().left;
        //$("<font width='100%' size='1' class='id'>" + "&nbsp" + tree.Id + "</font>").insertBefore("li[id='li_" + tree.Id + "']>ins");
        //$(".id").css({ position: 'absolute', left: offset });
        //$(node).css({ 'marginLeft': '12px' });

        //This will append the id text (THIS CODE WILL EVENTUALY REPLACE THE CODE ABOVE
        var offset = $("li[id='li_1']").position().left;
        $("<font width='100%' size='1' class='id' id = 'id_" + tree.Id + "'>" + "&nbsp" + tree.Id + "</font>").insertBefore("li[id='li_" + tree.Id + "']>ins");
        $(".id").css({ position: 'absolute', left: offset });
        var id_width = $("font[id='id_" + tree.Id + "']").width();
        var ins_left = $(node).children("ins").position().left;
        if (id_width > ins_left) {
            $(node).css("margin-left", id_width - ins_left + 13.5);
        }
        else {
            $(node).css("margin-left", 13.5);
        }

        if (!$('input[id=\'Subtotals\']').is(':checked')) $('.subtotal, .filler').hide();

        //Set the id for the automatically generated ul
        //$.jstree._reference(node).open_node(node);
        if ($(node).children("ul").length == 0) {
            $(node).append("<ul id='" + "li_ul_" + tree.Id + "' style='display:inline'>" + "</ul>");
            //$.jstree._reference(node).open_node(node);
        }
    }
    ///////////////////////////////////////End Updating Node//////////////////////////////////////////////////////////////////////////
    //Recursive call
    if (tree._Children != null) {
        for (var i = 0; i < tree._Children.length; i++) {
            if(tree._Children[i].Id == undefined) continue;
            RenderTree(tree._Children[i]);
        }
    }
    $("#container").jstree("deselect_node", $("li[id='" + "li_" + parentId + "']"));
}


//This function was replaced by conditional serialization in the server side
function PruneTree(tree,Root) {
    var countDots = tree.Id.split('.').length - 1;
    if (tree.Id != '1' && countDots == Root.ExpandedLevels) {
        tree._Children = null;
        return;
    }
    else {
        for (var i = 0; i < tree._Children.length; i++){
            PruneTree(tree._Children[i], Root);
        }
    } 
}

// Decompress an LZW-encoded string
function lzw_decode(s) {
    var dict = {};
    var data = (s + "").split("");
    var currChar = data[0];
    var oldPhrase = currChar;
    var out = [currChar];
    var code = 256;
    var phrase;
    for (var i = 1; i < data.length; i++) {
        var currCode = data[i].charCodeAt(0);
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

$(function () {
    descriptionhidden = false;
    ajaxDone = false;
    asynchronous = true;
    mobile = false;
    copyNodesId = "";
    treeIsLoaded = false;
    expandingLevels = false;
    creatingNewNode = false;
    // TO CREATE AN INSTANCE
    // select the tree container using jQuery
    $("#container")
        // call .jstree with the options object
        .jstree({
            // the plugins array allows you to configure the active plugins on this instance
            "plugins": ["themes", "html_data", "ui", "crrm", "hotkeys", "contextmenu"],
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
                                var selectedNodes = $("#container").jstree("get_selected");
                                if (selectedNodes.length > 1) {
                                    for (var i = 0; i < selectedNodes.length; i++) {
                                        removeNodeIds += selectedNodes[i].id.replace(/li_/g, "") + ";";
                                        //$("span[id='remove_" + selectedNodes[i].id.replace(/li_/g, "") + "']").click();                                           
                                    }
                                    $("#removeNode").children("label").text("Remove multiple nodes?");
                                    removenodedialog.dialog("open");
                                    //removeNodes();
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
                                var selectedNodes = $("#container").jstree("get_selected");
                                this.copy(selectedNodes);
                                if (selectedNodes.length <= 1) copyNodesId = obj.attr("id").replace(/li_/g, "");
                                else
                                    for (var i = 0; i < selectedNodes.length; i++) {
                                        copyNodesId += selectedNodes[i].id.replace(/li_/g, "") + ";";
                                    }
                            }
                        },
                        "Paste": {
                            "label": "Paste",
                            "action": function (obj) {
                                //this.paste(obj);
                                var targetNodeId = obj.attr("id").replace(/li_/g, "");
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
                                            $(".loading").hide();
                                            $("#overlay").remove();
                                            creatingNewNode = false;
                                        },
                                        success: function (result) {
                                            if (result == "_SessionTimeout_") {
                                                document.location = "../SessionTimeOut.html";
                                                return false;
                                            }

                                            for (var i = 0; i < result.length; i++) {
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
                                            $(price).text("Price: " + result[0].total);
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
            //$.jstree._reference("#li_1").open_node("#li_1");
            //Set up the filler for the first node on creation.
            //var firstNode = "li[id='li_1']";
            //var checkboxWidth = $("input[id='ckbx_1']").length ? 13 : 0;
            //if (!mobile) {
            //    $(firstNode).children(".filler").css("width", $(firstNode).width() - $(firstNode).children('a').width() - $(firstNode).children(".subtotal").width() - $(firstNode).children("ins").width() - checkboxWidth - $(firstNode).children("img").width() - 25);
            //    $(firstNode).children(".filler").css("margin-right", $(firstNode).children(".subtotal").width());
            //}
            //else $(firstNode).children(".filler").remove();
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
                    //Open the first level once
                    //$.jstree._reference("#li_1").open_node("#li_1");
                },
                success: function (result) {
                    //ExpLevels = result.expandedLevels;
                    if (result != null) {
                        var decompressed = lzw_decode(result);
                        var jsonObject = JSON.parse(decompressed);
                        PruneTree(jsonObject, jsonObject);
                        RenderTree(jsonObject);
                        //This update is to fix a bug, should be removed later
                        //UpdateTreeSync();
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
            
            //ExpandLevels(1, ExpLevels);

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
        // data.inst is the instance which triggered this event
        //data.inst.select_node("#li_1", true);
        //debugger;
        var li_id = data.rslt.obj.attr('id');
        //Get children from server if not already inserted
        var children_ul = data.rslt.obj.find('ul');
        //Hide the node if is hidden and the show hidden checkbox is unchecked.....This is needed because when the open node event is called it will unhide children nodes
        if (!$('input[id=\'HiddenFields\']').is(':checked')) HideNodes(li_id.replace(/li_/g, ""), '');
        //show_all = $("input[id='HiddenFields']").is(':checked') ? "true" : "false";
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
                        //Expand levels
                        //var jsonObject = jQuery.parseJSON(result.responseText);
                        //for (var i = 0; i < jsonObject.length; i++) {
                        //    if ((jsonObject[i].id.split(".").length - 1) < ExpLevels)
                        //        $.jstree._reference("li[id='li_" + jsonObject[i].id + "']").open_node("li[id='li_" + jsonObject[i].id + "']");
                        //}
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
                        //Expand levels
                        //var jsonObject = jQuery.parseJSON(result.responseText);
                        //for (var i = 0; i < jsonObject.length; i++) {
                        //    if ((jsonObject[i].id.split(".").length - 1) < ExpLevels)
                        //        $.jstree._reference("li[id='li_" + jsonObject[i].id + "']").open_node("li[id='li_" + jsonObject[i].id + "']");
                        //}
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
        //debugger;
        //window.location.href = data.rslt.obj.attr("href");
        //The previous line doesnt work, data.rslt.obj.attr("href") returns "undefined"
        //window.location.assign(data.args[0].href);
        document.getElementById('Iframe15').src = data.args[0].href;
    });

    //If client is mobile....
    if (/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)) {
        // some code..
        mobile = true;
        //ToggleDescription();
        //$("#Formulas,#FormulaText, #Description, #DescriptionText").hide();
    }
    //		$(window).on('offline online', function (event) {
    //		    //alert('You are ' + event.type + '!');
    //		    if (event.type == 'offline')
    //		    {
    //		    	$('#container *').prop('disabled',true);
    //	    		//Show overlay
    //            	//offline_overlay = $('<div></div>').prependTo('body').attr('id', 'offline_overlay');
    //            	//$("#offline_overlay").show();
    //		    }
    //		    else 
    //		    {
    //		    	$('#container *').prop('disabled',false);
    //		    	//offline_overlay.remove();
    //		    }
    //		});
    //		Offline.on('down', function(){$('#container *').prop('disabled',true);});
    //		Offline.on('up', function(){$('#container *').prop('disabled',false);});

    //Resize fillers on window resize
    $(window).on("resize", function () {
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
        Refill();
        //Resize description and tree
        containerOffsetTop = $("#container").offset().top;
        $("#Iframe15").height($(window).height() - descriptionOffsetTop - 70);
        $("#container").height($(window).height() - containerOffsetTop);
    });

    //code to style the buttons and inputs
    //$("input[type=submit], button").button();

    // code to show dialog box
    var iframe = $('<iframe frameborder="0" marginwidth="0" marginheight="0" allowfullscreen></iframe>');
    dialog = $("<div></div>").append(iframe).appendTo("body").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: "auto",
        height: "auto",
        close: function () {
            iframe.attr("src", "about:blank");
        }
    });
    $("#Iframe15").on("load", function (e) {
        if (descriptionhidden) {
            e.preventDefault();
            var src = document.getElementById("Iframe15").contentWindow.location.href;
            var width = !mobile ? $("#hr").width() * 40 / 100 : "100%";
            var height = width;
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
        title: "Edit node",
        close: function () {
            $(".loading").hide();
            $("#overlay").remove();
        },
        buttons:
        {
            "Save": function () {
                SaveNodeInfo();
                return false;
            }
        }
    });

    // code to show the edit formula dialog
    newnodedialog = $("#inewnodeInfo").dialog({
        autoOpen: false,
        modal: true,
        resizable: false,
        width: "auto",
        height: "auto",
        title: "Insert new node",
        close: function () {
        },
        buttons:
        {
            "Save": function () {
                //SaveNodeInfo();
                NewNode();
                //alert("New node created!");
            }
        }
    });

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
    //Setup ajax error code messages
    //        $.ajaxSetup({
    //		error: function(jqXHR, exception) {
    //				if (jqXHR.status === 0) {
    //					alert('Not connect.\n Verify Network.');
    //				} else if (jqXHR.status == 404) {
    //					alert('Requested page not found. [404]');
    //				} else if (jqXHR.status == 500) {
    //					alert('Internal Server Error [500].');
    //				} else if (exception === 'parsererror') {
    //					alert('Requested JSON parse failed.');
    //				} else if (exception === 'timeout') {
    //					alert('Time out error.');
    //				} else if (exception === 'abort') {
    //					alert('Ajax request aborted.');
    //				} else {
    //					alert('Uncaught Error.\n' + jqXHR.responseText);
    //				}
    //			}
    //		});
    //Code to fix issue with dialog box sticking to mouse. doesnt work
    $("#body").mouseleave(function () {
        $(this).mouseup();
    });

    //If window width is less than 1024 then hide the description
    if ($(window).width() < "1024")
        //$("input[id='Description']").attr('checked', false);
        $("input[id='Description']").click();

    //Disable context menu
    document.addEventListener("contextmenu", function(e){
    e.preventDefault();
    }, false);

});
//Setting the tooltips
//$(function () {
//    $(document).tooltip();
//});
    //$.jstree._reference("#li_1").open_node("#li_1");
