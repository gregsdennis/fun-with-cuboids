using System.Diagnostics;

namespace Cuboids.Core;

[DebuggerDisplay("{DebugString()}")]
public class CellNode : IEquatable<CellNode>
{
	private readonly Guid _id = Guid.NewGuid();

	private List<Cell>? _connections;
	private List<Cell>? _usedConnections;

	public Cell Cell { get; }

	public List<Cell> Connections => _connections ??= new();
	public List<Cell> UsedConnections => _usedConnections ??= new();

	public int Sequence { get; set; }

	public CellNode(Cell cell)
	{
		Cell = cell;
	}

	public CellNode(CellNode other)
	{
		Cell = other.Cell;
		_connections = other._connections == null ? null : new List<Cell>(other.Connections);
		_usedConnections = other._usedConnections == null ? null : new List<Cell>(other.UsedConnections);
		Sequence = other.Sequence;
	}

	public IEnumerable<Cell> GetOpenConnections()
	{
		if (_connections == null) return Cell.Neighbors.Select(x => x.Cell);

		return Cell.Neighbors
			.Where(cell => Connections.All(conn => conn.Id != cell.Cell.Id))
			.Select(x => x.Cell)
			.Except(UsedConnections);
	}

	public bool Equals(CellNode? other)
	{
		if (ReferenceEquals(null, other)) return false;
		if (ReferenceEquals(this, other)) return true;
		if (other.GetType() != GetType()) return false;
		return _id.Equals(other._id);
	}

	public override bool Equals(object? obj)
	{
		return Equals(obj as CellNode);
	}

	public override int GetHashCode()
	{
		return _id.GetHashCode();
	}

	public string DebugString()
	{
		return $"Id: {Cell.Id} / Connections: [{(_connections == null ? "(none)" : string.Join(", ", _connections.Select(x => x.Id)))}]";
	}
}