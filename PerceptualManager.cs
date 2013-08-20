using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nperceptual
{
    public class PerceptualManager : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PerceptualManager));
        private PXCMSession session = null;

        public void Init()
        {
            pxcmStatus sts = PXCMSession.CreateInstance(out session);
            log.DebugFormat("PXCM Status: {0}", sts);
        }

        public void Dispose()
        {
            if (session != null)
            {
                session.Dispose();
            }
        }
    }
}
