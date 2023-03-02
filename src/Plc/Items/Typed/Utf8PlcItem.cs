#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

using System.Text;

namespace Phoenix.Data.Plc.Items.Typed
{
	/// <summary>
	/// <see cref="IPlcItem"/> for <see cref="Encoding.UTF8"/> <see cref="string"/>s.
	/// </summary>
	public class Utf8PlcItem : TextPlcItem, IDeepCloneable<Utf8PlcItem>
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
		
		/// <inheritdoc />
		public Utf8PlcItem(ushort dataBlock, ushort position, ushort length, string identifier = default)
			: base(dataBlock, position, length, Encoding.UTF8, identifier) { }

		/// <inheritdoc />
		public Utf8PlcItem(ushort dataBlock, ushort position, string initialValue, string identifier = default)
			: base(dataBlock, position, initialValue, Encoding.UTF8, identifier) { }

		/// <inheritdoc />
		/// <summary>
		/// Constructor used by <see cref="DynamicUtf8PlcItem"/>.
		/// </summary>
		internal Utf8PlcItem(ushort dataBlock, ushort position, string initialValue, bool canChangeSize, string identifier = default)
			: base(dataBlock, position, initialValue, Encoding.UTF8, canChangeSize, identifier) { }

		#endregion

		#region Methods

		#region Clone

		/// <inheritdoc />
		public new Utf8PlcItem Clone() => this.Clone(null);

		/// <summary>
		/// Creates a deep copy of the current instance.
		/// </summary>
		/// <param name="identifier"> A new identifier for the clone. </param>
		/// <returns> A new <see cref="Utf8PlcItem"/>. </returns>
		public new Utf8PlcItem Clone(string identifier)
		{
			return new Utf8PlcItem(base.DataBlock, base.Position, this.Value, base.AutomaticallyAdaptSize, identifier);
		}

		#endregion

		#endregion
	}
}