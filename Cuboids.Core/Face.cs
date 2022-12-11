using System.Collections;

namespace Cuboids.Core;

public class Face : IEnumerable<Cell>
{
	public Cell[] Cells { get; }
	public int Count => Cells.Length;

	/// <summary>
	/// Gets the top edge of the face
	/// </summary>
	public Cell[] Top { get; }
	/// <summary>
	/// Gets the left edge of the face
	/// </summary>
	public Cell[] Left { get; }
	/// <summary>
	/// Gets the bottom edge of the face
	/// </summary>
	public Cell[] Bottom { get; }
	/// <summary>
	/// Gets the right edge of the face
	/// </summary>
	public Cell[] Right { get; }

	public Face(short start, short x, short y)
	{
		Cells = BuildFace(start, x, y);

		Top = Cells.Take(x).ToArray();
		Left = Cells.Where((_, i) => i % x == 0).ToArray();
		Bottom = Cells.TakeLast(x).ToArray();
		Right = Cells.Where((_, i) => i % x == x - 1).ToArray();
	}

	private static Cell[] BuildFace(int start, int x, int y)
	{
		var cells = Enumerable.Range(start, x * y).Select(id => new Cell((short)id)).ToArray();

		for (int j = 0; j < y; j++)
		{
			for (int i = 0; i < x; i++)
			{
				var index = i + j * x;
				var cell = cells[index];

				// up
				if (j != 0)
					cell.Neighbors.Add(new CellLink(cells[index - x], Direction.Up));
				// left
				if (i != 0)
					cell.Neighbors.Add(new CellLink(cells[index - 1], Direction.Left));
				// down
				if (j != y - 1)
					cell.Neighbors.Add(new CellLink(cells[index + x], Direction.Down));
				// right
				if (i != x - 1)
					cell.Neighbors.Add(new CellLink(cells[index + 1], Direction.Right));
			}
		}

		return cells;
	}

	public IEnumerator<Cell> GetEnumerator()
	{
		return ((IEnumerable<Cell>)Cells).GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}