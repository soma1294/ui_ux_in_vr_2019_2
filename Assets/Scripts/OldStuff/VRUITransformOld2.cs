using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
[ExecuteInEditMode]
public class VRUITransformOld2 : MonoBehaviour
{
    private readonly static string NO_ANCHOR_DEFINED_MESSAGE = "No anchor defined! This should not happen! MiddleCenter anchor set as anchor!";

    [SerializeField]
    [HideInInspector]
    private Vector3 posOnPanel = Vector3.zero;
    [SerializeField]
    [HideInInspector]
    private Vector3 rotOnPanel = Vector3.zero;
    [SerializeField]
    [HideInInspector]
    private VRUIAnchor anchor = VRUIAnchor.MiddleCenter;
    [SerializeField]
    [HideInInspector]
    private Bounds boundsOfParent;
    [SerializeField]
    //[HideInInspector]
    private Vector3 anchorPosition = Vector3.zero;

    [SerializeField]
    [HideInInspector]
    private VRUIAnchor oldAnchor = VRUIAnchor.MiddleCenter;

    private void Reset()
    {
        //On undo, we want to Reposition the panel...
        Undo.undoRedoPerformed -= RepositionElement;
        Undo.undoRedoPerformed += RepositionElement;
        //Reorient the panel...
        Undo.undoRedoPerformed -= ReorientElement;
        Undo.undoRedoPerformed += ReorientElement;
        //And setup the anchor
        Undo.undoRedoPerformed -= SetupAnchor;
        Undo.undoRedoPerformed += SetupAnchor;
        PositionOnPanel = Vector3.zero;
        RotationOnPanel = Vector3.zero;
        Anchor = VRUIAnchor.MiddleCenter;
        oldAnchor = VRUIAnchor.MiddleCenter;
        anchorPosition = Vector3.zero;
        if (HasParentVRUITransformOld2())
        {
            SetupAnchor();
        }
        RepositionElement();
        ReorientElement();
    }

    private void OnDestroy()
    {
        //Remove our fuction from the delegate when this Monobehaviour is destroyed
        Undo.undoRedoPerformed -= RepositionElement;
        Undo.undoRedoPerformed -= ReorientElement;
        Undo.undoRedoPerformed -= SetupAnchor;
    }

    /// <summary>
    /// The anchoring of objects with a VRUITransformOld2 component only behaves correctly with other VRUITransformOld2 components. That is why we need a way to check
    /// if the parent has a VRUITransformOld2 component before we do anything with the anchors.
    /// </summary>
    /// <returns>Whether or not the objects parent has a VRUITransformOld2 component</returns>
    public bool HasParentVRUITransformOld2()
    {
        //If there is no parent at all we do not need to check for a VRUITransformOld2 in the parent.
        if (transform.parent)
        {
            VRUITransformOld2 parent = transform.parent.gameObject.GetComponent<VRUITransformOld2>();
            return parent;
        }
        return false;
    }

    public void SetupAnchor()
    {
        if (!HasParentVRUITransformOld2())
            return;
        //We save the old anchor position so we can use it to calculate the correct PositionOnPanel after the anchor change.
        Vector3 oldAnchorPosition = AnchorPosition;
        //Get bounding box of the parent to calculate the anchorpoints if no bounds were defined
        if (transform.parent.GetComponent<VRUIPanelBehaviour>())
        {
            boundsOfParent = transform.parent.GetComponent<VRUIPanelBehaviour>().PanelBounds;
            //Debug.Log("Panel: " + boundsOfParent);
        }
        else if (transform.parent.GetComponent<MeshFilter>())
        {
            //Mesh.bounds is called because it has its bounding box aligned to local space
            boundsOfParent = transform.parent.gameObject.GetComponent<MeshFilter>().sharedMesh.bounds;
            //Debug.Log("Meshfilter: " + boundsOfParent);
        }
        else
        {
            //If we find no reference for the bounding box we just create a default box with size one.
            boundsOfParent = new Bounds(transform.parent.position, Vector3.one);
            //Debug.Log("No bounds found");
        }
        //Check which anchorpoint was chosen and calculate the position of said anchor.
        switch (anchor)
        {
            case VRUIAnchor.TopLeft:
                anchorPosition = transform.parent.position + -(transform.right * boundsOfParent.extents.x) + (transform.up * boundsOfParent.extents.y);
                break;
            case VRUIAnchor.TopCenter:
                anchorPosition = transform.parent.position + (transform.up * boundsOfParent.extents.y);
                break;
            case VRUIAnchor.TopRight:
                anchorPosition = transform.parent.position + (transform.right * boundsOfParent.extents.x) + (transform.up * boundsOfParent.extents.y);
                break;
            case VRUIAnchor.MiddleLeft:
                anchorPosition = transform.parent.position + -(transform.right * boundsOfParent.extents.x);
                break;
            case VRUIAnchor.MiddleCenter:
                anchorPosition = transform.parent.position;
                break;
            case VRUIAnchor.MiddleRight:
                anchorPosition = transform.parent.position + (transform.right * boundsOfParent.extents.x);
                break;
            case VRUIAnchor.BottomLeft:
                anchorPosition = transform.parent.position + -(transform.right * boundsOfParent.extents.x) + -(transform.up * boundsOfParent.extents.y);
                break;
            case VRUIAnchor.BottomCenter:
                anchorPosition = transform.parent.position + -(transform.up * boundsOfParent.extents.y);
                break;
            case VRUIAnchor.BottomRight:
                anchorPosition = transform.parent.position + (transform.right * boundsOfParent.extents.x) + -(transform.up * boundsOfParent.extents.y);
                break;
            default:
                anchor = VRUIAnchor.MiddleCenter;
                anchorPosition = transform.parent.position;
                Debug.LogError(NO_ANCHOR_DEFINED_MESSAGE);
                break;
        }
        if (oldAnchor != Anchor)
        {
            PositionOnPanel = oldAnchorPosition - AnchorPosition + PositionOnPanel;
        }
        oldAnchor = Anchor;
    }

    /// <summary>
    /// Repositions the gameobject according to the Vector3 value in PositionOnPanel.
    /// If the gameobject has a parented gameobject with a VRUITransformOld2 component it sets the local position in relation to the AnchorPosition and the PositionOnPanel.
    /// If the gameobject doesn't have a parented gameobject with a VRUITransformOld2 component it sets transform.positon as PositionOnPanel.
    /// </summary>
    public void RepositionElement()
    {
        transform.position = (transform.right * PositionOnPanel.x) + (transform.up * PositionOnPanel.y) + (transform.forward * PositionOnPanel.z) + AnchorPosition;
    }

    /// <summary>
    /// Changes the rotation of the gameobject according to the value in RotationOnPanel.
    /// If the gameobject has a parented gameobject with a VRUITransformOld2 component it sets the local euler roation to RotationOnPanel.
    /// If the gameobject doesn't have a parented gameobject with a VRUITransformOld2 component it sets the global euler rotation to RotationOnPanel.
    /// </summary>
    public void ReorientElement()
    {
        transform.localEulerAngles = RotationOnPanel;
    }

    public Vector3 PositionOnPanel
    {
        get { return posOnPanel; }
        set { posOnPanel = value; }
    }

    public Vector3 RotationOnPanel
    {
        get { return rotOnPanel; }
        set { rotOnPanel = value; }
    }

    public VRUIAnchor Anchor
    {
        get { return anchor; }
        set
        {
            //oldAnchor = Anchor;
            anchor = value;
            //SetupAnchor();
        }
    }

    public Vector3 AnchorPosition
    {
        get { return anchorPosition; }
    }

    public Bounds BoundsOfParent
    {
        get { return boundsOfParent; }
        set { boundsOfParent = value; }
    }
}
