#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;

namespace Phoenix.Data.Plc.Items
{
	/// <summary>
	/// Event arguments used by a typed <see cref="IPlcItem{TValue}"/>.
	/// </summary>
	/// <typeparam name="TValue"> The type of the plc items value. </typeparam>
	public class PlcItemChangedEventArgs<TValue> : EventArgs
	{
		/// <summary> The <see cref="IPlcItem"/>. </summary>
		/// <remarks> Do not use the generic version of the plc item here, to prevent access to its value. </remarks>
		public IPlcItem PlcItem { get; }

		/// <summary> The identifier of the plc item. </summary>
		public string Identifier => this.PlcItem.Identifier;

		/// <summary> The new value of the plc item. </summary>
		/// <remarks> Do not use the value of the <see cref="PlcItem"/> itself, as this one may have been changed in the meantime again. Therefore the <see cref="PlcItem"/> is not the generic version to better prevent this. </remarks>
		public TValue NewValue { get; }

		/// <summary> The old value of the plc item. </summary>
		public TValue OldValue { get; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="plcItem"> The <see cref="IPlcItem"/> that changed. </param>
		/// <param name="oldValue"> The old value of the plc item. </param>
		/// <param name="newValue"> The new value of the plc item. </param>
		public PlcItemChangedEventArgs(IPlcItem plcItem, TValue oldValue, TValue newValue)
		{
			this.PlcItem = plcItem;
			this.OldValue = oldValue;
			this.NewValue = newValue;
		}

		/// <summary> Returns a string that represents the current object. </summary>
		public override string ToString() => $"[<{this.GetType().Name}> :: PlcItem: {this.PlcItem} | OldValue: {this.OldValue} | NewValue: {this.NewValue}]";
	}
}