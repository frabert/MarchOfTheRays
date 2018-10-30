using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarchOfTheRays.Core;
using System.Drawing;
using System.Numerics;

namespace MarchOfTheRays
{
    [Serializable]
    struct SerializableVector3
    {
        public float X, Y, Z;
    }

    [Serializable]
    public class RenderingSettings
    {
        public int MaximumIterations = 100;
        public float MaximumDistance = 20.0f;
        public float Epsilon = 0.001f;
        public float StepSize = 1.0f;

        SerializableVector3 camPos = new SerializableVector3() { X = 2, Y = 2, Z = 2 };
        SerializableVector3 camTarget = new SerializableVector3();
        SerializableVector3 upDir = new SerializableVector3() { Y = 1 };

        public Vector3 CameraPosition
        {
            get => new Vector3(camPos.X, camPos.Y, camPos.Z);
            set
            {
                camPos.X = value.X;
                camPos.Y = value.Y;
                camPos.Z = value.Z;
            }
        }

        public Vector3 CameraTarget
        {
            get => new Vector3(camTarget.X, camTarget.Y, camTarget.Z);
            set
            {
                camTarget.X = value.X;
                camTarget.Y = value.Y;
                camTarget.Z = value.Z;
            }
        }

        public Vector3 CameraUp
        {
            get => new Vector3(upDir.X, upDir.Y, upDir.Z);
            set
            {
                upDir.X = value.X;
                upDir.Y = value.Y;
                upDir.Z = value.Z;
            }
        }
    }

    [Serializable]
    public class Document
    {
        public List<(INode, PointF)> Nodes = new List<(INode, PointF)>();
        public RenderingSettings Settings = new RenderingSettings();
    }
}
