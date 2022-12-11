using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cuboids.Core;

namespace CuboidsApp;

public class Runner
{
	public event EventHandler NetGenerated;
	public event EventHandler NewNetFound;

	public List<Net> FinalNets { get; set; }

	public async Task Run()
	{
		var cuboid = new Cuboid(1, 5, 1);

		await BuildDistinctNetGraphs(cuboid);
	}

	private async Task BuildDistinctNetGraphs(Cuboid cuboid)
	{
		FinalNets = new List<Net>();
		var net = new Net(cuboid.Cells[0]);

		await BuildNetTree(net, cuboid.Cells.Skip(1).ToArray(), Array.Empty<(Cell cell, CellNode location)>());
	}

	// Recursion hasn't been an issue so far.  It might be an issue as the cuboid size grows, though.
	// 10x10x10 could potentially have 1000 recursive calls.

	/// <summary>
	/// Recursively generates nets by place one cell at a time.  Unique nets are saved to <see cref="FinalNets"/>.
	/// </summary>
	private async Task BuildNetTree(Net net, Cell[] unplacedCells, (Cell cell, CellNode location)[] placementsExploredElsewhere)
	{
		if (!unplacedCells.Any())
		{
			// no more cells to place.  we're done!
			NetGenerated?.Invoke(this, EventArgs.Empty);

			lock (FinalNets)
			{
				// check to see if it's unique
				if (!FinalNets.Contains(net, NetEquivalenceComparer.Instance))
				{
					FinalNets.Add(net);
					NewNetFound?.Invoke(this, EventArgs.Empty);
				}
				return;
			}
		}

		var openConnections = net.GetOpenConnections();
		// get any cells that haven't been placed but have places available
		var availablePlacements = unplacedCells
			.Join(openConnections,
				uc => uc.Id,
				oc => oc.conn.Id,
				(uc, oc) => (cell: uc, location: oc.node))
			// ignore cell placements that will be or have been explored in other recursive branches
			.Except(placementsExploredElsewhere)
			.ToArray();

		// add these placements to the list of placements to ignore for future runs
		placementsExploredElsewhere = placementsExploredElsewhere.Concat(availablePlacements).ToArray();

		// build nets and only take the distinct set
		var localNets = availablePlacements
			.Select(p => (placement: p, net: new Net(net, p.cell, p.location.Cell.Id)))
			.DistinctBy(x => x.net, NetEquivalenceComparer.Instance);

		// run next level in parallel
		var tasks = localNets.Select(n => BuildNetTree(n.net,
			unplacedCells.Where(x => x != n.placement.cell).ToArray(),
			placementsExploredElsewhere));

		await Task.WhenAll(tasks);
	}
}