using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsInColliderChecker : ValidTeleportAreaChecker
{
    public Transform parentOfBorder;
    public float colliderHeight = 2.0f;
    public float colliderScale = 1.0f;
    [Tooltip("The layers that will be ignored by the checker.")]
    public LayerMask layerMask;
    public bool currentAreaValid;

    private OVRBoundary boundary;
    private Vector3 dimensions;
    private GameObject colliderContainer;

    private Transform borderTransform;
    private BoxCollider boxCollider;
    private BoundaryColliderBehaviour boundaryColliderBehaviour;
    private Collider otherCollider;
    private Vector3 heightOffset;

    // Start is called before the first frame update
    void Start()
    {
        heightOffset = new Vector3(0.0f, colliderHeight / 2, 0.0f);
        //layerMask = (1 << LayerMask.NameToLayer("Teleport")) | (1 << LayerMask.NameToLayer("Player")) | (1 << LayerMask.NameToLayer("VRUI"));
        boundary = new OVRBoundary();
        borderTransform = parentOfBorder.GetChild(0).transform;
        colliderContainer = new GameObject("ColliderContainer");
        boxCollider = colliderContainer.AddComponent<BoxCollider>();
        boxCollider.isTrigger = true;
        boundaryColliderBehaviour = colliderContainer.AddComponent<BoundaryColliderBehaviour>();
        UpdateColliderSize();
        OVRManager.InputFocusAcquired += UpdateColliderSize;
    }

    // Update is called once per frame
    void Update()
    {
        currentAreaValid = IsValidTeleportArea;
    }

    private void FixedUpdate()
    {
        colliderContainer.transform.position = borderTransform.position + heightOffset * colliderScale;
        CheckIfValidTeleportArea();
    }

    private void UpdateColliderSize()
    {
        if (OVRManager.boundary.GetConfigured())
        {
            dimensions = boundary.GetDimensions(OVRBoundary.BoundaryType.OuterBoundary);
            boxCollider.size = (dimensions + new Vector3(0.0f, colliderHeight, 0.0f)) * colliderScale;
        }
        else
        {
            boxCollider.size = new Vector3(1.0f, colliderHeight, 1.0f) * colliderScale;
        }
    }

    private void CheckIfValidTeleportArea()
    {
        if (!boundaryColliderBehaviour.otherCollider)
        {
            IsValidTeleportArea = true;
            return;
        }
        IsValidTeleportArea = layerMask == (layerMask | (1 << boundaryColliderBehaviour.otherCollider.gameObject.layer));
    }
}
