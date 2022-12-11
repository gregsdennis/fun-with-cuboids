using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Cuboids.Core;

namespace CuboidsApp;

public class NetLayout : Control
{
	public Net? Net
	{
		get { return (Net)GetValue(NetProperty); }
		set { SetValue(NetProperty, value); }
	}

	// Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
	public static readonly DependencyProperty NetProperty =
		DependencyProperty.Register(nameof(Net), typeof(Net), typeof(NetLayout), new PropertyMetadata(null, Redraw));

	private static void Redraw(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		var control = (NetLayout)d;
		control.InvalidateVisual();
		var net = (Net)e.NewValue;

		var rows = net.Layout.GetLength(0);
		var columns = net.Layout.GetLength(1);

		control.Width = columns * 20;
		control.Height = rows * 20;
	}

	static NetLayout()
	{
		DefaultStyleKeyProperty.OverrideMetadata(typeof(NetLayout), new FrameworkPropertyMetadata(typeof(NetLayout)));
	}

	protected override void OnRender(DrawingContext drawingContext)
	{
		if (Net == null) return;

		// Data may change during a render, so hold on to the board we're drawing now
		var data = Net.Layout;
		var rows = Net.Layout.GetLength(0);
		var columns = Net.Layout.GetLength(1);

		var width = 20;
		var height = 20;

		for (int i = 0; i < rows; i++)
		{
			for (int j = 0; j < columns; j++)
			{
				if (data[i,j].id == -1) continue;
				drawingContext.DrawRectangle(Brushes.Blue, null, new Rect(j * width, i * height, width, height));
			}
		}
	}
}
