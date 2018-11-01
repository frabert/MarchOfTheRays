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
using WeifenLuo.WinFormsUI.Docking;

namespace MarchOfTheRays
{
    partial class MainForm : Form
    {
        void InitializeHelpBox()
        {
            HelpPanel CreateHelpPanel()
            {
                var helpBox = new HelpPanel();

                SelectionChanged += (s, e) =>
                {
                    var activeDocument = (GraphEditorForm)dockPanel.ActiveDocument;
                    if (activeDocument == null) return;

                    var selectedItems = activeDocument.Canvas.SelectedElements.ToList();
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

                helpBox.Show(dockPanel, DockState.DockRight);

                return helpBox;
            }

            var helpPanel = CreateHelpPanel();

            ShowHelpPanel += (s, e) =>
            {
                if(helpPanel.IsDisposed)
                {
                    helpPanel = CreateHelpPanel();
                    helpPanel.Show(dockPanel, DockState.DockRight);
                }
            };
        }
    }
}