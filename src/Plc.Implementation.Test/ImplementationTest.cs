using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Phoenix.Data.Plc.Test;

namespace Phoenix.Data.Plc.Implementation.Test
{
	public abstract class ImplementationTest<TPlc> : IDisposable
		where TPlc : IPlc
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields

		private readonly object _lock;

		#endregion

		#region Properties

		protected TPlc Plc { get; }

		protected Data.Plc.Test.Data Data { get; }

		#endregion

		#region (De)Constructors

		protected ImplementationTest(TPlc plc) : this(_ => plc) { }

		protected ImplementationTest(Func<Data.Plc.Test.Data, TPlc> plcFactory)
		{
			// Save parameters.

			// Initialize fields.
			_lock = new object();
			this.Data = new Data.Plc.Test.Data();
			this.Plc = plcFactory.Invoke(this.Data);
		}

		#endregion

		#region Methods

		/// <summary>
		/// Connects to the <see cref="Plc"/>, executes the <paramref name="callback"/> and finally closes the connection again.
		/// </summary>
		/// <param name="callback"> The test callback to execute. </param>
		protected void ExecuteTest(Func<TPlc, Task> callback)
		{
			lock (_lock)
			{
				try
				{
					this.Plc.Connect();
					callback.Invoke(this.Plc).Wait();
				}
				finally
				{
					this.Plc.Disconnect();
				}
			}
		}

		#region Implementation of IDisposable

		/// <inheritdoc />
		public void Dispose()
		{
			this.Plc.Dispose();
		}

		#endregion

		#endregion
	}
}