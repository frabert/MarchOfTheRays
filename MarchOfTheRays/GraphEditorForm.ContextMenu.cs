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
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.FloatConstantNode());
            });

            canvasContextMenu.Items.Add("Float3 constant", null, (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.Float3ConstantNode());
            });

            canvasContextMenu.Items.Add("Binary operation", null, (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.BinaryNode());
            });

            canvasContextMenu.Items.Add("Unary operation", null, (s, e) =>
            {
                var controlCoords = canvas.PointToClient(Cursor.Position);
                var worldCoords = canvas.GetWorldCoordinates(controlCoords);

                AddNode(worldCoords, new Core.UnaryNode());
            });

            Canvas.ContextMenuStrip = canvasContextMenu;
        }
    }
}