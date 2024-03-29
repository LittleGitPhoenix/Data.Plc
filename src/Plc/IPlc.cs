﻿#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc;

/// <summary>
/// Interface for all plc classes.
/// </summary>
public interface IPlc : IDisposable
{
	#region Events

	/// <summary>
	/// Raised if the connection to the plc has been established.
	/// </summary>
	event EventHandler<PlcConnectionState> Connected;

	/// <summary>
	/// Raised if the connection to the plc has been disconnected.
	/// </summary>
	event EventHandler<PlcConnectionState> Disconnected;

	/// <summary>
	/// Raised if the connection to the plc has been interrupted.
	/// </summary>
	event EventHandler<PlcConnectionState> Interrupted;

	#endregion

	#region Properties

	/// <summary>
	/// The id of the plc.
	/// </summary>
	int Id { get; }

	/// <summary> The name of the plc. </summary>
	string Name { get; }

	/// <summary> The current <see cref="PlcConnectionState"/>. </summary>
	PlcConnectionState ConnectionState { get; }

	#endregion

	#region Connection

	/// <summary>
	/// Establishes a connection to the plc.
	/// </summary>
	/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
	bool Connect();

	/// <summary>
	/// Disconnects the link to the plc.
	/// </summary>
	/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
	bool Disconnect();

	/// <summary>
	/// Re-establishes the link to the plc.
	/// </summary>
	/// <returns> <c>True</c> on success, otherwise <c>False</c>. </returns>
	bool Reconnect();

	#endregion

	#region Read

	/// <summary>
	/// Reads the value of the <paramref name="plcItems"/> from the plc.
	/// </summary>
	/// <param name="plcItems"> The <see cref="IPlcItem"/>s to read. </param>
	/// <param name="cancellationToken"> An optional <see cref="CancellationToken"/> for cancelling the read operation. </param>
	/// <returns> An awaitable task. </returns>
	/// <exception cref="ReadPlcException"> Thrown if an exception occurred while reading. </exception>
	/// <exception cref="DisposedReadPlcException"> Thrown if the <see cref="IPlc"/> used for reading has already been or is disposed while reading. </exception>
	/// <remarks> The order in which the items will be read, is not guaranteed. </remarks>
	Task ReadItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default);

	#endregion

	#region Write

	/// <summary>
	/// Writes the <paramref name="plcItems"/> to the plc.
	/// </summary>
	/// <param name="plcItems"> The <see cref="IPlcItem"/>s to write. </param>
	/// <param name="cancellationToken"> An optional <see cref="CancellationToken"/> for cancelling the write operation. </param>
	/// <returns> An awaitable task yielding <c>True</c> on success, otherwise <c>False</c>. </returns>
	/// <exception cref="WritePlcException"> Thrown if an exception occurred while writing. </exception>
	/// <exception cref="DisposedWritePlcException"> Thrown if the <see cref="IPlc"/> used for writing has already been or is disposed while reading. </exception>
	/// <remarks> The order in which the items will be written, is not guaranteed. </remarks>
	Task<bool> WriteItemsAsync(ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default);

	#endregion
}