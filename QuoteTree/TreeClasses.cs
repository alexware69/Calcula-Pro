using System.Collections.Specialized;
using Newtonsoft.Json;
using NCalc;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.Text;
using System.Xml.Serialization;

namespace QuoteTree;
[Serializable]
public enum NodeType { Math, Decision, Text, Conditional, SumSet, Reference, Date, Today }

public interface INode
{
    string Name
    {
        get;
        set;
    }

    string Id
    {
        get;
        set;
    }

    NodeType Type
    {
        get;
        set;
    }

    string TypeStr
    {
        get;
        set;
    }

    decimal Subtotal
    {
        get;
        set;
    }

    string TotalStr
    {
        get;
        set;
    }

    int DecimalPlaces
    {
        get;
        set;
    }

    List<ANode>? Children
    {
        get;
        set;
    }

    List<string>? Dependents
    {
        get;
        set;
    }

    string DependentsStr
    {
        get;
        set;
    }

    List<string>? References
    {
        get;
        set;
    }

    int ExpandedLevels
    {
        get;
        set;
    }

    bool Leaf
    {
        get;
        set;
    }

    decimal Discount
    {
        set;
        get;
    }

    bool MaxIsSet
    {
        set;
        get;
    }

    decimal Max
    {
        set;
        get;
    }

    bool MinIsSet
    {
        set;
        get;
    }

    decimal Min
    {
        set;
        get;
    }

    bool Template
    {
        get;
        set;
    }

    int Order
    {
        set;
        get;
    }

    bool Selected
    {
        set;
        get;
    }

    bool Hidden
    {
        set;
        get;
    }

    bool ReadOnly
    {
        set;
        get;
    }

    bool Expanded
    {
        set;
        get;
    }

    string Units
    {
        set;
        get;
    }

    bool Report
    {
        set;
        get;
    }

    bool ReportValue
    {
        set;
        get;
    }

    bool Optional
    {
        get;
        set;
    }

    bool Disabled
    {
        get;
    }

    string DisableCondition
    {
        get;
        set;
    }

    string DisabledMessage
    {
        get;
        set;
    }

    ANode? Parent
    {
        get;
        set;
    }

    string Description
    {
        get;
        set;
    }

    QTree ParentTree
    {
        get;
        set;
    }

    decimal Amount
    {
        get;
        set;
    }

    bool CheckBox
    {
        get;
        set;
    }

    string Url
    {
        get;
        set;
    }

    bool Complete
    {
        get;
        set;
    }

    decimal Total();
    ANode? FindChildNode(string name);
    string GetPath();
    bool IsComplete();
    void SortChildren();
    ANode Clone();
    string NewId();
    void Remove();
    void RemoveNodeFromDependencies(ANode node, ANode start);
    void RemoveBranchFromDependencies(ANode node, ANode start);
}

[Serializable]
[XmlInclude(typeof(MathNode))]
[XmlInclude(typeof(DecisionNode))]
[XmlInclude(typeof(ConditionalNode))]
[XmlInclude(typeof(TextNode))]
[XmlInclude(typeof(SumSetNode))]
[XmlInclude(typeof(ReferenceNode))]
[XmlInclude(typeof(DateNode))]
[XmlInclude(typeof(TodayNode))]
public abstract class ANode : INode
{
    #region Private fields
    [JsonProperty]
    private List<ANode>? _Children;
    private int _DecimalPlaces;
    private bool _Selected;
    private bool _Hidden;
    [JsonProperty]
    private readonly bool _Leaf;
    private string _Units = "";
    private QTree? _ParentTree;

    #endregion

    #region Public Properties
    public string Name { get; set; } = "";
    public string Id { get; set; } = "";

    public NodeType Type { get; set; }

    public string TypeStr
    {
        get { return Type.ToString(); }
        set { }
    }

    public decimal Subtotal
    {
        get
        {
            try
            {
                return Total();
            }
            catch (Exception)
            {
                return -1;
            }
        }
        set { }
    }

    public virtual int DecimalPlaces
    {
        get { return _DecimalPlaces; }
        set { if (value <= 10) _DecimalPlaces = value; }
    }

    public bool Complete
    {
        get
        {
            bool comp;
            return IsComplete();
        }
        set { }
    }
    [JsonIgnore]
    public List<ANode>? Children
    {
        get { return _Children; }
        set { _Children = value; }
    }

    [XmlIgnore]
    public List<string>? Dependents { get; set; }

    [XmlIgnore]
    public string DependentsStr
    {
        get
        {
            string dep = "";
            foreach (string n in Dependents!) dep = dep + n + ";";
            return dep;
        }
        set { }
    }

    [XmlIgnore]
    public List<string>? References { get; set; }

    public virtual decimal Discount { get; set; }

    public bool MaxIsSet { get; set; }

    public virtual decimal Max { get; set; }

    public bool MinIsSet { get; set; }

    public virtual decimal Min { get; set; }

    public bool Template { get; set; }

    public bool Leaf
    {
        get { return Children == null || Children.Count == 0; }
        set { }
    }

    public int Order { get; set; }

    public virtual bool Selected
    {
        get
        {
            if (Optional) return _Selected && !Disabled;
            else return _Selected;
        }
        set
        {
            _Selected = value;
            if (value && Parent != null && Parent.Type == NodeType.Decision)
            {
                foreach (ANode n in Parent.Children!)
                {
                    if (n.Name != Name) n.Selected = false;
                }
            }
        }
    }

    public bool Hidden
    {
        get
        {
            if (Template) return true;
            return _Hidden;
        }
        set { _Hidden = value; }
    }

    public bool ReadOnly { get; set; }

    public bool Expanded { get; set; }

    public int ExpandedLevels { get; set; }

    public virtual string Units
    {
        get { return _Units; }
        set { _Units = value.Trim(); }
    }

    public bool Report { get; set; }

    public bool ReportValue { get; set; }

    public virtual bool Optional { get; set; }

    public virtual bool Disabled
    {
        get
        {
            try
            {
                if (DisableCondition == null || DisableCondition.Trim()?.Length == 0) return false;
                const bool expression_result = false;
                Expression e = new(DisableCondition);
                e.EvaluateFunction += new EvaluateFunctionHandler(EvaluateFunction);
                e.EvaluateParameter += new EvaluateParameterHandler(EvaluateParameter);
                object result = e.Evaluate();
                return Convert.ToBoolean(result);
            }
            catch (CircularReferenceException)
            {
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public string DisableCondition { get; set; } = "";

    public virtual string DisabledMessage { get; set; } = "";

    [XmlIgnore]
    public ANode? Parent { get; set; }

    public string Description { get; set; } = "";

    [XmlIgnore]
    public QTree ParentTree
    {
        get { return _ParentTree!; }
        set { _ParentTree = value; }
    }

    public virtual decimal Amount { get; set; }

    public bool CheckBox { get; set; }

    public string Url { get; set; } = "";

    public string Error { get; set; } = "";

    public bool EditChildren { get; set; }

    public string TotalStr
    {
        get
        {
            decimal total;
            try
            {
                total = Total();
                string formatString = string.Concat("{0:F", DecimalPlaces, "}");
                if (IsCurrencySymbol(Units))
                {
                    return total == 0 ? Units + 0 : Units + string.Format(formatString, total);
                }
                else
                {
                    string _units = Units != "" ? " " + Units : "";
                    return total == 0 ? 0 + _units : string.Format(formatString, total) + _units;
                }
            }
            catch (Exception)
            {
                ParentTree.TotalCounter = 0;
                return "error";
            }
        }
        set { }
    }

    protected const int TotalCounterMax = 50;
    protected const int EvaluateParameterCounterMax = 50;

    #endregion

    #region Abstract Methods

    abstract public decimal Total();
    abstract public bool IsComplete();
    abstract public bool HasErrors();

    #endregion

    #region Non-abstract methods
    // don't serialize the node property if level greater than ExpandedLevels
    /*public bool ShouldSerializeChildren()
    {
        QTree tree = ParentTree;
        int expandedLevels = tree.Root!.ExpandedLevels;
        string id = Id;
        int countDots = id.Split(".".ToCharArray()).Length - 1;
        return countDots < expandedLevels;

    }*/

    public static bool IsCurrencySymbol(string s)
    {
        if (char.TryParse(s, out char output))
        {
            if (CharUnicodeInfo.GetUnicodeCategory(output) == UnicodeCategory.CurrencySymbol) return true;
        }
        return false;
    }

    public string NewId()
    {
        if (Children == null || Children.Count == 0)
        {
            return Id + ".1";
        }
        else
        {
            int last = 0;
            string[] split;
            foreach (ANode child in Children)
            {
                split = child.Id.Split(".".ToCharArray());
                if (int.Parse(split[^1]) > last)
                    last = int.Parse(split[^1]);
            }
            return Id + "." + (last + 1).ToString();
        }
    }

    public void Remove()
    {
        if (Parent != null && (Parent.Type == NodeType.Date || Parent.Type == NodeType.Today)) return;
        try
        {
            RemoveBranchFromDependencies(this, ParentTree.Root!);
            Parent!.Children!.Remove(this);
        }
        catch (Exception) { }
    }

    public void RemoveBranchFromDependencies(ANode node, ANode start)
    {
        RemoveNodeFromDependencies(node, start);
        foreach (ANode child in node.Children!)
            RemoveBranchFromDependencies(child, start);
    }

    public void RemoveNodeFromDependencies(ANode node, ANode start)
    {
        if (start.Dependents!.Contains(node.Id))
            start.Dependents.Remove(node.Id);
        if (start.References!.Contains(node.Id)) start.References.Remove(node.Id);
        foreach (ANode child in start.Children!)
            RemoveNodeFromDependencies(node, child);
    }

    public bool BranchSelected()
    {
        return BranchSelected(this);
    }

    private static bool BranchSelected(ANode node)
    {
        if (!node.Selected) return false;
        if (node.Parent != null)
            return BranchSelected(node.Parent);
        else
            return true;
    }

    public bool BranchHidden()
    {
        ANode? parent = this;
        while (parent != null)
        {
            if (parent.Hidden) return true;
            else parent = parent.Parent;
        }
        return false;
    }

    public static string GetValueFromDirectory(string field, string dir)
    {
        string? line;
        string s = "";
        string[] splitted;

        try
        {
            using StreamReader sr = new(dir + Path.DirectorySeparatorChar + "values.txt");

            // Read and display lines from the file until the end of 
            // the file is reached.
            while ((line = sr.ReadLine()) != null)
            {
                s += line;
            }
        }
        catch (Exception)
        {
        }

        string the_field = "";
        splitted = s.Split(";".ToCharArray());
        foreach (string part in splitted)
        {
            string[] splitEqual = part.Split("=".ToCharArray());
            if (string.Equals(splitEqual[0].Trim(), field, StringComparison.OrdinalIgnoreCase))
            {
                //This is a hack, in case that there are more than one '=' characters in the expression
                for (int i = 1; i < splitEqual.Length; i++)
                {
                    the_field += splitEqual[i].Trim();
                    if (i + 1 < splitEqual.Length) the_field += "=";
                }

                the_field = the_field.Replace("\"", "");
                break;
            }
        }
        return the_field;
    }
    public void SortChildren()
    {
        _Children!.Sort(new NodeComparer());
        foreach (ANode n in _Children)
            n.SortChildren();
    }
    public MemoryStream Serialize()
    {
        MemoryStream ms = new();
        BinaryFormatter? formater = new();
        formater.Serialize(ms, this);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }
    public ANode Clone()
    {
        MemoryStream? ms;
        object node;
        BinaryFormatter? formater = new();
        ms = Serialize();
        node = formater.Deserialize(ms);
        ms.Close();
        ms.Dispose();
        System.GC.Collect();
        return (node as ANode)!;
    }
    public string GetPath()
    {
        string s = Name;
        ANode n = this;
        while (n?.Parent != null)
        {
            n = n.Parent;
            s = n.Name + "\\" + s;
        }
        return s;
    }
    public ANode? FindChildNode(string name)
    {
        if (_Children == null) return null;
        foreach (ANode n in _Children)
            if (n != null && n.Name == name) return n;
        return null;
    }

    private static string ExtractString(string text)
    {
        StringBuilder sb = new(text);
        int startIndex = 0; // (Don't) Skip initial quote
        int slashIndex;
        while ((slashIndex = sb.ToString().IndexOf('$', startIndex)) != -1)
        {
            char escapeType = sb[slashIndex + 1];
            switch (escapeType)
            {
                case 'u':
                    string hcode = string.Concat(sb[slashIndex + 4], sb[slashIndex + 5]);
                    string lcode = string.Concat(sb[slashIndex + 2], sb[slashIndex + 3]);
                    char unicodeChar = Encoding.Unicode.GetChars(new byte[] { System.Convert.ToByte(hcode, 16), System.Convert.ToByte(lcode, 16) })[0];
                    sb.Remove(slashIndex, 6).Insert(slashIndex, unicodeChar);
                    break;
                case 'n': sb.Remove(slashIndex, 2).Insert(slashIndex, '\n'); break;
                case 'r': sb.Remove(slashIndex, 2).Insert(slashIndex, '\r'); break;
                case 't': sb.Remove(slashIndex, 2).Insert(slashIndex, '\t'); break;
                case '\'': sb.Remove(slashIndex, 2).Insert(slashIndex, '\''); break;
                case '\\': sb.Remove(slashIndex, 2).Insert(slashIndex, '\\'); break;
                default: throw new Exception("Invalid escape sequence: \\" + escapeType);
            }

            startIndex = slashIndex + 1;
        }
        return sb.ToString();
    }

    private static int CalculateMonthDifference(DateTime startDate, DateTime endDate)
    {
        DateTime tmp;
        if (startDate > endDate)
        {
            tmp = startDate;
            startDate = endDate;
            endDate = tmp;
        }

        int monthsDifference = ((endDate.Year - startDate.Year) * 12) + endDate.Month - startDate.Month;

        // Adjust for cases where the endDate day is earlier than the startDate day
        if (endDate.Day < startDate.Day)
        {
            monthsDifference--;
        }

        return monthsDifference;
    }

    public static int CalculateYearDifference(DateTime startDate, DateTime endDate)
    {
        DateTime tmp;
        if (startDate > endDate)
        {
            tmp = startDate;
            startDate = endDate;
            endDate = tmp;
        }

        int YearsPassed = endDate.Year - startDate.Year;
        // Are we before the start date this year? If so subtract one year from the mix
        if (endDate.Month < startDate.Month || (endDate.Month == startDate.Month && endDate.Day < startDate.Day))
        {
            YearsPassed--;
        }
        return YearsPassed;
    }

    public static void EvaluateFunction(string name, FunctionArgs args)
    {
        if (name == "DayDiff")
        {
            var startDate = (DateTime)args.Parameters[0].Evaluate();
            var endDate = (DateTime)args.Parameters[1].Evaluate();

            DateTime tmp;
            if (startDate > endDate)
            {
                tmp = startDate;
                startDate = endDate;
                endDate = tmp;
            }

            var timespan = endDate - startDate;
            args.Result = timespan.TotalDays; // double (you can convert to int if you wish a whole number!)
        }

        if (name == "MonthDiff")
        {
            // Define two dates
            var date1 = (DateTime)args.Parameters[0].Evaluate();
            var date2 = (DateTime)args.Parameters[1].Evaluate();

            // Calculate the difference in months
            int monthsDifference = CalculateMonthDifference(date1, date2);
            args.Result = monthsDifference;
        }

        if (name == "YearDiff")
        {
            // Define two dates
            var date1 = (DateTime)args.Parameters[0].Evaluate();
            var date2 = (DateTime)args.Parameters[1].Evaluate();

            // Calculate the difference in years
            int yearsDifference = CalculateYearDifference(date1, date2);
            args.Result = yearsDifference;
        }
    }

    public void EvaluateParameter(string name, ParameterArgs args)
    {
        ParentTree.EvaluateParameterCounter++;
        if (ParentTree.EvaluateParameterCounter > EvaluateParameterCounterMax)
        {
            ParentTree.EvaluateParameterCounter--;
            throw new CircularReferenceException();
        }
        try
        {
            string endsWith = "";
            string tempName = name;

            if (name.EndsWith(".max", StringComparison.OrdinalIgnoreCase))
            {
                endsWith = ".max";
                tempName = Regex.Replace(name, @"\.max", "", RegexOptions.IgnoreCase);
            }
            else if (name.EndsWith(".min", StringComparison.OrdinalIgnoreCase))
            {
                endsWith = ".min";
                tempName = Regex.Replace(name, @"\.min", "", RegexOptions.IgnoreCase);
            }
            else if (name.EndsWith(".discount", StringComparison.OrdinalIgnoreCase))
            {
                endsWith = ".discount";
                tempName = Regex.Replace(name, @"\.discount", "", RegexOptions.IgnoreCase);
            }
            else if (name.EndsWith(".selected", StringComparison.OrdinalIgnoreCase))
            {
                endsWith = ".selected";
                tempName = Regex.Replace(name, @"\.selected", "", RegexOptions.IgnoreCase);
            }
            else if (name.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase))
            {
                endsWith = ".disabled";
                tempName = Regex.Replace(name, @"\.disabled", "", RegexOptions.IgnoreCase);
            }
            tempName = tempName.Trim();
            string extractedString = tempName;
            if (tempName.Contains('$')) tempName = ExtractString(tempName);
            switch (endsWith)
            {
                case ".selected":
                    if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                    {
                        args.Result = Selected ? 1 : 0;
                    }
                    else
                    if (name.Contains('\\'))
                    {
                        args.Result = ParentTree.GetNodeFromPath(tempName)!.Selected ? 1 : 0;
                    }
                    else
                    if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                    {
                        tempName = tempName[1..^1];
                        args.Result = ParentTree.GetNodeFromId(tempName)!.Selected ? 1 : 0;
                    }
                    else
                    {
                        foreach (ANode child in Children!)
                        {
                            extractedString = name;
                            if (name.Contains('$')) extractedString = ExtractString(name);
                            if (child.Name == extractedString)
                            {
                                args.Result = child.Selected ? 1 : 0;
                                break;
                            }
                        }
                    }
                    break;
                case ".disabled":
                    if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                    {
                        args.Result = Disabled ? 1 : 0;
                    }
                    else
                    if (name.Contains('\\'))
                    {
                        args.Result = ParentTree.GetNodeFromPath(tempName)!.Disabled ? 1 : 0;
                    }
                    else
                    if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                    {
                        tempName = tempName[1..^1];
                        args.Result = ParentTree.GetNodeFromId(tempName)!.Disabled ? 1 : 0;
                    }
                    else
                    {
                        foreach (ANode child in Children!)
                        {
                            extractedString = name;
                            if (name.Contains('$')) extractedString = ExtractString(name);
                            if (child.Name == extractedString)
                            {
                                args.Result = child.Disabled ? 1 : 0;
                                break;
                            }
                        }
                    }
                    break;
                case ".max":
                    if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                    {
                        args.Result = double.Parse(Max.ToString());
                    }
                    else
                    if (name.Contains('\\'))
                    {
                        args.Result = double.Parse(ParentTree.GetNodeFromPath(tempName)!.Max.ToString());
                    }
                    else
                    if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                    {
                        tempName = tempName[1..^1];
                        args.Result = double.Parse(ParentTree.GetNodeFromId(tempName)!.Max.ToString());
                    }
                    else
                    {
                        foreach (ANode child in Children!)
                        {
                            extractedString = name;
                            if (name.Contains('$')) extractedString = ExtractString(name);
                            if (child.Name == extractedString)
                            {
                                args.Result = double.Parse(child.Max.ToString());
                                break;
                            }
                        }
                    }
                    break;
                case ".min":
                    if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                    {
                        args.Result = double.Parse(Min.ToString());
                    }
                    else
                    if (name.Contains('\\'))
                    {
                        args.Result = double.Parse(ParentTree.GetNodeFromPath(tempName)!.Min.ToString());
                    }
                    else
                    if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                    {
                        tempName = tempName[1..^1];
                        args.Result = double.Parse(ParentTree.GetNodeFromId(tempName)!.Min.ToString());
                    }
                    else
                    {
                        foreach (ANode child in Children!)
                        {
                            extractedString = name;
                            if (name.Contains('$')) extractedString = ExtractString(name);
                            if (child.Name == extractedString)
                            {
                                args.Result = double.Parse(child.Min.ToString());
                                break;
                            }
                        }
                    }
                    break;
                case ".discount":
                    if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                    {
                        args.Result = double.Parse(Discount.ToString());
                    }
                    else
                    if (name.Contains('\\'))
                    {
                        args.Result = double.Parse(ParentTree.GetNodeFromPath(tempName)!.Discount.ToString());
                    }
                    else
                    if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                    {
                        tempName = tempName[1..^1];
                        args.Result = double.Parse(ParentTree.GetNodeFromId(tempName)!.Discount.ToString());
                    }
                    else
                    {
                        foreach (ANode child in Children!)
                        {
                            extractedString = name;
                            if (name.Contains('$')) extractedString = ExtractString(name);
                            if (child.Name == extractedString)
                            {
                                args.Result = double.Parse(child.Discount.ToString());
                                break;
                            }
                        }
                    }
                    break;
                default:
                    if (name.Contains('\\'))
                    {
                        extractedString = name;
                        if (name.Contains('$')) extractedString = ExtractString(name);
                        ANode nodeFromPath = ParentTree.GetNodeFromPath(extractedString)!;

                        if (nodeFromPath.Type == NodeType.Date || nodeFromPath.Type == NodeType.Today)
                        {
                            if (nodeFromPath.Type == NodeType.Date)
                            {
                                args.Result = DateTime.Parse(((DateNode)nodeFromPath).Expression);
                            }

                            if (nodeFromPath.Type == NodeType.Today)
                            {
                                args.Result = DateTime.Parse(((TodayNode)nodeFromPath).Expression);
                            }
                        }
                        else
                        {
                            args.Result = double.Parse(nodeFromPath.Total().ToString());
                        }
                    }
                    else
                    if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                    {
                        tempName = tempName[1..^1];
                        args.Result = double.Parse(ParentTree.GetNodeFromId(tempName)!.Total().ToString());
                    }
                    else
                    {
                        foreach (ANode child in Children!)
                        {
                            extractedString = name;
                            if (name.Contains('$')) extractedString = ExtractString(name);
                            if (child.Name == extractedString)
                            {
                                if (child.Type == NodeType.Date || child.Type == NodeType.Today)
                                {
                                    if (child.Type == NodeType.Date)
                                    {
                                        args.Result = DateTime.Parse(((DateNode)child).Expression);
                                    }

                                    if (child.Type == NodeType.Today)
                                    {
                                        args.Result = DateTime.Parse(((TodayNode)child).Expression);
                                    }
                                }
                                else
                                {
                                    args.Result = double.Parse(child.Total().ToString());
                                }

                                break;
                            }
                        }
                    }
                    break;
            }
        }
        catch (CircularReferenceException)
        {
            ParentTree.EvaluateParameterCounter--;
            throw;
        }
        catch (Exception) { args.Result = 0; }
        ParentTree.EvaluateParameterCounter--;
    }
    #endregion
}

[Serializable]
public class NodeComparer : IComparer<ANode>
{
    int IComparer<ANode>.Compare(ANode? a, ANode? b)
    {
        if (a == null) return 0;
        if (b == null) return 0;
        return a.Order - b.Order;
    }
}

[Serializable]
public class TextNode : ANode
{
    // *******Fields*****
    string _Expression = "";

    // *****Properties*****
    public string Expression
    {
        get { return _Expression; }
        set
        {
            if (!ReadOnly) _Expression = value;
            Entered = true;
        }
    }

    public bool Entered { get; set; }

    // *****Methods*****

    public override bool HasErrors()
    {
        return false;
    }

    public override bool IsComplete()
    {
        if (!BranchSelected() || BranchHidden()) return true;
        if (!Entered && (!Optional || (Optional && Selected))) return false;
        foreach (ANode n in Children!)
        {
            if (n.Selected && !n.IsComplete()) return false;
        }
        return true;
    }

    public override decimal Total()
    {
        return 0;
    }

    public TextNode()
    {
        Name = "";
        Discount = 0;
        MaxIsSet = MinIsSet = false;
        Min = Max = 0;
        DecimalPlaces = 0;
        Expression = "";
        Order = 0;
        Selected = false;
        Children = new List<ANode>();
        Dependents = new List<string>();
        References = new List<string>();
        Type = NodeType.Text;
        Parent = null;
        Amount = 1;
        Entered = false;
        Optional = false;
        DisableCondition = "0";
        DisabledMessage = "";
        Description = "";
        Hidden = false;
        ReadOnly = false;
        Expanded = false;
        ExpandedLevels = 0;
        Units = "";
        Report = false;
        ReportValue = false;
        EditChildren = false;
        Template = false;
    }

    public TextNode(string path, ANode? parent, QTree parentTree, string id)
        : this()
    {
        if (parent != null) Parent = parent;
        ParentTree = parentTree;
        Name = path.Split(Path.DirectorySeparatorChar)[^1];
        Id = id;
        string value = GetValueFromDirectory("id", path);
        if (value != "") Id = value;

        value = GetValueFromDirectory("units", path);
        if (value != "") Units = value;

        value = GetValueFromDirectory("expression", path);
        if (value != "")
        {
            Expression = value;
        }
        else
        {
            value = GetValueFromDirectory("text", path);
            if (value != "") Expression = value;
        }

        if (GetValueFromDirectory("maxisset", path) == "true") { MaxIsSet = true; }
        if (GetValueFromDirectory("minisset", path) == "true") { MinIsSet = true; }
        value = GetValueFromDirectory("max", path);
        if (int.TryParse(value, out int intResult)) Max = intResult;

        value = GetValueFromDirectory("min", path);
        if (int.TryParse(value, out intResult)) Min = intResult;

        value = GetValueFromDirectory("decimalplaces", path);
        if (int.TryParse(value, out intResult)) DecimalPlaces = intResult;

        value = GetValueFromDirectory("order", path);
        if (int.TryParse(value, out intResult)) Order = intResult;

        value = GetValueFromDirectory("discount", path);
        if (decimal.TryParse(value, out decimal decimalResult)) Discount = decimalResult;

        value = GetValueFromDirectory("amount", path);
        if (decimal.TryParse(value, out decimalResult)) Amount = decimalResult;

        value = GetValueFromDirectory("expandedlevels", path);
        if (int.TryParse(value, out intResult)) ExpandedLevels = intResult;

        value = GetValueFromDirectory("disablecondition", path);
        if (value != "") DisableCondition = value;

        value = GetValueFromDirectory("disabledmessage", path);
        if (value != "") DisabledMessage = value;

        if (GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (parent == null || (parent.Type != NodeType.Decision)) Selected = true; }
        if (GetValueFromDirectory("report", path) == "true") { Report = true; }
        if (GetValueFromDirectory("reportvalue", path) == "true") { ReportValue = true; }
        if (GetValueFromDirectory("editchildren", path) == "true") { EditChildren = true; }
        if (GetValueFromDirectory("hidden", path) == "true") { Hidden = true; }
        if (CheckBox)
            if (GetValueFromDirectory("selected", path) == "true") { Selected = true; }

        //Set node url
        Url = "TreeView/ChangeTreeValue?id=" + Id;
    }

    public TextNode(NameValueCollection values, QTree tree)
        : this()
    {
        string id = values["id"]!;
        ANode? node;
        if (!string.IsNullOrEmpty(id))
        {
            node = tree.GetNodeFromId(id)!;
            if (node.Type == NodeType.Date || node.Type == NodeType.Today) return;
            if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
        }
        else
        {
            node = null;
        }

        //check for same name
        if (node?.Children != null)
        {
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;
        }

        //first set the node as writeable
        ReadOnly = false;
        if (node == null) Id = "1";
        else Id = node.NewId();
        Expression = values["expression"]!;
        EditChildren = values["editChildren"] == "true";
        Name = values["name"]!;
        if (int.TryParse(values["expandedLevels"], out int intResult))
            ExpandedLevels = intResult;
        if (int.TryParse(values["decimalPlaces"], out intResult))
            DecimalPlaces = intResult;
        if (int.TryParse(values["order"], out intResult))
            Order = intResult;
        if (decimal.TryParse(values["min"], out decimal decimalResult))
        {
            Min = decimalResult;
            MinIsSet = true;
        }
        if (decimal.TryParse(values["max"], out decimalResult))
        {
            Max = decimalResult;
            MaxIsSet = true;
        }
        if (decimal.TryParse(values["discount"], out decimalResult))
            Discount = decimalResult;
        Hidden = values["hidden"] == "true";
        Report = values["report"] == "true";
        ReportValue = values["reportValue"] == "true";
        Template = values["template"] == "true";
        ReadOnly = values["readOnly"] == "true";

        if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (node == null || (node.Type != NodeType.Decision)) Selected = true; }
        DisableCondition = values["disable"]!;
        DisabledMessage = values["disabledMessage"]!;
        if (node != null && node.Type == NodeType.Decision)
        {
            Dependents!.AddRange(node.Dependents!);
            Dependents.Add(node.Id);
        }
        Parent = node!;
        ParentTree = tree;
        Url = "TreeView/ChangeTreeValue?id=" + Id;

        //Add new node to children
        if (!HasErrors())
        {
            if (node != null)
            {
                node.Children!.Add(this);
            }
            else
            {
                tree.Root = this;
            }

            node?.SortChildren();
        }
    }
}

[Serializable]
public class MathNode : ANode
{
    // *******Fields*****
    string _Expression = "";

    // *****Properties*****
    public string Expression
    {
        get
        {
            //First check if parent is a Today node
            if (Parent != null && Parent.Type == NodeType.Today)
            {
                return Name switch
                {
                    "Month" => DateTime.Today.Month.ToString(),
                    "Day" => DateTime.Today.Day.ToString(),
                    "Year" => DateTime.Today.Year.ToString(),
                    _ => _Expression,
                };
            }
            else
            {
                return _Expression;
            }
        }
        set
        {
            if (!ReadOnly)
            {
                if (Parent != null && Parent.Type == NodeType.Date)
                {
                    bool valueInt = int.TryParse(value, out _);
                    if (valueInt)
                    {
                        _Expression = value;
                        Entered = true;
                    }
                }
                else
                {
                    _Expression = value;
                    Entered = true;
                }
            }
        }
    }

    public bool Entered { get; set; }

    // *****Methods*****
    public override decimal Total()
    {
        //First check if parent is a Today node
        if (Parent != null && Parent.Type == NodeType.Today)
        {
            ReadOnly = false;
            switch (Name)
            {
                case "Month":
                    Expression = DateTime.Today.Month.ToString();
                    break;
                case "Day":
                    Expression = DateTime.Today.Day.ToString();
                    break;
                case "Year":
                    Expression = DateTime.Today.Year.ToString();
                    break;
                default:
                    break;
            }
            ReadOnly = true;
            return decimal.Parse(Expression);
        }

        ParentTree.TotalCounter++;
        if (ParentTree.TotalCounter > TotalCounterMax)
        {
            ParentTree.TotalCounter = 0;
            throw new CircularReferenceException();
        }
        if (Optional && !Selected && !(Parent != null && Parent.Type == NodeType.Decision))
        {
            ParentTree.TotalCounter--;
            return 0;
        }

        decimal formula_result = 0;
        Expression e = new(_Expression);
        e.EvaluateFunction += new EvaluateFunctionHandler(EvaluateFunction);
        e.EvaluateParameter += new EvaluateParameterHandler(EvaluateParameter);

        try
        {
            object result = e.Evaluate();
            //Restrict node to math expressions
            if (e.ParsedExpression.GetType().Name == "TernaryExpression")
            {
                ParentTree.TotalCounter = 0;
                throw new Exception("Incorrect expression for this type of node");
            }
            formula_result = decimal.Parse(result.ToString()!);
        }
        catch (CircularReferenceException)
        {
            ParentTree.TotalCounter = 0;
            throw;
        }
        catch (Exception)
        {
            ParentTree.TotalCounter = 0;
            throw;
        }

        ParentTree.TotalCounter--;
        if (MaxIsSet && formula_result > Max)
        {
            return Amount * (Max - (Max * Discount / 100));
        }
        else
        {
            if (MinIsSet && formula_result < Min) return Amount * (Min - (Min * Discount / 100));
            else return Amount * (formula_result - (formula_result * Discount / 100));
        }
    }
    public override bool HasErrors()
    {
        try
        {
            Expression e = new(_Expression);
            e.EvaluateFunction += new EvaluateFunctionHandler(EvaluateFunction);
            e.EvaluateParameter += new EvaluateParameterHandler(EvaluateParameter);
            bool hasErrors = e.HasErrors();
            Error = e.Error;
            return hasErrors;
        }
        catch (Exception) { return false; }
    }

    public override bool IsComplete()
    {
        if (!BranchSelected() || BranchHidden()) return true;
        //Check for Entered property
        if (decimal.TryParse(Expression, out _) && !ReadOnly)
        {
            if (!Entered && (!Optional || (Optional && Selected))) return false;
        }
        foreach (ANode n in Children!)
        {
            if (n.Selected && !n.IsComplete()) return false;
        }
        return true;
    }

    public MathNode()
    {
        Name = "";
        Expression = "";
        Discount = 0;
        MaxIsSet = MinIsSet = false;
        Min = Max = 0;
        Order = 0;
        if (Parent != null)
        {
            DecimalPlaces = 0;
        }
        else
        {
            DecimalPlaces = 2;
        }

        Selected = false;
        Children = new List<ANode>();
        Dependents = new List<string>();
        References = new List<string>();
        Type = NodeType.Math;
        Parent = null;
        Amount = 1;
        Entered = false;
        Optional = false;
        DisableCondition = "0";
        DisabledMessage = "";
        Description = "";
        Hidden = false;
        ReadOnly = false;
        Expanded = false;
        ExpandedLevels = 0;
        Units = "";
        Report = false;
        ReportValue = false;
        EditChildren = false;
        Template = false;
        CheckBox = false;
    }

    public MathNode(string path, ANode? parent, QTree parentTree, string id)
        : this()
    {
        if (parent != null) Parent = parent;
        ParentTree = parentTree;
        Name = path.Split(Path.DirectorySeparatorChar)[^1];
        Id = id;
        string value = GetValueFromDirectory("id", path);
        if (value != "") Id = value;

        value = GetValueFromDirectory("units", path);
        if (value != "") Units = value;

        value = GetValueFromDirectory("expression", path);
        if (value != "")
        {
            Expression = value;
        }
        else
        {
            value = GetValueFromDirectory("formula", path);
            if (value != "") Expression = value;
        }

        if (GetValueFromDirectory("maxisset", path) == "true") { MaxIsSet = true; }
        if (GetValueFromDirectory("minisset", path) == "true") { MinIsSet = true; }
        value = GetValueFromDirectory("max", path);
        if (int.TryParse(value, out int intResult)) Max = intResult;

        value = GetValueFromDirectory("min", path);
        if (int.TryParse(value, out intResult)) Min = intResult;

        if (parent != null && (parent.Type == NodeType.Date || parent.Type == NodeType.Today))
        {
            DecimalPlaces = 0;
        }
        else
        {
            value = GetValueFromDirectory("decimalplaces", path);
            if (int.TryParse(value, out intResult)) DecimalPlaces = intResult;
        }

        value = GetValueFromDirectory("order", path);
        if (int.TryParse(value, out intResult)) Order = intResult;

        value = GetValueFromDirectory("discount", path);
        if (decimal.TryParse(value, out decimal decimalResult)) Discount = decimalResult;

        value = GetValueFromDirectory("amount", path);
        if (decimal.TryParse(value, out decimalResult)) Amount = decimalResult;

        value = GetValueFromDirectory("expandedlevels", path);
        if (int.TryParse(value, out intResult)) ExpandedLevels = intResult;

        value = GetValueFromDirectory("disablecondition", path);
        if (value != "") DisableCondition = value;

        value = GetValueFromDirectory("disabledmessage", path);
        if (value != "") DisabledMessage = value;

        if (GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (parent == null || (parent.Type != NodeType.Decision)) Selected = true; }
        if (GetValueFromDirectory("report", path) == "true") { Report = true; }
        if (GetValueFromDirectory("reportvalue", path) == "true") { ReportValue = true; }
        if (GetValueFromDirectory("editchildren", path) == "true") { EditChildren = true; }
        if (GetValueFromDirectory("template", path) == "true") { Template = true; }
        if (GetValueFromDirectory("hidden", path) == "true") { Hidden = true; }
        if (GetValueFromDirectory("readonly", path) == "true") { ReadOnly = true; }
        if (CheckBox)
            if (GetValueFromDirectory("selected", path) == "true") { Selected = true; }

        //To set the url for the node
        if (decimal.TryParse(Expression, out _) || EditChildren)
        {
            Url = "TreeView/ChangeTreeValue?id=" + Id;
        }
        else
        {
            Url = "TreeView/Description?id=" + Id;
        }
    }

    public MathNode(NameValueCollection values, QTree tree)
        : this()
    {
        string id = values["id"]!;
        ANode? node;
        if (!string.IsNullOrEmpty(id))
        {
            node = tree.GetNodeFromId(id);
            if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
            if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
        }
        else
        {
            node = null;
        }

        //check for same name
        if (node?.Children != null)
        {
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;
        }

        //first set the node as writeable
        ReadOnly = false;
        if (node == null) Id = "1";
        else Id = node.NewId();
        Expression = values["expression"]!;
        EditChildren = values["editChildren"] == "true";
        Name = values["name"]!;
        Units = values["units"]!;
        if (int.TryParse(values["expandedLevels"], out int intResult))
            ExpandedLevels = intResult;
        if (int.TryParse(values["order"], out intResult))
            Order = intResult;
        if (int.TryParse(values["decimalPlaces"], out intResult))
            DecimalPlaces = intResult;
        if (decimal.TryParse(values["min"], out decimal decimalResult))
        {
            Min = decimalResult;
            MinIsSet = true;
        }
        if (decimal.TryParse(values["max"], out decimalResult))
        {
            Max = decimalResult;
            MaxIsSet = true;
        }
        if (decimal.TryParse(values["discount"], out decimalResult))
            Discount = decimalResult;
        Hidden = values["hidden"] == "true";
        Report = values["report"] == "true";
        ReportValue = values["reportValue"] == "true";
        Template = values["template"] == "true";
        ReadOnly = values["readOnly"] == "true";

        if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (node == null || (node.Type != NodeType.Decision)) Selected = true; }
        DisableCondition = values["disable"]!;
        DisabledMessage = values["disabledMessage"]!;
        if (node != null && node.Type == NodeType.Decision)
        {
            Dependents!.AddRange(node.Dependents!);
            Dependents.Add(node.Id);
        }
        Parent = node;
        ParentTree = tree;
        //To set the url for the node
        if (decimal.TryParse(Expression, out _) || EditChildren)
        {
            Url = "TreeView/ChangeTreeValue?id=" + Id;
        }
        else
        {
            Url = "TreeView/Description?id=" + Id;
        }

        //Add new node to children
        if (!HasErrors())
        {
            if (node != null)
            {
                node.Children!.Add(this);
            }
            else
            {
                ParentTree.Root = this;
            }

            node?.SortChildren();
        }
    }
}

[Serializable]
public class DateNode : ANode
{
    // *****Properties*****

    public string Expression { get; set; } = "";

    // *****Methods*****
    public override decimal Total()
    {
        return 0;
    }

    public override bool HasErrors()
    {
        return false;
    }

    public override bool IsComplete()
    {
        if (!BranchSelected()) return true;
        foreach (ANode n in Children!)
        {
            if (n.Selected && !n.IsComplete()) return false;
        }
        return true;
    }
    public DateNode()
    {
        Name = "";
        Discount = 0;
        MaxIsSet = MinIsSet = false;
        Min = Max = 0;
        Order = 0;
        DecimalPlaces = 0;
        Selected = true;
        Children = new List<ANode>();
        Dependents = new List<string>();
        References = new List<string>();
        Type = NodeType.Date;
        Parent = null;
        Amount = 1;
        Optional = false;
        DisableCondition = "0";
        DisabledMessage = "";
        Description = "";
        Hidden = false;
        ReadOnly = false;
        Expanded = false;
        ExpandedLevels = 0;
        Units = "";
        Report = false;
        ReportValue = false;
        EditChildren = true;
        Template = false;
        CheckBox = false;
    }

    public DateNode(string path, ANode? parent, QTree parentTree, string id)
        : this()
    {
        if (parent != null) Parent = parent;
        ParentTree = parentTree;
        Name = path.Split(Path.DirectorySeparatorChar)[^1];
        Id = id;
        string value = GetValueFromDirectory("id", path);
        if (value != "") Id = value;

        value = GetValueFromDirectory("units", path);
        if (value != "") Units = value;

        value = GetValueFromDirectory("expression", path);
        if (value != "")
        {
            Expression = value;
        }
        else
        {
            value = GetValueFromDirectory("formula", path);
            if (value != "") Expression = value;
        }

        if (GetValueFromDirectory("maxisset", path) == "true") { MaxIsSet = true; }
        if (GetValueFromDirectory("minisset", path) == "true") { MinIsSet = true; }
        value = GetValueFromDirectory("max", path);
        if (int.TryParse(value, out int intResult)) Max = intResult;

        value = GetValueFromDirectory("min", path);
        if (int.TryParse(value, out intResult)) Min = intResult;

        value = GetValueFromDirectory("order", path);
        if (int.TryParse(value, out intResult)) Order = intResult;

        value = GetValueFromDirectory("decimalplaces", path);
        if (int.TryParse(value, out intResult)) DecimalPlaces = intResult;

        value = GetValueFromDirectory("discount", path);
        if (decimal.TryParse(value, out decimal decimalResult)) Discount = decimalResult;

        value = GetValueFromDirectory("amount", path);
        if (decimal.TryParse(value, out decimalResult)) Amount = decimalResult;

        value = GetValueFromDirectory("expandedlevels", path);
        if (int.TryParse(value, out intResult)) ExpandedLevels = intResult;

        value = GetValueFromDirectory("disablecondition", path);
        if (value != "") DisableCondition = value;

        value = GetValueFromDirectory("disabledmessage", path);
        if (value != "") DisabledMessage = value;

        if (GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (parent == null || (parent.Type != NodeType.Decision)) Selected = true; }
        if (GetValueFromDirectory("report", path) == "true") { Report = true; }
        if (GetValueFromDirectory("reportvalue", path) == "true") { ReportValue = true; }
        if (GetValueFromDirectory("editchildren", path) == "true") { EditChildren = true; }
        else if (GetValueFromDirectory("editchildren", path) == "false") { EditChildren = false; }
        if (GetValueFromDirectory("template", path) == "true") { Template = true; }
        if (GetValueFromDirectory("hidden", path) == "true") { Hidden = true; }

        //To set the url for the node
        Url = "TreeView/Description?id=" + Id;
    }

    public DateNode(NameValueCollection values, QTree tree)
        : this()
    {
        string id = values["id"]!;
        ANode? node;
        if (!string.IsNullOrEmpty(id))
        {
            node = tree.GetNodeFromId(id);
            if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
            if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
        }
        else
        {
            node = null;
        }

        //check for same name
        if (node?.Children != null)
        {
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;
        }

        //first set the node as writeable
        ReadOnly = false;
        if (node == null) Id = "1";
        else Id = node.NewId();
        EditChildren = true;
        Name = values["name"]!;
        Units = values["units"]!;
        if (int.TryParse(values["expandedLevels"], out int intResult))
            ExpandedLevels = intResult;
        if (int.TryParse(values["order"], out intResult))
            Order = intResult;
        if (int.TryParse(values["decimalPlaces"], out intResult))
            DecimalPlaces = intResult;
        if (decimal.TryParse(values["min"], out decimal decimalResult))
        {
            Min = decimalResult;
            MinIsSet = true;
        }
        if (decimal.TryParse(values["max"], out decimalResult))
        {
            Max = decimalResult;
            MaxIsSet = true;
        }
        if (decimal.TryParse(values["discount"], out decimalResult))
            Discount = decimalResult;
        Hidden = values["hidden"] == "true";
        Report = values["report"] == "true";
        ReportValue = values["reportValue"] == "true";
        Template = values["template"] == "true";
        ReadOnly = values["readOnly"] == "true";

        if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        Selected = true;
        DisableCondition = values["disable"]!;
        DisabledMessage = values["disabledMessage"]!;
        if (node != null && node.Type == NodeType.Decision)
        {
            Dependents!.AddRange(node.Dependents!);
            Dependents.Add(node.Id);
        }
        Parent = node;
        ParentTree = tree;
        EditChildren = true;
        //To set the url for the node
        Url = "TreeView/Description?id=" + Id;

        //Add new node to children
        if (!HasErrors())
        {
            if (node != null)
            {
                node.Children!.Add(this);
            }
            else
            {
                ParentTree.Root = this;
            }

            node?.SortChildren();
        }
        //Add the date child nodes to this node
        ANode month = new MathNode
        {
            Id = NewId(),
            Name = "Month",
            Parent = this,
            ParentTree = tree
        };
        month.Url = "TreeView/ChangeTreeValue?id=" + month.Id;
        (month as MathNode)!.Expression = "1";
        month.Min = 1;
        month.Max = 12;
        month.Dependents!.Add(Id);
        month.Order = 0;
        month.DecimalPlaces = 0;
        month.Selected = true;
        month.MinIsSet = true;
        month.MaxIsSet = true;
        Children!.Add(month);

        ANode day = new MathNode
        {
            Id = NewId(),
            Name = "Day",
            Parent = this,
            ParentTree = tree
        };
        day.Url = "TreeView/ChangeTreeValue?id=" + day.Id;
        (day as MathNode)!.Expression = "1";
        day.Min = 1;
        day.Max = 31;
        day.Dependents!.Add(Id);
        day.Order = 1;
        day.DecimalPlaces = 0;
        day.Selected = true;
        day.MinIsSet = true;
        day.MaxIsSet = true;
        Children.Add(day);

        ANode year = new MathNode
        {
            Id = NewId(),
            Name = "Year",
            Parent = this,
            ParentTree = tree
        };
        year.Url = "TreeView/ChangeTreeValue?id=" + year.Id;
        (year as MathNode)!.Expression = "2000";
        year.Min = 2000;
        year.Max = 2100;
        year.Dependents!.Add(Id);
        year.Order = 2;
        year.DecimalPlaces = 0;
        year.Selected = true;
        year.MinIsSet = true;
        year.MaxIsSet = true;
        Children.Add(year);

        //Set the expression
        Expression = ((MathNode)Children[0]).Expression + "/" + ((MathNode)Children[1]).Expression + "/" + ((MathNode)Children[2]).Expression;
    }
}

[Serializable]
public class TodayNode : ANode
{
    // *****Properties*****
    public string Expression { get; set; } = DateTime.Today.ToShortDateString();

    // *****Methods*****
    public override decimal Total()
    {
        return 0;
    }

    public override bool HasErrors()
    {
        return false;
    }

    public override bool IsComplete()
    {
        return true;
    }

    public TodayNode()
    {
        Name = "";
        Discount = 0;
        MaxIsSet = MinIsSet = false;
        Min = Max = 0;
        Order = 0;
        DecimalPlaces = 0;
        Selected = false;
        Children = new List<ANode>();
        Dependents = new List<string>();
        References = new List<string>();
        Type = NodeType.Today;
        Parent = null;
        Amount = 1;
        Optional = false;
        DisableCondition = "0";
        DisabledMessage = "";
        Description = "";
        Hidden = false;
        ReadOnly = false;
        Expanded = false;
        ExpandedLevels = 0;
        Units = "";
        Report = false;
        ReportValue = false;
        EditChildren = false;
        Template = false;
        CheckBox = false;
    }

    public TodayNode(string path, ANode? parent, QTree parentTree, string id)
        : this()
    {
        if (parent != null) Parent = parent;
        ParentTree = parentTree;
        Name = path.Split(Path.DirectorySeparatorChar)[^1];
        Id = id;
        string value = GetValueFromDirectory("id", path);
        if (value != "") Id = value;

        value = GetValueFromDirectory("units", path);
        if (value != "") Units = value;

        if (GetValueFromDirectory("maxisset", path) == "true") { MaxIsSet = true; }
        if (GetValueFromDirectory("minisset", path) == "true") { MinIsSet = true; }
        value = GetValueFromDirectory("max", path);
        if (int.TryParse(value, out int intResult)) Max = intResult;

        value = GetValueFromDirectory("min", path);
        if (int.TryParse(value, out intResult)) Min = intResult;

        value = GetValueFromDirectory("order", path);
        if (int.TryParse(value, out intResult)) Order = intResult;

        value = GetValueFromDirectory("decimalplaces", path);
        if (int.TryParse(value, out intResult)) DecimalPlaces = intResult;

        value = GetValueFromDirectory("discount", path);
        if (decimal.TryParse(value, out decimal decimalResult)) Discount = decimalResult;

        value = GetValueFromDirectory("amount", path);
        if (decimal.TryParse(value, out decimalResult)) Amount = decimalResult;

        value = GetValueFromDirectory("expandedlevels", path);
        if (int.TryParse(value, out intResult)) ExpandedLevels = intResult;

        value = GetValueFromDirectory("disablecondition", path);
        if (value != "") DisableCondition = value;

        value = GetValueFromDirectory("disabledmessage", path);
        if (value != "") DisabledMessage = value;

        if (GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (parent == null || (parent.Type != NodeType.Decision)) Selected = true; }
        if (GetValueFromDirectory("report", path) == "true") { Report = true; }
        if (GetValueFromDirectory("reportvalue", path) == "true") { ReportValue = true; }
        EditChildren = false;
        if (GetValueFromDirectory("template", path) == "true") { Template = true; }
        if (GetValueFromDirectory("hidden", path) == "true") { Hidden = true; }

        //To set the url for the node
        Url = "TreeView/Description?id=" + Id;
    }

    public TodayNode(NameValueCollection values, QTree tree)
        : this()
    {
        string id = values["id"]!;
        ANode? node;
        if (!string.IsNullOrEmpty(id))
        {
            node = tree.GetNodeFromId(id);
            if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
            if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
        }
        else
        {
            node = null;
        }

        //check for same name
        if (node?.Children != null)
        {
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;
        }

        //first set the node as writeable
        ReadOnly = false;
        if (node == null) Id = "1";
        else Id = node.NewId();
        EditChildren = true;
        Name = values["name"]!;
        Units = values["units"]!;
        if (int.TryParse(values["expandedLevels"], out int intResult))
            ExpandedLevels = intResult;
        if (int.TryParse(values["order"], out intResult))
            Order = intResult;
        if (int.TryParse(values["decimalPlaces"], out intResult))
            DecimalPlaces = intResult;
        if (decimal.TryParse(values["min"], out decimal decimalResult))
        {
            Min = decimalResult;
            MinIsSet = true;
        }
        if (decimal.TryParse(values["max"], out decimalResult))
        {
            Max = decimalResult;
            MaxIsSet = true;
        }
        if (decimal.TryParse(values["discount"], out decimalResult))
            Discount = decimalResult;
        Hidden = values["hidden"] == "true";
        Report = values["report"] == "true";
        ReportValue = values["reportValue"] == "true";
        Template = values["template"] == "true";
        ReadOnly = values["readOnly"] == "true";

        if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (node == null || (node.Type != NodeType.Decision)) Selected = true; }
        DisableCondition = values["disable"]!;
        DisabledMessage = values["disabledMessage"]!;
        if (node != null && node.Type == NodeType.Decision)
        {
            Dependents!.AddRange(node.Dependents!);
            Dependents.Add(node.Id);
        }
        Parent = node;
        ParentTree = tree;
        EditChildren = false;
        //To set the url for the node
        Url = "TreeView/Description?id=" + Id;

        //Add new node to children
        if (!HasErrors())
        {
            if (node != null)
            {
                node.Children!.Add(this);
            }
            else
            {
                ParentTree.Root = this;
            }

            node?.SortChildren();
        }
        //Add the date child nodes to this node
        ANode month = new MathNode
        {
            Id = NewId(),
            Name = "Month",
            Parent = this,
            ParentTree = tree
        };
        month.Url = "TreeView/Description?id=" + month.Id;
        (month as MathNode)!.Expression = DateTime.Today.Month.ToString();
        month.Min = 1;
        month.Max = 12;
        month.Dependents!.Add(Id);
        month.ReadOnly = true;
        month.Order = 0;
        month.DecimalPlaces = 0;
        Children!.Add(month);

        ANode day = new MathNode
        {
            Id = NewId(),
            Name = "Day",
            Parent = this,
            ParentTree = tree
        };
        day.Url = "TreeView/Description?id=" + day.Id;
        (day as MathNode)!.Expression = DateTime.Today.Day.ToString();
        day.Min = 1;
        day.Max = 31;
        day.Dependents!.Add(Id);
        day.ReadOnly = true;
        day.Order = 1;
        day.DecimalPlaces = 0;
        Children.Add(day);

        ANode year = new MathNode
        {
            Id = NewId(),
            Name = "Year",
            Parent = this,
            ParentTree = tree
        };
        year.Url = "TreeView/Description?id=" + year.Id;
        (year as MathNode)!.Expression = DateTime.Today.Year.ToString();
        year.MaxIsSet = year.MinIsSet = false;
        year.Min = 0;
        year.Max = 0;
        year.Dependents!.Add(Id);
        year.ReadOnly = true;
        year.Order = 2;
        year.DecimalPlaces = 0;
        Children.Add(year);

        //Set the expression
        Expression = ((MathNode)Children[0]).Expression + "/" + ((MathNode)Children[1]).Expression + "/" + ((MathNode)Children[2]).Expression;
    }
}

[Serializable]
public class ConditionalNode : ANode
{
    // *******Fields*****
    string _Expression = "";

    #region Properties
    public string Expression
    {
        get { return _Expression; }
        set { if (!ReadOnly) _Expression = value; }
    }
    #endregion

    // *****Methods*****

    public override decimal Total()
    {
        ParentTree.TotalCounter++;
        if (ParentTree.TotalCounter > TotalCounterMax)
        {
            ParentTree.TotalCounter = 0;
            throw new CircularReferenceException();
        }
        decimal formula_result = 0;
        Expression e = new(_Expression);
        e.EvaluateFunction += new EvaluateFunctionHandler(EvaluateFunction);
        e.EvaluateParameter += new EvaluateParameterHandler(EvaluateParameter);

        try
        {
            object result = e.Evaluate();
            //Restrict node to conditional expressions
            if (e.ParsedExpression.GetType().Name != "TernaryExpression")
            {
                ParentTree.TotalCounter = 0;
                throw new Exception("Incorrect expression for this type of node");
            }
            formula_result = decimal.Parse(result.ToString()!);
            ParentTree.TotalCounter--;
            if (MaxIsSet && formula_result > Max)
            {
                return Amount * (Max - (Max * Discount / 100));
            }
            else
            {
                if (MinIsSet && formula_result < Min) return Amount * (Min - (Min * Discount / 100));
                else return Amount * (formula_result - (formula_result * Discount / 100));
            }
        }
        catch (CircularReferenceException)
        {
            ParentTree.TotalCounter = 0;
            throw;
        }
        catch (Exception)
        {
            ParentTree.TotalCounter = 0;
            throw;
        }
    }

    public override bool HasErrors()
    {
        try
        {
            Expression e = new(_Expression);
            e.EvaluateFunction += new EvaluateFunctionHandler(EvaluateFunction);
            e.EvaluateParameter += new EvaluateParameterHandler(EvaluateParameter);
            bool hasErrors = e.HasErrors();
            Error = e.Error;
            return hasErrors;
        }
        catch (Exception) { return false; }
    }

    public override bool IsComplete()
    {
        if (Children == null || Children.Count == 0 || !BranchSelected()) return true;
        foreach (ANode n in Children)
        {
            if (n.Selected && !n.IsComplete()) return false;
        }
        return true;
    }

    public ConditionalNode()
    {
        Name = "";
        Discount = 0;
        MaxIsSet = MinIsSet = false;
        Min = Max = 0;
        Order = 0;
        DecimalPlaces = 2;
        Selected = false;
        Children = new List<ANode>();
        Dependents = new List<string>();
        References = new List<string>();
        Type = NodeType.Conditional;
        Parent = null;
        Amount = 1;
        Optional = false;
        DisableCondition = "0";
        DisabledMessage = "";
        Description = "";
        Hidden = false;
        ReadOnly = false;
        Expanded = false;
        ExpandedLevels = 0;
        Units = "";
        Report = false;
        ReportValue = false;
        EditChildren = false;
        Template = false;
        CheckBox = false;
    }

    public ConditionalNode(string path, ANode? parent, QTree parentTree, string id)
        : this()
    {
        if (parent != null) Parent = parent;
        ParentTree = parentTree;
        Name = path.Split(Path.DirectorySeparatorChar)[^1];
        Id = id;
        string value = GetValueFromDirectory("id", path);
        if (value != "") Id = value;

        value = GetValueFromDirectory("units", path);
        if (value != "") Units = value;

        value = GetValueFromDirectory("expression", path);
        if (value != "")
        {
            Expression = value;
        }
        else
        {
            value = GetValueFromDirectory("formula", path);
            if (value != "") Expression = value;
        }

        if (GetValueFromDirectory("maxisset", path) == "true") { MaxIsSet = true; }
        if (GetValueFromDirectory("minisset", path) == "true") { MinIsSet = true; }
        value = GetValueFromDirectory("max", path);
        if (int.TryParse(value, out int intResult)) Max = intResult;

        value = GetValueFromDirectory("min", path);
        if (int.TryParse(value, out intResult)) Min = intResult;

        value = GetValueFromDirectory("order", path);
        if (int.TryParse(value, out intResult)) Order = intResult;

        value = GetValueFromDirectory("decimalplaces", path);
        if (int.TryParse(value, out intResult)) DecimalPlaces = intResult;

        value = GetValueFromDirectory("discount", path);
        if (decimal.TryParse(value, out decimal decimalResult)) Discount = decimalResult;

        value = GetValueFromDirectory("amount", path);
        if (decimal.TryParse(value, out decimalResult)) Amount = decimalResult;

        value = GetValueFromDirectory("expandedlevels", path);
        if (int.TryParse(value, out intResult)) ExpandedLevels = intResult;

        value = GetValueFromDirectory("disablecondition", path);
        if (value != "") DisableCondition = value;

        value = GetValueFromDirectory("disabledmessage", path);
        if (value != "") DisabledMessage = value;

        if (GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (parent == null || (parent.Type != NodeType.Decision)) Selected = true; }
        if (GetValueFromDirectory("report", path) == "true") { Report = true; }
        if (GetValueFromDirectory("reportvalue", path) == "true") { ReportValue = true; }
        if (GetValueFromDirectory("editchildren", path) == "true") { EditChildren = true; }
        if (GetValueFromDirectory("template", path) == "true") { Template = true; }
        if (GetValueFromDirectory("hidden", path) == "true") { Hidden = true; }
        if (CheckBox)
            if (GetValueFromDirectory("selected", path) == "true") { Selected = true; }
        //To set the url for the node
        Url = "TreeView/Description?id=" + Id;
    }

    public ConditionalNode(NameValueCollection values, QTree tree)
        : this()
    {
        string id = values["id"]!;
        ANode? node;
        if (!string.IsNullOrEmpty(id))
        {
            node = tree.GetNodeFromId(id);
            if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
            if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
        }
        else
        {
            node = null;
        }

        //check for same name
        if (node?.Children != null)
        {
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;
        }

        //first set the node as writeable
        ReadOnly = false;
        if (node == null) Id = "1";
        else Id = node.NewId();
        Expression = values["expression"]!;
        EditChildren = values["editChildren"] == "true";
        Name = values["name"]!;
        if (int.TryParse(values["expandedLevels"], out int intResult))
            ExpandedLevels = intResult;
        if (int.TryParse(values["order"], out intResult))
            Order = intResult;
        if (int.TryParse(values["decimalPlaces"], out intResult))
            DecimalPlaces = intResult;
        if (decimal.TryParse(values["min"], out decimal decimalResult))
        {
            Min = decimalResult;
            MinIsSet = true;
        }
        if (decimal.TryParse(values["max"], out decimalResult))
        {
            Max = decimalResult;
            MaxIsSet = true;
        }
        if (decimal.TryParse(values["discount"], out decimalResult))
            Discount = decimalResult;
        Hidden = values["hidden"] == "true";
        Report = values["report"] == "true";
        ReportValue = values["reportValue"] == "true";
        Template = values["template"] == "true";
        ReadOnly = values["readOnly"] == "true";

        if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (node == null || (node.Type != NodeType.Decision)) Selected = true; }
        DisableCondition = values["disable"]!;
        DisabledMessage = values["disabledMessage"]!;
        if (node != null && node.Type == NodeType.Decision)
        {
            Dependents!.AddRange(node.Dependents!);
            Dependents.Add(node.Id);
        }
        Parent = node;
        ParentTree = tree;
        Url = "TreeView/Description?id=" + Id;

        //Add new node to children
        if (!HasErrors())
        {
            if (node != null)
            {
                node.Children!.Add(this);
            }
            else
            {
                tree.Root = this;
            }

            node?.SortChildren();
        }
    }
}

[Serializable]
public class DecisionNode : ANode
{
    // *****Properties*****
    public bool IsUserDecision { get; set; }

    // *****Methods** ***

    public override bool HasErrors()
    {
        return false;
    }

    public override bool IsComplete()
    {
        if (Children == null || Children.Count == 0 || !BranchSelected()) return true;

        foreach (ANode n in Children)
        {
            if (n.Selected && n.IsComplete()) return true;
        }
        return false;
    }

    public override decimal Total()
    {
        ParentTree.TotalCounter++;
        if (ParentTree.TotalCounter > TotalCounterMax)
        {
            ParentTree.TotalCounter = 0;
            throw new CircularReferenceException();
        }

        if (Optional && !Selected && !(Parent != null && Parent.Type == NodeType.Decision))
        {
            ParentTree.TotalCounter--;
            return 0;
        }

        //Get selected child total
        decimal selected_child_result = 0;

        foreach (ANode n in Children!)
        {
            if (n.Selected) { selected_child_result = n.Total(); break; }
        }

        ParentTree.TotalCounter--;
        if (MaxIsSet && selected_child_result > Max)
        {
            return Amount * (Max - (Max * Discount / 100));
        }
        else
        {
            if (MinIsSet && selected_child_result < Min) return Amount * (Min - (Min * Discount / 100));
            else return Amount * (selected_child_result - (selected_child_result * Discount / 100));
        }
    }

    public DecisionNode()
    {
        Name = "";
        Discount = 0;
        MaxIsSet = MinIsSet = false;
        Min = Max = 0;
        Order = 0;
        DecimalPlaces = 2;
        Selected = false;
        Children = new List<ANode>();
        Dependents = new List<string>();
        References = new List<string>();
        Type = NodeType.Decision;
        Parent = null;
        Amount = 1;
        Optional = false;
        DisableCondition = "0";
        DisabledMessage = "";
        Description = "";
        Hidden = false;
        ReadOnly = false;
        Expanded = false;
        ExpandedLevels = 0;
        Units = "";
        Report = false;
        ReportValue = false;
        Template = false;
    }

    public DecisionNode(string path, ANode? parent, QTree parentTree, string id)
        : this()
    {
        ParentTree = parentTree;
        if (parent != null) Parent = parent;

        string value = GetValueFromDirectory("id", path);
        if (value != "") Id = value;

        value = GetValueFromDirectory("units", path);
        if (value != "") Units = value;

        if (GetValueFromDirectory("maxisset", path) == "true") { MaxIsSet = true; }
        if (GetValueFromDirectory("minisset", path) == "true") { MinIsSet = true; }
        value = GetValueFromDirectory("max", path);
        if (int.TryParse(value, out int intResult)) Max = intResult;

        value = GetValueFromDirectory("min", path);
        if (int.TryParse(value, out intResult)) Min = intResult;

        value = GetValueFromDirectory("order", path);
        if (int.TryParse(value, out intResult)) Order = intResult;

        value = GetValueFromDirectory("decimalplaces", path);
        if (int.TryParse(value, out intResult)) DecimalPlaces = intResult;

        value = GetValueFromDirectory("discount", path);
        if (decimal.TryParse(value, out decimal decimalResult)) Discount = decimalResult;

        Name = path.Split(Path.DirectorySeparatorChar)[^1];
        Id = id;

        value = GetValueFromDirectory("amount", path);
        if (decimal.TryParse(value, out decimalResult)) Amount = decimalResult;

        value = GetValueFromDirectory("expandedlevels", path);
        if (int.TryParse(value, out intResult)) ExpandedLevels = intResult;

        value = GetValueFromDirectory("disablecondition", path);
        if (value != "") DisableCondition = value;

        value = GetValueFromDirectory("disabledmessage", path);
        if (value != "") DisabledMessage = value;

        if (GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (parent != null && parent.Type != NodeType.Decision) Selected = true; }
        if (GetValueFromDirectory("report", path) == "true") { Report = true; }
        if (GetValueFromDirectory("reportvalue", path) == "true") { ReportValue = true; }
        if (parent == null) Selected = true;
        if (GetValueFromDirectory("hidden", path) == "true") { Hidden = true; }
        if (CheckBox)
            if (GetValueFromDirectory("selected", path) == "true") { Selected = true; }

        //Set the node url
        Url = "TreeView/Description?id=" + Id;
    }

    public DecisionNode(NameValueCollection values, QTree tree)
        : this()
    {
        string id = values["id"]!;
        ANode? node;
        if (!string.IsNullOrEmpty(id))
        {
            node = tree.GetNodeFromId(id);
            if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
            if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
        }
        else
        {
            node = null;
        }

        //check for same name
        if (node?.Children != null)
        {
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;
        }

        //first set the node as writeable
        ReadOnly = false;
        if (node == null) Id = "1";
        else Id = node.NewId();
        Name = values["name"]!;
        if (int.TryParse(values["expandedLevels"], out int intResult))
            ExpandedLevels = intResult;
        if (int.TryParse(values["order"], out intResult))
            Order = intResult;
        if (int.TryParse(values["decimalPlaces"], out intResult))
            DecimalPlaces = intResult;
        if (decimal.TryParse(values["min"], out decimal decimalResult))
        {
            Min = decimalResult;
            MinIsSet = true;
        }
        if (decimal.TryParse(values["max"], out decimalResult))
        {
            Max = decimalResult;
            MaxIsSet = true;
        }
        if (decimal.TryParse(values["discount"], out decimalResult))
            Discount = decimalResult;
        Hidden = values["hidden"] == "true";
        Report = values["report"] == "true";
        ReportValue = values["reportValue"] == "true";
        Template = values["template"] == "true";
        ReadOnly = values["readOnly"] == "true";

        if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (node == null || (node.Type != NodeType.Decision)) Selected = true; }
        DisableCondition = values["disable"]!;
        DisabledMessage = values["disabledMessage"]!;
        if (node != null && node.Type == NodeType.Decision)
        {
            Dependents!.AddRange(node.Dependents!);
            Dependents.Add(node.Id);
        }
        Parent = node;
        ParentTree = tree;
        Url = "TreeView/Description?id=" + Id;

        //Add new node to children
        if (node != null)
        {
            node.Children!.Add(this);
        }
        else
        {
            tree.Root = this;
        }

        node?.SortChildren();
    }
}

[Serializable]
public class SumSetNode : ANode
{
    // *******Fields*****

    // *****Properties*****

    // *****Methods*****
    public override bool HasErrors()
    {
        return false;
    }

    public override bool IsComplete()
    {
        if (Children == null || Children.Count == 0 || !BranchSelected()) return true;
        foreach (ANode n in Children)
        {
            if (n.Selected && !n.IsComplete()) return false;
        }
        return true;
    }

    public override decimal Total()
    {
        ParentTree.TotalCounter++;
        if (ParentTree.TotalCounter > TotalCounterMax)
        {
            ParentTree.TotalCounter = 0;
            throw new CircularReferenceException();
        }

        decimal sum = 0;
        foreach (ANode n in Children!)
        {
            if (!n.Template) sum += n.Total();
        }
        ParentTree.TotalCounter--;
        if (MaxIsSet && sum > Max)
        {
            return Amount * (Max - (Max * Discount / 100));
        }
        else
        {
            if (MinIsSet && sum < Min) return Amount * (Min - (Min * Discount / 100));
            else return Amount * (sum - (sum * Discount / 100));
        }
    }

    public SumSetNode()
    {
        Name = "";
        Discount = 0;
        MaxIsSet = MinIsSet = false;
        Min = Max = 0;
        Order = 0;
        DecimalPlaces = 2;
        Selected = false;
        Children = new List<ANode>();
        Dependents = new List<string>();
        References = new List<string>();
        Type = NodeType.SumSet;
        Parent = null;
        Amount = 1;
        Optional = false;
        DisableCondition = "0";
        DisabledMessage = "";
        Description = "";
        Hidden = false;
        ReadOnly = false;
        Expanded = false;
        ExpandedLevels = 0;
        Units = "";
        Report = false;
        ReportValue = false;
        EditChildren = false;
        Template = false;
    }

    public SumSetNode(string path, ANode? parent, QTree parentTree, string id)
        : this()
    {
        if (parent != null) Parent = parent;
        ParentTree = parentTree;
        Name = path.Split(Path.DirectorySeparatorChar)[^1];
        Id = id;
        string value = GetValueFromDirectory("id", path);
        if (value != "") Id = value;

        value = GetValueFromDirectory("units", path);
        if (value != "") Units = value;

        if (GetValueFromDirectory("maxisset", path) == "true") { MaxIsSet = true; }
        if (GetValueFromDirectory("minisset", path) == "true") { MinIsSet = true; }
        value = GetValueFromDirectory("max", path);
        if (int.TryParse(value, out int intResult)) Max = intResult;

        value = GetValueFromDirectory("min", path);
        if (int.TryParse(value, out intResult)) Min = intResult;

        value = GetValueFromDirectory("order", path);
        if (int.TryParse(value, out intResult)) Order = intResult;

        value = GetValueFromDirectory("decimalplaces", path);
        if (int.TryParse(value, out intResult)) DecimalPlaces = intResult;

        value = GetValueFromDirectory("discount", path);
        if (decimal.TryParse(value, out decimal decimalResult)) Discount = decimalResult;

        value = GetValueFromDirectory("amount", path);
        if (decimal.TryParse(value, out decimalResult)) Amount = decimalResult;

        value = GetValueFromDirectory("expandedlevels", path);
        if (int.TryParse(value, out intResult)) ExpandedLevels = intResult;

        value = GetValueFromDirectory("disablecondition", path);
        if (value != "") DisableCondition = value;

        value = GetValueFromDirectory("disabledmessage", path);
        if (value != "") DisabledMessage = value;

        if (GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (parent == null || (parent.Type != NodeType.Decision)) Selected = true; }
        if (GetValueFromDirectory("report", path) == "true") { Report = true; }
        if (GetValueFromDirectory("reportvalue", path) == "true") { ReportValue = true; }
        if (GetValueFromDirectory("editchildren", path) == "true") { EditChildren = true; }
        if (GetValueFromDirectory("template", path) == "true") { Template = true; }
        if (GetValueFromDirectory("hidden", path) == "true") { Hidden = true; }
        if (CheckBox)
            if (GetValueFromDirectory("selected", path) == "true") { Selected = true; }

        //To set the node url
        Url = "TreeView/AppendNodes?id=" + Id;
    }

    public SumSetNode(NameValueCollection values, QTree tree)
        : this()
    {
        string id = values["id"]!;
        ANode? node;
        if (!string.IsNullOrEmpty(id))
        {
            node = tree.GetNodeFromId(id);
            if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
            if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
        }
        else
        {
            node = null;
        }

        //check for same name
        if (node?.Children != null)
        {
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;
        }

        //first set the node as writeable
        ReadOnly = false;
        if (node == null) Id = "1";
        else Id = node.NewId();
        Name = values["name"]!;
        EditChildren = values["editChildren"] == "true";
        if (int.TryParse(values["expandedLevels"], out int intResult))
            ExpandedLevels = intResult;
        if (int.TryParse(values["order"], out intResult))
            Order = intResult;
        if (int.TryParse(values["decimalPlaces"], out intResult))
            DecimalPlaces = intResult;
        if (decimal.TryParse(values["min"], out decimal decimalResult))
        {
            Min = decimalResult;
            MinIsSet = true;
        }
        if (decimal.TryParse(values["max"], out decimalResult))
        {
            Max = decimalResult;
            MaxIsSet = true;
        }
        if (decimal.TryParse(values["discount"], out decimalResult))
            Discount = decimalResult;
        Hidden = values["hidden"] == "true";
        Report = values["report"] == "true";
        ReportValue = values["reportValue"] == "true";
        Template = values["template"] == "true";
        ReadOnly = values["readOnly"] == "true";

        if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { Optional = true; CheckBox = true; }
        else { if (node == null || (node.Type != NodeType.Decision)) Selected = true; }
        DisableCondition = values["disable"]!;
        DisabledMessage = values["disabledMessage"]!;
        if (node != null && node.Type == NodeType.Decision)
        {
            Dependents!.AddRange(node.Dependents!);
            Dependents.Add(node.Id);
        }
        Parent = node;
        ParentTree = tree;
        Url = "TreeView/AppendNodes?id=" + Id;

        //Add new node to children
        if (node != null)
        {
            node.Children!.Add(this);
        }
        else
        {
            tree.Root = this;
        }

        node?.SortChildren();
    }
}

[Serializable]
public class ReferenceNode : ANode
{
    // *******Fields*****
    string _Expression = "";

    // *****Properties*****
    public string Expression
    {
        get { return _Expression; }
        set { if (!ReadOnly) _Expression = value; }
    }
    public ANode? TargetNode { get; set; } = null;
    public override decimal Discount => TargetNode != null ? TargetNode.Discount : 0;

    public override decimal Min
    {
        get { return TargetNode != null ? TargetNode.Min : 0; }
    }

    public override decimal Max
    {
        get { return TargetNode != null ? TargetNode.Max : 0; }
    }

    public override int DecimalPlaces
    {
        get { return TargetNode != null ? TargetNode.DecimalPlaces : 2; }
    }

    public override decimal Amount
    {
        get { return TargetNode != null ? TargetNode.Amount : 0; }
    }

    public override string Units
    {
        get { return TargetNode != null ? TargetNode.Units : ""; }
    }

    public override bool Selected
    {
        get { return TargetNode?.Selected == true; }
    }

    public override bool Optional
    {
        get { return TargetNode?.Optional == true; }
    }

    public override bool Disabled
    {
        get { return TargetNode?.Disabled == true; }
    }

    public override string DisabledMessage
    {
        get { return TargetNode != null ? TargetNode.DisabledMessage : ""; }
    }

    // *****Methods*****

    public override bool HasErrors()
    {
        return false;
    }

    public override bool IsComplete()
    {
        return TargetNode?.IsComplete() != false;
    }

    public override decimal Total()
    {
        return TargetNode != null ? TargetNode.Total() : 0;
    }

    public ReferenceNode()
    {
        Name = "";
        //this.Discount = 0;
        //this.Min = this.Max = 0;
        Expression = "";
        Order = 0;
        //this.Selected = false;
        Children = new List<ANode>();
        Dependents = new List<string>();
        References = new List<string>();
        Type = NodeType.Reference;
        Parent = null;
        //this.Amount = 1;
        //this.Optional = false;
        Description = "";
        Hidden = false;
        ReadOnly = false;
        Expanded = false;
        ExpandedLevels = 0;
        //this.Units = "";
        Report = false;
        ReportValue = false;
        Template = false;
    }

    public ReferenceNode(string path, ANode? parent, QTree parentTree, string id)
        : this()
    {
        if (parent != null) Parent = parent;
        ParentTree = parentTree;
        Name = path.Split(Path.DirectorySeparatorChar)[^1];
        Id = id;

        string value = GetValueFromDirectory("expression", path);
        if (value != "")
        {
            Expression = value;
        }
        else
        {
            value = GetValueFromDirectory("target", path);
            if (value != "") Expression = value;
        }

        TargetNode = ParentTree.GetNodeFromPath(Expression)!;

        value = GetValueFromDirectory("expandedlevels", path);
        if (int.TryParse(value, out int intResult)) ExpandedLevels = intResult;

        value = GetValueFromDirectory("order", path);
        if (int.TryParse(value, out intResult)) Order = intResult;

        if (GetValueFromDirectory("report", path) == "true") { Report = true; }
        if (GetValueFromDirectory("reportvalue", path) == "true") { ReportValue = true; }
        if (GetValueFromDirectory("template", path) == "true") { Template = true; }
        if (GetValueFromDirectory("hidden", path) == "true") { Hidden = true; }
        if (CheckBox)
            if (GetValueFromDirectory("selected", path) == "true") { Selected = true; }

        //To set the node url
        Url = "TreeView/Description?id=" + Id;
    }

    public ReferenceNode(NameValueCollection values, QTree tree)
        : this()
    {
        string id = values["id"]!;
        ANode? node;
        if (!string.IsNullOrEmpty(id))
        {
            node = tree.GetNodeFromId(id);
            if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
            if (node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
        }
        else
        {
            node = null;
        }

        //check for same name
        if (node?.Children != null)
        {
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;
        }

        //first set the node as writeable
        ReadOnly = false;
        if (node == null) Id = "1";
        else Id = node.NewId();
        Expression = values["expression"]!;
        TargetNode = tree.GetNodeFromPath(values["expression"]!)!;
        Name = values["name"]!;
        if (int.TryParse(values["expandedLevels"], out int intResult))
            ExpandedLevels = intResult;
        if (int.TryParse(values["order"], out intResult))
            Order = intResult;
        Hidden = values["hidden"] == "true";
        Report = values["report"] == "true";
        ReportValue = values["reportValue"] == "true";
        Template = values["template"] == "true";
        ReadOnly = values["readOnly"] == "true";

        if (node != null && node.Type == NodeType.Decision)
        {
            Dependents!.AddRange(node.Dependents!);
            Dependents.Add(node.Id);
        }
        Parent = node;
        ParentTree = tree;
        Url = "TreeView/Description?id=" + Id;

        //Add new node to children
        if (node != null)
        {
            node.Children!.Add(this);
        }
        else
        {
            tree.Root = this;
        }

        node?.SortChildren();
    }
}

[Serializable]
public class QTree
{
    // *****Fields*****
    ANode? _Root;
    // Next field is used to detect circular references.
    [XmlIgnore]
    public int TotalCounter = 0;
    [XmlIgnore]
    public int EvaluateParameterCounter = 0;

    public ANode? Root
    {
        get { return _Root; }
        set { _Root = value; }
    }

    // *****Methods*****

    public Dictionary<string, string> GetSelections()
    {
        Dictionary<string, string> selection = new();
        GetSelections(Root!, 0, selection);
        return selection;
    }

    //this method is not used right now
    public static string GetSelectionsString(ANode start)
    {
        string s = "";
        if (start?.Hidden == false && start.Selected && start.Report)
        {
            s += start.Name;
            if (start.ReportValue)
            {
                s += " = ";
                if (start.Units != "$") s += decimal.Round(start.Total(), 2) + " " + start.Units;
                else s += start.Units + decimal.Round(start.Total(), 2);
            }

            if (start.Children == null || start.Children.Count == 0)
            {
                return s;
            }
            else
            {
                foreach (ANode n in start.Children)
                {
                    string s1 = GetSelectionsString(n);
                    if (n != null && s1.Trim() != "") s += "| " + s1;
                    s1 = "";
                }
                return s;
            }
        }
        return s;
    }

    //this method is not used right now
    public static string GetSelectionsString(ANode start, int indent)
    {
        string s = "";
        string indentspace = "";
        if (start?.Hidden == false && start.Selected && start.Report)
        {
            s += start.Name;
            if (start.ReportValue)
            {
                s += " = ";
                if (start.Units != "$") s += decimal.Round(start.Total(), 2) + " " + start.Units;
                else s += start.Units + decimal.Round(start.Total(), 2);
            }

            for (int i = 0; i < indent; i++) indentspace += " ";

            if (start.Children == null || start.Children.Count == 0 || DontReportChildren(start))
            {
                return indentspace + s;
            }
            else
            {
                s = indentspace + s;
                foreach (ANode n in start.Children)
                {
                    string s1 = GetSelectionsString(n, indent + 2);
                    if (n != null && s1.Trim() != "") s = s + "|" + s1;
                    s1 = "";
                }
                return s;
            }
        }
        return s;
    }

    public static string GetSelections(ANode start, int indent, Dictionary<string, string> selection)
    {
        string s = "";
        string indentspace = "";
        if (start?.Hidden == false && start.Selected && start.Report)
        {
            s += start.Name;
            if (start.ReportValue)
            {
                if (start.Type == NodeType.Text)
                {
                    s += " [";
                    s += ((TextNode)start).Expression;
                    s += "]";
                }
                else
                {
                    s += " [";
                    decimal total = start.Total();
                    string formatString = string.Concat("{0:F", start.DecimalPlaces, "}");

                    if (ANode.IsCurrencySymbol(start.Units))
                    {
                        if (total == 0) s += start.Units + 0;
                        else s += start.Units + string.Format(formatString, total);
                    }
                    else
                    {
                        string _units = start.Units != "" ? " " + start.Units : "";
                        if (total == 0) s += 0 + _units;
                        else s += string.Format(formatString, total) + _units;
                    }

                    s += "]";
                }
            }

            for (int i = 0; i < indent; i++) indentspace += " ";
            selection.Add(start.Id, indentspace + s);

            if (start.Children == null || start.Children.Count == 0 || DontReportChildren(start))
            {
                return indentspace + s;
            }
            else
            {
                s = indentspace + s;
                int counter = 1;
                foreach (ANode n in start.Children)
                {
                    string childID = n.Id;
                    string s1 = GetSelections(n, indent + 2, selection);
                    if (n != null && s1.Trim() != "")
                    {
                        s = s + "|" + s1;
                        counter++;
                    }
                    s1 = "";
                }
                return s;
            }
        }
        return s;
    }

    public static bool DontReportChildren(ANode node)
    {
        foreach (ANode child in node.Children!)
        {
            if (!child.Hidden && child.Report) return false;
        }
        return true;
    }

    public static string GetValueFromDirectory(string field, string dir)
    {
        string? line;
        string s = "";
        string[] splitted;

        try
        {
            using StreamReader sr = new(dir + Path.DirectorySeparatorChar + "values.txt");

            // Read and display lines from the file until the end of 
            // the file is reached.
            while ((line = sr.ReadLine()) != null)
            {
                s += line;
            }
        }
        catch (Exception)
        {
            return "";
        }

        if (s?.Length == 0 || !s!.Contains(field))
        {
            return "";
        }
        else
        {
            string the_field = "";
            splitted = s.Split(";".ToCharArray());
            foreach (string part in splitted)
            {
                if (part.Split("=".ToCharArray())[0].Contains(field))
                {
                    the_field = part.Split("=".ToCharArray())[1];
                    the_field = the_field.Replace("\"", "");
                    break;
                }
            }
            return the_field;
        }
    }

    public void Fill(string path, ref ANode? node, ANode? parent, string id)
    {
        string type = GetValueFromDirectory("type", path);

        //Create nodes from directory folders
        if (type != null)
        {
            type = type.ToLower();
            if (type == "decision") node = new DecisionNode(path, parent, this, id);

            if (type == "math") node = new MathNode(path, parent, this, id);

            if (type == "text") node = new TextNode(path, parent, this, id);

            if (type == "sumset") node = new SumSetNode(path, parent, this, id);

            if (type == "conditional") node = new ConditionalNode(path, parent, this, id);

            if (type == "reference") node = new ReferenceNode(path, parent, this, id);

            if (type == "today") node = new TodayNode(path, parent, this, id);

            if (type == "date") node = new DateNode(path, parent, this, id);

            string[] dirs = Directory.GetDirectories(path);
            if (dirs.Length != 0)
            {
                int child_id = 1;
                foreach (string dir in dirs)
                {
                    ANode? newnode = null;
                    Fill(dir, ref newnode, node, id + "." + child_id.ToString());
                    if (newnode != null && node != null) node.Children!.Add(newnode);
                    child_id++;
                }
            }
        }
    }

    public ANode? GetNodeFromId(string id)
    {
        try
        {
            string[] arr = id.Split(".".ToCharArray());
            ANode? node = _Root;
            string ID = node!.Id;
            for (int i = 1; i < arr.Length; i++)
            {
                ID += "." + arr[i];
                foreach (ANode child in node.Children!)
                {
                    if (child.Id == ID)
                    {
                        node = child;
                        break;
                    }
                }
            }
            return node;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public ANode GetNode(string name)
    {
        return GetNode(name, _Root!)!;
    }

    public ANode? GetNodeFromPath(string path)
    {
        path = path.Replace("\\", "/");
        string[] parts = path.Split("/".ToCharArray());
        ANode? n = _Root;
        try
        {
            if (parts.Length == 0 || n!.Name.Trim() != parts[0].Trim()) return null;
            else if (parts.Length == 1) return n;
            for (int i = 1; i < parts.Length; i++)
            {
                if (parts[i].Trim()?.Length == 0) return n;
                if (n?.FindChildNode(parts[i].Trim()) != null) n = n.FindChildNode(parts[i].Trim());
                else return null;
            }
            return n;
        }
        catch (Exception) { return null; }
    }

    private static ANode? GetNode(string name, ANode start)
    {
        if (start == null)
        {
            return null;
        }
        else
            if (start.Name == name)
        {
            return start;
        }
        else
                if (start.Children == null || start.Children.Count == 0)
        {
            return null;
        }
        else
        {
            ANode? tmp;
            foreach (ANode n in start.Children)
            {
                tmp = GetNode(name, n);
                if (tmp != null) return tmp;
            }
            return null;
        }
    }

    public static void RemoveNodesFromDirectory(string removedNodePaths)
    {
        //delete nodes
        if (!string.IsNullOrEmpty(removedNodePaths))
        {
            var splitted = removedNodePaths.Trim().Split(";".ToCharArray());
            foreach (var nodePath in splitted)
            {
                if (Directory.Exists(nodePath)) Directory.Delete(nodePath, true);
            }
        }
    }
    public static void SaveTreeTo(ANode start, string path, Dictionary<string, string> renamed)
    {
        try
        {
            if (!Directory.Exists(path + Path.DirectorySeparatorChar + start.Name))
            {
                //if the node was renamed rename the folder.
                if (renamed?.ContainsKey(start.Id) == true)
                {
                    if (Directory.Exists(path + Path.DirectorySeparatorChar + renamed[start.Id]))
                    {
                        Directory.Move(path + Path.DirectorySeparatorChar + renamed[start.Id], path + Path.DirectorySeparatorChar + start.Name);
                    }
                }
                else
                {
                    Directory.CreateDirectory(path + Path.DirectorySeparatorChar + start.Name);
                }
            }
            StreamWriter sw = File.CreateText(path + Path.DirectorySeparatorChar + start.Name + Path.DirectorySeparatorChar + "values.txt");

            sw.WriteLine("id=\"" + start.Id + "\";");
            sw.WriteLine("type=\"" + start.Type.ToString() + "\";");
            sw.WriteLine("order=\"" + start.Order.ToString() + "\";");
            sw.WriteLine("decimalplaces=\"" + start.DecimalPlaces.ToString() + "\";");
            sw.WriteLine("report=\"" + start.Report.ToString().ToLower() + "\";");
            sw.WriteLine("reportvalue=\"" + start.ReportValue.ToString().ToLower() + "\";");
            if (start.Type != NodeType.Reference)
            {
                sw.WriteLine("units=\"" + start.Units + "\";");
                sw.WriteLine("optional=\"" + start.Optional.ToString().ToLower() + "\";");
                if (start.MaxIsSet)
                    sw.WriteLine("max=\"" + start.Max.ToString() + "\";");
                else sw.WriteLine("max=\"∞\";");
                sw.WriteLine("maxisset=\"" + start.MaxIsSet.ToString().ToLower() + "\";");
                if (start.MinIsSet)
                    sw.WriteLine("min=\"" + start.Min.ToString() + "\";");
                else sw.WriteLine("min=\"∞\";");
                sw.WriteLine("minisset=\"" + start.MinIsSet.ToString().ToLower() + "\";");
                sw.WriteLine("discount=\"" + start.Discount.ToString() + "\";");
                sw.WriteLine("disablecondition=\"" + start.DisableCondition + "\";");
                sw.WriteLine("disabledmessage=\"" + start.DisabledMessage + "\";");
            }
            sw.WriteLine("expandedlevels=\"" + start.ExpandedLevels.ToString() + "\";");
            sw.WriteLine("hidden=\"" + start.Hidden.ToString().ToLower() + "\";");
            sw.WriteLine("readonly=\"" + start.ReadOnly.ToString().ToLower() + "\";");
            sw.WriteLine("template=\"" + start.Template.ToString().ToLower() + "\";");
            if (start.Optional)
                sw.WriteLine("selected=\"" + start.Selected.ToString().ToLower() + "\";");

            if (start.Type == NodeType.Math)
            {
                sw.WriteLine("expression=\"" + (start as MathNode)!.Expression + "\";");
                sw.WriteLine("editchildren=\"" + (start as MathNode)!.EditChildren.ToString().ToLower() + "\";");
            }

            if (start.Type == NodeType.Date)
            {
                sw.WriteLine("expression=\"" + (start as DateNode)!.Expression + "\";");
                sw.WriteLine("editchildren=\"" + (start as DateNode)!.EditChildren.ToString().ToLower() + "\";");
            }

            if (start.Type == NodeType.Today)
            {
                //sw.WriteLine("expression=\"" + (start as TodayNode)!.Expression + "\";"); 
                sw.WriteLine("editchildren=\"" + (start as TodayNode)!.EditChildren.ToString().ToLower() + "\";");
            }

            if (start.Type == NodeType.Text)
            {
                sw.WriteLine("expression=\"" + (start as TextNode)!.Expression + "\";");
                sw.WriteLine("editchildren=\"" + (start as TextNode)!.EditChildren.ToString().ToLower() + "\";");
            }

            if (start.Type == NodeType.Conditional)
            {
                sw.WriteLine("expression=\"" + (start as ConditionalNode)!.Expression + "\";");
                sw.WriteLine("editchildren=\"" + (start as ConditionalNode)!.EditChildren.ToString().ToLower() + "\";");
            }

            if (start.Type == NodeType.Reference)
            {
                sw.WriteLine("expression=\"" + (start as ReferenceNode)!.Expression + "\";");
            }
            sw.Close();

            foreach (ANode child in start.Children!)
                SaveTreeTo(child, path + Path.DirectorySeparatorChar + start.Name, renamed!);
        }
        catch (Exception e)
        {
            _ = e.Message;
        }
    }

    //Create a scaffolding of dependencies by hierarchy and immediate references
    public static void SetDependentsByHierarchy(ANode node, Stack<ANode> s)
    {
        char[] charArr = { '*', '/', '+', '-', '|', '[', ']', '?', '&', '!', '(', ')', '>', '<', '=', ':', ';' };
        s.Push(node);
        foreach (ANode child in node.Children!)
        {
            if (node.Type == NodeType.Decision
                || node.Type == NodeType.SumSet
                || node.Type == NodeType.Date
                || (node.Type == NodeType.Math && Array.IndexOf((node as MathNode)!.Expression.Split(charArr), child.Name) > -1)
                || (node.Type == NodeType.Conditional && Array.IndexOf((node as ConditionalNode)!.Expression.Split(charArr), child.Name) > -1)
                )
            {
                foreach (ANode dependent in s)
                {
                    if (!child.Dependents!.Contains(dependent.Id))
                        child.Dependents.Add(dependent.Id);
                }

                SetDependentsByHierarchy(child, s);
                //Add reference
                if (!child.References!.Contains(node.Id)) child.References.Add(node.Id);
            }
            else
            {
                SetDependentsByHierarchy(child, new Stack<ANode>());
            }
        }
        s.Pop();
    }

    //Takes care of the external references and then some more.
    public Tuple<ANode, ANode>? SetDependentsByReference(ANode node, bool recursive)
    {
        Tuple<ANode, ANode>? tuple = null;
        ANode? NodeFromPath, NodeFromId;
        if (node.Type == NodeType.Math || node.Type == NodeType.Conditional || node.Type == NodeType.Reference)
        {
            string expression = "";
            switch (node.Type)
            {
                case NodeType.Math:
                    expression = (node as MathNode)!.Expression;
                    break;
                case NodeType.Conditional:
                    expression = (node as ConditionalNode)!.Expression;
                    break;
                case NodeType.Reference:
                    expression = (node as ReferenceNode)!.Expression;
                    break;
                case NodeType.Decision:
                    break;
                case NodeType.Text:
                    break;
                case NodeType.SumSet:
                    break;
                case NodeType.Date:
                    break;
                case NodeType.Today:
                    break;
                default:
                    expression = "";
                    break;
            }
            //set dependecies for node expression
            string clean_expression = expression;
            string s1;

            clean_expression = Regex.Replace(clean_expression, @"\.selected", "", RegexOptions.IgnoreCase);
            clean_expression = Regex.Replace(clean_expression, @"\.disabled", "", RegexOptions.IgnoreCase);
            clean_expression = Regex.Replace(clean_expression, @"\.max", "", RegexOptions.IgnoreCase);
            clean_expression = Regex.Replace(clean_expression, @"\.min", "", RegexOptions.IgnoreCase);
            clean_expression = Regex.Replace(clean_expression, @"\.discount", "", RegexOptions.IgnoreCase);

            foreach (string s in clean_expression.Split(new char[] { '*', '/', '+', '-', '|', '[', ']', '?', '&', '!', '(', ')', '>', '<', '=', ':', ';' }))
            {
                s1 = s.Trim();
                if (s.Contains('\\')) //if the node is a full path
                {
                    NodeFromPath = GetNodeFromPath(s1);
                    if (NodeFromPath != null)
                    {
                        //check for circular references
                        tuple = SetDependenciesRecursively(NodeFromPath, node);
                        if (tuple != null) return tuple;

                        //Add reference
                        if (!NodeFromPath.References!.Contains(node.Id))
                            NodeFromPath.References.Add(node.Id);
                    }
                }
                else
                if (s1.StartsWith("{") && s1.EndsWith("}")) //if the node is an id
                {
                    NodeFromId = GetNodeFromId(s1[1..^1]);
                    if (NodeFromId != null)
                    {
                        //check for circular references
                        tuple = SetDependenciesRecursively(NodeFromId, node);
                        if (tuple != null) return tuple;

                        //Add reference
                        if (!NodeFromId.References!.Contains(node.Id))
                            NodeFromId.References.Add(node.Id);
                    }
                }
                else
                {
                    foreach (ANode child in node.Children!)
                    {
                        //check for local references
                        if (s.Contains(child.Name))
                        {
                            //check for circular references
                            tuple = SetDependenciesRecursively(child, node);
                            if (tuple != null) return tuple;

                            //Add reference
                            if (!child.References!.Contains(node.Id)) child.References.Add(node.Id);
                        }
                    }
                }
            }
        }
        //set dependencies for disable expression
        if (node.DisableCondition != null)
        {
            string s1;
            string clean_disable_expression = node.DisableCondition;
            clean_disable_expression = Regex.Replace(clean_disable_expression, @"\.selected", "", RegexOptions.IgnoreCase);
            clean_disable_expression = Regex.Replace(clean_disable_expression, @"\.disabled", "", RegexOptions.IgnoreCase);
            clean_disable_expression = Regex.Replace(clean_disable_expression, @"\.max", "", RegexOptions.IgnoreCase);
            clean_disable_expression = Regex.Replace(clean_disable_expression, @"\.min", "", RegexOptions.IgnoreCase);
            clean_disable_expression = Regex.Replace(clean_disable_expression, @"\.discount", "", RegexOptions.IgnoreCase);
            string[] splitted_disable_expression = clean_disable_expression.Split(new char[] { '*', '/', '+', '-', '|', '[', ']', '?', '&', '!', '(', ')', '>', '<', '=', ':', ';' });

            foreach (string s in splitted_disable_expression)
            {
                s1 = s.Trim();
                if (s.Contains('\\')) //if the node is a full path
                {
                    NodeFromPath = GetNodeFromPath(s1);
                    if (NodeFromPath != null)
                    {
                        //check for circular references
                        tuple = SetDependenciesRecursively(NodeFromPath, node);
                        if (tuple != null) return tuple;

                        //Add reference
                        if (!NodeFromPath.References!.Contains(node.Id))
                            NodeFromPath.References.Add(node.Id);
                    }
                }
                else
                if (s1.StartsWith("{") && s1.EndsWith("}")) //if the node is an id
                {
                    NodeFromId = GetNodeFromId(s1[1..^1]);
                    if (NodeFromId != null)
                    {
                        //check for circular references
                        tuple = SetDependenciesRecursively(NodeFromId, node);
                        if (tuple != null) return tuple;

                        //Add reference
                        if (!NodeFromId.References!.Contains(node.Id))
                            NodeFromId.References.Add(node.Id);
                    }
                }
                else
                {
                    foreach (ANode child in node.Children!)
                    {
                        //check for local references
                        if (s.Contains(child.Name))
                        {
                            //check for circular references
                            tuple = SetDependenciesRecursively(child, node);
                            if (tuple != null) return tuple;

                            //Add reference
                            if (!child.References!.Contains(node.Id)) child.References.Add(node.Id);
                        }
                    }
                }
            }
        }
        if (node.Type == NodeType.Decision || node.Type == NodeType.SumSet || node.Type == NodeType.Date)
        {
            foreach (ANode child in node.Children!)
            {
                tuple = SetDependenciesRecursively(child, node);
                if (tuple != null) return tuple;
            }
        }

        foreach (ANode child in node.Children!)
        {
            if (recursive)
            {
                tuple = SetDependentsByReference(child, true);
                if (tuple != null) return tuple;
            }
        }
        return tuple;
    }

    public Tuple<ANode, ANode>? SetDependents()
    {
        RemoveDependents(Root!);
        RemoveReferences(Root!);
        //the following method call is no longer needed, improves greatly performance.
        //SetDependentsByHierarchy(Root, stack);
        //This needs to be done twice in order to catch all dependents
        Tuple<ANode, ANode>? tuple = SetDependentsByReference(Root!, true) ?? SetDependentsByReference(Root!, true);
        return tuple;
    }

    public static void RemoveDependents(ANode start)
    {
        if (start != null)
        {
            start.Dependents!.Clear();
            foreach (ANode child in start.Children!)
                RemoveDependents(child);
        }
    }

    public static void RemoveReferences(ANode start)
    {
        if (start != null)
        {
            start.References!.Clear();
            foreach (ANode child in start.Children!)
                RemoveReferences(child);
        }
    }

    //If A depends on B and B depends on C and D then A depends on C and D
    public Tuple<ANode, ANode>? SetDependenciesRecursively(ANode start, ANode dependent)
    {
        if (dependent.Dependents!.Contains(start.Id)) return new Tuple<ANode, ANode>(start, dependent);
        if (!start.Dependents!.Contains(dependent.Id)) start.Dependents.Add(dependent.Id);
        Tuple<ANode, ANode>? tuple;
        foreach (ANode node in GetDependencies(start))
        {
            if (dependent.Dependents.Contains(node.Id)) return new Tuple<ANode, ANode>(node, dependent);
            if (!node.Dependents!.Contains(dependent.Id)) node.Dependents.Add(dependent.Id);
            tuple = SetDependenciesRecursively(node, dependent);
            if (tuple != null) return tuple;
        }
        foreach (string dependent2 in dependent.Dependents)
        {
            if (GetNodeFromId(dependent2)!.Dependents!.Contains(start.Id)) return new Tuple<ANode, ANode>(start, GetNodeFromId(dependent2)!);
            tuple = SetDependenciesRecursively(start, GetNodeFromId(dependent2)!);
            if (tuple != null) return tuple;
        }
        return null;
    }

    public List<ANode> GetDependencies(ANode dependent)
    {
        List<ANode> list = new();
        GetDependencies(dependent, _Root!, list);
        return list;
    }

    private static void GetDependencies(ANode dependent, ANode start, List<ANode> returnedList)
    {
        if (start.Dependents!.Contains(dependent.Id)) returnedList.Add(start);
        foreach (ANode child in start.Children!)
            GetDependencies(dependent, child, returnedList);
    }

    public static void ResetEntered(ANode node)
    {
        if (node.Type == NodeType.Math)
            ((MathNode)node).Entered = false;
        if (node.Type == NodeType.Text)
            ((TextNode)node).Entered = false;
        foreach (ANode n in node.Children!)
            ResetEntered(n);
    }

    public ANode? SaveNodeInfo(NameValueCollection values)
    {
        try
        {
            string id = values["id"]!;
            ANode node = GetNodeFromId(id)!;

            //Check for empty name
            if (values["name"]!.Trim()?.Length == 0) return null;

            //check for same name
            if (node != Root)
            {
                foreach (ANode n in node.Parent!.Children!)
                {
                    if (n.Name.Trim() == values["name"]!.Trim() && n != node)
                    {
                        return null;
                    }
                }
            }

            //first set the read-only attribute to false
            node.ReadOnly = false;
            if (node.Parent == null || !(node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today))
                node.Name = values["name"]!.Trim();

            node.Units = values["units"]!.Trim();
            switch (node.Type)
            {
                case NodeType.Math:
                    (node as MathNode)!.Expression = values["expression"]!.Trim();
                    (node as MathNode)!.EditChildren = values["editChildren"] == "true";

                    //To set the url for the node
                    decimal flag;
                    if (decimal.TryParse((node as MathNode)!.Expression, out flag) || (node as MathNode)!.EditChildren)
                    {
                        node.Url = "TreeView/ChangeTreeValue?id=" + node.Id;
                    }
                    else
                    {
                        node.Url = "TreeView/Description?id=" + node.Id;
                    }
                    break;
                case NodeType.Text:
                    (node as TextNode)!.Expression = values["expression"]!.Trim();
                    (node as TextNode)!.EditChildren = values["editChildren"] == "true";

                    //Set node url
                    node.Url = "TreeView/ChangeTreeValue?id=" + node.Id;
                    break;
                case NodeType.Date:
                    (node as DateNode)!.EditChildren = values["editChildren"] == "true";

                    //Set node url
                    node.Url = "TreeView/Description?id=" + node.Id;
                    break;
                case NodeType.Today:
                    (node as TodayNode)!.EditChildren = false;

                    //Set node url
                    node.Url = "TreeView/Description?id=" + node.Id;
                    break;
                case NodeType.Conditional:
                    (node as ConditionalNode)!.Expression = values["expression"]!.Trim();
                    (node as ConditionalNode)!.EditChildren = values["editChildren"] == "true";

                    //To set the url for the node
                    node.Url = "TreeView/Description?id=" + node.Id;
                    break;
                case NodeType.SumSet:
                    (node as SumSetNode)!.EditChildren = values["editChildren"] == "true";

                    //To set the url for the node
                    node.Url = "TreeView/Description?id=" + node.Id;
                    break;
                case NodeType.Reference:
                    (node as ReferenceNode)!.Expression = values["expression"]!.Trim();
                    (node as ReferenceNode)!.TargetNode = GetNodeFromPath(values["expression"]!)!;
                    //Set node url
                    node.Url = "TreeView/Description?id=" + node.Id;
                    break;
                default:
                    //To set the url for the node
                    node.Url = "TreeView/Description?id=" + node.Id;
                    break;
            }

            if (node.Parent != null && node.Parent.Type == NodeType.Date)
            {
                ((DateNode)node.Parent).Expression = ((MathNode)node.Parent.Children![0]).Expression + "/" + ((MathNode)node.Parent.Children![1]).Expression + "/" + ((MathNode)node.Parent.Children![2]).Expression;
            }

            if (node.Parent != null && node.Parent.Type == NodeType.Today)
            {
                ((TodayNode)node.Parent).Expression = ((MathNode)node.Parent.Children![0]).Expression + "/" + ((MathNode)node.Parent.Children![1]).Expression + "/" + ((MathNode)node.Parent.Children![2]).Expression;
            }

            if (int.TryParse(values["expandedLevels"], out int intResult))
                node.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                node.Order = intResult;

            if (node.Type == NodeType.Math && node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today))
            {
                node.DecimalPlaces = 0;
            }
            else
            if (int.TryParse(values["decimalPlaces"], out intResult))
            {
                node.DecimalPlaces = intResult;
            }

            if (node.Type != NodeType.Reference)
            {
                if (decimal.TryParse(values["min"], out decimal decimalResult))
                {
                    node.Min = decimalResult;
                    node.MinIsSet = true;
                }
                else
                {
                    node.MinIsSet = false;
                }

                if (decimal.TryParse(values["max"], out decimalResult))
                {
                    node.Max = decimalResult;
                    node.MaxIsSet = true;
                }
                else
                {
                    node.MaxIsSet = false;
                }

                if (decimal.TryParse(values["discount"], out decimalResult))
                    node.Discount = decimalResult;
                node.Optional = values["optional"] == "true";
                node.DisableCondition = values["disable"]!.Trim();
                node.DisabledMessage = values["disabledMessage"]!.Trim();
            }
            node.Hidden = values["hidden"] == "true";
            node.Report = values["report"] == "true";
            node.ReportValue = values["reportValue"] == "true";
            node.Template = values["template"] == "true";
            node.ReadOnly = values["readOnly"] == "true";

            Root!.SortChildren();

            return node;
        }
        catch (Exception e)
        {
            _ = e.Message;
            return null;
        }
    }

    public ANode? NewNode(NameValueCollection values)
    {
        ANode? newnode = null;
        string nodeType = values["type"]!;
        switch (nodeType)
        {
            case "Math":
                newnode = new MathNode(values, this);
                break;
            case "Text":
                newnode = new TextNode(values, this);
                break;
            case "Conditional":
                newnode = new ConditionalNode(values, this);
                break;
            case "Decision":
                newnode = new DecisionNode(values, this);
                break;
            case "SumSet":
                newnode = new SumSetNode(values, this);
                break;
            case "Reference":
                newnode = new ReferenceNode(values, this);
                break;
            case "Date":
                newnode = new DateNode(values, this);
                //Set the expression
                ((DateNode)newnode).Expression = ((MathNode)newnode.Children![0]).Expression + "/" + ((MathNode)newnode.Children![1]).Expression + "/" + ((MathNode)newnode.Children![2]).Expression;
                break;
            case "Today":
                newnode = new TodayNode(values, this);
                //Set the expression
                ((TodayNode)newnode).Expression = ((MathNode)newnode.Children![0]).Expression + "/" + ((MathNode)newnode.Children![1]).Expression + "/" + ((MathNode)newnode.Children![2]).Expression;
                break;
            default:
                break;
        }
        if (newnode == null || newnode.Name.Trim()?.Length == 0) return null;
        return newnode;
    }
    public ANode? CloneNode(string sourceId, string targetId)
    {
        try
        {
            ANode source = GetNodeFromId(sourceId)!;
            ANode target = GetNodeFromId(targetId)!;
            ANode clone = source.Clone();

            clone.Parent = target;

            //check for same name
            bool equals = false;
            int counter = 0;

            foreach (ANode n in target.Children!)
            {
                if (n.Name.Trim() == clone.Name.Trim())
                {
                    equals = true;
                }
                else
                    if (n.Name.Trim().Length > clone.Name.Trim().Length && n.Name.Trim().StartsWith(clone.Name.Trim()))
                {
                    int lengthDifference = n.Name.Trim().Length - clone.Name.Trim().Length;
                    string ending = n.Name.Trim().Substring(clone.Name.Trim().Length, lengthDifference);
                    if (ending.StartsWith(" "))
                    {
                        if (int.TryParse(ending.Remove(0, 1), out int result))
                        {
                            if (result > counter) counter = result;
                        }
                    }
                }
            }

            if (counter > 0)
            {
                clone.Name = clone.Name.Trim() + " " + (counter + 1).ToString();
            }
            else
                if (equals)
            {
                clone.Name = clone.Name.Trim() + " 1";
            }

            if (target.Type == NodeType.Decision)
            {
                clone.Optional = true;
                clone.CheckBox = true;
                clone.Selected = false;
            }
            clone.Parent = target;
            clone.ParentTree = this;
            target.Children.Add(clone);

            //fix the node id and url
            FixClone(clone, this);

            System.GC.Collect();
            return clone;
        }
        catch (Exception)
        {
            return null;
        }
    }
    public ANode? CloneTemplate(string sourceId, string targetId)
    {
        try
        {
            ANode source = GetNodeFromId(sourceId)!;
            ANode target = GetNodeFromId(targetId)!;
            ANode clone = source.Clone();
            clone.Parent = target;
            clone.ParentTree = this;
            clone.Template = false;
            clone.Hidden = false;
            FixClone(clone, this);
            target.Children!.Add(clone);
            //Set dependencies
            Stack<ANode> stack = new();
            SetDependentsByHierarchy(Root!, stack);
            return clone;
        }
        catch (Exception) { return null; }
    }

    public static void FixClone(ANode node, QTree parentTree)
    {
        node.Id = node.Parent!.NewId();
        node.Url = node.Url.Split('=')[0] + "=" + node.Id;
        node.References!.Clear();
        node.Dependents!.Clear();
        node.ParentTree = parentTree;
        FixClonedChildren(node.Children!, node);
    }

    public static void FixClonedChildren(List<ANode> children, ANode parent)
    {
        if (children != null && children.Count != 0 && parent != null)
        {
            string[] id_splitted;
            foreach (ANode child in children)
            {
                id_splitted = child.Id.Split('.');
                child.Id = parent.Id + "." + id_splitted[^1];
                child.Url = child.Url.Split('=')[0] + "=" + child.Id;
                child.References!.Clear();
                child.Dependents!.Clear();
                child.Parent = parent;
                child.ParentTree = parent.ParentTree;
                FixClonedChildren(child.Children!, child);
            }
        }
    }

    //Need to finish this implementation
    public static void Refactor(List<ANode> references, string oldname, string newname)
    {
        foreach (ANode reference in references)
        {
            switch (reference.Type)
            {
                case NodeType.Math:
                    break;
            }
        }
    }

    public QTree(string path, bool dependencies)
    {
        _Root = null;
        Fill(path, ref _Root, null, "1");
        _Root!.SortChildren();
        if (dependencies)
        {
            //This needs to be done twice in order to catch all dependents
            SetDependentsByReference(_Root, true);
            SetDependentsByReference(_Root, true);
        }
    }

    public QTree(QTree t)
    {
        BinaryFormatter formater = new();
        MemoryStream serial = t.Serialize();
        _Root = (formater.Deserialize(serial) as QTree)!._Root;
        serial.Close();
        serial.Dispose();
    }

    public QTree()
    {
        _Root = null;
    }

    public MemoryStream Serialize()
    {
        MemoryStream ms = new();
        BinaryFormatter formater = new();
        formater.Serialize(ms, this);
        ms.Seek(0, SeekOrigin.Begin);
        return ms;
    }

    public static QTree Deserialize(byte[] byte_array)
    {
        BinaryFormatter formater = new();
        using MemoryStream memory_stream = new(byte_array);
        QTree tree = (formater.Deserialize(memory_stream) as QTree)!;
        tree!.TotalCounter = 0;
        return tree;
    }

    public string SerializeToString()
    {
        XmlSerializer serializer = new(GetType());

        using StringWriter writer = new();
        serializer.Serialize(writer, this);

        return writer.ToString();
    }
}

public class CircularReferenceException : Exception
{
    public CircularReferenceException() : base("Circular reference") { }
}