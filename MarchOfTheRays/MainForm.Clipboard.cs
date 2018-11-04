using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MarchOfTheRays
{
    partial class MainForm
    {
        [Serializable]
        class ClipboardContents
        {
            public HashSet<Core.INode> Nodes = new HashSet<Core.INode>();
            public Dictionary<Core.INode, PointF> Locations = new Dictionary<Core.INode, PointF>();
            public HashSet<Graph> Graphs = new HashSet<Graph>();
            public Dictionary<Core.INode, Graph> Subgraphs = new Dictionary<Core.INode, Graph>();
        }

        void Copy()
        {
            var contents = new ClipboardContents();

            var originalsToClones = new Dictionary<Core.INode, Core.INode>();
            if (ActiveEditor == null) return;

            void CloneGraph(Core.INode parent, Graph originalGraph)
            {
                if (contents.Graphs.Contains(originalGraph)) return;

                var clonedGraph = new Graph
                {
                    Name = originalGraph.Name
                };

                var clonedNodes = new Dictionary<Core.INode, Core.INode>();

                Core.INode CloneNode(Core.INode node)
                {
                    if (clonedNodes.TryGetValue(node, out var clone)) return clone;
                    else
                    {
                        var clonedNode = (Core.INode)node.Clone();
                        clonedNodes[node] = clonedNode;
                        switch(clonedNode)
                        {
                            case Core.IUnaryNode u:
                                {
                                    if (u.Input == null) break;
                                    u.Input = CloneNode(u.Input);
                                }
                                break;
                            case Core.IBinaryNode b:
                                {
                                    if (b.Left != null) b.Left = CloneNode(b.Left);
                                    if (b.Right != null) b.Right = CloneNode(b.Right);
                                }
                                break;
                            case Core.INAryNode n:
                                {
                                    for(int i = 0; i < n.InputCount; i++)
                                    {
                                        if(n.GetInput(i) != null)
                                        {
                                            n.SetInput(i, CloneNode(n.GetInput(i)));
                                        }
                                    }
                                }
                                break;
                        }

                        return clonedNode;
                    }
                }

                foreach(var node in originalGraph.Nodes)
                {
                    var clone = CloneNode(node);
                    clonedGraph.Nodes.Add(clone);
                    clonedGraph.NodePositions[clone] = originalGraph.NodePositions[node];
                }

                foreach(var onode in originalGraph.OutputNodes)
                {
                    clonedGraph.OutputNodes.Add((Core.OutputNode)CloneNode(onode));
                }

                contents.Graphs.Add(clonedGraph);
                contents.Subgraphs[parent] = clonedGraph;
            }

            Core.INode CloneElement(Editor.NodeElement element)
            {
                if (element.Tag is Core.InputNode) return null;

                var original = (Core.INode)element.Tag;
                if (originalsToClones.TryGetValue(original, out var clone))
                {
                    return clone;
                }
                else
                {
                    clone = (Core.INode)original.Clone();
                    originalsToClones.Add(original, clone);
                    contents.Nodes.Add(clone);
                    contents.Locations[clone] = element.Location;
                    switch (clone)
                    {
                        case Core.IUnaryNode n:
                            {
                                if (n.Input == null) break;
                                var inputNode = ActiveEditor.Elements[n.Input];
                                n.Input = inputNode.Selected ? CloneElement(inputNode) : null;
                            }
                            break;
                        case Core.IBinaryNode n:
                            {
                                if (n.Left != null)
                                {
                                    var leftNode = ActiveEditor.Elements[n.Left];
                                    n.Left = leftNode.Selected ? CloneElement(leftNode) : null;
                                }

                                if (n.Right != null)
                                {
                                    var rightNode = ActiveEditor.Elements[n.Right];
                                    n.Right = rightNode.Selected ? CloneElement(rightNode) : null;
                                }
                            }
                            break;
                        case Core.INAryNode n:
                            {
                                for(int i = 0; i < n.InputCount; i++)
                                {
                                    var inp = n.GetInput(i);
                                    if(inp != null)
                                    {
                                        var node = ActiveEditor.Elements[inp];
                                        n.SetInput(i, node.Selected ? CloneElement(node) : null);
                                    }
                                }
                            }
                            break;
                    }

                    if(clone is Core.ICompositeNode)
                    {
                        CloneGraph(clone, document.Subgraphs[original]);
                    }

                    return clone;
                }
            }

            foreach (var selectedElem in ActiveEditor.Canvas.SelectedElements)
            {
                if (selectedElem.Tag is Core.OutputNode) continue;
                if (selectedElem.Tag is Core.InputNode) continue;
                CloneElement(selectedElem);
            }

            Clipboard.SetData("MarchOfTheRays", contents);
            OnClipboardChanged();
        }

        void Paste()
        {
            if (ActiveEditor == null) return;

            var clipboardData = (ClipboardContents)Clipboard.GetData("MarchOfTheRays");
            if (clipboardData == null || clipboardData.Nodes.Count == 0) return;
            ActiveEditor.Canvas.SelectElements(_ => false);

            foreach(var graph in clipboardData.Graphs)
            {
                document.Graphs.Add(graph);
            }

            foreach(var kvp in clipboardData.Subgraphs)
            {
                document.Subgraphs[kvp.Key] = kvp.Value;
            }

            var nodes = ActiveEditor.AddNodes(clipboardData.Nodes.Select(node => (clipboardData.Locations[node] + new SizeF(10, 10), node)));
            foreach (var node in nodes)
            {
                node.Selected = true;
            }

            var edges = new List<(Editor.NodeElement, Editor.NodeElement, int)>();

            foreach (var node in clipboardData.Nodes)
            {
                var dest = ActiveEditor.Elements[node];
                switch (node)
                {
                    case Core.IUnaryNode n:
                        if (n.Input != null)
                        {
                            var source = ActiveEditor.Elements[n.Input];
                            edges.Add((source, dest, 0));
                        }
                        break;
                    case Core.IBinaryNode n:
                        if (n.Left != null)
                        {
                            var source = ActiveEditor.Elements[n.Left];
                            edges.Add((source, dest, 0));
                        }
                        if (n.Right != null)
                        {
                            var source = ActiveEditor.Elements[n.Right];
                            edges.Add((source, dest, 1));
                        }
                        break;
                    case Core.INAryNode n:
                        {
                            for(int i = 0; i < n.InputCount; i++)
                            {
                                var inp = n.GetInput(i);
                                if(inp != null)
                                {
                                    var source = ActiveEditor.Elements[inp];
                                    edges.Add((source, dest, i));
                                }
                            }
                        }
                        break;
                }
            }
            ActiveEditor.Canvas.AddEdges(edges);
            //ActiveEditor.Canvas.Center(clipboardData[0].Item2);
        }

        bool CanPaste
        {
            get
            {
                var clipboardData = (ClipboardContents)Clipboard.GetData("MarchOfTheRays");
                return (clipboardData != null && clipboardData.Nodes.Count != 0);
            }
        }
    }
}