function CheckBoxClickHandler(e,i){let t=e.id,l=t.replace(/ckbx_/g,""),n=$("input[id='"+t+"']").parent().attr("id"),d=n.replace(/li_/g,""),r=$("li[id='"+n+"']").parent().attr("id");if("Decision"==$("input[id='nodetype_"+r.replace(/li_ul_/g,"")+"']").attr("value")&&(e.checked&&$("input[id='Compact']").is(":checked")?$("li[id='"+n+"']").siblings().hide():($("li[id='"+n+"']").siblings().show(),$("li[id='"+n+"']").siblings().each((function(){RefreshFillers(this.id.replace(/li_/g,""),!1)})))),asynchronous=!1,e.checked&&0==$("ul[id='li_ul_"+l+"']").children().length?$.jstree._reference("li[id='li_"+l+"']").open_node("li[id='li_"+l+"']"):(!e.checked&&$("input[id='Compact']").is(":checked")&&$("#container").jstree("close_node",$("li[id='li_"+l+"']")),e.checked&&$("#container").jstree("open_node",$("li[id='li_"+l+"']"))),asynchronous=!0,e.checked){let e=$("li[id='"+n+"']").parent("ul").parent("li");for(;"li_1"!=e.attr("id")&&(!$("input[id='ckbx_"+e.attr("id").replace(/li_/g,"")+"']").length||$("input[id='ckbx_"+e.attr("id").replace(/li_/g,"")+"']").is(":checked"));)e=e.parent("ul").parent("li");"li_1"!=e.attr("id")&&($("input[id='ckbx_"+e.attr("id").replace(/li_/g,"")+"']").attr("checked",!0),CheckBoxClickHandler(document.getElementById("ckbx_"+e.attr("id").replace(/li_/g,"")),!1))}let o=e.checked?"true":"false";if($.ajax({url:"SetCheckboxState?id="+l+"&state="+o,type:"GET",dataType:"json",async:!1,cache:!1,beforeSend:function(){},complete:function(){},success:function(e){$("#price").text("Total: "+e.total);let i=";"+$("input[id='dependents_"+d+"']").attr("value"),t=GetBranch(d).split(";");for(let e=0;e<t.length;e++)-1==i.indexOf(";"+t[e]+";")&&(i=i+";"+t[e]);let l=$("li[id='"+n+"']").find("li");for(let e=0;e<l.length;e++)i+=";"+l[e].id.replace(/li_/g,"");UpdateNodesFromServer(i)}}),e.checked&&"Decision"==$("input[id='nodetype_"+r.replace(/li_ul_/g,"")+"']").attr("value")){let e=$("li[id='"+n+"']").siblings().children("input:checked");if(0!=e.length){let i=";"+e[0].id.replace(/ckbx_/g,""),t=$("li[id='li_"+e[0].id.replace(/ckbx_/g,"")+"']").find("li").filter((function(){return"false"==$("input[id='isoptional_"+$(this).attr("id").replace(/li_/g,"")+"']").attr("value")}));for(let e=0;e<t.length;e++)i+=";"+t[e].id.replace(/li_/g,"");let l=";"+$("input[id='dependents_"+e[0].id.replace(/ckbx_/g,"")+"']").attr("value"),n=l.split(";");for(let e=0;e<n.length;e++)-1==i.indexOf(";"+n[e]+";")&&(i=i+";"+n[e]);i+=l,asynchronous=!0,UpdateNodesFromServer(i)}}e.checked&&!descriptionhidden&&$("li[id='li_"+l+"']").children("a").click(),e.checked||"Decision"!=$("input[id='nodetype_"+r.replace(/li_ul_/g,"")+"']").attr("value")||descriptionhidden||$("li[id='li_"+r.replace(/li_ul_/g,"")+"']").children("a").click(),i&&$("input[id='"+t+"']").is(":checked")&&(asynchronous,ExpandLevels2(l,$("input[id='expandedlevels_"+l+"']").attr("value")))}function UpdateParent(e){$.ajax({url:"NodeInfo?id="+e,type:"GET",dataType:"json",cache:!1,beforeSend:function(){},complete:function(){},success:function(e){let i=$("li[id='li_"+e.id+"']").parent().attr("id");e.complete||e.optional&&!e.selected?$("li[id='li_"+e.id+"']").children("img").hide():$("li[id='li_"+e.id+"']").children("img").show(),UpdateNode(e),"container"!=$("li[id='li_"+e.id+"']").parent().parent().attr("id")&&UpdateParent(i.replace(/li_ul_/g,""))}})}function GetBranch(e){let i="";return function e(t){i+=t+";";let l,n="li[id='li_"+t+"']";"container"!=$(n).parent().parent().attr("id")&&(l=$(n).parent().attr("id"),e(l.replace(/li_ul_/g,"")))}(e),i}function getScrollbarWidth(e){return e.offsetWidth-e.clientWidth}function UpdateNode(e){let i="li[id='li_"+e.id+"']";if($(i).length){let t,l;if($(i).css("gray-space","normal"),$(i).width($("#container").width()-($(i).offset().left-$("#container").offset().left)),$("input[id='ckbx_"+e.id+"']").remove(),e.optional&&(t=1==e.selected?"checked":"",l="<input type='checkbox' ",e.disabled&&($(i).siblings().show(),l+="disabled='true'"),l+=" id='ckbx_"+e.id+"' style='padding:0px 0px 0px 0px; margin:0px 0px 0px 2px;' onchange='CheckBoxClickHandler(this, true);' "+t+"/>",$(i).children("ins").after(l)),l="<img src='../Images/blinkcircle.gif' class='incomplete' style='display: none;' height='14' width='14'>",$(i).children(".incomplete").remove(),$(i).children("ins").after(l),e.complete||e.optional&&!e.selected?$(i).children(".incomplete").hide():$(i).children(".incomplete").show(),0==$(i).children("a").length){$(i).append("<a></a>");let t="../"+e.url;$(i).children("a").attr("href",t),$(i).children("a").attr("target","details")}$(i).children("a").empty(),$(i).children("a").append("<span class='name' id='name_"+e.id+"'>"+e.name+"</span>"),$(i).children("a").append("<span class='formula' id='formula_"+e.id+"' > "+("Decision"!=e.type&&"SumSet"!=e.type&&"Date"!=e.type&&"Today"!=e.type?" &nbsp;[<i>"+e.expression+"</i>]":"")+"</span>"),$("input[id='Formulas']").is(":checked")||$(i).children("a").children(".formula").hide(),$(i).children(".subtotal").remove(),$(i).children(".filler").remove();let n,d,r,o,c,a,s,h,p,f=$("<div class='filler'>&nbsp;</div>"),u=$("<span class='subtotal' id='subtotal_"+e.id+"'><font size='1'></font>  <font>["+e.subtotal+"]</font></span>");$(i).children("a").after(u),$(i).children("a").after(f),window.matchMedia&&window.matchMedia("(prefers-color-scheme: dark)").matches&&$(i).children("a").attr("style","color:gray;"),window.matchMedia&&window.matchMedia("(prefers-color-scheme: light)").matches&&$(i).children("a").attr("style","color:black;"),e.leaf&&$("li[id='li_"+e.id+"']").children("a").attr("style","color:red;"),e.hidden&&$("li[id='li_"+e.id+"']").children("a").attr("style","color:green"),h=e.checkbox?13:0,s=e.complete?0:16;let g=$(i).children("a").children(".formula"),_=$(i).children("a").children(".name");if($(i).width($("#container").width()-($(i).offset().left-$("#container").offset().left)),n=$(i).width(),r=$(i).children("a").width(),idWidth=0,d=0,o=$(i).children(".subtotal").width(),c=$(i).children("ins").width(),"none"==g.css("display")?(g.show(),a=g.width(),g.hide()):a=g.width(),p=_.width(),0==g.children("i").length&&(a=0),r+o+c+idWidth+d+h+s+25>n){let e=r+o+c+idWidth+d+h+s+25-n;a>=e?g.width(a-e):(g.width(0),_.width(p-(e-a)))}else g.width(a+5),_.width(p+5);let m=getScrollbarWidth(document.getElementById("container"));if($(i).children(".filler").css("width",n-r-o-c-idWidth-d-h-s-30-m),n-r-o-c-idWidth-d-h-s-30-m>0&&$(i).children(".filler").css("margin-right",o+m),"FONT"!=$("li[id='li_"+e.id+"']").children()[0].tagName){let t=$("li[id='li_1']").position().left;$("<font width='100%' size='1' class='id'>&nbsp"+e.id+"</font>").insertBefore("li[id='li_"+e.id+"']>ins"),$(".id").css({position:"absolute",left:t});let l=$("font[id='id_"+e.Id+"']").width(),n=$(i).children("ins").position().left;l>n?$(i).css("margin-left",l-n+13.5):$(i).css("margin-left",13.5)}}}function UpdateNodeFromServer(e){$.ajax({url:"NodeInfo?id="+e,type:"GET",dataType:"json",cache:!1,beforeSend:function(){},complete:function(){},success:function(e){e.complete||e.optional&&!e.selected?$("li[id='li_"+e.id+"']").children("img").hide():$("li[id='li_"+e.id+"']").children("img").show(),UpdateNode(e)}})}function UpdateNodesFromServer(e){let i=[];i=e.split(";"),$.ajax({url:"NodesInfo",type:"POST",dataType:"json",data:{array:i},traditional:!0,async:"true"==asynchronous,cache:!1,beforeSend:function(){},complete:function(){},success:function(e){let i="",t="";for(let l=0;l<e.length;l++)e[l].complete||e[l].optional&&!e[l].selected?t+="li[id='li_"+e[l].id+"'],":i+="li[id='li_"+e[l].id+"'],",UpdateNode(e[l])}})}function UpdateDependents(e){if("object"==typeof e){let i,t="";for(let i=0;i<e.length;i++)t+=";"+e[i]+";"+$("input[id='dependents_"+e[i]+"']").attr("value");i=t.split(";"),i=i.filter((function(e,t){return i.indexOf(e)===t})),t="";for(let e=0;e<i.length;e++)t+=i[e]+";";UpdateNodesFromServer(t)}else{UpdateNodesFromServer(e+";"+$("input[id='dependents_"+e+"']").attr("value"))}}function UpdateTree(e){$.ajax({url:"NodeInfo?id="+e,type:"GET",dataType:"json",cache:!1,beforeSend:function(){},complete:function(){},success:function(e){e.complete||e.optional&&!e.selected?$("li[id='li_"+e.id+"']").children("img").hide():$("li[id='li_"+e.id+"']").children("img").show(),UpdateNode(e);let i="li[id='li_"+e.id+"']",t=$(i).children("ul").children("li");if(t&&t.length>0)for(let e=0;e<t.length;e++)UpdateTree(t[e].id.replace(/li_/g,""))}})}function UpdateTreeSync(){let e="";!function i(t){e+=t+";";let l=$("li[id='li_"+t+"']").children("ul").children("li");if(l&&l.length>0)for(let e=0;e<l.length;e++)i(l[e].id.replace(/li_/g,""))}(1),UpdateNodesFromServer(e)}function HideNodes(e){"true"==$("input[id='ishidden_"+e+"']").attr("value")&&$("li[id='li_"+e+"']").hide();let i=$("li[id='li_"+e+"']").children("ul").children("li");if(i&&i.length>0)for(let e=0;e<i.length;e++)HideNodes(i[e].id.replace(/li_/g,""))}function ShowNodes(e){"true"==$("input[id='ishidden_"+e+"']").attr("value")&&($("li[id='li_"+e+"']").css("display","block"),RefreshFillers(e,!1));let i=$("li[id='li_"+e+"']").children("ul").children("li");if(i&&i.length>0)for(let e=0;e<i.length;e++)ShowNodes(i[e].id.replace(/li_/g,""))}function UnHideBranch(e){$("li[id='li_"+e+"']").show();let i=$("li[id='li_"+e+"']").parent().attr("id");"container"!=$("li[id='li_"+e+"']").parent().parent().attr("id")&&UnHideBranch(i.replace(/li_ul_/g,""))}function ToggleSubtotals(){$("input[id='Subtotals']").is(":checked")?($(".subtotal").show(),$(".filler").show(),RefreshFillers(1,!0)):($(".subtotal").hide(),$(".filler").hide())}function ToggleFormulas(){expandingLevels=!1,$("input[id='Formulas']").is(":checked")?($(".formula").show(),RefreshFillers("1",!0)):($(".formula").hide(),RefreshFillers("1",!0))}function ToggleFormulasAsync(){expandingLevels=!1,$("input[id='Formulas']").is(":checked")?($(".filler").width(0),$(".formula").width(0),$(".formula").show(),UpdateTree("1")):($(".formula").hide(),UpdateTree("1"))}function ToggleHiddenFields(){expandingLevels=!1,$("input[id='HiddenFields']").is(":checked")?ShowNodes("1"):HideNodes("1")}function ToggleDescription(){if(expandingLevels=!1,descriptionhidden=!descriptionhidden,$("#description").toggle(),"right"==$("#container").css("float")?($("#container").css("float","left"),$("#container").css("width","100%")):($("#container").css("float","right"),$("#container").css("width","60%")),"left"==$("#container").css("float")?$("#hr").css("width","100%"):$("#hr").css("width","59.5%"),RefreshFillers(1,!0),!descriptionhidden){let e=document.getElementById("Iframe15"),i=e.contentDocument||e.contentWindow.document;i.location=i.location}}function ToggleDescriptionAsync(){if(expandingLevels=!1,descriptionhidden=!descriptionhidden,$("#description").toggle(),"right"==$("#container").css("float")?($("#container").css("float","left"),$("#container").css("width","100%")):($("#container").css("float","right"),$("#container").css("width","60%")),"left"==$("#container").css("float")?$("#hr").css("width","100%"):$("#hr").css("width","59.5%"),$(".filler").width(0),$(".formula").width(0),UpdateTree("1"),!descriptionhidden){let e=document.getElementById("Iframe15"),i=e.contentDocument||e.contentWindow.document;i.location=i.location}}function ToggleCompact(){if(expandingLevels=!1,$("input[id='Compact']").is(":checked")){$("#container li").filter((function(){let e=$(this).parent().attr("id");return null!=e&&"Decision"==$("input[id='nodetype_"+e.replace(/li_ul_/g,"")+"']").attr("value")&&!$(this).children(":checkbox").is(":checked")&&1==$(this).siblings().children("input:checked").length})).hide()}else{let e=$("#container li").filter((function(){return"none"==$(this).css("display")&&"false"==$("input[id='ishidden_"+$(this).attr("id").replace(/li_/g,"")+"']").attr("value")}));e.show();for(let i=0;i<e.length;i++)RefreshFillers(e[i].id.replace(/li_/g,""),!1)}}function ToggleId(){$("input[id='Id']").is(":checked")?ShowIds():HideIds()}function ShowIds(){$(".id").show()}function HideIds(){$(".id").hide()}function EnlargeFillers(e){let i="li[id='li_"+e+"']";$(i).children(".filler").css("width",$(i).children(".filler").width()+$(i).children("a").children(".formula").width());let t=$(i).children("ul").children("li");if(t&&t.length>0)for(let e=0;e<t.length;e++)EnlargeFillers(t[e].id.replace(/li_/g,""))}function ShortenFillers(e){let i="li[id='li_"+e+"']";$(i).children(".filler").css("width",$(i).children(".filler").width()-$(i).children("a").children(".formula").width());let t=$(i).children("ul").children("li");if(t&&t.length>0)for(let e=0;e<t.length;e++)ShortenFillers(t[e].id.replace(/li_/g,""))}function Refill(){RefreshFillers(1,!0)}function RefreshFillers(e,i){let t="li[id='li_"+e+"']";if($(t).length){$(t).width($("#container").width()-($(t).offset().left-$("#container").offset().left));let l=$(t).children("a").children(".name").text(),n=$(t).children("a").children(".formula").children("i").text(),d=($(t).children(".subtotal").children("font").eq(0).text(),$(t).children(".subtotal").children("font").eq(1).text());$(t).children("a").empty(),$(t).children("a").append("<span class='name' id='name_"+e+"'>"+l+"</span>"),$(t).children("a").append("<span class='formula' id='formula_"+e+"' >"+("Decision"!=$("input[id='nodetype_"+e+"']").attr("value")&&"Date"!=$("input[id='nodetype_"+e+"']").attr("value")&&"Today"!=$("input[id='nodetype_"+e+"']").attr("value")?" &nbsp;[<i>"+n.trim()+"</i>]":"")+"</span>"),$("input[id='Formulas']").is(":checked")||$(t).children("a").children(".formula").hide(),$(t).children(".subtotal").remove(),$(t).children(".filler").remove();let r,o,c,a,s,h,p,f,u=$("<div class='filler'>&nbsp;</div>"),g=$("<span class='subtotal' id='subtotal_"+e+"'><font size='1'></font> <font>"+d+"</font></span>");$(t).children("a").after(g),$(t).children("a").after(u),p=$("input[id='ckbx_"+e+"']").length?13:0,h=$(t).children("img").is(":visible")?16:0;let _=$(t).children("a").children(".formula"),m=($(t).children("a"),$(t).children("a").children(".name"));if(r=$(t).width(),o=$(t).children("a").width(),c=$(t).children(".subtotal").width(),a=$(t).children("ins").width(),"none"==_.css("display")?(_.show(),s=_.width(),_.hide()):s=_.width(),f=m.width(),o+c+a+p+h+25>r){let e=o+c+a+p+h+25-r;"none"!=_.css("display")&&s>=e+25?_.width(s-e-25):(_.width(0),"none"!=_.css("display")?m.width(f-(e-s)):m.width(f-e))}else"none"!=_.css("display")&&_.width(s+5),m.width(f+5);let w=getScrollbarWidth(document.getElementById("container"));if($(t).children(".filler").css("width",r-o-c-a-p-h-25-w),r-o-c-a-p-h-25-w>0&&$(t).children(".filler").css("margin-right",c+w),1==i){let e=$(t).children("ul").children("li");if(e&&e.length>0)for(let i=0;i<e.length;i++)RefreshFillers(e[i].id.replace(/li_/g,""),!0)}}$("input[id='Subtotals']").is(":checked")||$(".subtotal, .filler").hide()}function OpenNode(e){let i=e.split(">"),t="li[id='li_1']";for(let e=0;e<i.length;e++)top.asynchronous=!1,$.jstree._reference(t).open_node(t,(function(){if(e==i.length-1)$.jstree._reference("li[id='li_@Model.id']").deselect_node("li[id='li_@Model.id']"),$.jstree._reference(t).select_node(t),$("html").scrollTop($(t).offset().top);else{let l=$(t).children("ul").children("li");for(let n=0;n<l.length;n++)if($(l[n]).children("a").children("font:first").text()==i[e+1]){t="li[id='"+$(l[n]).attr("id")+"']";break}}}));top.asynchronous=!0}function ExpandLevels(e,i){0!=i&&(asynchronous=!1,$.jstree._reference("li[id='li_"+e+"']").open_node("li[id='li_"+e+"']",(function(){let t=$("li[id='li_"+e+"']").children("ul").children("li");if(t&&t.length>0)for(let e=0;e<t.length;e++)ExpandLevels(t[e].id.replace(/li_/g,""),i-1)})),asynchronous=!0)}function ExpandLevels2(e,i){if(expandingLevels=!0,0!=i){let t=$("li[id='li_"+e+"']").children("ul");if(0==t.length||0==t.children("li").length)$.ajax({url:"ChildNodes?id="+e,type:"GET",dataType:"json",cache:!1,beforeSend:function(){},complete:function(e){let t=jQuery.parseJSON(e.responseText);for(let e=0;e<t.length;e++)ExpandLevels2(t[e].id,i-1)},success:function(i){Assemble(i,"li_"+e),$("#container").jstree("open_node",$("li[id='li_"+e+"']"))}});else{$("#container").jstree("open_node",$("li[id='li_"+e+"']")),RefreshFillers(e,!0);let t=$("li[id='li_"+e+"']").children("ul").children("li");for(let e=0;e<t.length;e++)ExpandLevels2(t[e].id.replace(/li_/g,""),i-1)}}}function Assemble(e,i){let t=i;$(i).children("ul");$("#container").jstree("select_node",$("li[id='"+t+"']"));for(let i=0;i<e.length;i++){$("#container").jstree("create",null,"last",{data:" ",attr:{class:"jstree-closed",id:"li_"+e[i].id}},!1,!0),UpdateNode(e[i]),1!=e[i].hidden||$("input[id='HiddenFields']").is(":checked")||$("li[id='li_"+e[i].id+"']").hide();let t,l=e[i].url;$("li[id='li_"+e[i].id+"']").children("a").attr("href",l),$("li[id='li_"+e[i].id+"']").children("a").attr("target","details"),window.matchMedia&&window.matchMedia("(prefers-color-scheme: dark)").matches&&$("li[id='li_"+e[i].id+"']").children("a").attr("style","color:gray;"),e[i].leaf&&$("li[id='li_"+e[i].id+"']").children("a").attr("style","color:red;"),e[i].hidden&&$("li[id='li_"+e[i].id+"']").children("a").attr("style","color:green"),t="<input type='hidden' id='nodetype_"+e[i].id+"' value='"+e[i].type+"'/> ",$("li[id='li_"+e[i].id+"']").append(t),t="<input type='hidden' id='ishidden_"+e[i].id+"' value='"+e[i].hidden+"'/> ",$("li[id='li_"+e[i].id+"']").append(t),t="<input type='hidden' id='isoptional_"+e[i].id+"' value='"+e[i].optional+"'/> ",$("li[id='li_"+e[i].id+"']").append(t),t="<input type='hidden' id='dependents_"+e[i].id+"' value='"+e[i].dependents+"'/> ",$("li[id='li_"+e[i].id+"']").append(t),t="<input type='hidden' id='editchildren_"+e[i].id+"' value='"+e[i].editChildren+"'/> ",$("li[id='li_"+e[i].id+"']").append(t),t="<input type='hidden' id='expandedlevels_"+e[i].id+"' value='"+e[i].expandedLevels+"'/> ",$("li[id='li_"+e[i].id+"']").append(t)}$("li[id='"+t+"']").children("ul").attr("id","li_ul_"+t.replace(/li_/g,"")),$("#container").jstree("deselect_node",$("li[id='"+t+"']"))}function RenderTree(e){let i=e.Id.substr(0,e.Id.lastIndexOf("."));1!=e.Id&&$("#container").jstree("select_node",$("li[id='li_"+i+"']")),$("#container").jstree("create",null,"last",{data:" ",attr:{class:"jstree-closed",id:"li_"+e.Id}},!1,!0);let t="li[id='li_"+e.Id+"']";if($(t).length){let i,l;$(t).css("gray-space","normal"),$(t).width($("#container").width()-($(t).offset().left-$("#container").offset().left)),$("input[id='ckbx_"+e.Id+"']").remove(),e.Optional&&(i=1==e.Selected?"checked":"",l="<input type='checkbox' ",e.Disabled&&($(t).siblings().show(),l+="disabled='true'"),l+=" id='ckbx_"+e.Id+"' style='padding:0px 0px 0px 0px; margin:0px 0px 0px 2px;' onchange='CheckBoxClickHandler(this, true);' "+i+"/>",$(t).children("ins").after(l)),l="<img src='../Images/blinkcircle.gif' class='incomplete' style='display: none;' height='14' width='14'>",$(t).children(".incomplete").remove(),$(t).children("ins").after(l),e.Complete||e.Optional&&!e.Selected?$(t).children(".incomplete").hide():$(t).children(".incomplete").show();let n="../"+e.Url;$(t).children("a").attr("href",n),$(t).children("a").attr("target","details"),$(t).children("a").empty(),$(t).children("a").append("<span class='name' id='name_"+e.Id+"'>"+e.Name+"</span>");let d="";d=null!=e.Expression||"ConditionalRules"==e.TypeStr?"&nbsp;[<i>"+e.Expression.trim()+"</i>]":"",$(t).children("a").append("<span class='formula' id='formula_"+e.Id+"' > "+d+"</span>"),$("input[id='Formulas']").is(":checked")||$(t).children("a").children(".formula").hide(),!$("input[id='HiddenFields']").is(":checked")&&e.Hidden&&$(t).hide();let r,o,c,a,s,h,p,f,u,g=$("<div class='filler'>&nbsp;</div>"),_=$("<span class='subtotal' id='subtotal_"+e.Id+"'><font size='1'></font>  <font>["+e.TotalStr+"]</font></span>");$(t).children("a").after(_),$(t).children("a").after(g),f=$("input[id='ckbx_"+e.Id+"']").length?13:0,p=$(t).children("img").is(":visible")?16:0;let m=$(t).children("a").children(".formula"),w=$(t).children("a").children(".name");if($(t).width($("#container").width()-($(t).offset().left-$("#container").offset().left)),r=$(t).width(),c=$(t).children("a").width(),idWidth=0,o=0,a=$(t).children(".subtotal").width(),s=$(t).children("ins").width(),"none"==m.css("display")?(m.show(),h=m.width(),m.hide()):h=m.width(),u=w.width(),0==m.children("i").length&&(h=0),c+a+s+idWidth+o+f+p+25>r){let e=c+a+s+idWidth+o+f+p+25-r;h>=e+25?m.width(h-e-25):(m.width(0),w.width(u-(e-h)))}else m.width(h+5),w.width(u+5);let y=getScrollbarWidth(document.getElementById("container"));$(t).children(".filler").css("width",r-c-a-s-idWidth-o-f-p-25-y),r-c-a-s-idWidth-o-f-p-25-y>0&&$(t).children(".filler").css("margin-right",a+y);let b=$("li[id='li_1']").position().left;$("<font width='100%' size='1' class='id' id = 'id_"+e.Id+"'>&nbsp"+e.Id+"</font>").insertBefore("li[id='li_"+e.Id+"']>ins"),$(".id").css({position:"absolute",left:b});let x=$("font[id='id_"+e.Id+"']").width(),v=$(t).children("ins").position().left;x>v?$(t).css("margin-left",x-v+13.5):$(t).css("margin-left",13.5),$("input[id='Subtotals']").is(":checked")||$(".subtotal, .filler").hide(),d="Decision"!=e.TypeStr&&"SumSet"!=e.TypeStr&&"ConditionalRules"!=e.TypeStr&&"Date"!=e.TypeStr&&"Today"!=e.TypeStr&&"Text"!=e.TypeStr||"ConditionalRules"==e.TypeStr||"Text"==e.TypeStr?"&nbsp;[<i>"+e.Expression+"</i>]":" &nbsp;&nbsp;&nbsp;",window.matchMedia&&window.matchMedia("(prefers-color-scheme: dark)").matches&&"rgb(255, 0, 0)"!=$(t).children("a").css("color")&&"rgb(0, 128, 0)"!=$(t).children("a").css("color")&&$(t).children("a").attr("style","color:gray;"),window.matchMedia&&window.matchMedia("(prefers-color-scheme: light)").matches&&"rgb(255, 0, 0)"!=$(t).children("a").css("color")&&"rgb(0, 128, 0)"!=$(t).children("a").css("color")&&$(t).children("a").attr("style","color:black;"),e.Leaf&&$(t).children("a").attr("style","color:red;"),e.Hidden&&$(t).children("a").attr("style","color:green"),l="<input type='hidden' id='nodetype_"+e.Id+"' value='"+e.TypeStr+"'/> ",$(t).append(l),l="<input type='hidden' id='ishidden_"+e.Id+"' value='"+e.Hidden+"'/> ",$(t).append(l),l="<input type='hidden' id='isoptional_"+e.Id+"' value='"+e.Optional+"'/> ",$(t).append(l),l="<input type='hidden' id='dependents_"+e.Id+"' value='"+e.DependentsStr+"'/> ",$(t).append(l),l="<input type='hidden' id='editchildren_"+e.Id+"' value='"+e.EditChildren+"'/> ",$(t).append(l),l="<input type='hidden' id='expandedlevels_"+e.Id+"' value='"+e.ExpandedLevels+"'/> ",$(t).append(l),0==$(t).children("ul").length&&$(t).append("<ul id='li_ul_"+e.Id+"' style='display:inline'></ul>")}if($("#container").jstree("deselect_node",$("li[id='li_"+i+"']")),null!=e._Children)for(let i=0;i<e._Children.length;i++)null!=e._Children[i].Id&&RenderTree(e._Children[i]);$("#container").jstree("deselect_node",$("li[id='li_"+i+"']"))}function PruneTree(e,i){let t=e.Id.split(".").length-1;if("1"==e.Id||t!=i.ExpandedLevels)for(let t=0;t<e._Children.length;t++)PruneTree(e._Children[t],i);else e._Children=null}function lzw_decode(e){let i,t={},l=(e+"").split(""),n=l[0],d=n,r=[n],o=256;for(let e=1;e<l.length;e++){let c=l[e].charCodeAt(0);i=c<256?l[e]:t[c]?t[c]:d+n,r.push(i),n=i.charAt(0),t[o]=d+n,o++,d=i}return r.join("")}$((function(){descriptionhidden=!1,ajaxDone=!1,asynchronous=!0,mobile=!1,treeIsLoaded=!1,expandingLevels=!1,dialogHeight=98*window.innerHeight/100,$("#container").jstree({plugins:["themes","html_data","ui","crrm"],themes:{theme:"default",dots:!1,icons:!1}}).bind("loaded.jstree",(function(e,i){ExpLevels=0,$.ajax({url:"getJson",type:"GET",dataType:"json",cache:!1,beforeSend:function(){},complete:function(){},success:function(e){let i=JSON.parse(e);PruneTree(i,i),RenderTree(i),RefreshFillers(1,!1),treeIsLoaded=!0,descriptionOffsetTop=$("#description").offset().top,$("#Iframe15").height($(window).height()-descriptionOffsetTop-70),$("#container").height($(window).height()-descriptionOffsetTop-40)}})})),$("#container").bind("open_node.jstree",(function(e,i){let t=i.rslt.obj.find("ul"),l=i.rslt.obj.attr("id");0==t.length||0==$(t).children("li").length?asynchronous?$.ajax({url:"ChildNodes?id="+l,type:"GET",dataType:"json",cache:!1,beforeSend:function(){},complete:function(e){},success:function(e){Assemble(e,l)}}):$.ajax({url:"ChildNodes?id="+l,type:"GET",dataType:"json",async:!1,cache:!1,beforeSend:function(){},complete:function(e){},success:function(e){Assemble(e,l)}}):treeIsLoaded&&!expandingLevels&&RefreshFillers(l.replace(/li_/g,""),!0)})),$("#container").bind("select_node.jstree",(function(e,i){i.args[0].href&&(document.getElementById("Iframe15").src=i.args[0].href)})),/Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini/i.test(navigator.userAgent)&&(mobile=!0),$(window).on("resize",(function(){let i=dialog.dialog("isOpen");if(dialogHeight=98*window.innerHeight/100,dialog.dialog("close"),dialog=$("<div></div>").append(e).appendTo("body").dialog({autoOpen:!1,modal:!0,resizable:!1,width:"auto",height:"auto",maxHeight:dialogHeight,title:"Description",close:function(){e.attr("src","about:blank")}}).css({overflow:"auto"}),$(window).width()<"1024"&&$("input[id='Description']").is(":checked")&&(descriptionOffsetTop=$("#description").offset().top,$("input[id='Description']").click()),$(window).width()>="1024"&&!$("input[id='Description']").is(":checked")?($("input[id='Description']").click(),descriptionOffsetTop=$("#description").offset().top):$("input[id='Description']").is(":checked")&&(descriptionOffsetTop=$("#description").offset().top),$("#description").is(":hidden")&&i){let i=document.getElementById("Iframe15").contentWindow.location.href,t=mobile?"100%":40*$("#hr").width()/100,l=t;e.attr({width:+t,height:+l,src:i}),dialog.dialog("open")}Refill(),containerOffsetTop=$("#container").offset().top,$("#Iframe15").height($(window).height()-descriptionOffsetTop-70),$("#container").height($(window).height()-containerOffsetTop)}));let e=$('<iframe style="border:0;" frameborder="0" marginwidth="0" marginheight="0" allowfullscreen></iframe>');dialog=$("<div></div>").append(e).appendTo("body").dialog({autoOpen:!1,modal:!0,resizable:!1,width:"auto",height:"auto",maxHeight:dialogHeight,title:"Description",close:function(){e.attr("src","about:blank")}}).css({overflow:"auto"}),$("#Iframe15").on("load",(function(i){if(descriptionhidden){i.preventDefault();let t=document.getElementById("Iframe15").contentWindow.location.href,l=mobile?"100%":40*$("#hr").width()/100,n=l;e.attr({width:+l,height:+n,src:t}),dialog.dialog("open")}})),$(window).width()<"1024"&&$("input[id='Description']").click()}));