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
        RichTextBox InitializeHelpBox()
        {
            var helpBox = new RichTextBox();
            helpBox.ReadOnly = true;
            helpBox.Dock = DockStyle.Fill;
            helpBox.BorderStyle = BorderStyle.None;

            SelectionChanged += (s, e) =>
            {
                var selectedItems = canvas.SelectedElements.ToList();
                if (selectedItems.Count == 1)
                {
                    var selectedItem = selectedItems[0].Tag;

                    var name = selectedItem.GetType().Name;
                    var filePath = $"HelpFiles/{name}.xml";
                    helpBox.Rtf = "";
                    if (File.Exists(filePath))
                    {
                        using (var file = File.OpenRead(filePath))
                        {
                            var doc = new Docs.Document(file);
                            helpBox.Rtf = doc.ToRtf();
                        }
                    }
                }
                else
                {
                    helpBox.Rtf = "";
                }
            };

            return helpBox;
        }
    }
}