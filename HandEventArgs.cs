using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hendyirawan.Nperceptual
{
    /// <summary>
    /// Event arguments for simpler handling of hand tracking.
    /// </summary>
    public class HandEventArgs
    {
        public HandLabel Hand;
        /// <summary>
        /// Hand position from left of world. 
        /// Visible range is from 0.0 (leftmost) to 1.0 (rightmost).
        /// Note that out-of-range values are allowed.
        /// </summary>
        public double Left;
        /// <summary>
        /// Hand position from top of world.
        /// Visible range is from 0.0 (topmost) to 1.0 (bottommost).
        /// Note that out-of-range values are allowed.
        /// </summary>
        public double Top;
        public HandOpenness Openness;
        public PXCMPoint3DF32 PositionImage;
        public PXCMPoint3DF32 PositionWorld;
    }
}
