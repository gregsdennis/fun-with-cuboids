using System.Text;

namespace Cuboids.Core;

public class Net
{
	private readonly Dictionary<short, CellNode> _lookup;

	/// <summary>
	/// The planar grid for the cells
	/// </summary>
	public (short id, Rotation rotation)[,] Layout { get; }

	public Net(Cell initialCell)
	{
		_lookup = new();

		var node = new CellNode(initialCell);
		_lookup[initialCell.Id] = node;
		Layout = new[,] { { (initialCell.Id, Rotation.Zero) } };
	}

	public Net(Net basis, Cell newCell, short location)
	{
		_lookup = basis._lookup.Values.ToDictionary(x => x.Cell.Id, x => new CellNode(x));

		var target = _lookup[location];

		Add(new(newCell), target);
		Layout = BuildLayout(basis.Layout, newCell, target.Cell);
	}

	/// <summary>
	/// Gets the collection of unmatched connections that exist
	/// along the perimeter of the layout.
	/// </summary>
	public IEnumerable<(CellNode node, Cell conn)> GetOpenConnections()
	{
		return _lookup.Values.SelectMany(
			node => node.GetOpenConnections()
				.Select(conn => (node, conn))
		);
	}

	/// <summary>
	/// Adds a cell to the net.
	/// </summary>
	public void Add(CellNode newNode, CellNode location)
	{
		newNode.Sequence = _lookup.Count;

		_lookup[newNode.Cell.Id] = newNode;

		Link(location, newNode);
		CloseLoops(newNode, location);
	}

	/// <summary>
	/// Closes any 2x2 loops.  This helps reduce the number of open connections.
	/// </summary>
	private void CloseLoops(CellNode newNode, CellNode location)
	{
		IEnumerable<CellNode> GetConnectedCellNodes(IEnumerable<Cell> cells)
		{
			return cells.Select(x => _lookup.TryGetValue(x.Id, out var c) ? c : null!)
				.Where(x => x != null!);
		}

		var firstConnections = GetConnectedCellNodes(location.Connections)
			.Where(x => x.Cell.Id != newNode.Cell.Id);
		var secondConnections = firstConnections
			.SelectMany(x => GetConnectedCellNodes(x.Connections))
			.Distinct()
			.Where(x => x.Cell.Id != location.Cell.Id)
			.ToList();

		var neighborIds = newNode.Cell.Neighbors.Select(x => x.Cell.Id).ToArray();
		foreach (var node in secondConnections.Where(x => neighborIds.Contains(x.Cell.Id)))
		{
			Link(newNode, node);
		}

	}

	private static void Link(CellNode newNode, CellNode location)
	{
		newNode.Connections.Add(location.Cell);
		location.Connections.Add(newNode.Cell);
	}

	private static (short id, Rotation rotation)[,] BuildLayout((short id, Rotation rotation)[,] reference, Cell newCell, Cell target)
	{
		int rowForTarget = -1, colForTarget = 0;
		int rows = reference.GetLength(0);
		int cols = reference.GetLength(1);

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				if (reference[i, j].id == target.Id)
				{
					rowForTarget = i;
					colForTarget = j;
				}
			}
		}
		if (rowForTarget == -1)
			throw new InvalidOperationException($"Could not find {target.Id} in layout");

		var linkFromNew = newCell.Neighbors.Single(x => x.Cell.Id == target.Id);
		var linkFromTarget = target.Neighbors.Single(x => x.Cell.Id == newCell.Id);

		int rowForNewCell = rowForTarget, colForNewCell = colForTarget;
		(short id, Rotation rotation)[,]? newLayout;
		var targetRotation = reference[rowForTarget, colForTarget].rotation;
		var directionToAdd = linkFromTarget.Direction.Rotate(targetRotation);
		if (directionToAdd == Direction.Up && rowForTarget == 0)
		{
			// need new first row
			newLayout = new (short id, Rotation rotation)[rows+1, cols];
			for (int i = 0; i < cols; i++)
			{
				newLayout[0, i] = (-1, Rotation.Zero);
			}
			Array.Copy(reference, 0, newLayout, cols, reference.Length);

			rowForTarget++;
		}
		else if (directionToAdd == Direction.Down && rowForTarget == rows-1)
		{
			// need new last row
			newLayout = new (short id, Rotation rotation)[rows+1, cols];
			for (int i = 0; i < cols; i++)
			{
				newLayout[rows, i] = (-1, Rotation.Zero);
			}
			Array.Copy(reference, newLayout, reference.Length);

			rowForNewCell++;
		}
		else if (directionToAdd == Direction.Left && colForTarget == 0)
		{
			// need new first column
			newLayout = new (short id, Rotation rotation)[rows, cols+1];
			for (int i = 0; i < rows; i++)
			{
				newLayout[i, 0] = (-1, Rotation.Zero);
				for (int j = 0; j < cols; j++)
				{
					newLayout[i, j + 1] = reference[i, j];
				}
			}

			colForTarget++;
		}
		else if (directionToAdd == Direction.Right && colForTarget == cols-1)
		{
			// need new last column
			newLayout = new (short id, Rotation rotation)[rows, cols+1];
			for (int i = 0; i < rows; i++)
			{
				for (int j = 0; j < cols; j++)
				{
					newLayout[i, j] = reference[i, j];
				}
				newLayout[i, cols] = (-1, Rotation.Zero);
			}

			colForNewCell++;
		}
		else
		{
			// no expansion, just add the new cell
			switch (directionToAdd)
			{
				case Direction.Up:
					rowForNewCell--;
					break;
				case Direction.Left:
					colForNewCell--;
					break;
				case Direction.Down:
					rowForNewCell++;
					break;
				case Direction.Right:
					colForNewCell++;
					break;
			}
			newLayout = new (short id, Rotation rotation)[rows, cols];
			Array.Copy(reference, newLayout, reference.Length);
		}

		// add the new cell

		var newCellRotation = linkFromTarget.Direction.GetRotation(linkFromNew.Direction).Compound(targetRotation);
		var current = newLayout[rowForNewCell, colForNewCell];
		if (current.id != -1)
			// should only happen if something goes really bad
			throw new Exception("Collision");
		newLayout[rowForNewCell, colForNewCell] = (newCell.Id, newCellRotation);

		return newLayout;
	}

	private const string Blank = "                                                            ";

	/// <summary>
	/// Textual representation of the layout (just IDs, no rotation info)
	/// Optimized for direct console writing
	/// </summary>
	public string Output()
	{
		var sb = new StringBuilder();
		var rows = Layout.GetLength(0);
		var cols = Layout.GetLength(1);
		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				var cell = Layout[i, j].id == -1 ? "   " : Layout[i, j].id.ToString().PadLeft(3);
				sb.Append(cell);
			}

			sb.Append(" ".PadRight(60 - cols * 3));
			sb.AppendLine();
		}

		for (int i = rows; i < 18; i++)
		{
			sb.AppendLine(Blank);
		}

		return sb.ToString();
	}
}
