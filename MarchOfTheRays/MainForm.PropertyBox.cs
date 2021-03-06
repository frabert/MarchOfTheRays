﻿using MarchOfTheRays.Properties;
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
        void InitializePropertyBox()
        {
            PropertyPanel CreatePropertyPanel()
            {
                var propertyBox = new PropertyPanel();

                SelectionChanged += (s, e) =>
                {
                    if (ActiveEditor == null) return;

                    var selectedItems = ActiveEditor.Canvas.SelectedElements.ToList();
                    if (selectedItems.Count == 1)
                    {
                        propertyBox.PropertyGrid.SelectedObject = selectedItems[0].Tag;
                    }
                    else
                    {
                        propertyBox.PropertyGrid.SelectedObject = null;
                    }
                };

                propertyBox.Show(dockPanel, DockState.DockRight);

                return propertyBox;
            }

            var panel = CreatePropertyPanel();

            ShowPropertyPanel += (s, e) =>
            {
                if (panel.IsDisposed)
                {
                    panel = CreatePropertyPanel();
                    panel.Show(dockPanel, DockState.DockRight);
                }
            };
        }
    }
}