using System.Diagnostics;

namespace Cuboids.Core;

[DebuggerDisplay("Id: {Id}")]
public class Cell
{
	public short Id { get; }
	/// <summary>
	/// All neighbors to this cell when in the cuboid form
	/// </summary>
	public List<CellLink> Neighbors { get; } = new();

	public Cell(short id)
	{
		Id = id;
	}
}
