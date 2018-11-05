using MarchOfTheRays.Properties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MarchOfTheRays
{
    partial class MainForm : Form
    {
        RenderForm PreviewForm { get; set; }

        void InitializeRendering()
        {
            Task previousPreviewTask = null;
            var tokenSource = new CancellationTokenSource();

            GraphChanged += (s, e) =>
            {
                if (Settings.Default.LivePreview)
                {
                    OnRenderPreview();
                }
            };

            async Task Render()
            {
                bool CheckCycles(Graph g)
                {
                    GraphEditorForm form = null;
                    graphForms.TryGetValue(g, out form);

                    if(form != null && !form.IsDisposed)
                    {
                        foreach(var val in form.Elements.Values)
                        {
                            val.Errored = false;
                        }
                    }

                    foreach(var onode in g.OutputNodes)
                    {
                        var cycles = Core.Compiler.CheckForCycles(onode, g.Nodes);
                        if (cycles.Count > 0)
                        {
                            if (form != null && !form.IsDisposed)
                            {
                                foreach (var kvp in form.Elements)
                                {
                                    kvp.Value.Errored = cycles.Contains(kvp.Key);
                                }
                            }
                            OnStatusChange(Strings.StatusCycleFound);
                            PreviewForm.Loading = false;
                            return false;
                        }
                    }

                    return true;
                }

                var renderer = new CpuRenderer.Renderer(
                        document.Settings.CameraPosition,
                        document.Settings.CameraTarget,
                        document.Settings.CameraUp,
                        document.Settings.MaximumIterations,
                        document.Settings.MaximumDistance,
                        document.Settings.Epsilon,
                        document.Settings.StepSize);

                PreviewForm.Loading = true;
                PreviewForm.Progress = 0;
                OnStatusChange(Strings.StatusSearchingCycles);
                foreach(var g in document.Graphs)
                {
                    if (!CheckCycles(g))
                    {
                        return;
                    }
                }

                Func<Vector3, float> func;

                try
                {
                    var newTokenSource = new CancellationTokenSource();

                    var param = Expression.Parameter(typeof(Vector3), "pos");
                    var body = document.MainGraph.OutputNodes[0].Compile(Core.NodeType.Float, new Dictionary<Core.INode, Expression>(), param);
                    var sb = new System.Text.StringBuilder();
                    var linq2glsl = new Linq2Glsl.GlslVisitor(sb);
                    linq2glsl.Visit(body);
                    var str = sb.ToString();
                    var lambda = Expression.Lambda<Func<Vector3, float>>(body, param);
                    func = lambda.Compile();

                    float totalProgress = 0;
                    float total = PreviewForm.ClientSize.Width * PreviewForm.ClientSize.Height;

                    var prog = new Progress<int>();
                    prog.ProgressChanged += (s1, e1) =>
                    {
                        totalProgress += e1;
                        PreviewForm.Progress = totalProgress / total;
                    };

                    OnStatusChange(Strings.StatusRendering);
                    var img = renderer.RenderImageAsync(PreviewForm.ClientSize.Width, PreviewForm.ClientSize.Height, func, 4, newTokenSource.Token, prog);

                    tokenSource = newTokenSource;

                    if (PreviewForm == null || PreviewForm.IsDisposed) return;

                    PreviewForm.Cursor = Cursors.WaitCursor;
                    var image = await img;
                    if (image != null) PreviewForm.BackgroundImage = image;
                    PreviewForm.Cursor = Cursors.Default;

                    PreviewForm.Loading = false;
                    OnStatusChange(Strings.StatusReady);
                }
                catch (Core.InvalidNodeException ex)
                {
                    Graph g = null;
                    foreach(var graph in document.Graphs)
                    {
                        if (graph.Nodes.Contains(ex.Node)) g = graph;
                    }

                    var editor = ShowGraph(g);
                    editor.Elements[ex.Node].Errored = true;
                    PreviewForm.Loading = false;
                    OnStatusChange(Strings.StatusInvalidNode);
                }
            }

            RenderPreview += async (s, e) =>
            {
                if (PreviewForm == null || PreviewForm.IsDisposed) return;

                if (previousPreviewTask != null)
                {
                    tokenSource.Cancel();
                    await previousPreviewTask;
                }

                previousPreviewTask = Render();
            };
        }

        void ShowPreviewForm()
        {
            if (PreviewForm != null && !PreviewForm.IsDisposed)
            {
                PreviewForm.Show();
                return;
            }

            PreviewForm = new RenderForm();
            PreviewForm.Owner = this;
            PreviewForm.FormBorderStyle = FormBorderStyle.SizableToolWindow;
            PreviewForm.ShowInTaskbar = false;
            PreviewForm.Text = Strings.PreviewFormTitle;
            PreviewForm.Size = Settings.Default.PreviewWindowSize;
            PreviewForm.BackgroundImageLayout = ImageLayout.Center;
            PreviewForm.NoActivation = true;
            PreviewForm.FormClosed += (s, e) =>
            {
                Settings.Default.PreviewWindowVisible = false;
            };
            PreviewForm.Resize += (s, e) =>
            {
                Settings.Default.PreviewWindowSize = PreviewForm.Size;
            };

            Settings.Default.PreviewWindowVisible = true;

            PreviewForm.Show();
        }
    }
}