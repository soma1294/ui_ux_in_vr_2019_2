using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VRUIOVRHandTrackingGestureController : VRUIGestureController
{
    private OVRHand ovrHand;

    private bool handTracking;

    public float minPinchStrength;

    // Start is called before the first frame update
    void Start()
    {
        handTracking = GetComponent<OVRCustomSkeleton>();
        ovrHand = GetComponent<OVRHand>();
    }

    // Update is called once per frame
    void Update()
    {
        //Handtracking gesture recognition
        if (handTracking)
        {
            OVRHand.TrackingConfidence indexFingerPointingConfidence = ovrHand.GetFingerConfidence(OVRHand.HandFinger.Index);
            float pinchStrength = ovrHand.GetFingerPinchStrength(OVRHand.HandFinger.Index);
            if (pinchStrength >= minPinchStrength)
            {
                VRUIGesture = VRUIGesture.Pinch;
            }
            else if (indexFingerPointingConfidence == OVRHand.TrackingConfidence.High && pinchStrength < minPinchStrength)
            {
                VRUIGesture = VRUIGesture.IndexPointing;
            }
            else
            {
                VRUIGesture = VRUIGesture.None;
            }
        }
        //Controller gesture recognition
        else
        {
            //Left hand
            if (Hand == HandSide.left)
            {
                if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) >= minPinchStrength)
                {
                    VRUIGesture = VRUIGesture.Pinch;
                }
                else if (!OVRInput.Get(OVRInput.Touch.PrimaryIndexTrigger))
                {
                    VRUIGesture = VRUIGesture.IndexPointing;
                }
                else
                {
                    VRUIGesture = VRUIGesture.None;
                }
            }
            //Right hand
            else
            {
                if (OVRInput.Get(OVRInput.Axis1D.SecondaryIndexTrigger) >= minPinchStrength)
                {
                    VRUIGesture = VRUIGesture.Pinch;
                }
                else if (!OVRInput.Get(OVRInput.Touch.SecondaryIndexTrigger))
                {
                    VRUIGesture = VRUIGesture.IndexPointing;
                }
                else
                {
                    VRUIGesture = VRUIGesture.None;
                }
            }
        }
    }
}