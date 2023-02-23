using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Spark
{
	[XLua.LuaCallCSharp]
	public abstract class Disposable : IDisposable
	{
		// TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
		~Disposable()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(false);
		}

		// This code added to correctly implement the disposable pattern.
		public void Dispose()
		{
			// Do not change this code. Put cleanup code in Dispose(bool disposing) above.
			Dispose(true);
			// TODO: uncomment the following line if the finalizer is overridden above.
			GC.SuppressFinalize(this);
		}

		private bool m_Disposed = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!m_Disposed) {
				if (disposing) {
					// TODO: dispose managed state (managed objects).
					DisposeManagedObjects();
				}

				// TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
				// TODO: set large fields to null.
				DisposeUnmanagedObjects();

				m_Disposed = true;
			}
		}

		/// <summary>
		/// dispose managed state (managed objects).
		/// </summary>
		protected abstract void DisposeManagedObjects();

		/// <summary>
		/// free unmanaged resources (unmanaged objects)
		/// </summary>
		protected virtual void DisposeUnmanagedObjects()
		{

		}
	}
}
