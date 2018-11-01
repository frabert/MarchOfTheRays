using MarchOfTheRays.Properties;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarchOfTheRays
{
    partial class MainForm : Form
    {
        PropertyGrid InitializePropertyBox()
        {
            var propertyBox = new PropertyGrid();
            propertyBox.Dock = DockStyle.Fill;

            SelectionChanged += (s, e) =>
            {
                var selectedItems = canvas.SelectedElements.ToList();
                if (selectedItems.Count == 1)
                {
                    propertyBox.SelectedObject = selectedItems[0].Tag;
                }
                else
                {
                    propertyBox.SelectedObject = null;
                }
            };

            return propertyBox;
        }
    }
}