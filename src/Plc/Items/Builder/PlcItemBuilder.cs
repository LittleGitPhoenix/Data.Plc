using System.Collections.Generic;
using System.Text;

namespace Phoenix.Data.Plc.Items.Builder
{
	/// <summary>
	/// Interface for building <see cref="IPlcItem"/>s.
	/// </summary>
	public interface IPlcItemBuilder
	{
		/// <summary>
		/// Start building an <see cref="IPlcItem"/>.
		/// </summary>
		/// <param name="identifier"> The identifier of the item to build. </param>
		/// <returns></returns>
		ITypePlcItemConstructor Construct(string identifier = null);
	}
	
	/// <summary>
	/// Builder class for <see cref="IPlcItem"/>s.
	/// </summary>
	public class PlcItemBuilder : IPlcItemBuilder
	{
		/// <inheritdoc />
		public ITypePlcItemConstructor Construct(string identifier = null)
		{
			return new PlcItemConstructor(identifier);
		}
	}
}