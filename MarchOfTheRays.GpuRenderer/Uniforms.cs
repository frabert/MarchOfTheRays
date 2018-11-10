using System;
using System.Numerics;
using OpenGL;

namespace MarchOfTheRays.GpuRenderer
{
    abstract class Uniform<T>
    {
        protected Program prog;
        protected int location;
        
        public string Name { get; }

        public Uniform(Program prog, string name)
        {
            Name = name;
            location = Gl.GetUniformLocation(prog.Name, name);
        }

        public abstract T GetValue();
        public abstract void SetValue(T value);
    }

    class FloatUniform : Uniform<float>
    {
        public FloatUniform(Program prog, string name) : base(prog, name) { }

        public override float GetValue()
        {
            float[] vals = new float[1];
            Gl.GetUniform(prog.Name, location, vals);
            return vals[0];
        }

        public override void SetValue(float value)
        {
            Gl.Uniform1(location, value);
        }
    }

    class Vector3Uniform : Uniform<Vector3>
    {
        public Vector3Uniform(Program prog, string name) : base(prog, name) { }

        public override Vector3 GetValue()
        {
            float[] vals = new float[3];
            Gl.GetUniform(prog.Name, location, vals);
            return new Vector3(vals[0], vals[1], vals[2]);
        }

        public override void SetValue(Vector3 value)
        {
            Gl.Uniform3(location,, value.X, value.Y, value.Z);
        }
    }
}
