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
            canvasContextMenu.Items.Add("Float constant", null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.FloatConstantNode());
            });

            canvasContextMenu.Items.Add("Float3 constant", null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float3ConstantNode());
            });

            canvasContextMenu.Items.Add("Binary operation", null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.BinaryNode());
            });

            canvasContextMenu.Items.Add("Unary operation", null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.UnaryNode());
            });

            canvasContextMenu.Items.Add("Custom node", null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                var graph = new Graph();
                graph.Name = "Custom node";

                var node = new Core.CompositeUnaryNode();
                node.Name = "Custom node";
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

            canvasContextMenu.Items.Add("Custom binary node", null, (s, e) =>
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