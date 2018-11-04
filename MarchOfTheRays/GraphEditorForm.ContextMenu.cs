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
    partial class GraphEditorForm : DockContent
    {
        void InitializeContextMenu()
        {
            var canvasContextMenu = new ContextMenuStrip();
            var expressions = new ToolStripMenuItem(Strings.Expressions);
            var primitives = new ToolStripMenuItem(Strings.Primitives);
            canvasContextMenu.Items.Add(expressions);
            canvasContextMenu.Items.Add(primitives);


            expressions.DropDownItems.Add(Strings.FloatConstant, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.FloatConstantNode());
            });

            expressions.DropDownItems.Add(Strings.Float3Constant, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float3ConstantNode());
            });

            expressions.DropDownItems.Add(Strings.Float3Constructor, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float3Constructor());
            });

            expressions.DropDownItems.Add(Strings.BinaryOperation, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.BinaryNode());
            });

            expressions.DropDownItems.Add(Strings.UnaryOperation, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.UnaryNode());
            });

            canvasContextMenu.Items.Add(Strings.InsertCustomNode, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                var graph = new Graph();
                graph.Name = Strings.CustomNode;

                var node = new Core.CompositeUnaryNode();
                node.Name = Strings.CustomNode;
                node.NameChanged += (s1, e1) => graph.Name = node.Name;

                node.Body = new Core.OutputNode();

                graph.Nodes.Add(node.Body);
                graph.Nodes.Add(node.InputNode);
                graph.OutputNodes.Add(node.Body);
                graph.NodePositions[node.InputNode] = PointF.Empty;
                graph.NodePositions[node.Body] = new PointF(200, 0);

                OnGraphCreated(node, graph);

                AddNode(worldCoords, node);
            });

            canvasContextMenu.Items.Add(Strings.InsertCustomBinaryNode, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                var graph = new Graph();
                graph.Name = "Custom binary node";

                var node = new Core.CompositeBinaryNode();
                node.Name = "Custom binary node";
                node.NameChanged += (s1, e1) => graph.Name = node.Name;

                node.Body = new Core.OutputNode();

                graph.Nodes.Add(node.Body);
                graph.Nodes.Add(node.LeftNode);
                graph.Nodes.Add(node.RightNode);
                graph.OutputNodes.Add(node.Body);
                graph.NodePositions[node.LeftNode] = PointF.Empty;
                graph.NodePositions[node.RightNode] = new PointF(0, 100);
                graph.NodePositions[node.Body] = new PointF(200, 50);

                OnGraphCreated(node, graph);

                AddNode(worldCoords, node);
            });

            Canvas.ContextMenuStrip = canvasContextMenu;
        }
    }
}