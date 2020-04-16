using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsOnTeleportAreaChecker : ValidTeleportAreaChecker
{
    public Transform parentOfBorder;
    public float checkRange = 0.01f;
    [Tooltip("The Layer the player can teleport to.")]
    public LayerMask layerMask;
    public bool currentAreaValid;

    private OVRBoundary boundary;
    private GameObject teleportAreaChecker;
    private Vector3 dimensions;
    private Vector3[] substituteBoundary;


    private Transform topLeft;
    private Transform topRight;
    private Transform bottomRight;
    private Transform bottomLeft;
    private Transform borderTransform;

    // Start is called before the first frame update
    void Start()
    {
        substituteBoundary = new Vector3[]
        {
            new Vector3(0.5f, 0.0f, 0.5f),
            new Vector3(-0.5f, 0.0f, 0.5f),
            new Vector3(-0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 0.0f, -0.5f)
        };
        //layerMask = 1 << LayerMask.NameToLayer("Teleport");
        boundary = new OVRBoundary();
        borderTransform = parentOfBorder.GetChild(0).transform;
        //Create the boundary container Gameobject
        teleportAreaChecker = new GameObject("TeleportAreaChecker");
        //Set corner from where to check if over teleportable area
        CreateTeleportAreaCheckers();
        OVRManager.InputFocusAcquired += UpdateTeleportAreaCheckers;
    }

    // Update is called once per frame
    void Update()
    {
        CheckIfValidTeleportArea();
        currentAreaValid = IsValidTeleportArea;
    }

    private void FixedUpdate()
    {
        teleportAreaChecker.transform.position = borderTransform.position;
    }

    private void UpdateTeleportAreaCheckers()
    {
        if (OVRManager.boundary.GetConfigured())
        {
            dimensions = boundary.GetDimensions(OVRBoundary.BoundaryType.OuterBoundary);

            topLeft.localPosition = new Vector3(-(dimensions.x / 2), 0.0f, dimensions.z / 2);
            topRight.localPosition = new Vector3((dimensions.x / 2), 0.0f, dimensions.z / 2);
            bottomRight.localPosition = new Vector3(dimensions.x / 2, 0.0f, -(dimensions.z / 2));
            bottomLeft.localPosition = new Vector3(-(dimensions.x / 2), 0.0f, -(dimensions.z / 2));
        }
        else
        {
            topRight.localPosition = substituteBoundary[0];
            topLeft.localPosition = substituteBoundary[1];
            bottomLeft.localPosition = substituteBoundary[2];
            bottomRight.localPosition = substituteBoundary[3];
        }
    }

    private void CreateTeleportAreaCheckers()
    {
        if (OVRManager.boundary.GetConfigured())
        {
            dimensions = boundary.GetDimensions(OVRBoundary.BoundaryType.OuterBoundary);

            topLeft = new GameObject("TopLeftChecker").transform;
            topLeft.SetParent(teleportAreaChecker.transform);
            topLeft.localPosition = new Vector3(-(dimensions.x / 2), 0.0f, dimensions.z / 2);

            topRight = new GameObject("TopRightChecker").transform;
            topRight.SetParent(teleportAreaChecker.transform);
            topRight.localPosition = new Vector3((dimensions.x / 2), 0.0f, dimensions.z / 2);

            bottomRight = new GameObject("BottomRightChecker").transform;
            bottomRight.SetParent(teleportAreaChecker.transform);
            bottomRight.localPosition = new Vector3(dimensions.x / 2, 0.0f, -(dimensions.z / 2));

            bottomLeft = new GameObject("BottomLeftChecker").transform;
            bottomLeft.SetParent(teleportAreaChecker.transform);
            bottomLeft.localPosition = new Vector3(-(dimensions.x / 2), 0.0f, -(dimensions.z / 2));
        }
        else
        {
            topLeft = new GameObject("TopLeftChecker").transform;
            topLeft.SetParent(teleportAreaChecker.transform);

            topRight = new GameObject("TopRightChecker").transform;
            topRight.SetParent(teleportAreaChecker.transform);

            bottomRight = new GameObject("BottomRightChecker").transform;
            bottomRight.SetParent(teleportAreaChecker.transform);

            bottomLeft = new GameObject("BottomLeftChecker").transform;
            bottomLeft.SetParent(teleportAreaChecker.transform);
            topRight.localPosition = substituteBoundary[0];
            topLeft.localPosition = substituteBoundary[1];
            bottomLeft.localPosition = substituteBoundary[2];
            bottomRight.localPosition = substituteBoundary[3];
        }
    }

    private void CheckIfValidTeleportArea()
    {
        bool hitTopLeft = Physics.Raycast(topLeft.position, -topLeft.up, checkRange, layerMask);
        bool hitTopRight = Physics.Raycast(topRight.position, -topRight.up, checkRange, layerMask);
        bool hitBottomRight = Physics.Raycast(bottomRight.position, -bottomRight.up, checkRange, layerMask);
        bool hitBottomLeft = Physics.Raycast(bottomLeft.position, -bottomLeft.up, checkRange, layerMask);

        IsValidTeleportArea = (hitTopLeft && hitTopRight && hitBottomRight && hitBottomLeft);
    }
}
