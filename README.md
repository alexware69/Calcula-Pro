<img align="left" src="https://github.com/alexware69/Calcula_Pro/blob/main/Images/ops.png" width="200" />

### Calcula Pro
[![Latest Release](https://img.shields.io/github/release/alexware69/Calcula-Pro.svg)](https://github.com/alexware69/Calcula-Pro/releases/latest)
[![License](https://img.shields.io/github/license/alexware69/Calcula-Pro.svg)](https://github.com/alexware69/Calcula-Pro/blob/master/LICENSE)
[![Github All Releases](https://img.shields.io/github/downloads/alexware69/Calcula-Pro/total.svg)](https://github.com/alexware69/Calcula-Pro/releases)
[![Github All Releases](https://img.shields.io/badge/platform-cross˗platform-blue)](https://github.com/alexware69/Calcula-Pro/releases)

<a href="https://buymeacoffee.com/alexware69">
    <img align="right" src="https://github.com/alexware69/Calcula_Pro/blob/main/Images/buy-me-a-coffee.jpeg" style="width: 100px; height: 100px; border: 2px solid black; margin-right: 10px;" />
</a>

<div style="text-align: right">
    
Calcula Pro is an innovative desktop app. It helps in **designing**, **implementing**, **documenting**, **presenting**, **quoting** and **storing** unfinished goods, insurance premiums, taxes and many others.
</div>

[![Get it from the Snap Store](https://snapcraft.io/static/images/badges/en/snap-store-black.svg)](https://snapcraft.io/calcula-pro)

#
> [!NOTE] 
> To open Calcula Pro in "Quote Only" mode, create a shortcut with this command and place it on the desktop. Set permissions as needed.  
> For **Windows**: ```"Calcula Pro.exe" /args --quoteonly```  
> For **MacOS**: ```open /Applications/Calcula\ Pro.app --args -quoteonly```

<img src="https://img1.wsimg.com/isteam/ip/223ae777-4571-4ca4-8b7f-77ff0ee746ab/Screenshot%202024-04-20%20at%2010.45.46%E2%80%AFAM.png"/>
<p align="center"><em>This is an example product definition of an advertising sign.</em></p>

#
<img src="https://img1.wsimg.com/isteam/ip/223ae777-4571-4ca4-8b7f-77ff0ee746ab/Calcula%20Pro-dark-be76945.png"/>
<p align="center"><em>With dark mode applied.</em></p>

## Features of Calcula Pro

* It is web-based.
* Near instant intelligent updates in web page.
* Product definitions are independent of product quotes and orders, changes to product definitions won’t affect existing quotes.
* Product definitions are a hierarchy of folders, which contain the details, formulas, images and descriptions.
* Folder nature of product definitions makes teamwork easy and natural, as well as making possible to backup and storing definitions offline.
* Readable formulas are made of words, now with autocomplete.
* Supports most common math expressions and logic conditions.
* Multiple same-name nodes are supported (compare to Excel single-name naming of cells)
* UTF-16 characters in node names and inside expressions.
* Shows node names, formulas, subtotals and node values with respective units and descriptions at the same time.
* Create and analyze price quotes of complex products.
* Shows dependencies and references.
* Two views available, wide screen for easier formula reading and shared screen for easier description viewing.
* Compact mode for cleaner view of selections.
* True dark mode.
* Seamless adjustment for window resizing and zooming.
* Supports documentation for each feature (node), including text, image and video, in the form of an html page.
* It is fast!

## Node properties

* **Name**: The name of the node. Only one name per level is allowed. It cannot contain the following characters:
```
* / \ + - | [ ] ? & ! ( ) > < = # { } ; : , _
```
* **Type**: Math, Decision, Conditional, Text, SumSet, Reference, Date, Today.
* **Units**: There are currently some common predefined units, custom units can be set.
* **Decimal Places**: Decimal places of the subtotal shown.
* **Expression**: Some node types require an expression or formula, like Math, Conditional and Reference.
* **Expanded Levels**: The number of levels that will be automatically expanded from the root node on product load and from any node on node selection. This is useful to cover the most screen on product load and to show multiple nested options on selection.
* **Order**: Zero by default.The order in which the node appears. It is zero-based.
* **Min**: The minimum value for that node. If a node’s subtotal es less than Min then its value will be replaced with Min.
* **Max**: The maximum value for that node. If a node’s subtotal es greater than Max then its value will be replaced with Max.
* **Discount**: Zero by default. The discount in percent applied to the subtotal.
* **Disable Condition**: An expression with a condition or conditions that when met will disable the node for selection. Very useful to implement exceptions.
Example:Plastic Face\Material Options\Polycarbonate .150 Clear.selected
* **Disable Message**: The message that will be shown in the description page when the Disable Condition is met.
* **Optional**: If set the node will have a checkbox. Optional nodes that are not children of Decision nodes when are not selected their displayed value is zero. Optional nodes children of Decision nodes will display their correspondent subtotal. Children of Decision nodes are automatically set to Optional.
* **Hidden**: Hidden nodes are not shown in the page. Also their value can not be changed in the description page.
* **Edit Children**: When set, all child nodes which are Math and their formula is a number (value nodes) will be editable in the description page.
* **Report**: When set, the node name will be shown (reported) in the Quote Details page.
* **Report Value**: When set, the node’s subtotal will be shown beside the node name in the Quote Details page.
* **Template**: When set, the node will be considered a template in order to repeat it multiple times inside a SumSet node. This is useful to let the user add multiple similar "bundles" which may contain different selections to the order.
* **Read Only**: If set for a Math node the node’s value cannot be changed in the description page. This is useful to create value nodes for "display only”.

## Expression Examples

Expressions can contain most common math functions in addition to IF conditions. IF conditions can query node properties like "selected", "min", "max" and "discount" and can query the current node’s properties through the use of the "this" keyword. Also nodes can be referenced by their id, so "Plastic Face\Length of Plastic" could be referenced as {1.6}.

**Math:**
```
* (Colors+Image Complexity)*(Sq Ft of Plastic-Sq Ft of Digital Face Per Print)
* Plastic Face\Length of Plastic*Plastic Face\Height of Plastic/144
* Round(TriFace Sign\BomGen\Approx Copy Size\Length*12/TriFace Sign\BomGen\Prism Centers, 0)
* {1.6}*{1.5}/144
```

**Conditional (IF conditions):**
```
* TriFace Sign\Motor Type\1_3 HP.selected?10:60
* TriFace Sign\BomGen\Side Frm=4?0.75:0
* TriFace Sign\BomGen\Side Frm.max> TriFace Sign\Length?0.75:0
* TriFace Sign\Louver Orientation\Vertical.selected?2*O10:0
* this.selected?price:0
```

**Reference:** Reference nodes are used to shorten the formulas, they are local but can get an external node’s properties like selected, min, max, discount and its subtotal value.  
```
* Plastic Face\Height of Plastic
```
## Supported Math Functions

| Name          | Description                                                                                                                                                                                                  | Usage               | Result |
|---------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|---------------------|--------|
| Abs           | Returns the absolute value of a specified number.                                                                                                                                                            | Abs(-1)             | 1M     |
| Acos          | Returns the angle whose cosine is the specified number.                                                                                                                                                      | Acos(1)             | 0d     |
| Asin          | Returns the angle whose sine is the specified number.                                                                                                                                                        | Asin(0)             | 0d     |
| Atan          | Returns the angle whose tangent is the specified number.                                                                                                                                                     | Atan(0)             | 0d     |
| Ceiling       | Returns the smallest integer greater than or equal to the specified number.                                                                                                                                  | Ceiling(1.5)        | 2d     |
| Cos           | Returns the cosine of the specified angle.                                                                                                                                                                   | Cos(0)              | 1d     |
| Exp           | Returns e raised to the specified power.                                                                                                                                                                     | Exp(0)              | 1d     |
| Floor         | Returns the largest integer less than or equal to the specified number.                                                                                                                                      | Floor(1.5)          | 1d     |
| IEEERemainder | Returns the remainder resulting from the division of a specified number by another specified number.                                                                                                         | IEEERemainder(3, 2) | 1d    |
| Log           | Returns the logarithm of a specified number.                                                                                                                                                                 | Log(1, 10)          | 0d     |
| Log10         | Returns the base 10 logarithm of a specified number.                                                                                                                                                         | Log10(1)            | 0d     |
| Max           | Returns the larger of two specified numbers.                                                                                                                                                                 | Max(1, 2)           | 2      |
| Min           | Returns the smaller of two numbers.                                                                                                                                                                          | Min(1, 2)           | 1      |
| Pow           | Returns a specified number raised to the specified power.                                                                                                                                                    | Pow(3, 2)           | 9d     |
| Round         | Rounds a value to the nearest integer or specified number of decimal places. The mid number behaviour can be changed by using EvaluateOption.RoundAwayFromZero during construction of the Expression object. | Round(3.222, 2)     | 3.22d  |
| Sign          | Returns a value indicating the sign of a number.                                                                                                                                                             | Sign(-10)           | -1     |
| Sin           | Returns the sine of the specified angle.                                                                                                                                                                     | Sin(0)              | 0d     |
| Sqrt          | Returns the square root of a specified number.                                                                                                                                                               | Sqrt(4)             | 2d     |
| Tan           | Returns the tangent of the specified angle.                                                                                                                                                                  | Tan(0)              | 0d     |
| Truncate      | Calculates the integral part of a number.                                                                                                                                                                    | Truncate(1.7)       | 1      |
| DayDiff      | Calculates the difference in days between two dates.                                                                                                                                                                    | DayDiff(#04/06/1999#,#03/12/2024#)       | 9107      |
| MonthDiff      | Calculates the difference in months between two dates.                                                                                                                                                                    | MonthDiff(#04/06/1999#,#03/12/2024#)       | 299      |
| YearDiff      | Calculates the difference in years between two dates.                                                                                                                                                                    | YearDiff(#04/06/1999#,#03/12/2024#)       | 24      |

