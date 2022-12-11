namespace Cuboids.Core;

/// <summary>
/// Cells are linked to another by the direction relative to
/// the origin cell (which is defined by the face that has the cell)
/// </summary>
public class CellLink
{
	public Cell Cell { get; }
	public Direction Direction { get; }

	public CellLink(Cell cell, Direction direction)
	{
		Cell = cell;
		Direction = direction;
	}
}