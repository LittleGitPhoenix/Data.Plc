﻿#region LICENSE NOTICE
//! This file is subject to the terms and conditions defined in file 'LICENSE.md', which is part of this source code package.
#endregion

namespace Phoenix.Data.Plc;

/// <summary>
/// General information about a plc instance.
/// </summary>
public interface IPlcInformation
{
	/// <summary>
	/// The id of the plc.
	/// </summary>
	/// <remarks> May be used for identification and log purposes. </remarks>
	int Id { get; }

	/// <summary> The name of the plc. </summary>
	/// <remarks> May be used for identification and log purposes. </remarks>
	string Name { get; }
}