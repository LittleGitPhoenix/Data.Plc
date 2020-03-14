#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion


using System;
using System.Collections;

namespace Phoenix.Data.Plc.Items
{
	internal class ChangeComparer : IEqualityComparer
	{
		/// <inheritdoc />
		public new bool Equals(object currentValue, object newValue)
		{
			if (Object.ReferenceEquals(currentValue, newValue)) return true;
			if (currentValue == null) return false;
			if (newValue == null) return false;

			if (currentValue is Array currentArray && newValue is Array newArray)
			{
				if (currentArray.Length != newArray.Length) return false;
				int index = 0;
				foreach (var item in currentArray)
				{
					try
					{
						if (!item.Equals(newArray.GetValue(index))) return false;
					}
					catch
					{
						return false;
					}

					index++;
				}

				return true;
			}
			else
			{
				// Directly compare the values via their equals method.
				return currentValue.Equals(newValue);
			}
		}

		/// <inheritdoc />
		public int GetHashCode(object @object)
			=> @object.GetHashCode();
	}
}