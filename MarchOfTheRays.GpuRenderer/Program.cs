using System;
using OpenGL;

namespace MarchOfTheRays.GpuRenderer
{
    class Program : IDisposable
    {
        public uint Name { get; }

        public Program(params Shader[] shaders)
        {
            Name = Gl.CreateProgram();
            foreach(var shader in shaders)
            {
                Gl.AttachShader(Name, shader.Name);
            }
            Gl.LinkProgram(Name);

            foreach(var shader in shaders)
            {
                Gl.DetachShader(Name, shader.Name);
            }
        }

        public void Use()
        {
            Gl.UseProgram(Name);
        }

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

                Gl.DeleteProgram(Name);

                disposedValue = true;
            }
        }
        
        ~Program() {
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
