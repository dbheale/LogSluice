using Avalonia;
using Avalonia.Controls;

namespace LogSluice.Behaviors;

public static class ScrollBehavior
{
    public static readonly AttachedProperty<bool> AutoScrollProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, bool>("AutoScroll", typeof(ScrollBehavior));

    // Hidden tracking properties to detect user scroll direction
    private static readonly AttachedProperty<double> LastOffsetYProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, double>("LastOffsetY", typeof(ScrollBehavior));
        
    private static readonly AttachedProperty<double> LastExtentHeightProperty =
        AvaloniaProperty.RegisterAttached<ScrollViewer, double>("LastExtentHeight", typeof(ScrollBehavior));

    public static bool GetAutoScroll(AvaloniaObject element) => element.GetValue(AutoScrollProperty);
    public static void SetAutoScroll(AvaloniaObject element, bool value) => element.SetValue(AutoScrollProperty, value);

    static ScrollBehavior()
    {
        AutoScrollProperty.Changed.AddClassHandler<ScrollViewer>(HandleAutoScrollChanged);
    }

    private static void HandleAutoScrollChanged(ScrollViewer sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.NewValue is bool isEnabled)
        {
            if (isEnabled)
            {
                // Initialize tracking variables when turned on
                sender.SetValue(LastOffsetYProperty, sender.Offset.Y);
                sender.SetValue(LastExtentHeightProperty, sender.Extent.Height);
                
                sender.PropertyChanged += ScrollViewer_PropertyChanged;
                sender.ScrollChanged += ScrollViewer_ScrollChanged;
                
                sender.ScrollToEnd(); 
            }
            else
            {
                // Clean up handlers when turned off
                sender.PropertyChanged -= ScrollViewer_PropertyChanged;
                sender.ScrollChanged -= ScrollViewer_ScrollChanged;
            }
        }
    }

    private static void ScrollViewer_PropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (sender is ScrollViewer scrollViewer)
        {
            // If the content grew, or the window resized, jump to the bottom
            if (e.Property == ScrollViewer.ExtentProperty || e.Property == ScrollViewer.ViewportProperty)
            {
                if (GetAutoScroll(scrollViewer))
                {
                    scrollViewer.ScrollToEnd();
                }
            }
        }
    }

    private static void ScrollViewer_ScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (sender is ScrollViewer sv)
        {
            if (GetAutoScroll(sv))
            {
                double currentY = sv.Offset.Y;
                double lastY = sv.GetValue(LastOffsetYProperty);
                double currentExtent = sv.Extent.Height;
                double lastExtent = sv.GetValue(LastExtentHeightProperty);
                
                double maxOffset = sv.Extent.Height - sv.Viewport.Height;
                bool isAtBottom = currentY >= (maxOffset - 2); // 2px tolerance for layout rounding
                
                // CRITICAL CHECK:
                // If the Y offset decreased (scrolled up)
                // AND the extent didn't shrink (it wasn't a file clear)
                // AND we are no longer at the bottom
                if (currentY < lastY && currentExtent >= lastExtent && !isAtBottom)
                {
                    // The user manually scrolled up! Turn off FollowTail.
                    SetAutoScroll(sv, false);
                }
            }
            
            // Always save the current state for the next scroll event
            sv.SetValue(LastOffsetYProperty, sv.Offset.Y);
            sv.SetValue(LastExtentHeightProperty, sv.Extent.Height);
        }
    }
}