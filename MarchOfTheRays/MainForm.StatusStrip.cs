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

namespace MarchOfTheRays
{
    partial class MainForm : Form
    {
        StatusStrip InitializeStatusStrip()
        {
            var statusStrip = new StatusStrip();
            var statusLabel = statusStrip.Items.Add("Ready.");

            StatusChange += (s, e) =>
            {
                statusLabel.Text = e;
            };

            return statusStrip;
        }
    }
}