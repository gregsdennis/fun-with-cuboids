using System.Diagnostics;

namespace Cuboids.Core;

[DebuggerDisplay("{Length}x{Width}x{Height}; SA: {SurfaceArea}; Conn: {Connections}")]
public class Cuboid
{
	// face order is top, front, left, back, right, bottom.

	// cell order starts at top-right and counts horizontally then vertically
	// rotation to a new face spins on vertical axis then horizontal axis

	/*  Unfolding a 2x2x2 into a face-cross would yield:
		
  	         15 14
	         13 12

	  8  9    0  1  18 20
	 10 11    2  3  17 19

  	          4  5
	          6  7
		
  	         21 22
	         23 24

	 */

	public short Length { get; }
	public short Width { get; }
	public short Height { get; }

	public int SurfaceArea { get; }
	public int Volume { get; }
	public int Connections { get; }

	public Face Top { get; }
	public Face Front { get; }
	public Face Left { get; }
	public Face Back { get; }
	public Face Right { get; }
	public Face Bottom { get; }
	public Cell[] Cells { get; }

	public Cuboid(short length, short width, short height)
	{
		Length = length;
		Width = width;
		Height = height;

		Volume = length * width * height;
		SurfaceArea = 2 * (length * width + length * height + width * height);

		// build all of the faces
		// this creates neighbor relationships for these cells
		Top = new Face(0, width, length);
		Front = new Face((short)Top.Count, width, height);
		Left = new Face((short)(Top.Count + Front.Count), length, height);
		Back = new Face((short)(Top.Count + Front.Count + Left.Count), width, height);
		Right = new Face((short)(Top.Count + Front.Count + Left.Count + Back.Count), length, height);
		Bottom = new Face((short)(Top.Count + Front.Count + Left.Count + Back.Count + Right.Count), width, length);

		// stitch the faces together 
		// this creates neighbor relationships for the meeting edge cells
		Stitch(Top.Top, Direction.Up, Back.Top.Reverse(), Direction.Up);
		Stitch(Top.Left, Direction.Left, Left.Top, Direction.Up);
		Stitch(Top.Bottom, Direction.Down, Front.Top, Direction.Up);
		Stitch(Top.Right, Direction.Right, Right.Top.Reverse(), Direction.Up);
		Stitch(Front.Left, Direction.Left, Left.Right, Direction.Right);
		Stitch(Left.Left, Direction.Left, Back.Right, Direction.Right);
		Stitch(Back.Left, Direction.Left, Right.Right, Direction.Right);
		Stitch(Right.Left, Direction.Left, Front.Right, Direction.Right);
		Stitch(Front.Bottom, Direction.Down, Bottom.Top, Direction.Up);
		Stitch(Left.Bottom, Direction.Down, Bottom.Left.Reverse(), Direction.Left);
		Stitch(Back.Bottom, Direction.Down, Bottom.Bottom.Reverse(), Direction.Down);
		Stitch(Right.Bottom, Direction.Down, Bottom.Right, Direction.Right);

		Cells = Top.Concat(Front)
			.Concat(Left)
			.Concat(Back)
			.Concat(Right)
			.Concat(Bottom)
			.OrderBy(x => x.Id)
			.ToArray();

		Connections = Cells.Sum(x => x.Neighbors.Count) / 2;
	}

	private static void Stitch(IEnumerable<Cell> faceEdge1, Direction direction1, IEnumerable<Cell> faceEdge2, Direction direction2)
	{
		var zipped = faceEdge1.Zip(faceEdge2);
		foreach (var (cell1, cell2) in zipped)
		{
			cell1.Neighbors.Add(new CellLink(cell2, direction1));
			cell2.Neighbors.Add(new CellLink(cell1, direction2));
		}
	}
}