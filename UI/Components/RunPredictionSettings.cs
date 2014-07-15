using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Globalization;
using Fetze.WinFormsColor;
using LiveSplit.TimeFormatters;
using LiveSplit.Model;
using LiveSplit.Model.Comparisons;

namespace LiveSplit.UI.Components
{
    public partial class RunPredictionSettings : UserControl
    {
        public Color TextColor { get; set; }
        public bool OverrideTextColor { get; set; }
        public Color TimeColor { get; set; }
        public bool OverrideTimeColor { get; set; }
        public TimeAccuracy Accuracy { get; set; }

        public Color BackgroundColor { get; set; }
        public Color BackgroundColor2 { get; set; }
        public GradientType BackgroundGradient { get; set; }
        public String GradientString
        {
            get { return BackgroundGradient.ToString(); }
            set { BackgroundGradient = (GradientType)Enum.Parse(typeof(GradientType), value); }
        }

        public String Comparison { get; set; }
        public LiveSplitState CurrentState { get; set; }
        public bool Display2Rows { get; set; }

        public LayoutMode Mode { get; set; }

        public RunPredictionSettings()
        {
            InitializeComponent();

            TextColor = Color.FromArgb(255, 255, 255);
            OverrideTextColor = false;
            TimeColor = Color.FromArgb(255, 255, 255);
            OverrideTimeColor = false;
            Accuracy = TimeAccuracy.Seconds;
            BackgroundColor = Color.Transparent;
            BackgroundColor2 = Color.Transparent;
            BackgroundGradient = GradientType.Plain;
            Comparison = "Current Comparison";
            Display2Rows = false;

            chkOverrideTextColor.DataBindings.Add("Checked", this, "OverrideTextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTextColor.DataBindings.Add("BackColor", this, "TextColor", false, DataSourceUpdateMode.OnPropertyChanged);
            chkOverrideTimeColor.DataBindings.Add("Checked", this, "OverrideTimeColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnTimeColor.DataBindings.Add("BackColor", this, "TimeColor", false, DataSourceUpdateMode.OnPropertyChanged);

            cmbGradientType.SelectedIndexChanged += cmbGradientType_SelectedIndexChanged;
            cmbGradientType.DataBindings.Add("SelectedItem", this, "GradientString", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor1.DataBindings.Add("BackColor", this, "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            btnColor2.DataBindings.Add("BackColor", this, "BackgroundColor2", false, DataSourceUpdateMode.OnPropertyChanged);
            cmbComparison.SelectedIndexChanged += cmbComparison_SelectedIndexChanged;
            cmbComparison.DataBindings.Add("SelectedItem", this, "Comparison", false, DataSourceUpdateMode.OnPropertyChanged);
            this.Load += PossibleTimeSaveSettings_Load;

            rdoSeconds.CheckedChanged += rdoSeconds_CheckedChanged;
            rdoHundredths.CheckedChanged += rdoHundredths_CheckedChanged;

            chkOverrideTextColor.CheckedChanged += chkOverrideTextColor_CheckedChanged;
            chkOverrideTimeColor.CheckedChanged += chkOverrideTimeColor_CheckedChanged;
        }

        void chkOverrideTimeColor_CheckedChanged(object sender, EventArgs e)
        {
            label2.Enabled = btnTimeColor.Enabled = chkOverrideTimeColor.Checked;
        }

        void chkOverrideTextColor_CheckedChanged(object sender, EventArgs e)
        {
            label1.Enabled = btnTextColor.Enabled = chkOverrideTextColor.Checked;
        }
        void cmbComparison_SelectedIndexChanged(object sender, EventArgs e)
        {
            Comparison = cmbComparison.SelectedItem.ToString();
        }

        void PossibleTimeSaveSettings_Load(object sender, EventArgs e)
        {
            chkOverrideTextColor_CheckedChanged(null, null);
            chkOverrideTimeColor_CheckedChanged(null, null);
            cmbComparison.Items.Clear();
            cmbComparison.Items.Add("Current Comparison");
            cmbComparison.Items.AddRange(CurrentState.Run.Comparisons.Where(x => x != BestSplitTimesComparisonGenerator.ComparisonName && x != NoneComparisonGenerator.ComparisonName).ToArray());
            if (!cmbComparison.Items.Contains(Comparison))
                cmbComparison.Items.Add(Comparison);
            rdoSeconds.Checked = Accuracy == TimeAccuracy.Seconds;
            rdoTenths.Checked = Accuracy == TimeAccuracy.Tenths;
            rdoHundredths.Checked = Accuracy == TimeAccuracy.Hundredths;
            if (Mode == LayoutMode.Horizontal)
            {
                chkTwoRows.Enabled = false;
                chkTwoRows.DataBindings.Clear();
                chkTwoRows.Checked = true;
            }
            else
            {
                chkTwoRows.Enabled = true;
                chkTwoRows.DataBindings.Clear();
                chkTwoRows.DataBindings.Add("Checked", this, "Display2Rows", false, DataSourceUpdateMode.OnPropertyChanged);
            }
        }

        void cmbGradientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnColor1.Visible = cmbGradientType.SelectedItem.ToString() != "Plain";
            btnColor2.DataBindings.Clear();
            btnColor2.DataBindings.Add("BackColor", this, btnColor1.Visible ? "BackgroundColor2" : "BackgroundColor", false, DataSourceUpdateMode.OnPropertyChanged);
            GradientString = cmbGradientType.SelectedItem.ToString();
        }

        void rdoHundredths_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void rdoSeconds_CheckedChanged(object sender, EventArgs e)
        {
            UpdateAccuracy();
        }

        void UpdateAccuracy()
        {
            if (rdoSeconds.Checked)
                Accuracy = TimeAccuracy.Seconds;
            else if (rdoTenths.Checked)
                Accuracy = TimeAccuracy.Tenths;
            else
                Accuracy = TimeAccuracy.Hundredths;
        }

        private T ParseEnum<T>(XmlElement element)
        {
            return (T)Enum.Parse(typeof(T), element.InnerText);
        }

        public void SetSettings(XmlNode node)
        {
            var element = (XmlElement)node;
            Version version;
            if (element["Version"] != null)
                version = Version.Parse(element["Version"].InnerText);
            else
                version = new Version(1, 0, 0, 0);
            TextColor = ParseColor(element["TextColor"]);
            OverrideTextColor = Boolean.Parse(element["OverrideTextColor"].InnerText);
            TimeColor = ParseColor(element["TimeColor"]);
            OverrideTimeColor = Boolean.Parse(element["OverrideTimeColor"].InnerText);
            Accuracy = ParseEnum<TimeAccuracy>(element["Accuracy"]);
            BackgroundColor = ParseColor(element["BackgroundColor"]);
            BackgroundColor2 = ParseColor(element["BackgroundColor2"]);
            GradientString = element["BackgroundGradient"].InnerText;
            Comparison = element["Comparison"].InnerText;
            if (version >= new Version(1, 4))
                Display2Rows = Boolean.Parse(element["Display2Rows"].InnerText);
            else
                Display2Rows = false;
        }

        public XmlNode GetSettings(XmlDocument document)
        {
            var parent = document.CreateElement("Settings");
            parent.AppendChild(ToElement(document, "Version", "1.4"));
            parent.AppendChild(ToElement(document, TextColor, "TextColor"));
            parent.AppendChild(ToElement(document, "OverrideTextColor", OverrideTextColor));
            parent.AppendChild(ToElement(document, TimeColor, "TimeColor"));
            parent.AppendChild(ToElement(document, "OverrideTimeColor", OverrideTimeColor));
            parent.AppendChild(ToElement(document, "Accuracy", Accuracy));
            parent.AppendChild(ToElement(document, BackgroundColor, "BackgroundColor"));
            parent.AppendChild(ToElement(document, BackgroundColor2, "BackgroundColor2"));
            parent.AppendChild(ToElement(document, "BackgroundGradient", BackgroundGradient));
            parent.AppendChild(ToElement(document, "Comparison", Comparison));
            parent.AppendChild(ToElement(document, "Display2Rows", Display2Rows));
            return parent;
        }

        private Color ParseColor(XmlElement colorElement)
        {
            return Color.FromArgb(Int32.Parse(colorElement.InnerText, NumberStyles.HexNumber));
        }

        private XmlElement ToElement(XmlDocument document, Color color, string name)
        {
            var element = document.CreateElement(name);
            element.InnerText = color.ToArgb().ToString("X8");
            return element;
        }

        private void ColorButtonClick(object sender, EventArgs e)
        {
            var button = (Button)sender;
            var picker = new ColorPickerDialog();
            picker.SelectedColorChanged += (s, x) => button.BackColor = picker.SelectedColor;
            picker.SelectedColor = picker.OldColor = button.BackColor;
            picker.ShowDialog(this);
            button.BackColor = picker.SelectedColor;
        }

        private XmlElement ToElement<T>(XmlDocument document, String name, T value)
        {
            var element = document.CreateElement(name);
            element.InnerText = value.ToString();
            return element;
        }
    }
}
