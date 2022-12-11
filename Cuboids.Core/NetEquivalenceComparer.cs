namespace Cuboids.Core;

/// <summary>
/// Compares two nets by attempting to match their layouts, including rotations and mirroring.
/// </summary>
public record NetEquivalenceComparer : IEqualityComparer<Net>
{
	public static NetEquivalenceComparer Instance { get; } = new();

	private NetEquivalenceComparer()
	{
	}

#pragma warning disable CS8767
	public bool Equals(Net a, Net b)
#pragma warning restore CS8767
	{
		var aRows = a.Layout.GetLength(0);
		var aCols = a.Layout.GetLength(1);
		var bRows = b.Layout.GetLength(0);
		var bCols = b.Layout.GetLength(1);

		if (aRows == bRows && aCols == bCols)
			return CheckArrays(a.Layout, b.Layout);

		if (aRows == bCols && aCols == bRows)
			return CheckRotatedArrays(a.Layout, b.Layout);

		return false;
	}

	/// <summary>
	/// Checks arrays that have the same dimensions
	/// </summary>
	private static bool CheckArrays((short id, Rotation rotation)[,] a, (short id, Rotation rotation)[,] b)
	{
		static bool Matches((short id, Rotation rotation) a, (short id, Rotation rotation) b)
		{
			return (a.id == -1 && b.id == -1) ||
			       (a.id != -1 && b.id != -1);
		}

		var rows = a.GetLength(0);
		var cols = a.GetLength(1);

		var oriented = true;
		var oriented180 = true;
		var mirrored = true;
		var mirrored180 = true;
		var any = true;

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				oriented &= Matches(a[i, j], b[i, j]);
				oriented180 &= Matches(a[i, j], b[rows - i - 1, cols - j - 1]);

				mirrored &= Matches(a[i, j], b[i, cols - j - 1]);
				mirrored180 &= Matches(a[i, j], b[rows - i - 1, j]);

				any &= oriented || oriented180 || mirrored || mirrored180;

				if (!any) return false;
			}
		}

		return true;
	}

	/// <summary>
	/// Checks arrays that have transposed dimensions
	/// </summary>
	private static bool CheckRotatedArrays((short id, Rotation rotation)[,] a, (short id, Rotation rotation)[,] b)
	{
		static bool Matches((short id, Rotation rotation) a, (short id, Rotation rotation) b)
		{
			return (a.id == -1 && b.id == -1) ||
			       (a.id != -1 && b.id != -1);
		}

		var rows = a.GetLength(0);
		var cols = a.GetLength(1);

		var oriented90 = true;
		var oriented270 = true;
		var mirrored90 = true;
		var mirrored270 = true;
		var any = true;

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < cols; j++)
			{
				oriented90 &= Matches(a[i, j], b[cols - j - 1, i]);
				oriented270 &= Matches(a[i, j], b[j, rows - i - 1]);

				mirrored90 &= Matches(a[i, j], b[cols - j - 1, rows - i - 1]);
				mirrored270 &= Matches(a[i, j], b[j, i]);

				any &= oriented90 || oriented270 || mirrored90 || mirrored270;

				if (!any) return false;
			}
		}

		return true;
	}

	public int GetHashCode(Net obj)
	{
		return 1;
	}
}