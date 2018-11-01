using MarchOfTheRays.Properties;
using System.Windows.Forms;
using System.Linq;
using System.Drawing;
using System.IO;

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
        }

        void Save(string path)
        {
            var nodes = canvas.Elements.Select(x => (x.Tag as Core.INode, x.Location)).ToList();

            using (var stream = File.Open(path, FileMode.Create))
            {
                Serialize(stream, new Document() { Nodes = nodes, Settings = settings });
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
                    Deserialize(stream);
                }

                OnDocumentOpened(openFileDialog.FileName);

                return true;
            }
            else
            {
                return false;
            }
        }

        void NewDocument()
        {
            var ev = new DocumentClosingEventArgs();
            OnDocumentClosing(ev);

            if (ev.Cancel) return;

            elements.Clear();
            canvas.Clear();
            AddNode(PointF.Empty, new Core.InputNode() { OutputType = Core.NodeType.Float3 });

            outputNode = new Core.OutputNode();
            AddNode(new PointF(200, 0), outputNode);
            canvas.ResetHistory();
            settings = new RenderingSettings();
            OnDocumentOpened(null);
        }
    }
}