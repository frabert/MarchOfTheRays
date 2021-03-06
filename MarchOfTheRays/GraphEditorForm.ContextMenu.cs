﻿using MarchOfTheRays.Properties;
using System.Drawing;
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
            var solidOps = new ToolStripMenuItem(Strings.SolidOperations);
            var rotations = new ToolStripMenuItem(Strings.Rotations);

            canvasContextMenu.Items.Add(expressions);
            canvasContextMenu.Items.Add(primitives);
            canvasContextMenu.Items.Add(solidOps);
            canvasContextMenu.Items.Add(rotations);

            primitives.DropDownItems.Add(Strings.Sphere, null, (s, e) =>
            {
                OnNodeImported(Resources.Sphere, Strings.Sphere);
            });

            primitives.DropDownItems.Add(Strings.Box, null, (s, e) =>
            {
                OnNodeImported(Resources.Box, Strings.Box);
            });

            primitives.DropDownItems.Add(Strings.Torus, null, (s, e) =>
            {
                OnNodeImported(Resources.Torus, Strings.Torus);
            });

            primitives.DropDownItems.Add(Strings.Plane, null, (s, e) =>
            {
                OnNodeImported(Resources.Plane, Strings.Plane);
            });

            solidOps.DropDownItems.Add(Strings.Union, null, (s, e) =>
            {
                OnNodeImported(Resources.Union, Strings.Union);
            });

            solidOps.DropDownItems.Add(Strings.Intersection, null, (s, e) =>
            {
                OnNodeImported(Resources.Intersection, Strings.Intersection);
            });

            solidOps.DropDownItems.Add(Strings.Subtraction, null, (s, e) =>
            {
                OnNodeImported(Resources.Subtraction, Strings.Subtraction);
            });

            rotations.DropDownItems.Add(Strings.AroundX, null, (s, e) => {
                OnNodeImported(Resources.x_rotation, Strings.AroundX);
            });

            rotations.DropDownItems.Add(Strings.AroundY, null, (s, e) => {
                OnNodeImported(Resources.y_rotation, Strings.AroundY);
            });

            rotations.DropDownItems.Add(Strings.AroundZ, null, (s, e) => {
                OnNodeImported(Resources.z_rotation, Strings.AroundZ);
            });

            expressions.DropDownItems.Add(Strings.FloatConstant, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.FloatConstantNode());
            });

            expressions.DropDownItems.Add(Strings.Float2Constant, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float2ConstantNode());
            });

            expressions.DropDownItems.Add(Strings.Float3Constant, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float3ConstantNode());
            });

            expressions.DropDownItems.Add(Strings.Float4Constant, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float4ConstantNode());
            });

            expressions.DropDownItems.Add(Strings.Float2Constructor, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float2Constructor());
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

                AddNode(worldCoords, new Core.BinaryOperationNode());
            });

            expressions.DropDownItems.Add(Strings.UnaryOperation, null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.UnaryOperationNode());
            });

            expressions.DropDownItems.Add("Swizzle", null, (s, e) =>
            {
                var controlCoords = Canvas.PointToClient(Cursor.Position);
                var worldCoords = Canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.SwizzleNode());
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
                graph.Name = Strings.CustomNode;

                var node = new Core.CompositeBinaryNode();
                node.Name = Strings.CustomNode;
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