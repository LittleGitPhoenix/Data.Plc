using System;
using NUnit.Framework;

namespace Phoenix.Data.Plc.Implementation.Test
{
	public abstract class ImplementationConnectionTest<TPlc> : ImplementationTest<TPlc>
		where TPlc : IPlc
	{
		#region Delegates / Events
		#endregion

		#region Constants
		#endregion

		#region Fields
		#endregion

		#region Properties
		#endregion

		#region (De)Constructors

		protected ImplementationConnectionTest(TPlc plc) : base(_ => plc) { }

		protected ImplementationConnectionTest(Func<Data.Plc.Test.Data, TPlc> plcFactory) : base(plcFactory) { }

		#endregion

		#region Methods

		#region Tests

		[Test]
		public void Connect()
		{
			var connectedCounter = 0;
			var disconnectedCounter = 0;
			var interruptedCounter = 0;
			void PlcConnectionStateChanged(object sender, PlcConnectionState newConnectionState)
			{
				switch (newConnectionState)
				{
					case PlcConnectionState.Connected:
						connectedCounter++;
						break;
					case PlcConnectionState.Disconnected:
						disconnectedCounter++;
						break;
					case PlcConnectionState.Interrupted:
						interruptedCounter++;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newConnectionState), newConnectionState, null);
				}
			}

			var plc = base.Plc;
			connectedCounter = 0;
			disconnectedCounter = 0;
			interruptedCounter = 0;
			plc.Connected += PlcConnectionStateChanged;
			plc.Disconnected += PlcConnectionStateChanged;
			plc.Interrupted += PlcConnectionStateChanged;

			var success = false;

			success = plc.Disconnect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 0); // Should still be null, as the event shouldn't be raised because the initial state is already 'Disconnected'.
			Assert.True(connectedCounter == 0);
			Assert.True(interruptedCounter == 0);

			success = plc.Connect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 0);
			Assert.True(connectedCounter == 1);
			Assert.True(interruptedCounter == 0);

			success = plc.Disconnect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 1);
			Assert.True(connectedCounter == 1);
			Assert.True(interruptedCounter == 0);

			plc.Connected -= PlcConnectionStateChanged;
			plc.Disconnected -= PlcConnectionStateChanged;
			plc.Interrupted -= PlcConnectionStateChanged;
		}

		[Test]
		public void Disconnect()
		{
			var connectedCounter = 0;
			var disconnectedCounter = 0;
			var interruptedCounter = 0;
			void PlcConnectionStateChanged(object sender, PlcConnectionState newConnectionState)
			{
				switch (newConnectionState)
				{
					case PlcConnectionState.Connected:
						connectedCounter++;
						break;
					case PlcConnectionState.Disconnected:
						disconnectedCounter++;
						break;
					case PlcConnectionState.Interrupted:
						interruptedCounter++;
						break;
					default:
						throw new ArgumentOutOfRangeException(nameof(newConnectionState), newConnectionState, null);
				}
			}

			var plc = base.Plc;
			connectedCounter = 0;
			disconnectedCounter = 0;
			interruptedCounter = 0;
			plc.Connected += PlcConnectionStateChanged;
			plc.Disconnected += PlcConnectionStateChanged;
			plc.Interrupted += PlcConnectionStateChanged;

			var success = false;

			success = plc.Connect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 0);
			Assert.True(connectedCounter == 1);
			Assert.True(interruptedCounter == 0);

			success = plc.Disconnect();
			Assert.True(success);
			Assert.True(disconnectedCounter == 1);
			Assert.True(connectedCounter == 1);
			Assert.True(interruptedCounter == 0);

			plc.Connected -= PlcConnectionStateChanged;
			plc.Disconnected -= PlcConnectionStateChanged;
			plc.Interrupted -= PlcConnectionStateChanged;
		}

		#endregion

		#endregion
	}
}