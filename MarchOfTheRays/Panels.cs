using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using MarchOfTheRays.Properties;

namespace MarchOfTheRays
{
    class HelpPanel : DockContent
    {
        RichTextBox rtfbox;
        public HelpPanel()
        {
            DockAreas = DockAreas.Float | DockAreas.DockBottom | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop;
            Text = Strings.HelpPanel;

            rtfbox = new RichTextBox();
            rtfbox.Dock = DockStyle.Fill;
            rtfbox.BorderStyle = BorderStyle.None;
            rtfbox.ReadOnly = true;
            Controls.Add(rtfbox);
        }

        public string Rtf
        {
            get => rtfbox.Rtf;
            set => rtfbox.Rtf = value;
        }
    }

    class PropertyPanel : DockContent
    {
        public PropertyGrid PropertyGrid { get; private set; }

        public PropertyPanel()
        {
            DockAreas = DockAreas.Float | DockAreas.DockBottom | DockAreas.DockLeft | DockAreas.DockRight | DockAreas.DockTop;
            Text = Strings.PropertiesPanel;

            PropertyGrid = new PropertyGrid();
            PropertyGrid.Dock = DockStyle.Fill;
            Controls.Add(PropertyGrid);
        }
    }
}
