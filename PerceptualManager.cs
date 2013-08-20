using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace nperceptual
{
    public class PerceptualManager : IDisposable
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(PerceptualManager));
        private PXCMSession session = null;
        private bool started = false;
        private Thread thread = null;

        public delegate void HandGeoNodeHandler(PXCMGesture.GeoNode.Label label,
            PXCMPoint3DF32 positionWorld);

        public HandGeoNodeHandler HandGeoNode;

        public void Init()
        {
            pxcmStatus sts = PXCMSession.CreateInstance(out session);
            log.DebugFormat("PXCM Status: {0}", sts);
        }

        public void Dispose()
        {
            if (started)
            {
                started = false;
                thread.Join();
                thread = null;
            }
            if (session != null)
            {
                session.Dispose();
                session = null;
            }
        }

        public void Start()
        {
            if (!started)
            {
                thread = new Thread(DoRecognition);
                thread.Priority = ThreadPriority.Lowest;
                thread.Start();
            }
        }

        public void Stop()
        {
            if (started)
            {
                started = false;
                thread.Join();
                thread = null;
            }
        }

        /// <summary>
        /// Should be called inside Thread.
        /// </summary>
        private void DoRecognition()
        {
            String deviceName = "";
            int iuid = 0;

            {
                PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
                desc.group = PXCMSession.ImplGroup.IMPL_GROUP_SENSOR;
                desc.subgroup = PXCMSession.ImplSubgroup.IMPL_SUBGROUP_VIDEO_CAPTURE;
                for (uint i = 0; ; i++)
                {
                    PXCMSession.ImplDesc desc1;
                    if (session.QueryImpl(ref desc, i, out desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    log.DebugFormat("Desc {0}", desc1);
                    PXCMCapture capture;
                    if (session.CreateImpl<PXCMCapture>(ref desc1, PXCMCapture.CUID, out capture) < pxcmStatus.PXCM_STATUS_NO_ERROR) continue;
                    log.DebugFormat("Capture {0}", capture);
                    try
                    {
                        for (uint j = 0; ; j++)
                        {
                            PXCMCapture.DeviceInfo dinfo;
                            if (capture.QueryDevice(j, out dinfo) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                            log.DebugFormat("DeviceInfo #{0}: {1} {2} {3}", j + 1, dinfo.did, dinfo.name, dinfo);
                            if (i == 0)
                            {
                                // only use first device, "Creative GestureCam" doesn't work
                                deviceName = dinfo.name.get();
                            }
                        }
                    }
                    finally
                    {
                        capture.Dispose();
                    }
                }

            }

            {
                PXCMSession.ImplDesc desc = new PXCMSession.ImplDesc();
                desc.cuids[0] = PXCMGesture.CUID;
                for (uint i = 0; ; i++)
                {
                    PXCMSession.ImplDesc desc1;
                    if (session.QueryImpl(ref desc, i, out desc1) < pxcmStatus.PXCM_STATUS_NO_ERROR) break;
                    log.DebugFormat("Module #{0}: {1} {2}", i + 1, desc1.iuid, desc1.friendlyName.get());
                    iuid = desc1.iuid;
                }
            }

            UtilMPipeline pp = new UtilMPipeline();
            log.InfoFormat("SetFilter {0}", (object)deviceName);
            pp.QueryCapture().SetFilter(deviceName);
            log.InfoFormat("Enabling gesture for {0}", iuid);
            pp.EnableGesture(iuid);
            try
            {
                if (pp.Init())
                {
                    log.Info("Initialized Perceptual");
                    started = true;
                    while (started)
                    {
                        if (!pp.AcquireFrame(true)) break;
                        try
                        {
                            PXCMGesture gesture = pp.QueryGesture();

                            // Pose
                            PXCMGesture.Gesture primaryHand;
                            PXCMGesture.Gesture secondaryHand;
                            gesture.QueryGestureData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, 0, out primaryHand);
                            gesture.QueryGestureData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_SECONDARY, 0, out secondaryHand);
                            Debug.WriteLine("Primary={0} {1} {2} Secondary={3} {4} {5}", primaryHand.label, primaryHand.confidence, primaryHand,
                                secondaryHand.label, secondaryHand.confidence, secondaryHand);
                            if (primaryHand.label == PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP || secondaryHand.label == PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP)
                            {
                                log.InfoFormat("Pose {0}", PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_UP);
                                //Dispatcher.InvokeAsync(ThumbsUp);
                            }
                            else if (primaryHand.label == PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_DOWN || secondaryHand.label == PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_DOWN)
                            {
                                log.InfoFormat("Pose {0}", PXCMGesture.Gesture.Label.LABEL_POSE_THUMB_DOWN);
                                //Dispatcher.InvokeAsync(ThumbsDown);
                            }
                            else if (primaryHand.label == PXCMGesture.Gesture.Label.LABEL_HAND_WAVE || secondaryHand.label == PXCMGesture.Gesture.Label.LABEL_HAND_WAVE)
                            {
                                log.InfoFormat("Pose {0}", PXCMGesture.Gesture.Label.LABEL_HAND_WAVE);
                                //Dispatcher.InvokeAsync(Wave);
                            }

                            // GeoNode
                            PXCMGesture.GeoNode primaryGeo;
                            gesture.QueryNodeData(0, PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, out primaryGeo);
                            log.InfoFormat("Primary Geo c={0} o={1}/{2} [{3} {4} {5}] [{6} {7} {8}]",
                                primaryGeo.confidence, primaryGeo.openness, primaryGeo.opennessState,
                                primaryGeo.positionImage.x, primaryGeo.positionImage.y, primaryGeo.positionImage.z,
                                primaryGeo.positionWorld.x, primaryGeo.positionWorld.y, primaryGeo.positionWorld.z);
                            if (primaryGeo.positionWorld.x != 0 && HandGeoNode != null)
                            {
                                
                                HandGeoNode(PXCMGesture.GeoNode.Label.LABEL_BODY_HAND_PRIMARY, primaryGeo.positionWorld);
                            }
                        }
                        finally
                        {
                            pp.ReleaseFrame();
                        }
                        Thread.Sleep(40);
                    }
                }
                else
                {
                    log.Warn("Cannot Init");
                }
            }
            finally
            {
                pp.Close();
                pp.Dispose();
                log.Info("Disposed.");
            }
        }

    }
}
