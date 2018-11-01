﻿using MarchOfTheRays.Properties;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;
using System.Runtime.Serialization.Formatters.Binary;

namespace MarchOfTheRays
{
    partial class MainForm : Form
    {
        string DocumentPath { get; set; }

        void InitializeDocument()
        {
            bool documentModified = false;

            DocumentChanged += (s, e) =>
            {
                documentModified = true;
            };

            GraphChanged += (s, e) =>
            {
                documentModified = true;
            };

            DocumentSaved += (s, e) =>
            {
                documentModified = false;
                DocumentPath = e;

                Text = Path.GetFileName(DocumentPath) + " - March of the Rays";
            };

            DocumentOpened += (s, e) =>
            {
                documentModified = false;
                DocumentPath = e;

                if (e != null)
                    Text = Path.GetFileName(DocumentPath) + " - March of the Rays";
                else
                    Text = "March of the Rays";
            };

            DocumentClosing += (s, e) =>
            {
                if (documentModified)
                {
                    var res = MessageBox.Show(Strings.AskSaveChanges, "March of the Rays", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                    if (res == DialogResult.Yes)
                    {
                        if (DocumentPath == null)
                        {
                            e.Cancel = !SaveAs();
                        }
                        else
                        {
                            Save(DocumentPath);
                            e.Cancel = false;
                        }
                    }
                    else if (res == DialogResult.No)
                    {
                        e.Cancel = false;
                    }
                    else
                    {
                        e.Cancel = true;
                    }
                }
            };

            FormClosing += (s, e) =>
            {
                var ev = new DocumentClosingEventArgs();
                OnDocumentClosing(ev);
                e.Cancel = ev.Cancel;
            };

            NewDocument();
        }

        void Save(string path)
        {
            using (var stream = File.Open(path, FileMode.Create))
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, document);
            }

            OnDocumentSaved(path);
        }

        bool SaveAs()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = $"{Strings.MtrFile}|*.mtr";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                Save(saveFileDialog.FileName);
                return true;
            }
            else
            {
                return false;
            }
        }

        bool Open()
        {
            var ev = new DocumentClosingEventArgs();
            OnDocumentClosing(ev);

            if (ev.Cancel) return false;

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = $"{Strings.MtrFile}|*.mtr";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                using (var stream = File.OpenRead(openFileDialog.FileName))
                {
                    var formatter = new BinaryFormatter();
                    var doc = (Document)formatter.Deserialize(stream);

                    foreach (var d in dockPanel.Documents.ToList())
                    {
                        var f = (GraphEditorForm)d;
                        f.Close();
                    }

                    graphForms.Clear();

                    document = doc;

                    var form = ShowGraph(document.MainGraph);
                }

                OnDocumentOpened(openFileDialog.FileName);
                OnDocumentChanged();
                OnGraphChanged();
                OnSelectionChanged();

                return true;
            }
            else
            {
                return false;
            }
        }

        GraphEditorForm ShowGraph(Graph g)
        {
            GraphEditorForm editorForm;
            if (graphForms.TryGetValue(g, out var editor) && !editor.IsDisposed)
            {
                editorForm = editor;
            }
            else
            {
                editorForm = new GraphEditorForm();
                editorForm.Graph = g;
                editorForm.Text = g.Name;
                editorForm.Canvas.ResetHistory();

                editorForm.DocumentChanged += (s, e) => OnDocumentChanged();
                editorForm.GraphChanged += (s, e) => OnGraphChanged();
                editorForm.SelectionChanged += (s, e) => OnSelectionChanged();

                graphForms[g] = editorForm;
            }
            editorForm.Show(dockPanel, DockState.Document);

            return editorForm;
        }

        void NewDocument()
        {
            var ev = new DocumentClosingEventArgs();
            OnDocumentClosing(ev);

            if (ev.Cancel) return;

            foreach (var d in dockPanel.Documents.ToList())
            {
                var f = (GraphEditorForm)d;
                f.Close();
            }

            graphForms.Clear();

            document = new Document();
            document.Settings = new RenderingSettings();
            document.MainGraph = new Graph() { Name = Strings.MainGraph };
            document.Graphs.Add(document.MainGraph);

            var form = ShowGraph(document.MainGraph);
            form.AddNode(PointF.Empty, new Core.InputNode() { OutputType = Core.NodeType.Float3 } );
            form.AddNode(new PointF(200, 0), new Core.OutputNode());
            form.Canvas.ResetHistory();

            OnDocumentOpened(null);
        }
    }
}