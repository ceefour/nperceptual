using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Hendyirawan.Nperceptual.Presentation
{
    /// <summary>
    /// Handles coordinate-level events from PerceptualManager,
    /// performs visual hittest and triggers control-level perceptual events.
    /// This is convenience helper for WPF apps.
    /// </summary>
    public class PerceptualAdapter
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PerceptualAdapter));
        public List<FrameworkElement> ExcludedControls = new List<FrameworkElement>();
        protected FrameworkElement parent;

        public delegate void HandMoveEventHandler(object sender, HandMoveEventArgs e);

        public HandMoveEventHandler Move;
        public RoutedEventHandler Open;
        public RoutedEventHandler Close;

        public PerceptualAdapter(PerceptualManager perceptualMgr, FrameworkElement parent)
        {
            this.parent = parent;
            perceptualMgr.PrimaryOpen += OnPrimaryOpen;
            perceptualMgr.PrimaryClose += OnPrimaryClose;
            perceptualMgr.PrimaryMove += OnPrimaryMove;
        }

        protected void OnPrimaryMove(PerceptualManager sender, HandEventArgs e)
        {
            if (Move != null)
            {
                Point p = new Point(e.Left * parent.ActualWidth, e.Top * parent.ActualHeight);
                    HandMoveEventArgs ev = new HandMoveEventArgs();
                    ev.Location = p;
                parent.Dispatcher.Invoke(delegate
                {
                    Move(parent, ev);
                });
            }
        }

        protected void OnPrimaryClose(PerceptualManager sender, HandEventArgs e)
        {
            if (Close != null)
            {
                RoutedEventArgs ev = new RoutedEventArgs();
                parent.Dispatcher.Invoke(delegate
                {
                    Close(parent, ev);
                });
            }

            Point p = new Point(e.Left * parent.ActualWidth, e.Top * parent.ActualHeight);

            parent.Dispatcher.Invoke(delegate
            {
                // InputHitTest for ClickEvent

                VisualTreeHelper.HitTest(parent, null,
                    new HitTestResultCallback(delegate(HitTestResult result)
                    {
                        if (ExcludedControls.Contains(result.VisualHit))
                        {
                            return HitTestResultBehavior.Continue;
                        }
                        else
                        {
                            if (result.VisualHit is FrameworkElement) {
                                ((FrameworkElement)result.VisualHit).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            }

                            log.DebugFormat("CLOSE hit {0}", result.VisualHit);
                            if (Close != null)
                            {
                                RoutedEventArgs ev = new RoutedEventArgs();
                                Close(result.VisualHit, ev);
                            }
                            return HitTestResultBehavior.Continue;
                        }
                    }), new PointHitTestParameters(p));
            });
        }

        protected void OnPrimaryOpen(PerceptualManager sender, HandEventArgs e)
        {
            if (Open != null)
            {
                RoutedEventArgs ev = new RoutedEventArgs();
                parent.Dispatcher.Invoke(delegate
                {
                    Open(parent, ev);
                });
            }

            Point p = new Point(e.Left * parent.ActualWidth, e.Top * parent.ActualHeight);
            parent.Dispatcher.Invoke(delegate
            {
                VisualTreeHelper.HitTest(parent, null,
                    new HitTestResultCallback(delegate(HitTestResult result)
                    {
                        if (ExcludedControls.Contains(result.VisualHit))
                        {
                            return HitTestResultBehavior.Continue;
                        }
                        else
                        {
                            log.DebugFormat("OPEN hit {0}", result.VisualHit);
                            if (Open != null)
                            {
                                RoutedEventArgs ev = new RoutedEventArgs();
                                Open(result.VisualHit, ev);
                            }
                            return HitTestResultBehavior.Continue;
                        }
                    }), new PointHitTestParameters(p));
            });
        }

    }
}
