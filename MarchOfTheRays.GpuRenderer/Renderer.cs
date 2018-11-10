using OpenGL;
using System;
using System.Globalization;
using System.Numerics;

namespace MarchOfTheRays.GpuRenderer
{
    public class Renderer : IDisposable
    {
        uint vbo, vao, ebo;
        Shader vertexShader;
        Program prog;
        float[] vertices =
        {
            -1, -1,
             1, -1,
            -1,  1,
             1,  1
        };

        uint[] indices =
        {
            0,1,2,3
        };

        Vector3Uniform cameraOrigin, cameraTarget, upDirection;

        int MAX_ITER = 100; // 100 is a safe number to use, it won't produce too many artifacts and still be quite fast
        float MAX_DIST = 20.0f; // Make sure you change this if you have objects farther than 20 units away from the camera
        float EPSILON = 0.001f; // At this distance we are close enough to the object that we have essentially hit it
        float STEP_SIZE = 1.0f;

        public Renderer()
        {
            vbo = Gl.GenBuffer();
            ebo = Gl.GenBuffer();
            vao = Gl.GenVertexArray();

            Gl.BindVertexArray(vao);

            Gl.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            Gl.BufferData(BufferTarget.ArrayBuffer, (uint)(sizeof(float) * vertices.Length), vertices, BufferUsage.StaticDraw);

            Gl.BindBuffer(BufferTarget.ElementArrayBuffer, ebo);
            Gl.BufferData(BufferTarget.ElementArrayBuffer, (uint)(sizeof(uint) * indices.Length), indices, BufferUsage.StaticDraw);

            Gl.VertexAttribPointer(0, 2, VertexAttribType.Float, false, 2 * sizeof(float), IntPtr.Zero);

            vertexShader = new Shader(ShaderType.VertexShader, @"
#version 330 core
layout (location = 0) in vec2 aPos;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, 0.0, 1.0);
}
");

            GenProgram();
        }

        void GenProgram()
        {
            var fragmentShader = new Shader(ShaderType.FragmentShader, $@"
#version 330 core
out vec4 FragColor;

uniform vec3 cameraOrigin;
uniform vec3 cameraTarget;
uniform vec3 upDirection;

float distFunc(vec3 pos)
{{
    return 
}}

void main()
{{
    vec3 cameraDir = normalize(cameraTarget - cameraOrigin);
    vec3 cameraRight = normalize(cross(upDirection, cameraOrigin));
    vec3 cameraUp = cross(cameraDir, cameraRight);

    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
}} 
");
            if (prog != null) prog.Dispose();
            prog = new Program(vertexShader, fragmentShader);
            prog.Use();
            cameraOrigin = new Vector3Uniform(prog, "cameraOrigin");
            cameraTarget = new Vector3Uniform(prog, "cameraTarget");
            upDirection = new Vector3Uniform(prog, "upDirection");
        }

        public Vector3 CameraOrigin
        {
            get => cameraOrigin.GetValue();
            set
            {
                cameraOrigin.SetValue(value);
            }
        }

        public Vector3 CameraTarget
        {
            get => cameraTarget.GetValue();
            set
            {
                cameraTarget.SetValue(value);
            }
        }

        public Vector3 UpDirection
        {
            get => upDirection.GetValue();
            set
            {
                upDirection.SetValue(value);
            }
        }

        public void Draw()
        {
            prog.Use();
            Gl.BindVertexArray(vao);
            Gl.DrawElements(PrimitiveType.TriangleStrip, 4, DrawElementsType.UnsignedInt, indices);
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    vertexShader.Dispose();
                }

                Gl.DeleteVertexArrays(vao);
                Gl.DeleteBuffers(vbo);

                disposedValue = true;
            }
        }

        ~Renderer()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(false);
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
