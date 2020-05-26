using UnityEngine;

public class TeleportingVRUI : MonoBehaviour
{
    public bool useHandTracking;
    public OVRHand[] hands;
    public Transform playerHead;

    private bool cancelTeleport = false;
    [SerializeField, Range(1, 7)]
    private byte resolutionLevel;   // What level of resolution should the arc be  
    [SerializeField]
    private float deepestPoint = 0f; // Deepest point on the map
    [SerializeField]
    private float velocity;          // How far will the arc be
    [SerializeField]
    private Color validColor;        // Color if the arc hits a ground
    [SerializeField]
    private Color invalidColor;      // Color if the arc hits nothing
    [SerializeField]
    private float lineThickness;
    [SerializeField]
    private Material material;
    [SerializeField]
    private OVRInput.Button buttonToCheck = OVRInput.Button.One;
    [SerializeField]
    private Transform virtualTeleportButtonTransform;
    [SerializeField]
    private Transform boundaryParent;

    public ValidTeleportAreaChecker[] checkers;

    private int resolution = 0;    // Resolution of the arc (2^n)
    private Vector3 teleportPos;       // Position to be teleported to
    private Vector3[] arcPoints;         // Points that defines the arc
    private Transform circle;            // Ref to the circle object
    private Transform vrPlayer;          // Player
    private LayerMask layerMask;         // Which layer should be noted during the ray shoot
    private LineRenderer lineRenderer;

    //Changes for UnityEvent
    private bool virtualButtonIsPressed;
    //Bool to check if area is valid
    private bool chosenAreaIsValid;

#if UNITY_EDITOR
    public bool useGizmos = false;
#endif

    private void Awake()
    {
        layerMask = 1 << LayerMask.NameToLayer("Teleport");
        resolution = 1 << resolutionLevel;
        circle = transform.GetChild(0);
        arcPoints = new Vector3[resolution + 1];
        lineRenderer = gameObject.AddComponent<LineRenderer>();
        vrPlayer = FindObjectOfType<OVRCameraRig>().transform;
        //circle.GetComponent<Renderer>().sharedMaterial.color = validColor;

        SetupLineRenderer();
        //EnableAll(false);
    }

    private void SetupLineRenderer()
    {
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lineRenderer.receiveShadows = false;
        lineRenderer.allowOcclusionWhenDynamic = false;
        lineRenderer.loop = false;
        lineRenderer.material = material;
        lineRenderer.material.color = validColor;
        lineRenderer.startWidth = lineThickness;
        lineRenderer.endWidth = lineThickness;
    }

    private void FixedUpdate()
    {
        if (checkers.Length > 0)
        {
            // Check if chosen area is valid
            foreach (ValidTeleportAreaChecker checker in checkers)
            {
                if (!checker.IsValidTeleportArea)
                {
                    chosenAreaIsValid = false;
                    break;
                }
                chosenAreaIsValid = true;
            }
        }
        //If there is no checker assigned teleport should always work
        else
        {
            chosenAreaIsValid = true;
        }
        OVRInput.Controller controller = OVRInput.Controller.None;
        bool isPressed;
        if (!useHandTracking)
            isPressed = (virtualButtonIsPressed || IsButtonPressed(buttonToCheck, out controller)) && !cancelTeleport;
        else
            isPressed = WantsToTeleportHandtracking();
        EnableAll(isPressed);
        // If the trigger is pressed...
        if (isPressed)
        {
            Vector3 controllerRotationEuler, controllerPos;
            float angleX, angleY, heightFromDeepestPoint;
            //Use physical controller
            if (!virtualButtonIsPressed && !useHandTracking)
            {
                // Get position and rotation from controller relative to the CameraRig
                controllerRotationEuler = (vrPlayer.rotation * OVRInput.GetLocalControllerRotation(controller)).eulerAngles;
                angleX = -controllerRotationEuler.x * Mathf.Deg2Rad;
                angleY = (-controllerRotationEuler.y + 90.0f) * Mathf.Deg2Rad;
                controllerPos = Quaternion.AngleAxis(vrPlayer.rotation.eulerAngles.y, Vector3.up) * OVRInput.GetLocalControllerPosition(controller) + vrPlayer.position;
                heightFromDeepestPoint = controllerPos.y - deepestPoint;
            }
            //Use handTracking
            else if (useHandTracking)
            {
                // Get position and rotation from controller relative to the CameraRig
                controllerRotationEuler = (hands[0].PointerPose).eulerAngles;
                angleX = -controllerRotationEuler.x * Mathf.Deg2Rad;
                angleY = (-controllerRotationEuler.y + 90.0f) * Mathf.Deg2Rad;
                controllerPos = Quaternion.AngleAxis(vrPlayer.rotation.eulerAngles.y, Vector3.up) * (playerHead.position - new Vector3(0, 0.25f, 0));
                heightFromDeepestPoint = controllerPos.y - deepestPoint;
            }
            //Use virtual button
            else
            {
                // Get position and rotation from controller relative to the CameraRig
                controllerRotationEuler = (virtualTeleportButtonTransform.rotation).eulerAngles;
                angleX = -controllerRotationEuler.x * Mathf.Deg2Rad;
                angleY = (-controllerRotationEuler.y + 90.0f) * Mathf.Deg2Rad;
                controllerPos = Quaternion.AngleAxis(vrPlayer.rotation.eulerAngles.y, Vector3.up) * virtualTeleportButtonTransform.position;
                heightFromDeepestPoint = controllerPos.y - deepestPoint;
            }
            // Calculate arc in physics, check if a teleportable area was found and draw the arc
            CreateArc(heightFromDeepestPoint, resolution, velocity, angleX, angleY, controllerPos, arcPoints);
            int lastPoint = 0;
            Vector3 hitPos = Vector3.zero;
            Vector3 hitNormal = Vector3.zero;
            CastRay(arcPoints, out lastPoint, out hitPos, out hitNormal);
            DrawTeleport(arcPoints, hitPos, hitNormal, lastPoint);
            // Store the found valid position 
            teleportPos = hitPos;
        }
        // or if the trigger were released and something was hit AND the checker is NOT assigned
        else if (teleportPos != Vector3.zero && checkers.Length == 0 && !cancelTeleport)
        {
            // Adjusting position because we move the origin. So we have to calculate the offset from the head
            Vector3 diff = vrPlayer.position - vrPlayer.GetChild(0).GetChild(1).position;
            Vector3 rootPos = teleportPos + diff;
            rootPos.y = teleportPos.y;
            vrPlayer.position = rootPos;
            teleportPos = Vector3.zero;
        }
        // or if the trigger were released and something was hit AND the checker IS assigned
        else if (teleportPos != Vector3.zero && chosenAreaIsValid && !cancelTeleport) 
        {
            // Adjusting position because we move the origin. So we have to calculate the offset from the head
            Vector3 diff = vrPlayer.position - vrPlayer.GetChild(0).GetChild(1).position;
            Vector3 rootPos = teleportPos + diff;
            rootPos.y = teleportPos.y;
            vrPlayer.position = rootPos;
            teleportPos = Vector3.zero;
        }
        boundaryParent.transform.position = circle.position;
        if (cancelTeleport)
        {
            teleportPos = Vector3.zero;
            chosenAreaIsValid = false;
            EnableAll(false);
            cancelTeleport = false;
        }
    }

    /// <summary>
    /// Helps with teleporting logic when we use handtracking. If we grab something we make a fist with the hand that grabs the object. Teleporting should not activate when we dont want to.
    /// We try to prevent this by checking the other hand. If the other hand is using only the index finger to pinch, we assume the player wants to teleport. If any other finger is pinched, we assume the
    /// player does not want to teleport.
    /// </summary>
    /// <returns></returns>
    private bool WantsToTeleportHandtracking()
    {
        bool indexPinchedLeft = hands[0].GetFingerIsPinching(OVRHand.HandFinger.Index);
        bool indexPinchedRight = hands[1].GetFingerIsPinching(OVRHand.HandFinger.Index);
        bool otherPinchedLeft = hands[0].GetFingerIsPinching(OVRHand.HandFinger.Middle) || hands[0].GetFingerIsPinching(OVRHand.HandFinger.Pinky) || hands[0].GetFingerIsPinching(OVRHand.HandFinger.Ring) || hands[0].GetFingerIsPinching(OVRHand.HandFinger.Pinky);
        bool otherPinchedRight = hands[1].GetFingerIsPinching(OVRHand.HandFinger.Middle) || hands[1].GetFingerIsPinching(OVRHand.HandFinger.Pinky) || hands[1].GetFingerIsPinching(OVRHand.HandFinger.Ring) || hands[1].GetFingerIsPinching(OVRHand.HandFinger.Pinky);

        //If we make a fist on our left hand
        if (indexPinchedLeft && otherPinchedLeft)
        {
            if (indexPinchedRight && otherPinchedRight)
            {
                return false;
            }
        }
        //If we make a fist on our right hand
        if (indexPinchedRight && otherPinchedRight)
        {
            if (indexPinchedLeft && otherPinchedLeft)
            {
                return false;
            }
        }
        return indexPinchedLeft && indexPinchedRight && hands[0].HandConfidence == OVRHand.TrackingConfidence.High && hands[1].HandConfidence == OVRHand.TrackingConfidence.High;
    }

    public void VirtualButtonIsPressed()
    {
        virtualButtonIsPressed = true;
    }

    public void VirtualButtonIsUp()
    {
        virtualButtonIsPressed = false;
    }

    public void CancelTeleport()
    {
        virtualButtonIsPressed = false;
        cancelTeleport = true;
    }

    // Returns true when a button was pressed and stores the controller on which the trigger was pressed
    private bool IsButtonPressed(OVRInput.Button button, out OVRInput.Controller controller)
    {
        controller = OVRInput.Controller.None;
        if (OVRInput.Get(button, OVRInput.Controller.RTouch))
            controller = OVRInput.Controller.RTouch;
        else if (OVRInput.Get(button, OVRInput.Controller.LTouch))
            controller = OVRInput.Controller.LTouch;

        return (controller != OVRInput.Controller.None);
    }

    // Returns true when a trigger was pressed and stores the controller on which the trigger was pressed
    private bool IsTriggerPressed(out OVRInput.Controller controller)
    {
        controller = OVRInput.Controller.None;
        if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.RTouch))
            controller = OVRInput.Controller.RTouch;
        else if (OVRInput.Get(OVRInput.Button.PrimaryIndexTrigger, OVRInput.Controller.LTouch))
            controller = OVRInput.Controller.LTouch;

        return (controller != OVRInput.Controller.None);
    }

    // Draws an arc using the LineRenderer
    private void DrawTeleport(Vector3[] p, Vector3 hitPos, Vector3 hitNormal, int lastPoint)
    {
        bool hasHit = (hitPos != Vector3.zero); // If something was hit
        lineRenderer.positionCount = (hasHit) ? lastPoint + 1 : arcPoints.Length;
        // When a checker was assigned, use it to check if the teleport position is really valid
        if (checkers.Length > 0)
        {
            lineRenderer.material.color = (hasHit && chosenAreaIsValid) ? validColor : invalidColor;
            circle.GetComponent<Renderer>().sharedMaterial.color = (hasHit && chosenAreaIsValid) ? validColor : invalidColor;
            material.color = (hasHit && chosenAreaIsValid) ? validColor : invalidColor;
        }
        else
        {
            lineRenderer.material.color = (hasHit) ? validColor : invalidColor;
            circle.GetComponent<Renderer>().sharedMaterial.color = (hasHit) ? validColor : invalidColor;
            material.color = (hasHit && chosenAreaIsValid) ? validColor : invalidColor;
        }
        
        int maxIteration = (hasHit) ? lastPoint : arcPoints.Length;

        EnableCircle(hasHit);
        foreach (Transform child in boundaryParent)
        {
            child.gameObject.SetActive(hasHit);
        }

        for (int i = 0; i < maxIteration; i++)
            lineRenderer.SetPosition(i, arcPoints[i]);

        if (hasHit)
        {
            lineRenderer.SetPosition(lastPoint, hitPos);
            SetCircle(hitPos, hitNormal);
        }
    }

    // Sets the position and rotation of the circle
    private void SetCircle(Vector3 pos, Vector3 normal)
    {
        if (circle != null)
        {
            circle.transform.position = pos + normal * 0.0001f;
            circle.transform.LookAt(-normal + pos);
        }
    }

    // Enables/Disables the LineRenderer and Circle (Visible Objects)
    private void EnableAll(bool isEnabled)
    {
        lineRenderer.enabled = isEnabled;
        EnableCircle(isEnabled);
        //boundaryParent.gameObject.SetActive(isEnabled);
        foreach (Transform child in boundaryParent)
        {
            child.gameObject.SetActive(isEnabled);
        }
    }

    // Enables/Disables Circle GameObject
    private void EnableCircle(bool isEnabled)
    {
        circle.gameObject.SetActive(isEnabled);
    }

    // Calculates an arc using 3D projectile motion, returns points in world space
    private void CreateArc(float deepest, int res, float velo, float angX, float angY, Vector3 controllerPos, Vector3[] p)
    {
        float cosX = Mathf.Cos(angX);
        float sinX = Mathf.Sin(angX);
        float cosY = Mathf.Cos(angY);
        float sinY = Mathf.Sin(angY);
        float height = deepest + (velo * velo * sinX * sinX) / 19.62f;
        float totalTime = (velo * sinX) / 9.81f + Mathf.Sqrt(height * 2.0f / 9.81f);
        float partTime = totalTime / res;
        float time = partTime;
        p[0] = controllerPos;   // First position is the origin of the arc

        for (int i = 1; i < (res + 1); i++)
        {
            p[i].x = velo * cosX * cosY * time;
            p[i].y = velo * sinX * time - 0.5f * 9.81f * time * time;
            p[i].z = velo * cosX * sinY * time;
            p[i] += controllerPos;  // Adds position of the controller e.g. the origin of the arc
            time += partTime;
        }
    }

    // Checks if a teleportable area is in the way of the arc. Shoots a ray every second calculated position
    private void CastRay(Vector3[] p, out int lastPoint, out Vector3 hitPos, out Vector3 hitNormal)
    {
        lastPoint = 0;
        hitPos = Vector3.zero;
        hitNormal = Vector3.zero;

        int length = p.Length - 1;
        int i = 0;
        RaycastHit hit = new RaycastHit();

        while (i < length)
        {
            if (!Physics.Linecast(p[i], p[i + 2], out hit, layerMask))
                i += 2;
            else
            {
                lastPoint = i + 1;
                break;
            }
        }

        hitPos = hit.point;
        hitNormal = hit.normal;
        // Needed to specify preciser which ray was the last
        if (i < length)
        {
            if (Physics.Linecast(p[i], (p[i + 1]), out hit, layerMask))
            {
                lastPoint = i;
                hitPos = hit.point;
                hitNormal = hit.normal;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!useGizmos)
            return;

        layerMask = 1 << LayerMask.NameToLayer("Teleport");
        arcPoints = new Vector3[32 + 1];
        float angleX = this.transform.rotation.eulerAngles.x * Mathf.Deg2Rad;
        float angleY = this.transform.rotation.eulerAngles.y * Mathf.Deg2Rad;

        Vector3 controllerPos = this.transform.position;
        float heightFromDeepestPoint = controllerPos.y - deepestPoint;

        CreateArc(heightFromDeepestPoint, 32, velocity, angleX, angleY, controllerPos, arcPoints);
        int lastPoint = 0;
        Vector3 hitPos = Vector3.zero;
        Vector3 hitNormal = Vector3.zero;
        CastRay(arcPoints, out lastPoint, out hitPos, out hitNormal);

        if (hitPos != Vector3.zero)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < lastPoint; i++)
                Gizmos.DrawLine(arcPoints[i], arcPoints[i + 1]);
            Gizmos.DrawLine(arcPoints[lastPoint], hitPos);
        }
        else
        {
            Gizmos.color = Color.red;
            for (int i = 1; i < arcPoints.Length; i++)
                Gizmos.DrawLine(arcPoints[i - 1], arcPoints[i]);
        }
    }
#endif
}
