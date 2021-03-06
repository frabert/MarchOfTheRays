﻿using System.ComponentModel;
using System.Windows.Forms;
using MarchOfTheRays.Properties;

namespace MarchOfTheRays
{
    public partial class RenderingSettingsDialog : Form
    {
        public RenderingSettingsDialog()
        {
            InitializeComponent();

            cameraGroup.Text = Strings.CameraSettings;
            lblPosition.Text = Strings.CameraPosition;
            lblTarget.Text = Strings.CameraTarget;
            lblUp.Text = Strings.CameraUpDirection;

            engineGroup.Text = Strings.RenderingEngineSettings;
            lblMaxIter.Text = Strings.MaxIterations;
            lblMaxDist.Text = Strings.MaxRenderDist;
            lblEps.Text = Strings.RenderingEpsilon;
            lblStep.Text = Strings.StepSize;
            apply.Text = Strings.Apply;
            cancel.Text = Strings.Cancel;

            Text = Strings.RenderSettingsTitle;
        }

        public RenderingSettings Value
        {
            get
            {
                return new RenderingSettings()
                {
                    CameraPosition = cameraPos.Value,
                    CameraTarget = cameraTarget.Value,
                    CameraUp = upDirection.Value,
                    Epsilon = (float)epsilon.Value,
                    MaximumDistance = (float)renderingDistance.Value,
                    MaximumIterations = (int)numIterations.Value,
                    StepSize = (float)stepSize.Value
                };
            }

            set
            {
                cameraPos.Value = value.CameraPosition;
                cameraTarget.Value = value.CameraTarget;
                upDirection.Value = value.CameraUp;
                epsilon.Value = (decimal)value.Epsilon;
                renderingDistance.Value = (decimal)value.MaximumDistance;
                numIterations.Value = value.MaximumIterations;
                stepSize.Value = (decimal)value.StepSize;
            }
        }

        bool IsValid => cameraPos.IsValid && cameraTarget.IsValid && upDirection.IsValid;

        private void apply_Click(object sender, System.EventArgs e)
        {
            if (IsValid)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                DialogResult = DialogResult.None;
            }
        }

        private void cancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if(DialogResult != DialogResult.Cancel) e.Cancel = !IsValid;
            base.OnClosing(e);
        }
    }
}
