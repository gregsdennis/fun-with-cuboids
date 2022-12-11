namespace Cuboids.Core;

public static class Extensions
{
	/// <summary>
	/// gets the new direction when a given direction is rotated a given amount
	/// </summary>
	public static Direction Rotate(this Direction direction, Rotation rotation)
	{
		return (Direction)(((int)direction + (int)rotation) % 4);
	}

	/// <summary>
	/// Gets the required rotation needed to achieve the specified direction
	/// </summary>
	public static Rotation GetRotation(this Direction target, Direction incoming)
	{
		return (Rotation)((target - incoming + 6) % 4);
	}

	/// <summary>
	/// Adds rotations
	/// </summary>
	public static Rotation Compound(this Rotation target, Rotation incoming)
	{
		return (Rotation)(((int)target + (int)incoming) % 4);
	}
}