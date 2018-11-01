﻿using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace MarchOfTheRays
{
    partial class MainForm
    {
        void Copy()
        {
            var clones = new List<(Core.INode, PointF)>();
            var originalsToClones = new Dictionary<Core.INode, Core.INode>();
            if (ActiveEditor == null) return;

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
                    clones.Add((clone, element.Location));
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

            Clipboard.SetData("MarchOfTheRays", clones);
            OnClipboardChanged();
        }

        void Paste()
        {
            if (ActiveEditor == null) return;

            var clipboardData = (List<(Core.INode, PointF)>)Clipboard.GetData("MarchOfTheRays");
            if (clipboardData == null || clipboardData.Count == 0) return;
            ActiveEditor.Canvas.SelectElements(_ => false);

            var nodes = ActiveEditor.AddNodes(clipboardData.Select(tuple => (tuple.Item2 + new SizeF(10, 10), tuple.Item1)));
            foreach (var node in nodes)
            {
                node.Selected = true;
            }

            var edges = new List<(Editor.NodeElement, Editor.NodeElement, int)>();

            foreach (var (node, pos) in clipboardData)
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
                }
            }
            ActiveEditor.Canvas.AddEdges(edges);
            ActiveEditor.Canvas.Center(clipboardData[0].Item2);
        }

        bool CanPaste
        {
            get
            {
                var clipboardData = (List<(Core.INode, PointF)>)Clipboard.GetData("MarchOfTheRays");
                return (clipboardData != null && clipboardData.Count != 0);
            }
        }
    }
}