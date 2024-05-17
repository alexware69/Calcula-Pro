
var iframeids = ["Iframe2"];
     var iframehide = "yes";
     var getFFVersion = navigator.userAgent.substring(navigator.userAgent.indexOf("Firefox")).split("/")[1];
     var FFextraHeight = parseFloat(getFFVersion) >= 0.1 ? 16 : 0;
     function resizeCaller() {
         var dyniframe = new Array();
         for (i = 0; i < iframeids.length; i++) {
             if (document.getElementById) resizeIframe(iframeids[i]);
             if ((document.all || document.getElementById) && iframehide == "no") {
                 var tempobj = document.all ? document.all[iframeids[i]] : document.getElementById(iframeids[i]);
                 tempobj.style.display = "block";
             } 
         } 
     }
     function resizeIframeByElement(frame) {
         var currentfr = frame;
         if (currentfr) {
             currentfr.style.display = "block";
             if (currentfr.contentDocument && currentfr.contentDocument.body.offsetHeight) currentfr.height = currentfr.contentDocument.body.offsetHeight;
             else if (currentfr.Document && currentfr.Document.body.scrollHeight) currentfr.height = document.getElementById('ctl00_ContentPlaceHolder1_DescriptionFrame').Document.body.scrollHeight + 5;
         } 
     }
     function resizeIframe(frameid) {
         var currentfr = document.getElementById(frameid);
         if (currentfr) {
             currentfr.style.display = "block";
             if (currentfr.contentDocument && currentfr.contentDocument.body.offsetHeight) currentfr.height = currentfr.contentDocument.body.offsetHeight + FFextraHeight;
             else if (currentfr.Document && currentfr.Document.body.scrollHeight) currentfr.height = currentfr.Document.body.scrollHeight;
             if (currentfr.addEventListener) currentfr.addEventListener("load", readjustIframe, false);
             else if (currentfr.attachEvent) {
                 currentfr.detachEvent("onload", readjustIframe);
                 currentfr.attachEvent("onload", readjustIframe);
             } 
         }
         //var dif = document.body.scrollHeight - window.frameElement.height;
         //window.frameElement.height = document.body.scrollHeight - 22;
     }
     function readjustIframe(loadevt) {
         var crossevt = (window.event) ? event : loadevt;
         var iframeroot = (crossevt.currentTarget) ? crossevt.currentTarget : crossevt.srcElement;
         if (iframeroot) resizeIframe(iframeroot.id);
     }
     function loadintoIframe(iframeid, url) {
         if (document.getElementById) document.getElementById(iframeid).src = url;
     }
     if (window.addEventListener) window.addEventListener("load", resizeCaller, false);
     else if (window.attachEvent) window.attachEvent("onload", resizeCaller);
     else window.onload = resizeCaller;
     
     //resize frame when resizing the window
     if (window.addEventListener) window.addEventListener("resize", resizeCaller, false);
     else if (window.attachEvent) window.attachEvent("onresize", resizeCaller);
     else window.onresize = resizeCaller;
 
 
    function maximizeWindow( ) {
    var offset = (navigator.userAgent.indexOf("Mac") != -1 || 
                  navigator.userAgent.indexOf("Gecko") != -1 || 
                  navigator.appName.indexOf("Netscape") != -1) ? 0 : 4;
    window.moveTo(-offset, -offset);
    window.resizeTo(screen.availWidth + (2 * offset), 
                   screen.availHeight + (2 * offset));
}
