using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody))]
[ExecuteInEditMode]
public class VRUISliderBehaviour : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The minimal value that can be set with this slider.")]
    private float minValue = 0f;
    [SerializeField]
    [Tooltip("Tha maximum value that can be set with this slider.")]
    private float maxValue = 100f;
    [SerializeField]
    [Tooltip("The value where the knob starts. Can not be higher than the maxValue or lower than the minValue.")]
    private float startValue;
    [SerializeField]
    [Tooltip("The length of the drawn path.")]
    private float lengthOfPath = 1f;
    [SerializeField]
    [Tooltip("The width of the drawn path.")]
    private float widthOfPath = 0.05f;
    [SerializeField]
    [Tooltip("The currently set value.")]
    private float currentValue;
    [SerializeField]
    [Tooltip("The z-position of the knob, relative to the path.")]
    float zPositionKnob = -0.025f;
    [SerializeField]
    [Tooltip("How fast the knob adjusts its position. 0 means it never reaches the new position, 1 it reaches it almost instantly.")]
    private float stifness = 0.5f;
    [SerializeField]
    [Tooltip("If the object/hand that touches the knob is farther away than this, the knob wont be moved.")]
    private float maxAllowedDistanceToMove;
    [SerializeField]
    [Tooltip("If this is true, it is assumed that the GestureController component is on the hand/object that can touch the knob.\n" +
             "If this is false, any trigger collider can interact with the knob.")]
    private bool useGestureController = true;
    [SerializeField]
    [Tooltip("Choose here which gesture can activate this button.")]
    private VRUIGesture gesture;
    [SerializeField]
    [Tooltip("The material used to render the path.")]
    private Material pathMaterial;
    private VRUIVibration vibrationBehaviour;
    [SerializeField]
    private GameObject path;
    [SerializeField]
    private GameObject physicalKnob;
    //OnValueChanged
    [System.Serializable]
    public class VRUISliderEvent : UnityEvent<float> { }
    [SerializeField]
    private VRUISliderEvent m_OnValueChanged = new VRUISliderEvent();
    public VRUISliderEvent onValueChanged
    {
        get { return m_OnValueChanged; }
        set { m_OnValueChanged = value; }
    }
    private float oldValue;

    //Variables that store the position and other such values of the path.
    private Vector3 startOfPath;
    private Vector3 endOfPath;
    //Variables that store values of the touching finger
    private Transform touchingObjectTransform;
    private bool knobIsTouched;
    private Vector3 startTouchPosition;
    private Vector3 startTouchKnobPosition;
    private Vector3 currentTouchPosition;
    private Vector3 deltaTouchPosition;
    private VRUIGestureController gestureController;
    private VRUIGestureController gestureControllerToMonitor;

    // Start is called before the first frame update
    void Start()
    {
        //Setup the variables
        if (startValue > maxValue)
            startValue = maxValue;
        if (startValue < minValue)
            startValue = minValue;
        if (path) {
            //Create the path visuals
            CreatePath();
        }
        if (physicalKnob) {
            //Place handle at the start position
            SetupSliderKnobPosition();
        }
        oldValue = currentValue;
        gestureController = null;
        gestureControllerToMonitor = null;
    }

    // Update is called once per frame
    void Update()
    {
        startOfPath = new Vector3(0.0f, -lengthOfPath / 2, 0.0f);
        endOfPath = new Vector3(0.0f, lengthOfPath / 2, 0.0f);
        if (path && physicalKnob)
            UpdateCurrentValue();
        if (Application.isPlaying)
        {
            if ((gestureControllerToMonitor && gestureControllerToMonitor.VRUIGesture != gesture) || Vector3.Distance(touchingObjectTransform.position, PhysicalKnob.transform.position) > maxAllowedDistanceToMove)
            {
                knobIsTouched = false;
                gestureControllerToMonitor = null;
                touchingObjectTransform = null;
            }
            if (currentValue != oldValue)
            {
                m_OnValueChanged.Invoke(CurrentValue);
            }
            oldValue = currentValue;
        }
    }

    private void FixedUpdate()
    {
        if (knobIsTouched)
        {
            currentTouchPosition = touchingObjectTransform.position;
            //deltaTouchPosition = currentTouchPosition - startTouchPosition;
            deltaTouchPosition = currentTouchPosition - startTouchKnobPosition;
            Vector3 localDeltaTouchPosition = transform.worldToLocalMatrix * deltaTouchPosition;
            Vector3 targetPosition;

            targetPosition.x = 0f;
            targetPosition.y = PhysicalKnob.transform.localPosition.y + localDeltaTouchPosition.y;
            targetPosition.z = zPositionKnob;
            if (targetPosition.y >= endOfPath.y)
            {
                targetPosition = new Vector3(0f, endOfPath.y, zPositionKnob);
            }
            else if (targetPosition.y <= startOfPath.y)
            {
                targetPosition = new Vector3(0f, startOfPath.y, zPositionKnob);
            }
            PhysicalKnob.transform.localPosition = Vector3.Lerp(PhysicalKnob.transform.localPosition, targetPosition, stifness);
        }
    }

    public void UpdateKnobPosition()
    {
        if (MaxValue == 0)
        {
            MaxValue = 1;
        }
        float distance = Vector3.Distance(startOfPath, endOfPath);
        Vector3 targetPos = (endOfPath - startOfPath).normalized 
                          * distance 
                          * ((CurrentValue - minValue) / (MaxValue - minValue))
                          - (endOfPath - startOfPath).normalized * (distance/2);
        PhysicalKnob.transform.localPosition = new Vector3(targetPos.x, targetPos.y, targetPos.z + zPositionKnob);
    }

    public void CreatePath()
    {
        startOfPath = new Vector3(0.0f, -lengthOfPath / 2, 0.0f);
        endOfPath = new Vector3(0.0f, lengthOfPath / 2, 0.0f);
        LineRenderer pathRenderer = path.GetComponent<LineRenderer>();
        pathRenderer.positionCount = 2;
        pathRenderer.SetPositions(new Vector3[] {
            startOfPath,
            endOfPath
        });
        pathRenderer.useWorldSpace = false;
        pathRenderer.startWidth = pathRenderer.endWidth = widthOfPath;
        pathRenderer.material = pathMaterial;
        pathRenderer.alignment = LineAlignment.TransformZ;
        pathRenderer.generateLightingData = true;
    }

    private void SetupSliderKnobPosition()
    {
        float distance = Vector3.Distance(startOfPath, endOfPath);
        Vector3 targetPos = (endOfPath - startOfPath).normalized
                          * distance
                          * ((StartValue - minValue) / (MaxValue - minValue))
                          - (endOfPath - startOfPath).normalized * (distance / 2);
        PhysicalKnob.transform.localPosition = new Vector3(targetPos.x, targetPos.y, targetPos.z + zPositionKnob);
    }

    //TODO: FIX see Update Knob Position
    private void UpdateCurrentValue()
    {
        Vector3 knobWithoutZ = new Vector3(PhysicalKnob.transform.localPosition.x, PhysicalKnob.transform.localPosition.y);
        float distanceKnobToStart = Vector3.Distance(knobWithoutZ, startOfPath);
        float distance = Vector3.Distance(endOfPath, startOfPath);

        if (distanceKnobToStart <= 0)
        {
            CurrentValue = MinValue;
        } else if (distanceKnobToStart >= distance)
        {
            CurrentValue = MaxValue;
        }
        else
        {
            CurrentValue = (distanceKnobToStart / distance * MaxValue) - (distanceKnobToStart / distance * MinValue) + MinValue;
        }
    }

    public void AddValueToCurrentValue(float value)
    {
        float newValue = currentValue + value;
        if (newValue >= maxValue) newValue = maxValue;
        if (newValue <= minValue) newValue = minValue;
        UpdateKnobPosition();
    }

    private void OnTriggerEnter(Collider other)
    {
        //Debug.Log("TriggerEnterSlider: " + other.gameObject.name);
        gestureController = other.attachedRigidbody.gameObject.GetComponent<VRUIGestureController>();
        if (useGestureController)
        {
            if (!gestureController)
                return;
            //Debug.Log("sliderGestureEnter= " + gestureController.VRUIGesture);
            if (gestureController.VRUIGesture != gesture)
                return;
        }
        touchingObjectTransform = other.transform;

        startTouchPosition = touchingObjectTransform.position;
        startTouchKnobPosition = PhysicalKnob.transform.position;
        currentTouchPosition = touchingObjectTransform.position;

        knobIsTouched = true;
    }

    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("TriggerEnterSlider: " + other.gameObject.name);
        gestureController = other.attachedRigidbody.gameObject.GetComponent<VRUIGestureController>();
        if (useGestureController)
        {
            if (!gestureController)
                return;
            //Debug.Log("sliderGestureEnter= " + gestureController.VRUIGesture);
            if (gestureController.VRUIGesture != gesture)
                return;
        }
        touchingObjectTransform = other.transform;

        startTouchPosition = touchingObjectTransform.position;
        startTouchKnobPosition = PhysicalKnob.transform.position;
        currentTouchPosition = touchingObjectTransform.position;

        knobIsTouched = true;
    }
    /*
    private void OnTriggerExit(Collider other)
    {
        if (useGestureController)
        {
            gestureController = other.attachedRigidbody.gameObject.GetComponent<VRUIGestureController>();
            if (!gestureController)
                return;
        }
        touchingObjectTransform = null;
        knobIsTouched = false;
        gestureControllerToMonitor = gestureController;
    }*/

    //TODO: Aufpassen ob Formel robust genung für kleinen Max und grossen Min Wert ist.
    public float MinValue
    {
        get { return minValue; }
        set { minValue = value; }
    }

    public float MaxValue
    {
        get { return maxValue; }
        set { maxValue = value; }
    }

    public float StartValue
    {
        get { return startValue; }
        set
        {
            if (value < minValue)
                startValue = minValue;
            else if (value > maxValue)
                startValue = maxValue;
            else
                startValue = value;
        }
    }

    public float LengthOfPath
    {
        get { return lengthOfPath; }
        set { lengthOfPath = Mathf.Abs(value); }
    }

    public float WidthOfPath
    {
        get { return widthOfPath; }
        set { widthOfPath = Mathf.Abs(value); }
    }

    public float CurrentValue
    {
        get { return currentValue; }
        set { currentValue = value; }
    }

    public Material PathMaterial
    {
        get { return pathMaterial; }
        set { pathMaterial = value; }
    }

    public VRUIVibration VibrationBehaviour
    {
        get { return vibrationBehaviour; }
        set { vibrationBehaviour = value; }
    }

    public GameObject Path
    {
        get { return path; }
        set { path = value; }
    }

    public GameObject PhysicalKnob
    {
        get { return physicalKnob; }
        set { physicalKnob = value; }
    }
}
