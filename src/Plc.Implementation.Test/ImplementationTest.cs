using System;
using System.Threading.Tasks;
using NUnit.Framework;

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

#if NET45
		protected static Task CompletedTask = Task.FromResult(false);
#endif

		private readonly object _lock;

		#endregion

		#region Properties

		/// <summary> The main <see cref="IPlc"/> object used for testing. </summary>
		protected TPlc Plc { get; }

		/// <summary> This must be an identical <see cref="Plc"/> object, but not the same instance. </summary>
		protected TPlc IdenticalPlc { get; }

		protected Data.Plc.Test.Data Data { get; }

		#endregion

		#region (De)Constructors

		static ImplementationTest()
		{
			// Change the default logger.
			Phoenix.Data.Plc.Logging.LogManager.LogAllReadAndWriteOperations = true;
			Phoenix.Data.Plc.Logging.LogManager.LoggerFactory = () => new Phoenix.Data.Plc.Logging.TraceLogger();
		}

		protected ImplementationTest(TPlc plc, TPlc identicalPlc)
		{
			// Save parameters.
			this.Plc = plc;
			this.IdenticalPlc = identicalPlc;

			// Initialize fields.
			_lock = new object();
			this.Data = new Data.Plc.Test.Data();
		}

		protected ImplementationTest(Func<Data.Plc.Test.Data, TPlc> plcFactory)
		{
			// Save parameters.

			// Initialize fields.
			_lock = new object();
			this.Data = new Data.Plc.Test.Data();
			this.Plc = plcFactory.Invoke(this.Data);
			this.IdenticalPlc = plcFactory.Invoke(this.Data);
		}

		#endregion

		#region Methods


		#region Setup

		[SetUp]
		public void Init()
		{
			this.CheckConnectivity();
		}

		/// <summary>
		/// Override this method to check connectivity with depended hardware. This may make an <see cref="Assert.Ignore()"/> to prevent the test from being run.
		/// </summary>
		protected virtual void CheckConnectivity() { }

		#endregion

		#region Execution

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

		#endregion

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