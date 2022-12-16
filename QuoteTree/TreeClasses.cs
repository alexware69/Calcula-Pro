using System.Collections.Specialized;
using System.Xml.Serialization;
using Newtonsoft.Json;
using NCalc;
using System.Collections;
using Microsoft.Extensions.Configuration;
using System.Text.RegularExpressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Globalization;
using System.Configuration;

namespace QuoteTree;
[Serializable]
	public enum NodeType { Math, Decision, Text, Conditional, ConditionalRules, Range, SumSet, Reference, Date, DateDiff, Today }

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
		//string TotalStr();
		ANode? FindChildNode(string name);
		string GetPath();
		bool IsComplete();
		void SortChildren();
		ANode Clone();
		string NewId ();
		void Remove();
		void RemoveNodeFromDependencies(ANode node, ANode start);
		void RemoveBranchFromDependencies(ANode node, ANode start);
	}

	[Serializable]
	[XmlInclude(typeof(MathNode))]
	[XmlInclude(typeof(DecisionNode))]
    [XmlInclude(typeof(TextNode))]
	[XmlInclude(typeof(ConditionalNode))]
	[XmlInclude(typeof(ConditionalRulesNode))]
	[XmlInclude(typeof(RangeNode))]
	[XmlInclude(typeof(SumSetNode))]
    [XmlInclude(typeof(ReferenceNode))]
    [XmlInclude(typeof(DateNode))]
    [XmlInclude(typeof(TodayNode))]
    public abstract class ANode : INode
	{

		#region Private fields

		private string _Name = "";
		private string _ID = "";
		private NodeType _Type;
        [JsonProperty]
        private List<ANode>? _Children;
        [XmlIgnore]
        
        private List<string>? _Dependents;
        [XmlIgnore]
        
        private List<string>? _References;
		private decimal _Discount;
        private bool _MaxIsSet;
		private decimal _Max;
        private bool _MinIsSet;
		private decimal _Min;
		private bool _Template;
		private int _Order;
		private bool _Selected;
		private bool _Hidden;
		private bool _ReadOnly;
		private bool _Expanded;
        [JsonProperty]
        private bool _Leaf;
        private int _ExpandedLevels;
		private string _Units = "";
		private bool _Report;
		private bool _ReportValue;
		private bool _Optional;
        private string _DisableCondition = "";
        private string _DisabledMessage = "";
        [XmlIgnore]
        private ANode? _Parent;
		private string _Description = "";
        [XmlIgnore]
        private QTree? _ParentTree;
		private decimal _Amount;
		private bool _CheckBox;
		private string _Url = "";
        private string _Error = "";
        private bool _EditChildren;

		#endregion

		#region Public Properties
		public string Name
		{
			get { return _Name; }
			set { _Name = value; }
		}
		public string Id
		{
			get { return _ID; }
			set { _ID = value; }
		}
		public NodeType Type
		{
			get { return _Type; }
			set { _Type = value; }
		}

        public string TypeStr
        {
            get { return this.Type.ToString(); }
            set {}
        }

        public decimal Subtotal
        {
            get 
            {
                decimal subt;
                try
                {
                    subt = this.Total();
                    return subt;
                }
                catch (Exception)
                {
                    return -1;
                }
            }
            set { }
        }

        public bool Complete
        {
            get
            {
                bool comp;               
                comp = this.IsComplete();
                return comp;
                
            }
            set { }
        }
       [JsonIgnore]
        public List<ANode>? Children
		{
			get { return _Children; }
			set { _Children = value; }
		}

        public List<string>? Dependents
		{
			get { return _Dependents; }
			set { _Dependents = value; }
		}

        public string DependentsStr
        {
            get {
                string dep = "";
                foreach (string n in this.Dependents!) dep = dep + n + ";";
                return dep;
            }
            set { }
        }

        public List<string>? References
		{
			get { return _References; }
			set { _References = value; }
		}

		public virtual decimal Discount
		{
			get { return _Discount; }
			set { _Discount = value; }
		}

        public bool MaxIsSet
		{
			get { return _MaxIsSet; }
			set { _MaxIsSet = value; }
		}
		public virtual decimal Max
		{
			get { return _Max; }
			set { _Max = value; }
		}

        public bool MinIsSet
		{
			get { return _MinIsSet; }
			set { _MinIsSet = value; }
		}

		public virtual decimal Min
		{
			get { return _Min; }
			set { _Min = value; }
		}

		public bool Template
		{
			get { return _Template; }
			set { _Template = value; }
		}

        public bool Leaf
        {
            get { return this.Children == null || this.Children.Count == 0; }
            set { }
        }

        public int Order
		{
			get { return _Order; }
			set { _Order = value; }
		}

		public virtual bool Selected
		{
			get 
            {
                if (this.Optional) return _Selected && !this.Disabled;                  
                else return _Selected;
            }
			set
			{
				_Selected = value;
				if (value == true && this.Parent != null && this.Parent.Type == NodeType.Decision)
				{
					foreach (ANode n in Parent.Children!)
					{
						if (n.Name != this.Name) n.Selected = false;
					}

				}
			}
		}

		public bool Hidden
		{
			get 
            {
                if (this.Template) return true;
                return _Hidden; 
            }
			set { _Hidden = value; }
		}

		public bool ReadOnly
		{
			get { return _ReadOnly; }
			set { _ReadOnly = value; }
		}

		public bool Expanded
		{
			get { return _Expanded; }
			set { _Expanded = value; }
		}

		public int ExpandedLevels
		{
			get { return _ExpandedLevels; }
			set { _ExpandedLevels = value; }
		}

		public virtual string Units
		{
			get { return _Units; }
			set { _Units = value.Trim(); }
		}

		public bool Report
		{
			get { return _Report; }
			set { _Report = value; }
		}

		public bool ReportValue
		{
			get { return _ReportValue; }
			set { _ReportValue = value; }
		}

		public virtual bool Optional
		{
			get { return _Optional; }
			set { _Optional = value; }
		}

        public virtual bool Disabled
        {
            get 
            {
                try
                {
                    if (this.DisableCondition == null || this.DisableCondition.Trim() == "") return false;
                    bool expression_result = false;
                    Expression e = new Expression(this.DisableCondition);
                    e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                    object result = e.Evaluate();
                    expression_result = Convert.ToBoolean(result);
                    return expression_result;
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

        public string DisableCondition
        {
            get { return _DisableCondition; }
            set { _DisableCondition = value; }
        }

        public virtual string DisabledMessage
        {
            get { return _DisabledMessage; }
            set { _DisabledMessage = value; }
        }

		[XmlIgnore]
       
        public ANode? Parent
		{
			get { return _Parent; }
			set { _Parent = value; }
		}

		public string Description
		{
			get { return _Description; }
			set { _Description = value; }
		}
        
        [XmlIgnore]
		public QTree ParentTree
		{
			get { return _ParentTree!; }
			set { _ParentTree = value; }
		}

		public virtual decimal Amount
		{
			get { return _Amount; }
			set { _Amount = value; }
		}

		public bool CheckBox
		{
			get { return _CheckBox; }
			set { _CheckBox = value; }
		}

		public string Url
		{
			get { return _Url; }
			set { _Url = value; }
		}

        public string Error
        {
            get { return _Error; }
            set { _Error = value; }
        }

        public bool EditChildren
        {
            get { return _EditChildren; }
            set { _EditChildren = value; }
        }

        public string TotalStr
        {
            get
            {
                decimal total;
                try
                {
                    total = this.Total();
                    if (IsCurrencySymbol(this.Units)) return total == 0 ? this.Units + 0 : this.Units + total.ToString("#.##");
                    else
                    {
                        string _units = this.Units != "" ? " " + this.Units : "";
                        return total == 0 ? 0 + _units : total.ToString("#.##") + _units;
                    }
                }
                catch (Exception)
                {
                    this.ParentTree.TotalCounter = 0;
                    return "error";
                }
            }
            set {}
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
        public bool ShouldSerializeChildren()
        {
            QTree tree = this.ParentTree;
            int expandedLevels = tree.Root!.ExpandedLevels;
            string id = this.Id;
            int countDots = id.Split(".".ToCharArray()).Length - 1;
            return (countDots < expandedLevels);
                
        }

    private bool IsCurrencySymbol(string s)
        {
            char output;
            if (char.TryParse(s, out output)) 
            {              
                if (CharUnicodeInfo.GetUnicodeCategory(output) == UnicodeCategory.CurrencySymbol) return true;
            }
            return false;
        }

		public string NewId()
		{
			if (this.Children == null || this.Children.Count == 0)
				return this.Id + ".1";
			else 
			{
				int last = 0;
				string[] split;
				foreach (ANode child in this.Children) 
				{
					split = child.Id.Split (".".ToCharArray ());
					if (int.Parse(split [split.Length - 1]) > last)
						last = int.Parse(split [split.Length - 1]);
				}
				return this.Id + "." + (last + 1).ToString();
			}
		}

		public void Remove()
		{
            if (this.Parent != null && (this.Parent.Type == NodeType.Date || this.Parent.Type == NodeType.Today)) return;
            try
            {
                RemoveBranchFromDependencies(this, ParentTree.Root!);
                this.Parent!.Children!.Remove(this);
            }
            catch (Exception) { }
		}

		public void RemoveBranchFromDependencies(ANode node, ANode start)
		{
			RemoveNodeFromDependencies (node, start);
			foreach (ANode child in node.Children!)
				RemoveBranchFromDependencies (child, start);
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

		private bool BranchSelected(ANode node)
		{
			if (!node.Selected) return false;
			if (node.Parent != null)
				return BranchSelected (node.Parent);
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

		public string GetValueFromDirectory(string field, string dir)
		{

			String? line;
			string s = "";
			string[] splitted;

			try
			{
				using (StreamReader sr = new StreamReader(dir + Path.DirectorySeparatorChar + "values.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{

			}

			//if (s == "" || (!s.Contains(field) && !s.Contains(field.ToLower()))) return "";
			//else
			//{
				string the_field = "";
				splitted = s.Split(";".ToCharArray());
				foreach (string part in splitted)
				{
                    string[] splitEqual = part.Split("=".ToCharArray());
                    if (splitEqual[0].Trim().ToLower() == field.ToLower())
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
			//}
		}
		public void SortChildren()
		{
			this._Children!.Sort(new NodeComparer());
			foreach (ANode n in this._Children)
				n.SortChildren();
		}
		public MemoryStream Serialize()
		{
			//FileStream fs = new FileStream("c:\\serialized.dat",FileMode.Create,FileAccess.Write);
			MemoryStream ms = new MemoryStream();
			BinaryFormatter? formater = new BinaryFormatter();
			//formater.Serialize(fs, this);
			formater.Serialize(ms, this);
			ms.Seek(0, SeekOrigin.Begin);
			formater = null;
			return ms;

		}
		public ANode Clone()
		{
			MemoryStream? ms;
			object node;
			BinaryFormatter? formater = new BinaryFormatter();
			ms = this.Serialize();
			node = formater.Deserialize(ms);
			ms.Close();
            ms.Dispose();
			ms = null;
			formater = null;
			System.GC.Collect();
			return (node as ANode)!;
		}
		public string GetPath()
		{
			string s = this.Name;
			ANode n = this;
			while (n != null && n.Parent != null)
			{
				n = n.Parent;
				s = n.Name + "\\" + s;

			}
			return s;

		}
		public ANode? FindChildNode(string name)
		{
			if (this._Children == null) return null;
			foreach (ANode n in this._Children)
				if (n != null && n.Name == name) return n;
			return null;
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

                if (name.EndsWith(".max",StringComparison.OrdinalIgnoreCase)) 
                { 
                    endsWith = ".max"; 
                    //tempName = name.Replace(".max", "");
                    tempName = Regex.Replace(name, @"\.max", "", RegexOptions.IgnoreCase);
                }
                else if (name.EndsWith(".min", StringComparison.OrdinalIgnoreCase)) 
                { 
                    endsWith = ".min"; 
                    //tempName = name.Replace(".min", "");
                    tempName = Regex.Replace(name, @"\.min", "", RegexOptions.IgnoreCase);
                }
                else if (name.EndsWith(".discount", StringComparison.OrdinalIgnoreCase)) 
                { 
                    endsWith = ".discount"; 
                    //tempName = name.Replace(".discount", "");
                    tempName = Regex.Replace(name, @"\.discount", "", RegexOptions.IgnoreCase);
                }
                else if (name.EndsWith(".selected", StringComparison.OrdinalIgnoreCase)) 
                { 
                    endsWith = ".selected"; 
                    //tempName = name.Replace(".selected", "");
                    tempName = Regex.Replace(name, @"\.selected", "", RegexOptions.IgnoreCase);
                }
                else if (name.EndsWith(".disabled", StringComparison.OrdinalIgnoreCase))
                {
                    endsWith = ".disabled";
                    //tempName = name.Replace(".selected", "");
                    tempName = Regex.Replace(name, @"\.disabled", "", RegexOptions.IgnoreCase);
                }
                tempName = tempName.Trim();
                switch (endsWith)
                {
                    case ".selected":
                        if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                            args.Result = this.Selected ? 1 : 0;
                        else
                        if (name.Contains("\\"))
                        {
                            args.Result = this.ParentTree.GetNodeFromPath(tempName)!.Selected ? 1 : 0;
                        }
                        else
                        if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                        {
                            tempName = tempName.Substring(1,tempName.Length - 2);
                            args.Result = this.ParentTree.GetNodeFromId(tempName)!.Selected ? 1 : 0;
                        }                     
                        else
                        {
                            foreach (ANode child in this.Children!)
                            {
                                if (child.Name == name)
                                {
                                    args.Result = child.Selected ? 1 : 0;
                                    break;
                                }
                            }
                        }
                        break;
                    case ".disabled":
                        if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                            args.Result = this.Disabled ? 1 : 0;
                        else 
                        if (name.Contains("\\"))
                            args.Result = this.ParentTree.GetNodeFromPath(tempName)!.Disabled ? 1 : 0;
                        else
                        if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                        {
                            tempName = tempName.Substring(1, tempName.Length - 2);
                            args.Result = this.ParentTree.GetNodeFromId(tempName)!.Disabled ? 1 : 0;
                        }
                        else
                        {
                            foreach (ANode child in this.Children!)
                            {
                                if (child.Name == name)
                                {
                                    args.Result = child.Disabled ? 1 : 0;
                                    break;
                                }
                            }
                        }
                        break;
                    case ".max":
                        if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                            args.Result = Double.Parse(this.Max.ToString());
                        else 
                        if (name.Contains("\\"))
                            args.Result = Double.Parse(this.ParentTree.GetNodeFromPath(tempName)!.Max.ToString());
                        else
                        if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                        {
                            tempName = tempName.Substring(1, tempName.Length - 2);
                            args.Result = Double.Parse(this.ParentTree.GetNodeFromId(tempName)!.Max.ToString());
                        }
                        else
                        {
                            foreach (ANode child in this.Children!)
                            {
                                if (child.Name == name)
                                {
                                    args.Result = Double.Parse(child.Max.ToString());
                                    break;
                                }
                            }
                        }
                        break;
                    case ".min":
                        if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                            args.Result = Double.Parse(this.Min.ToString());
                        else 
                        if (name.Contains("\\"))
                            args.Result = Double.Parse(this.ParentTree.GetNodeFromPath(tempName)!.Min.ToString());
                        else
                        if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                        {
                            tempName = tempName.Substring(1, tempName.Length - 2);
                            args.Result = Double.Parse(this.ParentTree.GetNodeFromId(tempName)!.Min.ToString());
                        }
                        else
                        {
                            foreach (ANode child in this.Children!)
                            {
                                if (child.Name == name)
                                {
                                    args.Result = Double.Parse(child.Min.ToString());
                                    break;
                                }
                            }
                        }
                        break;
                    case ".discount":
                        if (tempName.Equals("this", StringComparison.OrdinalIgnoreCase))
                            args.Result = Double.Parse(this.Discount.ToString());
                        else
                        if (name.Contains("\\"))
                            args.Result = Double.Parse(this.ParentTree.GetNodeFromPath(tempName)!.Discount.ToString());
                        else
                        if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                        {
                            tempName = tempName.Substring(1, tempName.Length - 2);
                            args.Result = Double.Parse(this.ParentTree.GetNodeFromId(tempName)!.Discount.ToString());

                        }
                        else
                        {
                            foreach (ANode child in this.Children!)
                            {
                                if (child.Name == name)
                                {
                                    args.Result = Double.Parse(child.Discount.ToString());
                                    break;
                                }
                            }
                        }
                        break;
                    default:
                        if (name.Contains("\\"))
                            args.Result = Double.Parse(this.ParentTree.GetNodeFromPath(name)!.Total().ToString());
                        else
                        if (tempName.StartsWith("{") && tempName.EndsWith("}"))
                        {
                            tempName = tempName.Substring(1, tempName.Length - 2);
                            args.Result = Double.Parse(this.ParentTree.GetNodeFromId(tempName)!.Total().ToString());

                        }
                        else
                        {
                            foreach (ANode child in this.Children!)
                            {
                                if (child.Name == name)
                                {
                                    args.Result = Double.Parse(child.Total().ToString());
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
			return (a.Order - b.Order);
		}
	}

    [Serializable]
	public class TextNode : ANode
	{
		// *******Fields*****
		string _Text = "";
        bool _Entered;
        //bool _EditChildren;

		// *****Properties*****
		public string Text
		{
			get { return _Text; }
            set 
            { 
                if (!this.ReadOnly) _Text = value; 
                Entered = true;
            }
		}

        public bool Entered
        {
            get { return _Entered; }
            set { _Entered = value; }
        }


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


		public decimal Total1()
		{
			return 0;
		}

		public TextNode()
		{
            this.Name = "";
			this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
			this.Min = this.Max = 0;
            this.Text = "";
			this.Order = 0;
			this.Selected = false;
			this.Children = new List<ANode>();
			this.Dependents = new List<string>();
			this.References = new List<string>();
			this.Type = NodeType.Text;
			this.Parent = null;
			this.Amount = 1;
            this.Entered = false;
			this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
			this.Description = "";
			this.Hidden = false;
			this.ReadOnly = false;
			this.Expanded = false;
			this.ExpandedLevels = 0;
			this.Units = "";
			this.Report = false;
			this.ReportValue = false;
            this.EditChildren = false;
			this.Template = false;
		}

		public TextNode(string path, ANode? parent, QTree parentTree, string id)
			: this()
		{
			if (parent != null) this.Parent = parent;
			this.ParentTree = parentTree;
			this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
			this.Id = id;
			string value = "";

			int intResult;
			decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
			if (value != "") this.Units = value;

			value = this.GetValueFromDirectory("text", path);
			if (value != "") this.Text = value;

            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
			value = this.GetValueFromDirectory("max", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }

			value = this.GetValueFromDirectory("order", path);
			int.TryParse(value, out intResult);
			if (value != "") this.Order = intResult;

			value = this.GetValueFromDirectory("discount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Discount = decimalResult;

			value = this.GetValueFromDirectory("amount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Amount = decimalResult;

			value = this.GetValueFromDirectory("expandedlevels", path);
			int.TryParse(value, out intResult);
			if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

			if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
			else { if (parent == null || (parent != null && parent.Type != NodeType.Decision)) this.Selected = true; }
			if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
			if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
            if (this.GetValueFromDirectory("editchildren", path) == "true") { this.EditChildren = true; }
			if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }
            if (this.CheckBox)
                if (this.GetValueFromDirectory("selected", path) == "true") { this.Selected = true; }


			//Set node url
            this.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + this.Id;

			//Get description from file in folder.
			string s = "";
			string? line = "";
			try
			{
				using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{ }
			this.Description = s;
		}

        public TextNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
            if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id)!;
                if (node.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.Text = values["expression"]!;
            this.EditChildren = values["editChildren"] == "true" ? true : false;
            this.Name = values["name"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
            if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (node == null || (node != null && node.Type != NodeType.Decision)) this.Selected = true; }
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node!;
            this.ParentTree = tree;
            this.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + this.Id;

            //Add new node to children
            if (!this.HasErrors())
            {
                if (node != null)
                {
                    node.Children!.Add(this);
                    //SetDependentsByHierarchy(Root, stack);
                    //SetDependentsByReference(node, false);
                }
                else
                {
                    tree.Root = this;
                }

                if (node != null) node.SortChildren();
            }
        }
	}

	[Serializable]
	public class MathNode : ANode
	{
		// *******Fields*****
		string _Formula = "";
        bool _Entered;
		//bool _EditChildren;

		// *****Properties*****
        //public bool EditChildren
        //{
        //    get { return _EditChildren; }
        //    set { _EditChildren = value; }
        //}
		public string Formula
		{
			get 
            {
                //First check if parent is a Today node
                if (this.Parent != null && this.Parent.Type == NodeType.Today)
                {
                    switch (this.Name)
                    {
                        case "Month":
                            return DateTime.Today.Month.ToString();

                        case "Day":
                            return DateTime.Today.Day.ToString();

                        case "Year":
                            return DateTime.Today.Year.ToString();
                         
                        default:
                            return _Formula;
                    }
                }
                else return _Formula;
            }
			set
            {
                if (!this.ReadOnly)
                {
                    if (this.Parent != null && this.Parent.Type == NodeType.Date)
                    {
                        int outInt;
                        bool valueInt = int.TryParse(value, out outInt);
                        if (valueInt)
                        {
                            _Formula = value;
                            Entered = true;
                        }
                    }
                    else
                    {
                        _Formula = value;
                        Entered = true;
                    }
                }
            }
		}

        public bool Entered
        {
            get { return _Entered; }
            set { _Entered = value; }
        }

		// *****Methods*****
        public override decimal Total()
        {
            //First check if parent is a Today node
            if (this.Parent != null &&this.Parent.Type == NodeType.Today)
            {
                this.ReadOnly = false;
                switch (this.Name)
                {
                    case "Month":
                        this.Formula = DateTime.Today.Month.ToString();
                        break;
                    case "Day":
                        this.Formula = DateTime.Today.Day.ToString();
                        break;
                    case "Year":
                        this.Formula = DateTime.Today.Year.ToString();
                        break;
                    default:
                        break;
                }
                this.ReadOnly = true;
                return decimal.Parse(this.Formula);
            }


            ParentTree.TotalCounter++;
            if (ParentTree.TotalCounter > TotalCounterMax)
            {
                ParentTree.TotalCounter = 0;
                throw new CircularReferenceException();
            }
            if (this.Optional && !this.Selected && !(this.Parent != null && this.Parent.Type == NodeType.Decision))
            {
                ParentTree.TotalCounter--;
                return 0;
            }

            decimal formula_result = 0;
            Expression e = new Expression(this._Formula);
            e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter); 

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
            if (this.MaxIsSet  && formula_result > this.Max) return this.Amount * (Max - Max * this.Discount / 100);
            else
            {
                if (this.MinIsSet && formula_result < this.Min) return this.Amount * (Min - Min * this.Discount / 100);
                else return this.Amount * (formula_result - formula_result * this.Discount / 100);
            }
        }
        public override bool HasErrors()
        {
            try
            {
                Expression e = new Expression(this._Formula);
                e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                bool hasErrors = e.HasErrors();
                this.Error = e.Error;
                return hasErrors;
            }
            catch(Exception) { return false; }
        }

		public override bool IsComplete()
		{
            decimal output;
			if (!BranchSelected() || BranchHidden()) return true;
            //Check for Entered property
            if (Decimal.TryParse(this.Formula, out output) && !ReadOnly)
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
			this.Name = "";
			this.Formula = "";
			this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
			this.Min = this.Max = 0;
			this.Order = 0;
			this.Selected = false;
			this.Children = new List<ANode>();
			this.Dependents = new List<string>();
			this.References = new List<string>();
			this.Type = NodeType.Math;
			this.Parent = null;
			this.Amount = 1;
            this.Entered = false;
			this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
			this.Description = "";
			this.Hidden = false;
			this.ReadOnly = false;
			this.Expanded = false;
			this.ExpandedLevels = 0;
			this.Units = "";
			this.Report = false;
			this.ReportValue = false;
			this.EditChildren = false;
			this.Template = false;
			this.CheckBox = false;
		}

		public MathNode(string path, ANode? parent, QTree parentTree, string id)
			: this()
		{
			if (parent != null) this.Parent = parent;
			this.ParentTree = parentTree;
			this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
			this.Id = id;
			string value = "";
			int intResult;
			decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
			if (value != "") this.Units = value;

			value = this.GetValueFromDirectory("formula", path);
			if (value != "") this.Formula = value; 

            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
            value = this.GetValueFromDirectory("max", path);
		    int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }
			value = this.GetValueFromDirectory("order", path);
			int.TryParse(value, out intResult);
			if (value != "") this.Order = intResult;

			value = this.GetValueFromDirectory("discount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Discount = decimalResult;

			value = this.GetValueFromDirectory("amount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Amount = decimalResult;

			value = this.GetValueFromDirectory("expandedlevels", path);
			int.TryParse(value, out intResult);
			if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

			if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
			else { if (parent == null || (parent != null && parent.Type != NodeType.Decision)) this.Selected = true; }
			if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
			if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
			if (this.GetValueFromDirectory("editchildren", path) == "true") { this.EditChildren = true; }
			if (this.GetValueFromDirectory("template", path) == "true") { this.Template = true; }
			if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }
            if (this.GetValueFromDirectory("readonly", path) == "true") { this.ReadOnly = true; }
            if (this.CheckBox)
                if (this.GetValueFromDirectory("selected", path) == "true") { this.Selected = true; }


			//To set the url for the node
			decimal flag;
			if (decimal.TryParse(this.Formula, out flag) || this.EditChildren)
			{
				this.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + this.Id;
			}
			else
			{
                this.Url = "TreeView" + "/Description" + "?id=" + this.Id;
			}

			//Set attributes from folder
			//try
			//{
			//    DirectoryInfo dirInfo = new DirectoryInfo(path);
			//    if ((FileAttributes.Hidden & dirInfo.Attributes) == FileAttributes.Hidden || this.template) { this.hidden = true; }
			//    else this.hidden = false;
			//}
			//catch (Exception) { }

			//Get description from file in folder.
			string s = "";
			string? line = "";
			try
			{
				using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{ }
			this.Description = s;

		}

        public MathNode(NameValueCollection values,QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
            if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.Formula = values["expression"]!;
            this.EditChildren = values["editChildren"] == "true" ? true : false;
            this.Name = values["name"]!;
            this.Units = values["units"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
            if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (node == null || (node != null && node.Type != NodeType.Decision)) this.Selected = true; }
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            //To set the url for the node
            decimal flag;
            if (decimal.TryParse(this.Formula, out flag) || this.EditChildren)
            {
                this.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + this.Id;
            }
            else
            {
                this.Url = "TreeView" + "/Description" + "?id=" + this.Id;
            }

            //Add new node to children
            if (!this.HasErrors())
            {
                if (node != null)
                {
                    node.Children!.Add(this);
                    //SetDependentsByHierarchy(this.Root, stack);
                    //SetDependentsByReference(node, false);
                }
                else
                {
                    this.ParentTree.Root = this;
                }

                if (node != null) node.SortChildren();
            }
        }
	}

    [Serializable]
    public class DateNode : ANode
    {
        // *******Fields*****
        string _Formula = "";
        //bool _Entered;
        //bool _EditChildren;

        // *****Properties*****
        //public bool EditChildren
        //{
        //    get { return _EditChildren; }
        //    set { _EditChildren = value; }
        //}
       
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
            this.Name = "";
            this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
            this.Min = this.Max = 0;
            this.Order = 0;
            this.Selected = true;
            this.Children = new List<ANode>();
            this.Dependents = new List<string>();
            this.References = new List<string>();
            this.Type = NodeType.Date;
            this.Parent = null;
            this.Amount = 1;
            this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
            this.Description = "";
            this.Hidden = false;
            this.ReadOnly = false;
            this.Expanded = false;
            this.ExpandedLevels = 0;
            this.Units = "";
            this.Report = false;
            this.ReportValue = false;
            this.EditChildren = true;
            this.Template = false;
            this.CheckBox = false;
        }

        public DateNode(string path, ANode? parent, QTree parentTree, string id)
            : this()
        {
            if (parent != null) this.Parent = parent;
            this.ParentTree = parentTree;
            this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
            this.Id = id;
            string value = "";
            int intResult;
            decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
            if (value != "") this.Units = value;

            value = this.GetValueFromDirectory("formula", path);
            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
            value = this.GetValueFromDirectory("max", path);
		    int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }

            value = this.GetValueFromDirectory("order", path);
            int.TryParse(value, out intResult);
            if (value != "") this.Order = intResult;

            value = this.GetValueFromDirectory("discount", path);
            decimal.TryParse(value, out decimalResult);
            if (value != "") this.Discount = decimalResult;

            value = this.GetValueFromDirectory("amount", path);
            decimal.TryParse(value, out decimalResult);
            if (value != "") this.Amount = decimalResult;

            value = this.GetValueFromDirectory("expandedlevels", path);
            int.TryParse(value, out intResult);
            if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

            if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (parent == null || (parent != null && parent.Type != NodeType.Decision)) this.Selected = true; }
            if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
            if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
            this.EditChildren = true;
            if (this.GetValueFromDirectory("template", path) == "true") { this.Template = true; }
            if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }


            //To set the url for the node
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;


            //Set attributes from folder
            //try
            //{
            //    DirectoryInfo dirInfo = new DirectoryInfo(path);
            //    if ((FileAttributes.Hidden & dirInfo.Attributes) == FileAttributes.Hidden || this.template) { this.hidden = true; }
            //    else this.hidden = false;
            //}
            //catch (Exception) { }

            //Get description from file in folder.
            string s = "";
            string? line = "";
            try
            {
                using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
                {

                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        s += line;
                    }
                }
            }
            catch (Exception)
            { }
            this.Description = s;

        }

        public DateNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
            if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.EditChildren = true;
            this.Name = values["name"]!;
            this.Units = values["units"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
            if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            this.Selected = true;
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            this.EditChildren = true;
            //To set the url for the node
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

            //Add new node to children
            if (!this.HasErrors())
            {
                if (node != null)
                {
                    node.Children!.Add(this);
                    //SetDependentsByHierarchy(this.Root, stack);
                    //SetDependentsByReference(node, false);
                }
                else
                {
                    this.ParentTree.Root = this;
                }

                if (node != null) node.SortChildren();
            }
            //Add the date child nodes to this node
            ANode month = new MathNode();
            month.Id = this.NewId();
            month.Name = "Month";
            month.Parent = this;
            month.ParentTree = tree;
            month.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + month.Id;
            (month as MathNode)!.Formula = "1";
            month.Min = 1;
            month.Max = 12;
            month.Dependents!.Add(this.Id);
            month.Order = 0;
            month.Selected = true;
            this.Children!.Add(month);

            ANode day = new MathNode();
            day.Id = this.NewId();
            day.Name = "Day";
            day.Parent = this;
            day.ParentTree = tree;
            day.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + day.Id;
            (day as MathNode)!.Formula = "1";
            day.Min = 1;
            day.Max = 31;
            day.Dependents!.Add(this.Id);
            day.Order = 1;
            day.Selected = true;
            this.Children.Add(day);

            ANode year = new MathNode();
            year.Id = this.NewId();
            year.Name = "Year";
            year.Parent = this;
            year.ParentTree = tree;
            year.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + year.Id;
            (year as MathNode)!.Formula = "2000";
            year.Min = 2000;
            year.Max = 2100;
            year.Dependents!.Add(this.Id);
            year.Order = 2;
            year.Selected = true;
            this.Children.Add(year);
        }
    }

    [Serializable]
    public class TodayNode : ANode
    {
        // *******Fields*****
        string _Formula = "";
        //bool _Entered;
        //bool _EditChildren;

        // *****Properties*****
        //public bool EditChildren
        //{
        //    get { return _EditChildren; }
        //    set { _EditChildren = value; }
        //}

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
            this.Name = "";
            this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
            this.Min = this.Max = 0;
            this.Order = 0;
            this.Selected = false;
            this.Children = new List<ANode>();
            this.Dependents = new List<string>();
            this.References = new List<string>();
            this.Type = NodeType.Today;
            this.Parent = null;
            this.Amount = 1;
            this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
            this.Description = "";
            this.Hidden = false;
            this.ReadOnly = false;
            this.Expanded = false;
            this.ExpandedLevels = 0;
            this.Units = "";
            this.Report = false;
            this.ReportValue = false;
            this.EditChildren = false;
            this.Template = false;
            this.CheckBox = false;
        }

        public TodayNode(string path, ANode? parent, QTree parentTree, string id)
            : this()
        {
            if (parent != null) this.Parent = parent;
            this.ParentTree = parentTree;
            this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
            this.Id = id;
            string value = "";
            int intResult;
            decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
            if (value != "") this.Units = value;

            value = this.GetValueFromDirectory("formula", path);
            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
            value = this.GetValueFromDirectory("max", path);
		    int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }
            value = this.GetValueFromDirectory("order", path);
            int.TryParse(value, out intResult);
            if (value != "") this.Order = intResult;

            value = this.GetValueFromDirectory("discount", path);
            decimal.TryParse(value, out decimalResult);
            if (value != "") this.Discount = decimalResult;

            value = this.GetValueFromDirectory("amount", path);
            decimal.TryParse(value, out decimalResult);
            if (value != "") this.Amount = decimalResult;

            value = this.GetValueFromDirectory("expandedlevels", path);
            int.TryParse(value, out intResult);
            if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

            if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (parent == null || (parent != null && parent.Type != NodeType.Decision)) this.Selected = true; }
            if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
            if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
            this.EditChildren = true;
            if (this.GetValueFromDirectory("template", path) == "true") { this.Template = true; }
            if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }


            //To set the url for the node
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;


            //Set attributes from folder
            //try
            //{
            //    DirectoryInfo dirInfo = new DirectoryInfo(path);
            //    if ((FileAttributes.Hidden & dirInfo.Attributes) == FileAttributes.Hidden || this.template) { this.hidden = true; }
            //    else this.hidden = false;
            //}
            //catch (Exception) { }

            //Get description from file in folder.
            string s = "";
            string? line = "";
            try
            {
                using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
                {

                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        s += line;
                    }
                }
            }
            catch (Exception)
            { }
            this.Description = s;

        }

        public TodayNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
            if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.EditChildren = true;
            this.Name = values["name"]!;
            this.Units = values["units"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
            if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (node == null || (node != null && node.Type != NodeType.Decision)) this.Selected = true; }
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            this.EditChildren = false;
            //To set the url for the node
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

            //Add new node to children
            if (!this.HasErrors())
            {
                if (node != null)
                {
                    node.Children!.Add(this);
                    //SetDependentsByHierarchy(this.Root, stack);
                    //SetDependentsByReference(node, false);
                }
                else
                {
                    this.ParentTree.Root = this;
                }

                if (node != null) node.SortChildren();
            }
            //Add the date child nodes to this node
            ANode month = new MathNode();
            month.Id = this.NewId();
            month.Name = "Month";
            month.Parent = this;
            month.ParentTree = tree;
            month.Url = "TreeView" + "/Description" + "?id=" + month.Id;
            (month as MathNode)!.Formula = DateTime.Today.Month.ToString();
            month.Min = 1;
            month.Max = 12;
            month.Dependents!.Add(this.Id);
            month.ReadOnly = true;
            month.Order = 0;
            this.Children!.Add(month);

            ANode day = new MathNode();
            day.Id = this.NewId();
            day.Name = "Day";
            day.Parent = this;
            day.ParentTree = tree;
            day.Url = "TreeView" + "/Description" + "?id=" + day.Id;
            (day as MathNode)!.Formula = DateTime.Today.Day.ToString();
            day.Min = 1;
            day.Max = 31;
            day.Dependents!.Add(this.Id);
            day.ReadOnly = true;
            day.Order = 1;
            this.Children.Add(day);

            ANode year = new MathNode();
            year.Id = this.NewId();
            year.Name = "Year";
            year.Parent = this;
            year.ParentTree = tree;
            year.Url = "TreeView" + "/Description" + "?id=" + year.Id;
            (year as MathNode)!.Formula = DateTime.Today.Year.ToString();
            year.MaxIsSet = year.MinIsSet = false;
            year.Min = 0;
            year.Max = 0;
            year.Dependents!.Add(this.Id);
            year.ReadOnly = true;
            year.Order = 2;
            this.Children.Add(year);
        }
    }

    [Serializable]
	public class ConditionalNode : ANode
	{
		// *******Fields*****
		string _Formula = "";
		string _ThenFormula = "";
		string _ElseFormula = "";
		string _IfCondition = "";
        //bool _EditChildren;

		#region Properties
		public string Formula
		{
			get { return _Formula; }
            set { if (!this.ReadOnly) _Formula = value; }
		}
		public string ThenFormula
		{
			get { return _ThenFormula; }
			set { _ThenFormula = value; }
		}
		public string ElseFormula
		{
			get { return _ElseFormula; }
			set { _ElseFormula = value; }
		}
		public string IfCondition
		{
			get { return _IfCondition; }
			set { _IfCondition = value; }
		}
        //public bool EditChildren
        //{
        //    get { return _EditChildren; }
        //    set { _EditChildren = value; }
        //}
		#endregion
       
		// *****Methods*****

        //public void parseFormula()
        //{
        //    string temp_formula = this.Formula;
        //    temp_formula = temp_formula.Trim();
        //    //Remove if
        //    temp_formula = temp_formula.Remove(0, 2);
        //    temp_formula = temp_formula.Trim();
        //    //Remove first and last parenthesis
        //    temp_formula = temp_formula.Remove(0, 1);
        //    temp_formula = temp_formula.Remove(temp_formula.Length - 1, 1);
        //    temp_formula = temp_formula.Trim();
        //    // Split if, then, else
        //    string[] formula_parts = temp_formula.Split(",".ToCharArray());
        //    this.IfCondition = formula_parts[0].Trim();
        //    this.ThenFormula = formula_parts[1].Trim();
        //    this.ElseFormula = formula_parts[2].Trim();
        //}

        public override decimal Total() 
        {
            ParentTree.TotalCounter++;
            if (ParentTree.TotalCounter > TotalCounterMax)
            {
                ParentTree.TotalCounter = 0;
                throw new CircularReferenceException();
            }
            decimal formula_result = 0;
            Expression e = new Expression(this._Formula);
            e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);

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
                if (this.MaxIsSet && formula_result > this.Max) return this.Amount * (Max - Max * this.Discount / 100);
                else
                {
                    if (this.MinIsSet && formula_result < this.Min) return this.Amount * (Min - Min * this.Discount / 100);
                    else return this.Amount * (formula_result - formula_result * this.Discount / 100);
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
                Expression e = new Expression(this._Formula);
                e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                bool hasErrors = e.HasErrors();
                this.Error = e.Error;
                return hasErrors;
            }
            catch(Exception) { return false; }
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
            this.Name = "";
			this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
			this.Min = this.Max = 0;
			this.Order = 0;
			this.Selected = false;
			this.Children = new List<ANode>();
			this.Dependents = new List<string>();
			this.References = new List<string>();
			this.Type = NodeType.Conditional;
			this.Parent = null;
			this.Amount = 1;
			this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
			this.Description = "";
			this.Hidden = false;
			this.ReadOnly = false;
			this.Expanded = false;
			this.ExpandedLevels = 0;
			this.Units = "";
			this.Report = false;
			this.ReportValue = false;
            this.EditChildren = false;
			this.Template = false;
			this.CheckBox = false;
		}

		public ConditionalNode(string path, ANode? parent, QTree parentTree, string id)
			: this()
		{
			if (parent != null) this.Parent = parent;
			this.ParentTree = parentTree;
			this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
			this.Id = id;
			string value = "";

			int intResult;
			decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
			if (value != "") this.Units = value;

			value = this.GetValueFromDirectory("formula", path);
			if (value != "") this.Formula = value;

            //parse the formula
            //parseFormula();
            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
			value = this.GetValueFromDirectory("max", path);
		    int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }

			value = this.GetValueFromDirectory("order", path);
			int.TryParse(value, out intResult);
			if (value != "") this.Order = intResult;

			value = this.GetValueFromDirectory("discount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Discount = decimalResult;

			value = this.GetValueFromDirectory("amount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Amount = decimalResult;

			value = this.GetValueFromDirectory("expandedlevels", path);
			int.TryParse(value, out intResult);
			if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

			if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
			else { if (parent == null || (parent != null && parent.Type != NodeType.Decision)) this.Selected = true; }
			if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
			if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
			if (this.GetValueFromDirectory("editchildren", path) == "true") { this.EditChildren = true; }
			if (this.GetValueFromDirectory("template", path) == "true") { this.Template = true; }
			if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }
            if (this.CheckBox)
                if (this.GetValueFromDirectory("selected", path) == "true") { this.Selected = true; }
			//To set the url for the node
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

			//Get description from file in folder.
			string s = "";
			string? line = "";
			try
			{
				using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{ }
			this.Description = s;
		}

        public ConditionalNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
           if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.Formula = values["expression"]!;
            //this.parseFormula();
            this.EditChildren = values["editChildren"] == "true" ? true : false;
            this.Name = values["name"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
           if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (node == null || (node != null && node.Type != NodeType.Decision)) this.Selected = true; }
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

            //Add new node to children
            if (!this.HasErrors())
            {
                if (node != null)
                {
                    node.Children!.Add(this);
                    //SetDependentsByHierarchy(Root, stack);
                    //SetDependentsByReference(node, false);
                }
                else
                {
                    tree.Root = this;
                }

                if (node != null) node.SortChildren();
            }
        }
	}

	[Serializable]
	public class ConditionalRulesNode : ANode
	{
		// *******Fields*****
		string _Expression = "";
        //bool _EditChildren;
		SerializableDictionary<string, string> _Rules = new SerializableDictionary<string, string>();

		#region Properties
		public string Expression
		{
			get { return _Expression; }
            set { if (!this.ReadOnly) _Expression = value; }
		}
        //public bool EditChildren
        //{
        //    get { return _EditChildren; }
        //    set { _EditChildren = value; }
        //}
		public SerializableDictionary<string, string> Rules
		{
			get { return _Rules; }
			set { _Rules = value; }
		}

		#endregion

		// *****Methods*****

		public void parseExpression()
		{
            this.Rules.Clear();
			char[] chars = {'[',']'};
			string[] splitted = this.Expression.Split(chars);
			foreach (string s in splitted)
			{
				string condition;
				string result;
				if (s.Contains("?"))
				{
					condition = s.Split('?')[0].Trim();
					result = s.Split('?')[1].Trim();
					this.Rules.Add(condition, result);
				}
			}
		}

        public override decimal Total() 
        {
            ParentTree.TotalCounter++;
            if (ParentTree.TotalCounter > TotalCounterMax)
            {
                ParentTree.TotalCounter = 0;
                throw new CircularReferenceException();
            }

            foreach (KeyValuePair<string, string> thisRule in this.Rules)
            {
                decimal formula_result = 0;
                Expression e = new Expression(thisRule.Key);
                e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);

                try
                {
                    object result = e.Evaluate();
                    if (Convert.ToBoolean(result)) 
                    {
                        Expression e1 = new Expression(thisRule.Value);
                        e1.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);

                        object result1 = e1.Evaluate();
                        formula_result = decimal.Parse(result1.ToString()!);
                        ParentTree.TotalCounter--;
                        if (this.MaxIsSet && formula_result > this.Max) return this.Amount * (Max - Max * this.Discount / 100);
                        else
                        {
                            if (this.MinIsSet && formula_result < this.Min) return this.Amount * (Min - Min * this.Discount / 100);
                            else return this.Amount * (formula_result - formula_result * this.Discount / 100);
                        }
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
            ParentTree.TotalCounter--;
            return 0;
        }

        public override bool HasErrors()
        {
            try
            {
                bool hasErrors = false;
                foreach (KeyValuePair<string, string> thisRule in this.Rules)
                {
                    Expression e = new Expression(thisRule.Key);
                    e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                    hasErrors = e.HasErrors();
                    this.Error = e.Error;
                    if (hasErrors) break;

                    Expression e1 = new Expression(thisRule.Value);
                    e1.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                    hasErrors = e.HasErrors();
                    this.Error = e.Error;
                    if (hasErrors) break;
                }
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

		public ConditionalRulesNode()
		{
            this.Name = "";
			this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
			this.Min = this.Max = 0;
			this.Order = 0;
			this.Selected = false;
			this.Children = new List<ANode>();
			this.Dependents = new List<string>();
			this.References = new List<string>();
			this.Type = NodeType.ConditionalRules;
			this.Parent = null;
			this.Amount = 1;
			this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
			this.Description = "";
			this.Hidden = false;
			this.ReadOnly = false;
			this.Expanded = false;
			this.ExpandedLevels = 0;
			this.Units = "";
			this.Report = false;
			this.ReportValue = false;
            this.EditChildren = false;
			this.Template = false;
			this.CheckBox = false;
		}

		public ConditionalRulesNode(string path, ANode? parent, QTree parentTree, string id)
			: this()
		{
			if (parent != null) this.Parent = parent;
			this.ParentTree = parentTree;
			this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
			this.Id = id;
			string value = "";

			int intResult;
			decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
			if (value != "") this.Units = value;

			value = this.GetValueFromDirectory("expression", path);
			if (value != "") { this.Expression = value; this.parseExpression(); }

            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
			value = this.GetValueFromDirectory("max", path);
		    int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }

			value = this.GetValueFromDirectory("order", path);
			int.TryParse(value, out intResult);
			if (value != "") this.Order = intResult;

			value = this.GetValueFromDirectory("discount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Discount = decimalResult;

			value = this.GetValueFromDirectory("amount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Amount = decimalResult;

			value = this.GetValueFromDirectory("expandedlevels", path);
			int.TryParse(value, out intResult);
			if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

			if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
			else { if (parent == null || (parent != null && parent.Type != NodeType.Decision)) this.Selected = true; }
			if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
			if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
			if (this.GetValueFromDirectory("editchildren", path) == "true") { this.EditChildren = true; }
			if (this.GetValueFromDirectory("template", path) == "true") { this.Template = true; }
			if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }

			//To set the url for the node
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

			//Get description from file in folder.
			string s = "";
			string? line = "";
			try
			{
				using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{ }
			this.Description = s;
		}

        public ConditionalRulesNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
            if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.Expression = values["expression"]!;
            this.parseExpression();
            this.EditChildren = values["editChildren"] == "true" ? true : false;
            this.Name = values["name"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
            if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (node == null || (node != null && node.Type != NodeType.Decision)) this.Selected = true; }
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

            //Add new node to children
            if (!this.HasErrors())
            {
                if (node != null)
                {
                    node.Children!.Add(this);
                    //SetDependentsByHierarchy(Root, stack);
                    //SetDependentsByReference(node, false);
                }
                else
                {
                    tree.Root = this;
                }

                if (node != null) node.SortChildren();
            }
        }
	}

	[Serializable]
	public class DecisionNode : ANode
	{
		// *******Fields*****
		bool _IsUserDecision;

		// *****Properties*****
		public bool IsUserDecision
		{
			get { return _IsUserDecision; }
			set { _IsUserDecision = value; }
		}

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

            if (this.Optional && !this.Selected && !(this.Parent != null && this.Parent.Type == NodeType.Decision))
            {
                ParentTree.TotalCounter--;
                return 0;
            }

			//Get selected child total
			decimal selected_child_result = 0;

			foreach (ANode n in this.Children!)
			{
				if (n.Selected) { selected_child_result = n.Total(); break; }
			}

            ParentTree.TotalCounter--;
			if (this.MaxIsSet && selected_child_result > this.Max) return this.Amount * (Max - Max * this.Discount / 100);
			else
			{
				if (this.MinIsSet && selected_child_result < this.Min) return this.Amount * (Min - Min * this.Discount / 100);
				else return this.Amount * (selected_child_result - selected_child_result * this.Discount / 100);
			}
		}

		public DecisionNode()
		{
            this.Name = "";
			this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
			this.Min = this.Max = 0;
			this.Order = 0;
			this.Selected = false;
			this.Children = new List<ANode>();
			this.Dependents = new List<string>();
			this.References = new List<string>();
			this.Type = NodeType.Decision;
			this.Parent = null;
			this.Amount = 1;
			this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
			this.Description = "";
			this.Hidden = false;
			this.ReadOnly = false;
			this.Expanded = false;
			this.ExpandedLevels = 0;
			this.Units = "";
			this.Report = false;
			this.ReportValue = false;
			this.Template = false;

		}

		public DecisionNode(string path, ANode? parent, QTree parentTree, string id)
			: this()
		{
			string value = "";
			this.ParentTree = parentTree;
			if (parent != null) this.Parent = parent;

			int intResult;
			decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
			if (value != "") this.Units = value;

            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
			value = this.GetValueFromDirectory("max", path);
		    int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }

			value = this.GetValueFromDirectory("order", path);
			int.TryParse(value, out intResult);
			if (value != "") this.Order = intResult;

			value = this.GetValueFromDirectory("discount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Discount = decimalResult;

			this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
			this.Id = id;

			value = this.GetValueFromDirectory("amount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Amount = decimalResult;

			value = this.GetValueFromDirectory("expandedlevels", path);
			int.TryParse(value, out intResult);
			if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

			if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
			else { if (parent != null && parent.Type != NodeType.Decision) this.Selected = true; }
			if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
			if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
			if (parent == null) this.Selected = true;
			if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }
            if (this.CheckBox)
                if (this.GetValueFromDirectory("selected", path) == "true") { this.Selected = true; }


			//Set the node url
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

			//Get description from file in folder.
			string s = "";
			string? line = "";
			try
			{
				using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{ }
			this.Description = s;
		}

        public DecisionNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
            if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.Name = values["name"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
            if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (node == null || (node != null && node.Type != NodeType.Decision)) this.Selected = true; }
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

            //Add new node to children
            if (node != null)
            {
                node.Children!.Add(this);
                //SetDependentsByHierarchy(Root, stack);
                //SetDependentsByReference(node, false);
            }
            else
            {
                tree.Root = this;
            }

            if (node != null) node.SortChildren();
        }
	}

	[Serializable]
	public class RangeNode : ANode
	{
		// *******Fields*****
		string _Range = "";
        //bool _EditChildren;

		// *****Properties*****
		public string Range
		{
			get { return _Range; }
            set { if (!this.ReadOnly) _Range = value; }
		}
        //public bool EditChildren
        //{
        //    get { return _EditChildren; }
        //    set { _EditChildren = value; }
        //}

		// *****Methods*****
        public override bool HasErrors()
        {
            try
            {
                bool hasErrors = false;

                string[] temp_formula_ranges;
                //Check to see if formula contain range expressions
                if (this._Range.Contains("|"))
                    temp_formula_ranges = this._Range.Split("|".ToCharArray());
                else
                {
                    Error = "Range nodes expressions should contain sections separated by the character '|'.";
                    return true;
                }

                for (int i = 0; i < temp_formula_ranges.Length; i++)
                {
                    if (i == 0)
                    {
                        Expression e = new Expression(temp_formula_ranges[i]);
                        e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                        hasErrors = e.HasErrors();
                        this.Error = e.Error;
                        if (hasErrors) return true;
                    }
                    else
                    {
                        string[] values = temp_formula_ranges[i].Split(":".ToCharArray());
                        if (values.Length == 0)
                        {
                            Error = "Range nodes sections should contain variables, values or math expressions separated by the character ':'.";
                            return true;
                        }
                        else
                            foreach (string value in values)
                            {
                                Expression e = new Expression(value);
                                e.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                                hasErrors = e.HasErrors();
                                this.Error = e.Error;
                                if (hasErrors) return true;
                            }
                    }
                }
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

        public override decimal Total()
        {
            ParentTree.TotalCounter++;
            if (ParentTree.TotalCounter > TotalCounterMax)
            {
                ParentTree.TotalCounter = 0;
                throw new CircularReferenceException();
            }

            if (this.Optional && !this.Selected && !(this.Parent != null && this.Parent.Type == NodeType.Decision))
            {
                ParentTree.TotalCounter--;
                return 0;
            }
            //EB.Math.Function function = new EB.Math.Function();
            string[] temp_formula_ranges;

            //Check to see if formula contain range expressions
            if (this._Range.Contains("|"))
                temp_formula_ranges = this._Range.Split("|".ToCharArray());
            else
            {
                ParentTree.TotalCounter--;
                return 0;
            }


            for (int i = 0; i < temp_formula_ranges.Length; i++)
            {
                if (i > 0)
                {
                    string[] values = temp_formula_ranges[i].Split(":".ToCharArray());
                    decimal floor, ceiling, value;

                    Expression eFloor = new Expression(values[0]);
                    eFloor.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                    Expression eCeiling = new Expression(values[1]);
                    eCeiling.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                    Expression eValue = new Expression(temp_formula_ranges[0]);
                    eValue.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                    try
                    {
                        object result = eFloor.Evaluate();
                        floor = decimal.Parse(result.ToString()!);
                        result = eCeiling.Evaluate();
                        ceiling = decimal.Parse(result.ToString()!);
                        result = eValue.Evaluate();
                        value = decimal.Parse(result.ToString()!);
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

                    switch (values.Length)
                    {
                        case 3:
                            if (floor <= value && value <= ceiling)
                            {
                                decimal output;
                                Expression eOutput = new Expression(values[2]);
                                eOutput.EvaluateParameter += new EvaluateParameterHandler(this.EvaluateParameter);
                                try 
                                {
                                    object result = eOutput.Evaluate();
                                    output = decimal.Parse(result.ToString()!);
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
                                if (this.MaxIsSet && output > this.Max) return this.Amount * (Max - Max * this.Discount / 100);
                                else
                                {
                                    if (this.MinIsSet && output < this.Min) return this.Amount * (Min - Min * this.Discount / 100);
                                    else return this.Amount * (output - output * this.Discount / 100);
                                }
                            }
                            break;
                        case 2:
                            if (floor <= value)
                            {
                                decimal output = ceiling;
                                ParentTree.TotalCounter--;
                                if (this.MaxIsSet && output > this.Max) return this.Amount * (Max - Max * this.Discount / 100);
                                else
                                {
                                    if (this.MinIsSet && output < this.Min) return this.Amount * (Min - Min * this.Discount / 100);
                                    else return this.Amount * (output - output * this.Discount / 100);
                                }
                            }
                            break;
                        default: break;
                    }
                }
            }
            ParentTree.TotalCounter--;
            return 0;
        }

		public RangeNode()
		{
            this.Name = "";
			this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
			this.Min = this.Max = 0;
			this.Order = 0;
			this.Selected = false;
			this.Children = new List<ANode>();
			this.Dependents = new List<string>();
			this.References = new List<string>();
			this.Type = NodeType.Range;
			this.Parent = null;
			this.Amount = 1;
			this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
			this.Description = "";
			this.Hidden = false;
			this.ReadOnly = false;
			this.Expanded = false;
			this.ExpandedLevels = 0;
			this.Units = "";
			this.Report = false;
			this.ReportValue = false;
            this.EditChildren = false;
			this.Template = false;
		}

		public RangeNode(string path, ANode? parent, QTree parentTree, string id)
			: this()
		{
			if (parent != null) this.Parent = parent;
			this.ParentTree = parentTree;
			this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
			this.Id = id;
			string value = "";

			int intResult;
			decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
			if (value != "") this.Units = value;

			value = this.GetValueFromDirectory("range", path);
			if (value != "") this.Range = value;

            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
			value = this.GetValueFromDirectory("max", path);
		    int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }

			value = this.GetValueFromDirectory("order", path);
			int.TryParse(value, out intResult);
			if (value != "") this.Order = intResult;

			value = this.GetValueFromDirectory("discount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Discount = decimalResult;

			value = this.GetValueFromDirectory("amount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Amount = decimalResult;

			value = this.GetValueFromDirectory("expandedlevels", path);
			int.TryParse(value, out intResult);
			if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

			if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
			else { if (parent == null || (parent != null && parent.Type != NodeType.Decision)) this.Selected = true; }
			if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
			if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
            if (this.GetValueFromDirectory("editchildren", path) == "true") { this.EditChildren = true; }
			if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }


			//Set node url
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

			//Get description from file in folder.
			string s = "";
			string? line = "";
			try
			{
				using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{ }
			this.Description = s;
		}

        public RangeNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
            if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.Range = values["expression"]!;
            this.EditChildren = values["editChildren"] == "true" ? true : false;
            this.Name = values["name"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
            if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (node == null || (node != null && node.Type != NodeType.Decision)) this.Selected = true; }
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

            //Add new node to children
            if (!this.HasErrors())
            {
                if (node != null)
                {
                    node.Children!.Add(this);
                    //SetDependentsByHierarchy(Root, stack);
                    //SetDependentsByReference(node, false);
                }
                else
                {
                    tree.Root = this;
                }

                if (node != null) node.SortChildren();
            }
        }
	}

	[Serializable]
	public class SumSetNode : ANode
	{
		// *******Fields*****
		//bool _EditChildren;


		// *****Properties*****
        //public bool EditChildren
        //{
        //    get { return _EditChildren; }
        //    set { _EditChildren = value; }
        //}


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
			foreach (ANode n in this.Children!)
			{
				if (!n.Template) sum += n.Total();

			}
            ParentTree.TotalCounter--;
            if (this.MaxIsSet && sum > this.Max) return this.Amount * (Max - Max * this.Discount / 100);
            else
            {
                if (this.MinIsSet && sum < this.Min) return this.Amount * (Min - Min * this.Discount / 100);
                else return this.Amount * (sum - sum * this.Discount / 100);
            }
		}

		public SumSetNode()
		{
            this.Name = "";
			this.Discount = 0;
            this.MaxIsSet = this.MinIsSet = false;
			this.Min = this.Max = 0;
			this.Order = 0;
			this.Selected = false;
			this.Children = new List<ANode>();
			this.Dependents = new List<string>();
			this.References = new List<string>();
			this.Type = NodeType.SumSet;
			this.Parent = null;
			this.Amount = 1;
			this.Optional = false;
            this.DisableCondition = "0";
            this.DisabledMessage = "";
			this.Description = "";
			this.Hidden = false;
			this.ReadOnly = false;
			this.Expanded = false;
			this.ExpandedLevels = 0;
			this.Units = "";
			this.Report = false;
			this.ReportValue = false;
			this.EditChildren = false;
			this.Template = false;
		}
        
		public SumSetNode(string path, ANode? parent, QTree parentTree, string id)
			: this()
		{
			if (parent != null) this.Parent = parent;
			this.ParentTree = parentTree;
			this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
			this.Id = id;
			string value = "";

			int intResult;
			decimal decimalResult;

            value = this.GetValueFromDirectory("id", path);
            if (value != "") this.Id = value;

            value = this.GetValueFromDirectory("units", path);
			if (value != "") this.Units = value;

            if (this.GetValueFromDirectory("maxisset", path) == "true") { this.MaxIsSet = true; }
            if (this.GetValueFromDirectory("minisset", path) == "true") { this.MinIsSet = true; }
			value = this.GetValueFromDirectory("max", path);
		    int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Max = intResult;
                //this.MaxIsSet = true;
            }

			value = this.GetValueFromDirectory("min", path);
			int.TryParse(value, out intResult);
			if (value != "") 
            {
                this.Min = intResult;
                //this.MinIsSet = true;
            }

			value = this.GetValueFromDirectory("order", path);
			int.TryParse(value, out intResult);
			if (value != "") this.Order = intResult;

			value = this.GetValueFromDirectory("discount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Discount = decimalResult;

			value = this.GetValueFromDirectory("amount", path);
			decimal.TryParse(value, out decimalResult);
			if (value != "") this.Amount = decimalResult;

			value = this.GetValueFromDirectory("expandedlevels", path);
			int.TryParse(value, out intResult);
			if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("disablecondition", path);
            if (value != "") this.DisableCondition = value;

            value = this.GetValueFromDirectory("disabledmessage", path);
            if (value != "") this.DisabledMessage = value;

			if (this.GetValueFromDirectory("optional", path) == "true" || (parent != null && parent.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
			else { if (parent == null || (parent != null && parent.Type != NodeType.Decision)) this.Selected = true; }
			if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
			if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
			if (this.GetValueFromDirectory("editchildren", path) == "true") { this.EditChildren = true; }
			if (this.GetValueFromDirectory("template", path) == "true") { this.Template = true; }
			if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }
            if (this.CheckBox)
                if (this.GetValueFromDirectory("selected", path) == "true") { this.Selected = true; }


			//To set the node url
            this.Url = "TreeView" + "/AppendNodes" + "?id=" + this.Id;


			//Get description from file in folder.
			string s = "";
			string? line = "";
			try
			{
				using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{ }
			this.Description = s;
		}

        public SumSetNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
           if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;
            decimal decimalResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();//.id + "." + (node.children.Count + 1).ToString ();
            this.Name = values["name"]!;
            this.EditChildren = values["editChildren"] == "true" ? true : false;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
           if (decimal.TryParse(values["min"], out decimalResult))
            {
                this.Min = decimalResult;
                this.MinIsSet = true;
            }
            if (decimal.TryParse(values["max"], out decimalResult))
            {
                this.Max = decimalResult;
                this.MaxIsSet = true;
            }
            if (decimal.TryParse(values["discount"], out decimalResult))
                this.Discount = decimalResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            //newnode.optional = values ["optional"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (values["optional"] == "true" || (node != null && node.Type == NodeType.Decision)) { this.Optional = true; this.CheckBox = true; }
            else { if (node == null || (node != null && node.Type != NodeType.Decision)) this.Selected = true; }
            this.DisableCondition = values["disable"]!;
            this.DisabledMessage = values["disabledMessage"]!;
            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

            //Add new node to children
            if (node != null)
            {
                node.Children!.Add(this);
                //SetDependentsByHierarchy(Root, stack);
                //SetDependentsByReference(node, false);
            }
            else
            {
                tree.Root = this;
            }

            if (node != null) node.SortChildren();
        }
	}

    [Serializable]
    public class ReferenceNode : ANode
    {
        // *******Fields*****
        string _Target = "";
        ANode? _TargetNode = null;

        // *****Properties*****
        public string Target
        {
            get { return _Target; }
            set { if (!this.ReadOnly) _Target = value; }
        }
        public ANode? TargetNode
        {
            get { return _TargetNode; }
            set { _TargetNode = value; }
        }
        public override decimal Discount 
        {
            get { return TargetNode != null ? TargetNode.Discount : 0; }
        }

        public override decimal Min
        {
            get { return TargetNode != null ? TargetNode.Min : 0; }
        }

        public override decimal Max
        {
            get { return TargetNode != null ? TargetNode.Max : 0; }
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
            get { return TargetNode != null ? TargetNode.Selected : false;}
        }

        public override bool Optional
        {
            get { return TargetNode != null ? TargetNode.Optional : false; }
        }

        public override bool Disabled
        {
            get { return TargetNode != null ? TargetNode.Disabled : false; }
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
            return TargetNode != null ? TargetNode.IsComplete() : true;
        }

        public override decimal Total()
        {
            return TargetNode != null ? TargetNode.Total() : 0;
        }

        public ReferenceNode()
        {
            this.Name = "";
            //this.Discount = 0;
            //this.Min = this.Max = 0;
            this.Target = "";
            this.Order = 0;
            //this.Selected = false;
            this.Children = new List<ANode>();
            this.Dependents = new List<string>();
            this.References = new List<string>();
            this.Type = NodeType.Reference;
            this.Parent = null;
            //this.Amount = 1;
            //this.Optional = false;
            this.Description = "";
            this.Hidden = false;
            this.ReadOnly = false;
            this.Expanded = false;
            this.ExpandedLevels = 0;
            //this.Units = "";
            this.Report = false;
            this.ReportValue = false;
            this.Template = false;
        }
        
        public ReferenceNode(string path, ANode? parent, QTree parentTree, string id)
            : this()
        {
            if (parent != null) this.Parent = parent;
            this.ParentTree = parentTree;
            this.Name = path.Split(Path.DirectorySeparatorChar)[path.Split(Path.DirectorySeparatorChar).Length - 1];
            this.Id = id;
            string value = "";
            int intResult;

            value = this.GetValueFromDirectory("target", path);
            if (value != "") this.Target = value;
            this.TargetNode = ParentTree.GetNodeFromPath(Target)!;

            value = this.GetValueFromDirectory("expandedlevels", path);
            int.TryParse(value, out intResult);
            if (value != "") this.ExpandedLevels = intResult;

            value = this.GetValueFromDirectory("order", path);
            int.TryParse(value, out intResult);
            if (value != "") this.Order = intResult;

            if (this.GetValueFromDirectory("report", path) == "true") { this.Report = true; }
            if (this.GetValueFromDirectory("reportvalue", path) == "true") { this.ReportValue = true; }
            if (this.GetValueFromDirectory("template", path) == "true") { this.Template = true; }
            if (this.GetValueFromDirectory("hidden", path) == "true") { this.Hidden = true; }
            if (this.CheckBox)
                if (this.GetValueFromDirectory("selected", path) == "true") { this.Selected = true; }

            //To set the node url
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;


            //Get description from file in folder.
            string s = "";
            string? line = "";
            try
            {
                using (StreamReader sr = new StreamReader(path + Path.DirectorySeparatorChar + "description.txt"))
                {

                    // Read and display lines from the file until the end of 
                    // the file is reached.
                    while ((line = sr.ReadLine()) != null)
                    {
                        s += line;
                    }
                }
            }
            catch (Exception)
            { }
            this.Description = s;
        }

        public ReferenceNode(NameValueCollection values, QTree tree)
            : this()
        {
            string id = values["id"]!;
            ANode? node;
            if (id != null && id != "")
            {
                node = tree.GetNodeFromId(id);
                if (node!.Type == NodeType.Date || node.Type == NodeType.Today) return;
                if(node.Parent != null && (node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) return;
            }
            else
                node = null;

            //check for same name
            if (node != null && node.Children != null)
            foreach (ANode n in node.Children)
                if (n.Name.Trim() == values["name"]!.Trim()) return;

            Stack<ANode> stack = new Stack<ANode>();
            int intResult;

            //first set the node as writeable
            this.ReadOnly = false;
            if (node == null) this.Id = "1";
            else this.Id = node.NewId();
            this.Target = values["expression"]!;
            this.TargetNode = tree.GetNodeFromPath(values["expression"]!)!;
            this.Name = values["name"]!;
            if (int.TryParse(values["expandedLevels"], out intResult))
                this.ExpandedLevels = intResult;
            if (int.TryParse(values["order"], out intResult))
                this.Order = intResult;
            this.Hidden = values["hidden"] == "true" ? true : false;
            this.Report = values["report"] == "true" ? true : false;
            this.ReportValue = values["reportValue"] == "true" ? true : false;
            this.Template = values["template"] == "true" ? true : false;
            this.ReadOnly = values["readOnly"] == "true" ? true : false;

            if (node != null && node.Type == NodeType.Decision)
            {
                this.Dependents!.AddRange(node.Dependents!);
                this.Dependents.Add(node.Id);
            }
            this.Parent = node;
            this.ParentTree = tree;
            this.Url = "TreeView" + "/Description" + "?id=" + this.Id;

            //Add new node to children
            if (node != null)
            {
                node.Children!.Add(this);
                //SetDependentsByHierarchy(Root, stack);
                //SetDependentsByReference(node, false);
            }
            else
            {
                tree.Root = this;
            }

            if (node != null) node.SortChildren();
        }
    }


	[Serializable]
	public class QTree
	{
		// *****Fields*****
		ANode? _Root;
        // Next field is used to detect circular references.
        public int TotalCounter = 0;
        public int EvaluateParameterCounter = 0;

		public ANode? Root
		{
			get { return _Root; }
			set { _Root = value;}
		}

		// *****Methods*****

        public Dictionary<string, string> GetSelections()
		{
            Dictionary<string, string> selection = new Dictionary<string, string>();
            GetSelections(Root!, 0, selection);
			return selection;
		}

        //this method is not used right now
		public string GetSelectionsString(ANode start)
		{
			string s = "";
			string s1 = "";
			if (start != null && !start.Hidden && start.Selected && start.Report)
			{
				s += start.Name;
				if (start.ReportValue)
				{
					s += " = ";
					if (start.Units != "$") s += decimal.Round(start.Total(), 2) + " " + start.Units;
					else s += start.Units + decimal.Round(start.Total(), 2);
				}

				if (start.Children == null || start.Children.Count == 0) return s;
				else
				{
					foreach (ANode n in start.Children)
					{
						s1 = GetSelectionsString(n);
						if (n != null && s1.Trim() != "") s += "| " + s1;
						s1 = "";
					}
					return s;
				}
			}
			return s;
		}

        //this method is not used right now
		public string GetSelectionsString(ANode start, int indent)
		{
			string s = "";
			string s1 = "";
			string indentspace = "";
			if (start != null && !start.Hidden && start.Selected && start.Report)
			{
				s += start.Name;
				if (start.ReportValue)
				{
					s += " = ";
					if (start.Units != "$") s += decimal.Round(start.Total(), 2) + " " + start.Units;
					else s += start.Units + decimal.Round(start.Total(), 2);
				}

				for (int i = 0; i < indent; i++) indentspace += " ";

				if (start.Children == null || start.Children.Count == 0 || dontReportChildren(start)) return indentspace + s; 
				else
				{
					s = indentspace + s;
					foreach (ANode n in start.Children)
					{
						s1 = GetSelectionsString(n, indent + 2);
						if (n != null && s1.Trim() != "") s =  s + "|" + s1;
						s1 = "";
					}
					return s;
				}
			}
			return s;
		}

        public string GetSelections(ANode start, int indent, Dictionary<string,string> selection)
        {
            string s = "";
            string s1 = "";
            string indentspace = "";
            if (start != null && !start.Hidden && start.Selected && start.Report)
            {
                s += start.Name;
                if (start.ReportValue)
                {
                    if(start.Type == NodeType.Text)
                    {
                        s += " [";
                        s += ((TextNode)start).Text;
                        s += "]";
                    }
                    else
                    {
                        s += " [";
                        if (start.Units != "$") s += decimal.Round(start.Total(), 2) + " " + start.Units;
                        else s += start.Units + decimal.Round(start.Total(), 2);
                        s += "]";
                    }
                }

                for (int i = 0; i < indent; i++) indentspace += " ";
                selection.Add(start.Id, indentspace + s);

                if (start.Children == null || start.Children.Count == 0 || dontReportChildren(start))
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
                        s1 = GetSelections(n, indent + 2, selection);
                        if (n != null && s1.Trim() != "")
                        {
                            s = s + "|" + s1;
                            counter++;
                            //selection.Add(childID, s1);
                        }
                        s1 = "";
                        
                    }
                    return s;
                }
            }
            return s;
        }

		public bool dontReportChildren(ANode node)
		{
			foreach (ANode child in node.Children!)
			{
				if (!child.Hidden && child.Report) return false;
			}
			return true;
		}

		public string GetValueFromDirectory(string field, string dir)
		{

			String? line;
			string s = "";
			string[] splitted;

			try
			{
				using (StreamReader sr = new StreamReader(dir + Path.DirectorySeparatorChar + "values.txt"))
				{

					// Read and display lines from the file until the end of 
					// the file is reached.
					while ((line = sr.ReadLine()) != null)
					{
						s += line;
					}
				}
			}
			catch (Exception)
			{
				return "";
			}

			//s = s.Replace(" ", "");
			if (s == "" || !s.Contains(field)) return "";
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
			string type = this.GetValueFromDirectory("type", path);

			//Create nodes from directory folders
			/*if (type.Trim() != "")
            {
                string first = type[0].ToString().ToUpper();
                string rest = type.Substring(1, type.Length -1).ToLower();
                type = first + rest;
                //type = type.ToCharArray()[0].ToString().ToUpper() + type.Substring(1);
            }

            else return;
            try
            {
                node = (ANode)System.Activator.CreateInstance(System.Type.GetType(type + "Node"),new object[] {path, parent, this});
            }

            catch (Exception e) { }*/

			if (type != null)
			{
                type = type.ToLower();
				if (type == "decision") node = new DecisionNode(path, parent, this, id);

				if (type == "math") node = new MathNode(path, parent, this, id);

                if (type == "text") node = new TextNode(path, parent, this, id);

				if (type == "range") node = new RangeNode(path, parent, this, id);

				if (type == "sumset") node = new SumSetNode(path, parent, this, id);

				if (type == "conditional") node = new ConditionalNode(path, parent, this, id);

				if (type == "conditionalrules") node = new ConditionalRulesNode(path, parent, this, id);

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
						Fill(dir, ref newnode, node!, id + "." + child_id.ToString());
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
				ANode? node = this._Root;
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
			return GetNode(name, this._Root!)!;
		}

		public ANode? GetNodeFromPath(string path)
		{
			path = path.Replace("\\", "/");
			string[] parts = path.Split("/".ToCharArray());
			ANode? n = this._Root;
            try
            {
                if (parts.Length == 0 || n!.Name.Trim() != parts[0].Trim()) return null;
                else if (parts.Length == 1) return n;
                for (int i = 1; i < parts.Length; i++)
                {
                    if (parts[i].Trim() == "") return n;
                    if (n != null && n.FindChildNode(parts[i].Trim()) != null) n = n.FindChildNode(parts[i].Trim());
                    else return null;
                }
                return n;
            }
            catch (Exception) { return null; }
		}

		ANode? GetNode(string name, ANode start)
		{
			if (start == null) return null;
			else
				if (start.Name == name) return start;
				else
					if (start.Children == null || start.Children.Count == 0) return null;
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

        public void removeNodesFromDirectory(string removedNodePaths)
        {
             //delete nodes
            if (removedNodePaths != null && removedNodePaths != "")
            {
                var splitted = removedNodePaths.Trim().Split(";".ToCharArray());
                foreach(var nodePath in splitted)
                {
                    if (Directory.Exists (nodePath)) Directory.Delete(nodePath,true);
                }
            }
        }
		public void SaveTreeTo(ANode start, string path,  Dictionary<string, string> renamed)
		{
			try
			{
                if (!Directory.Exists(path + Path.DirectorySeparatorChar + start.Name))
                {
                    //if the node was renamed rename the folder.
                    if (renamed != null && renamed.ContainsKey(start.Id))
                    {
                        if (Directory.Exists(path + Path.DirectorySeparatorChar + renamed[start.Id])) 
                        {
                            Directory.Move(path + Path.DirectorySeparatorChar + renamed[start.Id], path + Path.DirectorySeparatorChar + start.Name);
                        }
                    }
                    else
                        Directory.CreateDirectory(path + Path.DirectorySeparatorChar + start.Name);
                }
				StreamWriter sw = File.CreateText(path + Path.DirectorySeparatorChar + start.Name + Path.DirectorySeparatorChar + "values.txt");

                sw.WriteLine("id=\"" + start.Id.ToString() + "\";");
                sw.WriteLine("type=\"" + start.Type.ToString() + "\";"); 
				sw.WriteLine("order=\"" + start.Order.ToString() + "\";"); 
				sw.WriteLine("report=\"" + start.Report.ToString().ToLower() + "\";"); 
				sw.WriteLine("reportvalue=\"" + start.ReportValue.ToString().ToLower() + "\";");
                if (start.Type != NodeType.Reference)
                {
                    sw.WriteLine("units=\"" + start.Units + "\";");
                    sw.WriteLine("optional=\"" + start.Optional.ToString().ToLower() + "\";");
                    if (start.MaxIsSet)
                        sw.WriteLine("max=\"" + start.Max.ToString() + "\";");
                    else sw.WriteLine("max=\"" + "∞" + "\";");
                    sw.WriteLine("maxisset=\"" + start.MaxIsSet.ToString().ToLower() + "\";"); 
                    if (start.MinIsSet)
                        sw.WriteLine("min=\"" + start.Min.ToString() + "\";");
                    else  sw.WriteLine("min=\"" + "∞" + "\";");
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
					sw.WriteLine("formula=\"" + (start as MathNode)!.Formula + "\";"); 
					sw.WriteLine("editchildren=\"" + (start as MathNode)!.EditChildren.ToString().ToLower() + "\";");
				}
                if (start.Type == NodeType.Text)
                {
                    sw.WriteLine("text=\"" + (start as TextNode)!.Text + "\";");
                    sw.WriteLine("editchildren=\"" + (start as TextNode)!.EditChildren.ToString().ToLower() + "\";");
                }
                if (start.Type == NodeType.Range)
                {
                    sw.WriteLine("range=\"" + (start as RangeNode)!.Range + "\";");
                    sw.WriteLine("editchildren=\"" + (start as RangeNode)!.EditChildren.ToString().ToLower() + "\";");
                }
                if (start.Type == NodeType.Conditional)
                {
                    sw.WriteLine("formula=\"" + (start as ConditionalNode)!.Formula + "\";");
                    sw.WriteLine("editchildren=\"" + (start as ConditionalNode)!.EditChildren.ToString().ToLower() + "\";");
                }
                if (start.Type == NodeType.ConditionalRules)
                {
                    sw.WriteLine("expression=\"" + (start as ConditionalRulesNode)!.Expression + "\";");
                    sw.WriteLine("editchildren=\"" + (start as ConditionalRulesNode)!.EditChildren.ToString().ToLower() + "\";");
                }
                if (start.Type == NodeType.Reference)
                {
                    sw.WriteLine("target=\"" + (start as ReferenceNode)!.Target + "\";");
                }
				sw.Close();

				foreach (ANode child in start.Children!)
					SaveTreeTo(child, path + Path.DirectorySeparatorChar + start.Name, renamed!);
			}
			catch(Exception e)
			{
				string message = e.Message;
			}
		}

		//Create a scaffolding of dependencies by hierarchy and immediate references
        public void SetDependentsByHierarchy(ANode node, Stack<ANode> s)
        {
            char[] charArr = { '*', '/', '+', '-', '|', '[', ']', '?', '&', '!', '(', ')', '>', '<', '=', ':', ';' };
            s.Push(node);
            foreach (ANode child in node.Children!)
            {
                if (node.Type == NodeType.Decision
                    || node.Type == NodeType.SumSet
                    || node.Type == NodeType.Date
                    || (node.Type == NodeType.Math && Array.IndexOf((node as MathNode)!.Formula.Split(charArr),child.Name) > -1)
                    || (node.Type == NodeType.Range && Array.IndexOf((node as RangeNode)!.Range.Split(charArr), child.Name) > -1)
                    || (node.Type == NodeType.Conditional && Array.IndexOf((node as ConditionalNode)!.Formula.Split(charArr), child.Name) > -1)
                    || (node.Type == NodeType.ConditionalRules && Array.IndexOf((node as ConditionalRulesNode)!.Expression.Split(charArr), child.Name) > -1))
                {
                    foreach (ANode dependent in s)
                        if (!child.Dependents!.Contains(dependent.Id))
                            child.Dependents.Add(dependent.Id);
                        
                    SetDependentsByHierarchy(child, s);
                    //Add reference
                    if (!child.References!.Contains(node.Id)) child.References.Add(node.Id);
                }
                else SetDependentsByHierarchy(child, new Stack<ANode>());
            }
            s.Pop();
        }

		//Takes care of the external references and then some more.
        public Tuple<ANode, ANode>? SetDependentsByReference(ANode node, bool recursive)
		{
            Tuple<ANode, ANode>? tuple = null;
            ANode? NodeFromPath, NodeFromId;
			if (node.Type == NodeType.Math || node.Type == NodeType.Range || node.Type == NodeType.Conditional || node.Type == NodeType.ConditionalRules || node.Type == NodeType.Reference)
			{
				string expression = "";
				switch (node.Type)
				{
				case NodeType.Math:
					expression = (node as MathNode)!.Formula;
					break;
				case NodeType.Range:
					expression = (node as RangeNode)!.Range;
					break;
				case NodeType.Conditional:
					expression = (node as ConditionalNode)!.Formula;
						break;
				case NodeType.ConditionalRules:
					foreach (KeyValuePair<string,string> rule in (node as ConditionalRulesNode)!.Rules)
						expression = expression + '|' + rule.Key + '|' + rule.Value + '|';
					break;
                case NodeType.Reference:
                    expression = (node as ReferenceNode)!.Target;
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
                clean_expression = Regex.Replace(clean_expression, @"Max", "", RegexOptions.IgnoreCase);
                clean_expression = Regex.Replace(clean_expression, @"Min", "", RegexOptions.IgnoreCase);
                //clean_expression = Regex.Replace(clean_expression, @"Round", "", RegexOptions.IgnoreCase);
                string[] splitted_expression = clean_expression.Split(new char[] { '*', '/', '+', '-', '|', '[', ']', '?', '&', '!', '(', ')', '>', '<', '=', ':', ';' });

                foreach (string s in splitted_expression)
				{
                    s1 = s.Trim();
					if (s.Contains("\\")) //if the node is a full path
					{
						NodeFromPath = this.GetNodeFromPath(s1);
						if (NodeFromPath != null) 
						{
                            //check for circular references
							tuple = SetDependenciesRecursively (NodeFromPath, node);
                            if (tuple != null) return tuple;
                            //following code should be moved to method SetDependenciesRecursively
                            //foreach (ANode dependent in node.Dependents)
                            //{
                            //    //check for circular references
                            //    tuple = SetDependenciesRecursively(NodeFromPath, dependent);
                            //    if (tuple != null) return tuple;
                            //}
							//foreach (ANode dependent in node.dependents)
							//	if (!NodeFromPath.dependents.Contains (dependent))
							//		NodeFromPath.dependents.Add (dependent);
							//Add reference
							if (!NodeFromPath.References!.Contains (node.Id))
								NodeFromPath.References.Add (node.Id);
						}
					}
                    else                   
                    if (s1.StartsWith("{") && s1.EndsWith("}")) //if the node is an id
                    {
                        NodeFromId = this.GetNodeFromId(s1.Substring(1, s1.Length - 2)); 
                        if (NodeFromId != null)
                        {
                            //check for circular references
                            tuple = SetDependenciesRecursively(NodeFromId, node);
                            if (tuple != null) return tuple;
                            //following code should be moved to method SetDependenciesRecursively
                            //foreach (ANode dependent in node.Dependents)
                            //{
                            //    //check for circular references
                            //    tuple = SetDependenciesRecursively(NodeFromPath, dependent);
                            //    if (tuple != null) return tuple;
                            //}
                            //foreach (ANode dependent in node.dependents)
                            //	if (!NodeFromPath.dependents.Contains (dependent))
                            //		NodeFromPath.dependents.Add (dependent);
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
                                //foreach (ANode dependent in node.Dependents)
                                //{
                                //    //check for circular references
                                //    if (dependent.Dependents.Contains(child)) return new Tuple<ANode,ANode>(dependent,child);
                                //    if (!child.Dependents.Contains(dependent)) child.Dependents.Add(dependent);
                                //}

                                //foreach (ANode dependent in GetDependencies(child))
                                //{
                                    //check for circular references
                                tuple = SetDependenciesRecursively(child, node);
                                if (tuple != null) return tuple;
                                //}
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
                    if (s.Contains("\\")) //if the node is a full path
                    {
                        NodeFromPath = this.GetNodeFromPath(s1);
                        if (NodeFromPath != null)
                        {
                            //check for circular references
                            tuple = SetDependenciesRecursively(NodeFromPath, node);
                            if (tuple != null) return tuple;
                            //foreach (ANode dependent in node.Dependents)
                            //{
                            //    //check for circular references
                            //    tuple = SetDependenciesRecursively(NodeFromPath, dependent);
                            //    if (tuple != null) return tuple;
                            //}
                            //foreach (ANode dependent in node.dependents)
                            //	if (!NodeFromPath.dependents.Contains (dependent))
                            //		NodeFromPath.dependents.Add (dependent);
                            //Add reference
                            if (!NodeFromPath.References!.Contains(node.Id))
                                NodeFromPath.References.Add(node.Id);
                        }
                    }
                    else
                    if (s1.StartsWith("{") && s1.EndsWith("}")) //if the node is an id
                    {
                        NodeFromId = this.GetNodeFromId(s1.Substring(1, s1.Length - 2));
                        if (NodeFromId != null)
                        {
                            //check for circular references
                            tuple = SetDependenciesRecursively(NodeFromId, node);
                            if (tuple != null) return tuple;
                            //foreach (ANode dependent in node.Dependents)
                            //{
                            //    //check for circular references
                            //    tuple = SetDependenciesRecursively(NodeFromPath, dependent);
                            //    if (tuple != null) return tuple;
                            //}
                            //foreach (ANode dependent in node.dependents)
                            //	if (!NodeFromPath.dependents.Contains (dependent))
                            //		NodeFromPath.dependents.Add (dependent);
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
                                //foreach (ANode dependent in node.Dependents)
                                //{
                                //    //check for circular references
                                //    if (dependent.Dependents.Contains(child)) return new Tuple<ANode, ANode>(dependent, child);
                                //    if (!child.Dependents.Contains(dependent)) child.Dependents.Add(dependent);
                                //}

                                //foreach (ANode dependent in GetDependencies(child))
                                //{
                                    //check for circular references
                                tuple = SetDependenciesRecursively(child, node);
                                if (tuple != null) return tuple;
                                //}
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
                    //foreach (ANode dependent in GetDependencies(child))
                    //{
                    tuple = SetDependenciesRecursively(child, node);
                    if (tuple != null) return tuple;
                    // }
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
            if (tuple != null) return tuple;
            else return null;
		}

        public Tuple<ANode, ANode>? SetDependents()
        {
            RemoveDependents(Root!);
            RemoveReferences(Root!);
            Stack<ANode> stack = new Stack<ANode>();
            Tuple<ANode, ANode>? tuple = null;
            //the following method call is no longer needed, improves greatly performance.
            //SetDependentsByHierarchy(Root, stack);
            //This needs to be done twice in order to catch all dependents
            tuple = SetDependentsByReference(Root!, true);
            if (tuple == null)
                tuple = SetDependentsByReference(Root!, true);

            if (tuple != null) return tuple;
            return null;
        }

        public void RemoveDependents(ANode start) 
        {
            if (start != null)
            {
                start.Dependents!.Clear();
                foreach (ANode child in start.Children!)
                    RemoveDependents(child);
            }
        }

        public void RemoveReferences(ANode start) 
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
            Tuple<ANode, ANode>? tuple = null;
            if (dependent.Dependents!.Contains(start.Id)) return new Tuple<ANode, ANode>(start, dependent);
			if (!start.Dependents!.Contains(dependent.Id)) start.Dependents.Add(dependent.Id);
			foreach (ANode node in GetDependencies(start))
			{
                if (dependent.Dependents.Contains(node.Id)) return new Tuple<ANode, ANode>(node, dependent);
				if (!node.Dependents!.Contains(dependent.Id)) node.Dependents.Add(dependent.Id);
				tuple = SetDependenciesRecursively(node, dependent);
                if (tuple != null) return tuple;
			}
            foreach (string dependent2 in dependent.Dependents)
            {
                if (this.GetNodeFromId(dependent2)!.Dependents!.Contains(start.Id)) return new Tuple<ANode, ANode>(start, this.GetNodeFromId(dependent2)!);
                tuple = SetDependenciesRecursively(start, this.GetNodeFromId(dependent2)!);
                if (tuple != null) return tuple;
            }
            return null;
		}

		public List<ANode> GetDependencies(ANode dependent)
		{
			List<ANode> list = new List<ANode>();
			GetDependencies(dependent, this._Root!, list);
			return list;
		}

		private void GetDependencies(ANode dependent, ANode start, List<ANode> returnedList) 
		{
			if (start.Dependents!.Contains(dependent.Id)) returnedList.Add(start);
			foreach (ANode child in start.Children!)
				GetDependencies(dependent, child, returnedList);
		}

        public void ResetEntered(ANode node)
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
                if (values["name"]!.Trim() == "") return null;
                
                //check for same name
                if (node != Root)
                    foreach (ANode n in node.Parent!.Children!)
                        if (n.Name.Trim() == values["name"]!.Trim() && n != node)
                        {
                            return null;
                        }


                //first set the read-only attribute to false
                node.ReadOnly = false;
                if (node.Parent == null || !(node.Parent.Type == NodeType.Date || node.Parent.Type == NodeType.Today)) 
                    node.Name = values["name"]!.Trim();
                    
                node.Units = values["units"]!.Trim();
                Stack<ANode> stack = new Stack<ANode>();
                switch (node.Type)
                {
                    case NodeType.Math:
                        (node as MathNode)!.Formula = values["expression"]!.Trim();
                        (node as MathNode)!.EditChildren = values["editChildren"] == "true" ? true : false;

                        //To set the url for the node
                        decimal flag;
                        if (decimal.TryParse((node as MathNode)!.Formula, out flag) || (node as MathNode)!.EditChildren)
                        {
                            node.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + node.Id;
                        }
                        else
                        {
                            node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        }
                        break;
                    case NodeType.Text:
                        (node as TextNode)!.Text = values["expression"]!.Trim();
                        (node as TextNode)!.EditChildren = values["editChildren"] == "true" ? true : false;

                        //Set node url
                        node.Url = "TreeView" + "/ChangeTreeValue" + "?id=" + node.Id;
                        break;
                    case NodeType.Range:
                        (node as RangeNode)!.Range = values["expression"]!.Trim();
                        (node as RangeNode)!.EditChildren = values["editChildren"] == "true" ? true : false;

                        //Set node url
                        node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        break;
                    case NodeType.Date:
                        (node as DateNode)!.EditChildren = values["editChildren"] == "true" ? true : false;

                        //Set node url
                        node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        break;
                    case NodeType.Today:
                        (node as TodayNode)!.EditChildren = false;

                        //Set node url
                        node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        break;
                    case NodeType.ConditionalRules:
                        (node as ConditionalRulesNode)!.Expression = values["expression"]!.Trim();
                        (node as ConditionalRulesNode)!.parseExpression();
                        (node as ConditionalRulesNode)!.EditChildren = values["editChildren"] == "true" ? true : false;

                        //To set the url for the node
                        node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        break;
                    case NodeType.Conditional:
                        (node as ConditionalNode)!.Formula = values["expression"]!.Trim();
                        //(node as ConditionalNode).parseFormula();
                        (node as ConditionalNode)!.EditChildren = values["editChildren"] == "true" ? true : false;

                        //To set the url for the node
                        node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        break;
                    case NodeType.SumSet:
                        (node as SumSetNode)!.EditChildren = values["editChildren"] == "true" ? true : false;

                        //To set the url for the node
                        node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        break;
                    case NodeType.Reference:
                        (node as ReferenceNode)!.Target = values["expression"]!.Trim();
                        (node as ReferenceNode)!.TargetNode = GetNodeFromPath(values["expression"]!)!;
                        //Set node url
                        node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        break;
                    default:
                        //To set the url for the node
                        node.Url = "TreeView" + "/Description" + "?id=" + node.Id;
                        break;
                }
                int intResult;
                decimal decimalResult;
                if (int.TryParse(values["expandedLevels"], out intResult))
                    node.ExpandedLevels = intResult;
                if (int.TryParse(values["order"], out intResult))
                    node.Order = intResult;
                if (node.Type != NodeType.Reference)
                {
                    if (decimal.TryParse(values["min"], out decimalResult))
                    {
                        node.Min = decimalResult;
                        node.MinIsSet = true;
                    }
                    else node.MinIsSet = false;
                    if (decimal.TryParse(values["max"], out decimalResult))
                    {
                        node.Max = decimalResult;
                        node.MaxIsSet = true;
                    }
                    else node.MaxIsSet = false;
                    if (decimal.TryParse(values["discount"], out decimalResult))
                        node.Discount = decimalResult;
                    node.Optional = values["optional"] == "true" ? true : false;
                    node.DisableCondition = values["disable"]!.Trim();
                    node.DisabledMessage = values["disabledMessage"]!.Trim();
                }
                node.Hidden = values["hidden"] == "true" ? true : false;
                node.Report = values["report"] == "true" ? true : false;
                node.ReportValue = values["reportValue"] == "true" ? true : false;
                node.Template = values["template"] == "true" ? true : false;
                node.ReadOnly = values["readOnly"] == "true" ? true : false;

                Root!.SortChildren();
                //SetDependentsByHierarchy(Root, stack);
                //SetDependentsByReference(node, false);

                return node;
            }
            catch (Exception e)
            {
                string message = e.Message;
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
                case "Range":
                    newnode = new RangeNode(values, this);
                    break;
                case "Conditional":
                    newnode = new ConditionalNode(values, this);
                    break;
                case "ConditionalRules":
                    newnode = new ConditionalRulesNode(values, this);
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
                    break;
                case "Today":
                    newnode = new TodayNode(values, this);
                    break;
                default:
                    break;
            }
            if (newnode == null || newnode.Name.Trim() == "") return null;
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
                            int result;
                            int lengthDifference = n.Name.Trim().Length - clone.Name.Trim().Length;
                            string ending  = n.Name.Trim().Substring(clone.Name.Trim().Length, lengthDifference);
                            if (ending.StartsWith(" "))
                                if (int.TryParse(ending.Remove(0,1), out result))
                                {
                                    if (result > counter) counter = result;
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
                //clone.Dependents = source.Dependents;
                //clone.References = source.References;
                target.Children.Add(clone);

                //fix the node id and url
                FixClone(clone, this);

                //Set dependencies
                //Stack<ANode> stack = new Stack<ANode>();
                //SetDependentsByHierarchy(Root, stack);
                //SetDependentsByReference(target);
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
                Stack<ANode> stack = new Stack<ANode>();
                SetDependentsByHierarchy(Root, stack);
                return clone;
            }
            catch (Exception) { return null; }
        }

        public void FixClone(ANode node, QTree parentTree)
        {
            node.Id = node.Parent!.NewId();
            node.Url = node.Url.Split('=')[0] + "=" + node.Id;
            node.References!.Clear();
            node.Dependents!.Clear();
            node.ParentTree = parentTree;
            FixClonedChildren(node.Children!, node);
        }

        public void FixClonedChildren(List<ANode> children, ANode parent)
        {
            if (children != null && children.Count != 0 && parent != null)
            {
                string[] id_splitted;
                foreach (ANode child in children)
                {
                    id_splitted = child.Id.Split('.');
                    child.Id = parent.Id + "." + id_splitted[id_splitted.Length - 1];
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
        public void Refactor(List<ANode> references, string oldname, string newname)
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
			//Stack<ANode> stack = new Stack<ANode>();
			this.Fill(path, ref _Root, null,"1");
			this._Root!.SortChildren();
			if (dependencies) {
				//this.SetDependentsByHierarchy (_Root, stack);
				//This needs to be done twice in order to catch all dependents
				this.SetDependentsByReference (_Root, true);
				this.SetDependentsByReference (_Root, true); 
			}
		}

		public QTree(QTree t)
		{
			BinaryFormatter formater = new BinaryFormatter();
			MemoryStream serial = t.Serialize();
			_Root = (formater.Deserialize(serial) as QTree)._Root;
            serial.Close();
            serial.Dispose();
		}

		public QTree()
		{
			this._Root = null;
		}

		public MemoryStream Serialize()
		{
			//FileStream fs = new FileStream("c:\\serialized.dat",FileMode.Create,FileAccess.Write);
			MemoryStream ms = new MemoryStream();
			BinaryFormatter formater = new BinaryFormatter();
			//formater.Serialize(fs, this);
			formater.Serialize(ms, this);
			ms.Seek(0, SeekOrigin.Begin);
			return ms;

		}

		public string SerializeToString()
		{
			XmlSerializer serializer = new XmlSerializer(this.GetType());

			using (StringWriter writer = new StringWriter())
			{
				serializer.Serialize(writer, this);

				return writer.ToString();
			}
		}

        public static QTree Deserialize(byte[] byte_array)
        {
            BinaryFormatter formater = new BinaryFormatter();
            using (MemoryStream memory_stream = new MemoryStream(byte_array))
            {
                QTree tree = ((formater.Deserialize(memory_stream)) as QTree)!;
                tree!.TotalCounter = 0;
                return tree;
            }
        }
	}

    public class CircularReferenceException : Exception
    {
        public CircularReferenceException() : base("Circular reference") { }
    }
