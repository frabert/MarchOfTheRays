﻿using MarchOfTheRays.Properties;
using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using WeifenLuo.WinFormsUI.Docking;

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

        bool Save(string path)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Create))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, document);
                }

                OnDocumentSaved(path);
                return true;
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format(Strings.CannotWriteFileText, path), Strings.CannotWriteFileCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        bool SaveAs()
        {
            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = $"{Strings.MtrFile}|*.mtr";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return Save(saveFileDialog.FileName);
            }
            else
            {
                return false;
            }
        }

        void Open(string file)
        {
            try
            {
                using (var stream = File.OpenRead(file))
                {
                    var formatter = new BinaryFormatter();
                    var doc = (Document)formatter.Deserialize(stream);
                    doc.CleanOrphanGraphs();
                    doc.InitializeEvents();

                    foreach (var d in dockPanel.Documents.ToList())
                    {
                        var f = (GraphEditorForm)d;
                        f.Close();
                    }

                    graphForms.Clear();

                    document = doc;

                    var form = ShowGraph(document.MainGraph);
                }

                OnDocumentOpened(file);
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format(Strings.CannotOpenFileText, file), Strings.CannotOpenFileCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void Open()
        {
            var ev = new DocumentClosingEventArgs();
            OnDocumentClosing(ev);

            if (ev.Cancel) return;

            var openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = $"{Strings.MtrFile}|*.mtr";
            openFileDialog.Multiselect = false;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                Open(openFileDialog.FileName);
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

                editorForm.NodeOpened += (s, e) =>
                {
                    if (e is Core.ICompositeNode && document.Subgraphs.TryGetValue(e, out var graph))
                    {
                        ShowGraph(graph);
                    }
                };

                editorForm.GraphCreated += (s, e) =>
                {
                    document.Graphs.Add(e.graph);
                    document.Subgraphs[e.node] = e.graph;
                };

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
            form.AddNode(PointF.Empty, new Core.InputNode() { OutputType = Core.NodeType.Float3 });
            form.AddNode(new PointF(200, 0), new Core.OutputNode());
            form.Canvas.ResetHistory();

            OnDocumentOpened(null);
        }

        /*void ExportNode(Core.ICompositeNode node, string path)
        {
            var graph = document.Subgraphs[node];
            var exportData = new ExportedNode()
            {
                Graph = graph,
                Name = node.Name,
                Type = node is Core.CompositeUnaryNode ? NodeType.Unary : NodeType.Binary
            };

            switch (node)
            {
                case Core.CompositeUnaryNode unode:
                    {
                        exportData.InputNodes.Add(unode.Input);
                    }
                    break;
                case Core.CompositeBinaryNode bnode:
                    {
                        exportData.InputNodes.Add(bnode.LeftNode);
                        exportData.InputNodes.Add(bnode.RightNode);
                    }
                    break;
            }

            try
            {
                using (var stream = File.Open(path, FileMode.Create))
                {
                    var formatter = new BinaryFormatter();
                    formatter.Serialize(stream, exportData);
                }
            }
            catch (Exception)
            {
                MessageBox.Show(string.Format(Strings.CannotWriteFileText, path), Strings.CannotWriteFileCaption, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        void ImportNode(Stream stream)
        {
            var formatter = new BinaryFormatter();
            var node = (ExportedNode)formatter.Deserialize(stream);

            document.Graphs.Add(node.Graph);

            Core.INode importedNode;

            switch (node.Type)
            {
                case NodeType.Unary:
                    {
                        importedNode = new Core.CompositeUnaryNode()
                        {
                            Body = node.Graph.OutputNodes[0],
                            InputNode = node.InputNodes[0],
                            Name = node.Name
                        };
                    }
                    break;
                case NodeType.Binary:
                    {

                    }
                    break;
            }
        }*/
    }
}