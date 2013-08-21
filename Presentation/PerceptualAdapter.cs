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
        /// <summary>
        /// Please set control's IsHitTestVisible = false.
        /// This property may be deprecated.
        /// </summary>
        public List<FrameworkElement> ExcludedControls = new List<FrameworkElement>();
        protected FrameworkElement parent;

        public delegate void HandMoveEventHandler(object sender, HandMoveEventArgs e);

        public HandMoveEventHandler Move;
        public HandMoveEventHandler Open;
        public HandMoveEventHandler Close;
        /// <summary>
        /// Primary Hand enters the world, usually means the cursor is now visible,
        /// but you can use Move event for that.
        /// </summary>
        public RoutedEventHandler Enter;
        /// <summary>
        /// Primary Hand leaves the world, usually means the cursor should be hidden.
        /// </summary>
        public RoutedEventHandler Leave;

        public PerceptualAdapter(PerceptualManager perceptualMgr, FrameworkElement parent)
        {
            this.parent = parent;
            perceptualMgr.PrimaryOpen += OnPrimaryOpen;
            perceptualMgr.PrimaryClose += OnPrimaryClose;
            perceptualMgr.PrimaryMove += OnPrimaryMove;
            perceptualMgr.PrimaryEnter += OnPrimaryEnter;
            perceptualMgr.PrimaryLeave += OnPrimaryLeave;
        }

        private void OnPrimaryLeave(PerceptualManager sender, HandEventArgs e)
        {
            if (Leave != null)
            {
                parent.Dispatcher.InvokeAsync(delegate
                {
                    Leave(parent, new RoutedEventArgs());
                });
            }
        }

        private void OnPrimaryEnter(PerceptualManager sender, HandEventArgs e)
        {
            if (Enter != null)
            {
                parent.Dispatcher.InvokeAsync(delegate
                {
                    Enter(parent, new RoutedEventArgs());
                });
            }
        }

        protected void OnPrimaryMove(PerceptualManager sender, HandEventArgs e)
        {
            if (Move != null)
            {
                Point p = new Point(e.Left * parent.ActualWidth, e.Top * parent.ActualHeight);
                HandMoveEventArgs ev = new HandMoveEventArgs();
                ev.Location = p;
                parent.Dispatcher.InvokeAsync(delegate
                {
                    Move(parent, ev);
                });
            }
        }

        protected void OnPrimaryClose(PerceptualManager sender, HandEventArgs e)
        {
            Point p = new Point(e.Left * parent.ActualWidth, e.Top * parent.ActualHeight);
            HandMoveEventArgs ev = new HandMoveEventArgs();
            ev.Location = p;

            parent.Dispatcher.InvokeAsync(delegate
            {
                IInputElement el = parent.InputHitTest(p);
                if (el != null)
                {
                    log.DebugFormat("CLICK hit {0}", el);
                    el.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                }

                if (Close != null)
                {
                        Close(parent, ev);
                }
            });

            parent.Dispatcher.InvokeAsync(delegate
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
                            //if (result.VisualHit is FrameworkElement) {
                            //    ((FrameworkElement)result.VisualHit).RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                            //}

                            log.DebugFormat("CLOSE hit {0}", result.VisualHit);
                            if (Close != null)
                            {
                                Close(result.VisualHit, ev);
                            }
                            return HitTestResultBehavior.Continue;
                        }
                    }), new PointHitTestParameters(p));
            });
        }

        protected void OnPrimaryOpen(PerceptualManager sender, HandEventArgs e)
        {
            Point p = new Point(e.Left * parent.ActualWidth, e.Top * parent.ActualHeight);
            HandMoveEventArgs ev = new HandMoveEventArgs();
            ev.Location = p;

            if (Open != null)
            {
                parent.Dispatcher.InvokeAsync(delegate
                {
                    Open(parent, ev);
                });
            }

            parent.Dispatcher.InvokeAsync(delegate
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
                                Open(result.VisualHit, ev);
                            }
                            return HitTestResultBehavior.Continue;
                        }
                    }), new PointHitTestParameters(p));
            });
        }

    }
}
