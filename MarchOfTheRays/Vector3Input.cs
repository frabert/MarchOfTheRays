using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Numerics;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.Design.Behavior;
using MarchOfTheRays.Properties;

namespace MarchOfTheRays
{
    [Designer(typeof(Vector3InputDesigner))] // Shows purple baselines in design view
    public partial class Vector3Input : UserControl
    {
        public Vector3Input()
        {
            InitializeComponent();

            txtX.TextChanged += (s, e) =>
            {
                ProcessInput(txtX, ref x, epX);
            };

            txtY.TextChanged += (s, e) =>
            {
                ProcessInput(txtY, ref y, epY);
            };

            txtZ.TextChanged += (s, e) =>
            {
                ProcessInput(txtZ, ref z, epZ);
            };

            Value = new Vector3();
        }

        static void ProcessInput(TextBox txt, ref float outValue, ErrorProvider ep)
        {
            if (float.TryParse(txt.Text, out var val))
            {
                outValue = val;
                ep.Clear();
            }
            else
            {
                ep.SetError(txt, Strings.NumberInputInvalid);
            }
        }

        float x, y, z;

        public Vector3 Value
        {
            get
            {
                return new Vector3(x, y, z);
            }
            set
            {
                x = value.X;
                y = value.Y;
                z = value.Z;

                txtX.Text = x.ToString();
                txtY.Text = y.ToString();
                txtZ.Text = z.ToString();
            }
        }

        public bool IsValid => string.IsNullOrEmpty(epX.GetError(txtX)) && string.IsNullOrEmpty(epY.GetError(txtY)) && string.IsNullOrEmpty(epZ.GetError(txtZ));
    }

    /*
     * From https://stackoverflow.com/questions/93541/baseline-snaplines-in-custom-winforms-controls
     */
    class Vector3InputDesigner : ControlDesigner
    {
        public override IList SnapLines
        {
            get
            {
                /* Code from above */
                IList snapLines = base.SnapLines;

                // *** This will need to be modified to match your user control
                Vector3Input control = Control as Vector3Input;
                if (control == null) { return snapLines; }

                // *** This will need to be modified to match the item in your user control
                // This is the control in your UC that you want SnapLines for the entire UC
                IDesigner designer = TypeDescriptor.CreateDesigner(
                    control.txtX, typeof(IDesigner));
                if (designer == null) { return snapLines; }

                // *** This will need to be modified to match the item in your user control
                designer.Initialize(control.txtX);

                using (designer)
                {
                    ControlDesigner boxDesigner = designer as ControlDesigner;
                    if (boxDesigner == null) { return snapLines; }

                    foreach (SnapLine line in boxDesigner.SnapLines)
                    {
                        if (line.SnapLineType == SnapLineType.Baseline)
                        {
                            // *** This will need to be modified to match the item in your user control
                            snapLines.Add(new SnapLine(SnapLineType.Baseline,
                                line.Offset + control.txtX.Top,
                                line.Filter, line.Priority));
                            break;
                        }
                    }
                }

                return snapLines;
            }

        }
    }
}