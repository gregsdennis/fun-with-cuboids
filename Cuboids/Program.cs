using System.Diagnostics;
using Cuboids.Core;

namespace Cuboids;

internal class Program
{
	private static readonly Random Random = new();
	private static readonly DateTime Start = Process.GetCurrentProcess().StartTime.ToUniversalTime();
	private static int _counter = 0;
	private static int _max = 0;
	private static int _current = 0;

	static async Task Main(string[] args)
	{

		short length = 1;
		short width = 5;
		short height = 1;

		var cuboid = new Cuboid(length, width, height);

		//var net = BuildRandomNet(cuboid);
		var distinctNetGraphs = await BuildDistinctNetGraphs(cuboid);

		//var firstSetOfSharedGraphCuboids = (await CollectSharedGraphCuboids(new[] { new short[]{1,5,1}, new short[]{2,3,1}})).ToList();

		//var maxEdgeLength = 10;
		//var compatibleCuboids = GetCompatibleCuboids(maxEdgeLength);
		//var cuboidsThatShareGraphs = CollectAllSharedGraphCuboids(compatibleCuboids).ToList();
	}

	//private static IEnumerable<IGrouping<Net, (Cuboid, Net)>> CollectSharedGraphCuboids(int[][] sizes)
	private static async Task<IEnumerable<(Net, Net)>> CollectSharedGraphCuboids(short[][] sizes)
	{
		var cuboidGroup = new[]
		{
			new Cuboid(sizes[0][0], sizes[0][1], sizes[0][2]),
			new Cuboid(sizes[1][0], sizes[1][1], sizes[1][2])
		};

		var graphsForCuboid0 = await BuildDistinctNetGraphs(cuboidGroup[0]);
		var graphsForCuboid1 = await BuildDistinctNetGraphs(cuboidGroup[1]);

		//var netsWithCuboid = cuboidGroup.Select(cuboid => (cuboid, BuildDistinctNetGraphs(cuboid)));

		return graphsForCuboid0.Join(graphsForCuboid1,
			x => x,
			y => y,
			(x, y) => (x, y),
			NetEquivalenceComparer.Instance);

		//var netsByGraph = netsWithCuboid.GroupBy(x => x.net, NetEquivalenceComparer.Instance);

		//foreach (var net in matched)
		//{
		//	yield return net;
		//}
	}

	//private static IEnumerable<IGrouping<Net, (Cuboid, Net)>> CollectAllSharedGraphCuboids(IEnumerable<IGrouping<int, Cuboid>> compatibleCuboids)
	//{
	//	foreach (var cuboidGroup in compatibleCuboids)
	//	{
	//		var netsWithCuboid = cuboidGroup.SelectMany(cuboid => BuildDistinctNetGraphs(cuboid).Select(net => (cuboid, net)));

	//		var netsByGraph = netsWithCuboid.GroupBy(x => x.net, NetEquivalenceComparer.Instance);

	//		foreach (var net in netsByGraph.Where(x => x.Count() > 1))
	//		{
	//			yield return net;
	//		}
	//	}
	//}

	private static Net BuildRandomNet(Cuboid cuboid)
	{
		var unplacedCells = cuboid.Cells.Skip(1).ToList();
		var net = new Net(cuboid.Cells[0]);

		while (unplacedCells.Any())
		{
			var openConnections = net.GetOpenConnections();
			var availableCells = unplacedCells
				.Join(openConnections,
					uc => uc.Id,
					oc => oc.conn.Id,
					(uc, oc) => (cell: uc, location: oc.node))
				.ToList();

			var randomIndex = Random.Next(availableCells.Count);
			var (selectedCell, location) = availableCells[randomIndex];

			var newNode = new CellNode(selectedCell);
			net.Add(newNode, location);

			unplacedCells.Remove(selectedCell);
		}

		return net;
	}

	private static async Task<Net[]> BuildDistinctNetGraphs(Cuboid cuboid)
	{
		var all = new List<Net>();
		var net = new Net(cuboid.Cells[0]);

		var timer = new System.Timers.Timer(100);
		timer.Elapsed += (_,_) =>  OutputFinalNets(all);
		var otherTimer = new System.Timers.Timer(1000);
		otherTimer.Elapsed += (_, _) =>
		{
			_max = _counter;
			_counter = 0;
		};

		timer.Start();
		otherTimer.Start();
		await BuildNetTree(net, cuboid.Cells.Skip(1).ToArray(), Array.Empty<(Cell cell, CellNode location)>(), all);
		timer.Stop();
		otherTimer.Stop();

		return all.ToArray();
	}

	private static void OutputFinalNets(IReadOnlyList<Net> finalNets)
	{
		if (_current >= finalNets.Count) return;

		var net = finalNets[_current];
		var grid = net.Output();

		Console.SetCursorPosition(0, 0);
		Console.WriteLine(grid);

		net = finalNets[^1];
		grid = net.Output();
		Console.WriteLine("Most recent:");
		Console.WriteLine(grid);

		Console.WriteLine($"Showing net #{_current}".PadRight(40));
		Console.WriteLine($"Unique nets found: {finalNets.Count}".PadRight(40));
		Console.WriteLine($"Nets /s: {_max}".PadRight(40));
		Console.WriteLine($"Run time: {DateTime.UtcNow - Start:g}".PadRight(40));

		_current++;
		_current %= finalNets.Count;
	}

	private static async Task BuildNetTree(Net net, Cell[] unplacedCells, (Cell cell, CellNode location)[] placementsExploredElsewhere, ICollection<Net> finalNets)
	{
		if (!unplacedCells.Any())
		{
			_counter++;

			lock (finalNets)
			{
				if (!finalNets.Contains(net, NetEquivalenceComparer.Instance))
					finalNets.Add(net);
				return;
			}
		}

		var openConnections = net.GetOpenConnections();
		var availablePlacements = unplacedCells
			.Join(openConnections,
				uc => uc.Id,
				oc => oc.conn.Id,
				(uc, oc) => (cell: uc, location: oc.node))
			.Except(placementsExploredElsewhere)
			.ToArray();

		placementsExploredElsewhere = placementsExploredElsewhere.Concat(availablePlacements).ToArray();

		var localNets = availablePlacements
			.Select(p => (placement: p, net: new Net(net, p.cell, p.location.Cell.Id)))
			.DistinctBy(x => x.net, NetEquivalenceComparer.Instance);

		var tasks = localNets.Select(n => BuildNetTree(n.net,
			unplacedCells.Where(x => x != n.placement.cell).ToArray(),
			placementsExploredElsewhere, finalNets));

		await Task.WhenAll(tasks);
	}

	private static IEnumerable<Cuboid> BuildDistinctCuboids(int maxEdgeLength)
	{
		var doneSizes = new List<(int sum, int product)>();

		for (short i = 1; i <= maxEdgeLength; i++)
		{
			for (short j = 1; j <= maxEdgeLength; j++)
			{
				for (short k = 1; k <= maxEdgeLength; k++)
				{
					var sum = i + j + k;
					var product = i * j * k;

					if (doneSizes.Contains((sum, product))) continue;
					doneSizes.Add((sum, product));

					yield return new Cuboid(i, j, k);
				}
			}
		}
	}

	private static IEnumerable<IGrouping<int, Cuboid>> GetCompatibleCuboids(int maxEdgeLength)
	{
		var distinctCuboids = BuildDistinctCuboids(maxEdgeLength);
		var compatibleCuboids = distinctCuboids
			.GroupBy(x => x.SurfaceArea)
			.Where(x => x.Count() > 1);
		return compatibleCuboids;
	}
}