using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivateCorrectTracking : MonoBehaviour
{
    public OVRHand[] handTrackingModels;
    public GameObject[] controllerTrackingModels;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(handTrackingModels[0].IsTracked)
        {
            //Deactivate controllerTracking controllermodels
            controllerTrackingModels[0].SetActive(false);
            controllerTrackingModels[1].SetActive(false);
            //Activate handTracking controllerModels
            handTrackingModels[0].gameObject.transform.GetChild(0).gameObject.SetActive(true);
            handTrackingModels[1].gameObject.transform.GetChild(0).gameObject.SetActive(true);
        } else if (!handTrackingModels[0].IsTracked && !handTrackingModels[1].IsTracked)
        {
            //Deactivate handTracking controllermodels
            handTrackingModels[0].gameObject.transform.GetChild(0).gameObject.SetActive(false);
            handTrackingModels[1].gameObject.transform.GetChild(0).gameObject.SetActive(false);
            //Activate controllerTracking controllerModels
            controllerTrackingModels[0].SetActive(true);
            controllerTrackingModels[1].SetActive(true);
        }
    }
}
