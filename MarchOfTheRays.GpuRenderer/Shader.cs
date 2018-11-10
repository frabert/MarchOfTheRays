using System;
using OpenGL;

namespace MarchOfTheRays.GpuRenderer
{
    class ShaderCompilationException : Exception
    {

    }

    class Shader : IDisposable
    {
        public Shader(ShaderType type, params string[] source)
        {
            Name = Gl.CreateShader(type);
            Gl.ShaderSource(Name, source);
            Gl.CompileShader(Name);

            Gl.GetShader(Name, ShaderParameterName.CompileStatus, out int status);
            if (status == Gl.FALSE) throw new ShaderCompilationException();
        }

        public uint Name { get; }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                }

                Gl.DeleteShader(Name);

                disposedValue = true;
            }
        }

        ~Shader()
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
