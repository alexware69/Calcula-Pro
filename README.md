# Online-Price-System-Core

## Features of the Online Price System

* Is web-based.
* Near instant intelligent updates in web page.
* Product definitions are independent of product quotes and orders, changes to product definitions won’t affect existing quotes.
* Product definitions are a hierarchy of folders, which contain the details, formulas, images and descriptions.
* Folder nature of product definitions makes teamwork easy and natural, as well as making possible to backup and storing definitions offline.
* Readable formulas are made of words.
* Supports most common math expressions and logic conditions.
* Multiple same-name fields are supported (compare to Excel single-name naming of cells)
* Shows node names, formulas, subtotals and node values with respective units and descriptions at the same time.
* Create and modify product definitions via web.
* Upload and download existing product definitions.
* Create and analyze price quotes of complex products.
* Calculations are server-based, which reduces the load on the client computer and keeps formulas and logic safe from viewing and modifying.
* Re-quoting based on the original formulas.
* Shows dependencies and references.
* Two views available, wide screen for easier formula reading and shared screen for easier description viewing.
* Compact mode for cleaner view of selections.
* Seamless adjustment for window resizing and zooming.
* Supports documentation for each feature (node), including text, image and video, in the form of an html page.
* It is fast!

## Node properties

* **Name**: The name of the node. Only one name per level is allowed. It cannot contain the following characters:
'*', '/', '+', '-', '|', '[', ']', '?', '&', '!', '(', ')', '>', '<', '=', ‘:'
* **Type**: Math, Decision, Conditional, Conditional Rules, Range, SumSet, Reference, Date, Today, DateDiff.
* **Units**: There are currently some common predefined units, custom units can be set.
* **Expression**: Some node types require an expression or formula, like Math, Conditional, Conditional Rules, Range and Reference.
* **Expanded Levels**: The number of levels that will be automatically expanded from the root node on product load and from any node on node selection. This is useful to cover the most screen on product load and to show multiple nested options on selection.
* **Order**: Zero by default.The order in which the node appears. It is zero-based.
* **Min**: Zero by default.The minimum value for that node. If a node’s subtotal es less than Min then its value will be replaced with Min.
* **Max**: Zero by default.The maximum value for that node. If a node’s subtotal es greater than Max then its value will be replaced with Max.
* **Discount**: Zero by default. The discount in percent applied to the subtotal.
* **Disable Condition**: An expression with a condition or conditions that when met will disable the node for selection. Very useful to implement exceptions.
Example:Plastic Face\Material Options\Polycardonate .150 Clear.selected
* **Disable Message**: The message that will be shown in the description page when the Disable Condition is met.
* **Optional**: If set the node will have a checkbox. Optional nodes that are not children of Decision nodes when are not selected their displayed value is zero. Optional nodes children of Decision nodes will display their correspondent subtotal. Children of Decision nodes are automatically set to Optional.
* **Hidden**: Hidden nodes are not shown in the page. Also their value can not be changed in the description page.
* **Edit Children**: When set, all child nodes which are Math and their formula is a number (value nodes) will be editable in the description page.
* **Report**: When set, the node name will be shown (reported) in the Quote Details page.
* **Report Value**: When set, the node’s subtotal will be shown beside the node name in the Quote Details page.
* **Template**: When set, the node will be considered a template in order to repeat it multiple times inside a SumSet node. This is useful to let the user add multiple similar “bundles” which may contain different selections to the order.
* **Read Only**: If set for a Math node the node’s value cannot be changed in the description page. This is useful to create value nodes for "display only”.

## Expression Examples

Expressions can contain most common math functions in addition to IF conditions. IF conditions can query node properties like“selected”, “min", “max"and "discount”and can query the current node’s properties through the use of the “this” keyword. Also nodes can be referenced by their id, so "Plastic Face\Length of Plastic” could be referenced as {1.6}.

**Math:**
```
* (Colors+Image Complexity)*(Sq Ft of Plastic-Sq Ft of Digital Face Per Print) 
* Plastic Face\Length of Plastic*Plastic Face\Height of Plastic/144 
* Round(TriFace Sign\BomGen\Approx Copy Size\Length*12/TriFace Sign\BomGen\Prism Centers; 0) 
* {1.6}*{1.5}/144 
```

**Range:**
```
* Height|0:36:42|37:48:54|49:60:66|61:72:78|73:102 
```

**Conditional (IF conditions):**
```
* if(TriFace Sign\Motor Type\1_3 HP.selected,10,60) (discontinued syntax) 
* TriFace Sign\Motor Type\1_3 HP.selected?10:60 (same as above with correct syntax) 
* TriFace Sign\BomGen\Side Frm=4?0.75:0 
* TriFace Sign\BomGen\Side Frm.max> TriFace Sign\Length?0.75:0 
* TriFace Sign\Louver Orientation\Vertical.selected?2*O10:0 
* this.selected?price:0 
```

**Conditional Rules:** Conditional Rules nodes return the value of the first matching rule. If no rule is matched they return zero.  
```
* [TriFace Sign\BomGen\Centers 4_6=4?4][TriFace Sign\BomGen\Centers 4_6=6?5.75] 
```

**Reference:** Reference nodes are used to shorten the formulas, they are local but can get an external node’s properties like selected, min, max, discount and its subtotal value.  
```
* Plastic Face\Height of Plastic
```
