# nperceptual

Finger points and gesture recognition library for
Microsoft .NET Framework using [Intel Perceptual SDK](http://software.intel.com/en-us/vcsource/tools/perceptual-computing-sdk).

## Dependencies (Managed via NuGet)

* [log4net](http://www.nuget.org/packages/log4net)

## Usage

The main entry point of the library is the `PerceptualManager` class.

Sample code to display "perceptual cursor" using any WPF Shape/Component,
with hand open-close ("grab") gesture detection, and `Button.ClickEvent` simulation.

```cs
private readonly PerceptualManager perceptualMgr;
private readonly PerceptualAdapter perceptualAdapter;

public MainWindow()
{
    InitializeComponent();
    perceptualMgr = new PerceptualManager();
    perceptualAdapter = new PerceptualAdapter(perceptualMgr, this);
    InitPerceptual();
}

private void InitPerceptual()
{
    perceptualMgr.Init();
    perceptualMgr.Start();
    perceptualAdapter.ExcludedControls.Add(hand);
    perceptualAdapter.Move = Perceptual_Move;
    perceptualAdapter.Close = Perceptual_Close;
    perceptualAdapter.Open = Perceptual_Open;
}

private void Perceptual_Open(object sender, RoutedEventArgs e)
{
    if (sender == this)
    {
        hand.Fill = new SolidColorBrush(Color.FromRgb(0x90, 0x90, 0x90));
    }
}

private void Perceptual_Close(object sender, RoutedEventArgs e)
{
    if (sender == this)
    {
        hand.Fill = new SolidColorBrush(Color.FromRgb(0xff, 0x33, 0));
    }
}

private void Perceptual_Move(object sender, HandMoveEventArgs e)
{
    if (sender == this)
    {
        hand.Margin = new Thickness(
            (-hand.ActualWidth / 2) + e.Location.X,
            (-hand.ActualHeight / 2) + e.Location.Y,
            0, 0);
    }
}

private void Window_Unloaded(object sender, RoutedEventArgs e)
{
    perceptualMgr.Dispose();
}

private void Window_Closed(object sender, EventArgs e)
{
    perceptualMgr.Dispose();
}
```
