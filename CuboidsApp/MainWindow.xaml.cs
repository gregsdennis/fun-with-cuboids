using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using Cuboids.Core;
using Timer = System.Timers.Timer;

namespace CuboidsApp;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	private static readonly DateTime Start = Process.GetCurrentProcess().StartTime;

	private Timer _timer;
	private Timer _otherTimer;
	private Runner _runner;
	private int _netsFoundCount;
	private int _netsGeneratedCount;
	private int _totalNets;

	public Net CurrentNet
	{
		get { return (Net)GetValue(CurrentNetProperty); }
		set { SetValue(CurrentNetProperty, value); }
	}

	public static readonly DependencyProperty CurrentNetProperty =
		DependencyProperty.Register(nameof(CurrentNet), typeof(Net), typeof(MainWindow), new PropertyMetadata(null));
	

	public Net LatestNet
	{
		get { return (Net)GetValue(LatestNetProperty); }
		set { SetValue(LatestNetProperty, value); }
	}

	public static readonly DependencyProperty LatestNetProperty =
		DependencyProperty.Register(nameof(LatestNet), typeof(Net), typeof(MainWindow), new PropertyMetadata(null));
	
	public int CurrentNetIndex
	{
		get { return (int)GetValue(CurrentNetIndexProperty); }
		set { SetValue(CurrentNetIndexProperty, value); }
	}

	public static readonly DependencyProperty CurrentNetIndexProperty =
	DependencyProperty.Register(nameof(CurrentNetIndex), typeof(int), typeof(MainWindow), new PropertyMetadata(0));
	
	public int UniqueNetCount
	{
		get { return (int)GetValue(UniqueNetCountProperty); }
		set { SetValue(UniqueNetCountProperty, value); }
	}

	public static readonly DependencyProperty UniqueNetCountProperty =
	DependencyProperty.Register(nameof(UniqueNetCount), typeof(int), typeof(MainWindow), new PropertyMetadata(0));
	
	public long TotalNetCount
	{
		get { return (long)GetValue(TotalNetCountProperty); }
		set { SetValue(TotalNetCountProperty, value); }
	}

	public static readonly DependencyProperty TotalNetCountProperty =
		DependencyProperty.Register(nameof(TotalNetCount), typeof(long), typeof(MainWindow), new PropertyMetadata(0L));
	
	public int NetsGeneratedPerSecond
	{
		get { return (int)GetValue(NetsGeneratedPerSecondProperty); }
		set { SetValue(NetsGeneratedPerSecondProperty, value); }
	}

	public static readonly DependencyProperty NetsGeneratedPerSecondProperty =
		DependencyProperty.Register(nameof(NetsGeneratedPerSecond), typeof(int), typeof(MainWindow), new PropertyMetadata(0));
	
	public int NetsFoundPerSecond
	{
		get { return (int)GetValue(NetsFoundPerSecondProperty); }
		set { SetValue(NetsFoundPerSecondProperty, value); }
	}

	public static readonly DependencyProperty NetsFoundPerSecondProperty =
		DependencyProperty.Register(nameof(NetsFoundPerSecond), typeof(int), typeof(MainWindow), new PropertyMetadata(0));

	public TimeSpan RunTime
	{
		get { return (TimeSpan)GetValue(RunTimeProperty); }
		set { SetValue(RunTimeProperty, value); }
	}

	public static readonly DependencyProperty RunTimeProperty =
		DependencyProperty.Register(nameof(RunTime), typeof(TimeSpan), typeof(MainWindow), new PropertyMetadata(TimeSpan.Zero));

	public MainWindow()
	{
		InitializeComponent();
		DataContext = this;
		
		_timer = new Timer(100);
		_timer.Elapsed += UpdateUI;
		
		_otherTimer = new Timer(1000);
		_otherTimer.Elapsed += UpdateStats;

		_runner = new();
		_runner.NetGenerated += NetGenerated;
		_runner.NewNetFound += NewNetFound;
	}

	protected override void OnActivated(EventArgs e)
	{
		base.OnActivated(e);

		Task.Run(_runner.Run);

		_timer.Start();
		_otherTimer.Start();
	}

	private void UpdateUI(object? sender, ElapsedEventArgs e)
	{
		var uniqueNets = _runner.FinalNets.Count;
		if (uniqueNets == 0) return;

		Dispatcher.SafeInvoke(() =>
		{
			// shouldn't happen, but does for some reason
			if (CurrentNetIndex >= _runner.FinalNets.Count) return;

			RunTime = DateTime.Now - Start;
			TotalNetCount = _totalNets;
			UniqueNetCount = uniqueNets;

			CurrentNet = _runner.FinalNets[CurrentNetIndex];
			CurrentNetIndex = (CurrentNetIndex + 1) % uniqueNets;

			LatestNet = _runner.FinalNets[uniqueNets - 1];
		});
	}

	private void UpdateStats(object? sender, ElapsedEventArgs e)
	{
		Dispatcher.SafeInvoke(() =>
		{
			NetsGeneratedPerSecond = _netsGeneratedCount;
			NetsFoundPerSecond = _netsFoundCount;
		});

		_netsFoundCount = 0;
		_netsGeneratedCount = 0;
	}

	private void NetGenerated(object? sender, EventArgs e)
	{
		Interlocked.Increment(ref _totalNets);
		Interlocked.Increment(ref _netsGeneratedCount);
	}

	private void NewNetFound(object? sender, EventArgs e)
	{
		Interlocked.Increment(ref _netsFoundCount);
	}
}