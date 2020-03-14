#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Phoenix.Data.Plc.Items;

namespace Phoenix.Data.Plc
{
	/// <summary>
	/// General extension class for <see cref="IPlc"/>.
	/// </summary>
	public static class PlcExtensions
	{
		#region Read

		/// <summary>
		/// Reads the value of the <paramref name="plcItem"/> from the plc.
		/// </summary>
		/// <param name="plc"> The extended <see cref="IPlcItem"/> instance. </param>
		/// <typeparam name="TValue"> The type of the <see cref="IPlcItem{TValue}.Value"/>. </typeparam>
		/// <param name="plcItem"> The <see cref="IPlcItem{TValue}"/> to read. </param>
		/// <param name="cancellationToken"> An optional <see cref="CancellationToken"/> for cancelling the read operation. </param>
		/// <returns> An awaitable task containing the result as <typeparamref name="TValue"/>. </returns>
		public static async Task<TValue> ReadItemAsync<TValue>(this IPlc plc, IPlcItem<TValue> plcItem, CancellationToken cancellationToken = default)
		{
			await plc.ReadItemAsync(plcItem as IPlcItem, cancellationToken);
			return plcItem.Value;
		}

		/// <summary>
		/// Reads the value of the <paramref name="plcItem"/> from the plc.
		/// </summary>
		/// <param name="plc"> The extended <see cref="IPlcItem"/> instance. </param>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> to read. </param>
		/// <param name="cancellationToken"> An optional <see cref="CancellationToken"/> for cancelling the read operation. </param>
		/// <returns> An awaitable task containing the result as <see cref="Byte"/> array. </returns>
		public static async Task<BitCollection> ReadItemAsync(this IPlc plc, IPlcItem plcItem, CancellationToken cancellationToken = default)
		{
			await plc.ReadItemsAsync(new[] { plcItem }, cancellationToken);
			return plcItem.Value;
		}

		#endregion

		#region Write

		/// <summary>
		/// Writes the <paramref name="plcItem"/> to the plc.
		/// </summary>
		/// <param name="plc"> The extended <see cref="IPlcItem"/> instance. </param>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> to write. </param>
		/// <param name="cancellationToken"> An optional <see cref="CancellationToken"/> for cancelling the write operation. </param>
		/// <returns> An awaitable task yielding <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static async Task<bool> WriteItemAsync(this IPlc plc, IPlcItem plcItem, CancellationToken cancellationToken = default)
			=> await plc.WriteItemsAsync(new[] { plcItem }, cancellationToken);

		/// <summary>
		/// Writes the <paramref name="plcItem"/> to the plc and afterwards reads and compares the written data for validation.
		/// </summary>
		/// <param name="plc"> The extended <see cref="IPlcItem"/> instance. </param>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> to write. </param>
		/// <param name="cancellationToken"> An optional <see cref="CancellationToken"/> for cancelling the write operation. </param>
		/// <returns> An awaitable task yielding <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static async Task<bool> WriteItemWithValidationAsync(this IPlc plc, IPlcItem plcItem, CancellationToken cancellationToken = default)
			=> await plc.WriteItemsWithValidationAsync(new[] { plcItem }, cancellationToken);

		/// <summary>
		/// Writes the <paramref name="plcItems"/> to the plc and afterwards reads and compares the written data for validation.
		/// </summary>
		/// <param name="plc"> The extended <see cref="IPlcItem"/> instance. </param>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s to write. </param>
		/// <param name="cancellationToken"> An optional <see cref="CancellationToken"/> for cancelling the write operation. </param>
		/// <returns> An awaitable task yielding <c>True</c> on success, otherwise <c>False</c>. </returns>
		public static async Task<bool> WriteItemsWithValidationAsync(this IPlc plc, ICollection<IPlcItem> plcItems, CancellationToken cancellationToken = default)
		{
			var success = await plc.WriteItemsAsync(plcItems, cancellationToken);

			// If the write failed somehow, then validation can be skipped even if it is requested.
			if (!success) return false;
			
			// Validate the write result.
			return await plc.ValidateWriteResultAsync(plcItems, cancellationToken);
		}

		/// <summary>
		/// Validates the write result of the <paramref name="plcItems"/> by reading and comparing their <see cref="IPlcItem.Value"/> with the target value.
		/// </summary>
		/// <param name="plc"> The extended <see cref="IPlcItem"/> instance. </param>
		/// <param name="plcItems"> The <see cref="IPlcItem"/>s whose write result should be validated. </param>
		/// <param name="cancellationToken"> An optional <see cref="CancellationToken"/> for cancelling the write operation. </param>
		/// <returns> An awaitable <see cref="Task"/> yielding <c>True</c> on success, otherwise <c>False</c>. </returns>
		private static async Task<bool> ValidateWriteResultAsync(this IPlc plc, ICollection<IPlcItem> plcItems, CancellationToken cancellationToken)
		{
			cancellationToken.ThrowIfCancellationRequested();

			// Create clones of the items, for later comparison of the target against the actual value.
			var clonedItems = plcItems
				.Select
				(
					plcItem =>
					{
						var clonedItem = plcItem.Clone();
						clonedItem.Value.SetAllBitsTo(false);
						return clonedItem;
					}
				)
				.ToArray()
				;
			
			await plc.ReadItemsAsync(clonedItems, cancellationToken);
			var targetValues = plcItems.Select(plcItem => plcItem.Value);
			var actualValues = clonedItems.Select(clonedItem => clonedItem.Value);
			return targetValues.SequenceEqual(actualValues);
		}

		#endregion
	}
}