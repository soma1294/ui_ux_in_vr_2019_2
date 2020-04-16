using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GuardianBoundaryMeshGenerator : MonoBehaviour
{
    public float borderThickness = 0.05f;
    public Transform parentOfBorder;
    public Transform centerEyeAnchor;
    //public GameObject playerMarkerPref;
    //public float playerMarkerFloatHeight = 0.15f;
    public Material guardianBorderMaterial;
    public bool useLineRenderer;
    public float simplifyByAmount;

    private Mesh meshTop;
    private Mesh meshBottom;

    private Vector3[] boundaryVert;
    private Vector3[] outerBoundaryVert;
    private Vector3[] allVert;
    private int[] trianglesTop;
    private int[] trianglesBottom;
    private Vector3 playerPosition = Vector3.zero;
    private Quaternion playerRotation;
    private Transform playerMarkerTransform;
    private Vector3[] substituteBoundary;

    private OVRBoundary boundary;
    private GameObject boundaryContainer;
    private LineRenderer lineRenderer;
    private void Awake()
    {
        boundaryContainer = new GameObject("GuardianBoundary");
        boundaryContainer.transform.SetParent(parentOfBorder);
        substituteBoundary = new Vector3[]
        {
            new Vector3(0.5f, 0.0f, 0.5f),
            new Vector3(-0.5f, 0.0f, 0.5f),
            new Vector3(-0.5f, 0.0f, -0.5f),
            new Vector3(0.5f, 0.0f, -0.5f)
        };
    }
    void Start()
    {
        if (!OVRManager.boundary.GetConfigured())
            Debug.LogError("No Valid Guardian found!");
        boundary = new OVRBoundary();
        meshTop = new Mesh();
        meshTop.name = "TopMesh";
        meshBottom = new Mesh();
        meshBottom.name = "BottomMesh";
        //Set the player marker
        //playerPosition = new Vector3(centerEyeAnchor.transform.localPosition.x, centerEyeAnchor.transform.localPosition.z, -playerMarkerFloatHeight);
        //playerRotation.eulerAngles = parentOfBorder.rotation.eulerAngles;
        //playerMarkerTransform = Instantiate(playerMarkerPref, playerPosition, Quaternion.identity, parentOfBorder).transform;
        if (!useLineRenderer)
        {
            //Create the boundary container Gameobject
            boundaryContainer.AddComponent<MeshFilter>();
            boundaryContainer.AddComponent<MeshRenderer>().material = guardianBorderMaterial;
            //Create mesh data
            OVRManager.InputFocusAcquired += CreateBoundaryMeshData;
            OVRManager.InputFocusAcquired += UpdateMesh;
            CreateBoundaryMeshData();
            UpdateMesh();
            UpdateBoundaryPosition();
            //boundaryContainer.AddComponent<OutlineCreator>().material = guardianBorderMaterial;
        }
        else
        {
            lineRenderer = boundaryContainer.AddComponent<LineRenderer>();
            lineRenderer.loop = true;
            lineRenderer.material = guardianBorderMaterial;
            lineRenderer.startWidth = borderThickness / 10;
            lineRenderer.endWidth = borderThickness / 10;
            CreateLineRendererBoundary();
            UpdateBoundaryPosition();
            OVRManager.InputFocusAcquired += CreateLineRendererBoundary;
        }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateBoundaryPosition();
    }

    private void CreateLineRendererBoundary()
    {
        //If there is no valid Guardian set up we cant get the vertices of the boundary
        if (OVRManager.boundary.GetConfigured())
        {
            boundaryVert = boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
            lineRenderer.positionCount = boundaryVert.Length;
            lineRenderer.SetPositions(boundaryVert);
            lineRenderer.useWorldSpace = false;
            boundaryContainer.transform.localPosition = new Vector3(boundaryContainer.transform.localPosition.x, boundaryContainer.transform.localPosition.y + 0.002f, boundaryContainer.transform.localPosition.z);
        }
        else
        {
            lineRenderer.positionCount = 4;
            lineRenderer.SetPositions(substituteBoundary);
            lineRenderer.useWorldSpace = false;
            boundaryContainer.transform.localPosition = new Vector3(boundaryContainer.transform.localPosition.x, boundaryContainer.transform.localPosition.y + 0.002f, boundaryContainer.transform.localPosition.z);
        }
        lineRenderer.Simplify(simplifyByAmount);
    }

    private void CreateBoundaryMeshData()
    {
        //If there is no valid Guardian set up we cant get the vertices of the boundary
        if (OVRManager.boundary.GetConfigured())
        {
            boundaryVert = boundary.GetGeometry(OVRBoundary.BoundaryType.OuterBoundary);
        }
        else
        {
            boundaryVert = substituteBoundary;
        }
        outerBoundaryVert = new Vector3[boundaryVert.Length];
        for (int i = 0; i < outerBoundaryVert.Length; i++)
        {
            outerBoundaryVert[i] = boundaryVert[i] * (1.0f + borderThickness);
        }

        allVert = new Vector3[boundaryVert.Length * 2];
        for (int i = 0, j = 0; j < allVert.Length; i++, j += 2)
        {
            allVert[j] = boundaryVert[i];
            allVert[j + 1] = outerBoundaryVert[i];
        }

        trianglesTop = new int[(allVert.Length - 2) * 3 + 6];
        int ti = 0, vi = 0;
        for (; ti < trianglesTop.Length - 6; ti += 6, vi += 2)
        {
            trianglesTop[ti + 5] = 0 + vi;
            trianglesTop[ti + 4] = trianglesTop[ti + 1] = 2 + vi;
            trianglesTop[ti + 3] = trianglesTop[ti + 2] = 1 + vi;
            trianglesTop[ti] = 3 + vi;
        }
        trianglesTop[ti + 5] = 0 + vi;
        trianglesTop[ti + 4] = trianglesTop[ti + 1] = 0;
        trianglesTop[ti + 3] = trianglesTop[ti + 2] = 1 + vi;
        trianglesTop[ti] = 1;

        trianglesBottom = new int[(allVert.Length - 2) * 3 + 6];
        ti = 0;
        vi = 0;
        for (; ti < trianglesBottom.Length - 6; ti += 6, vi += 2)
        {
            trianglesBottom[ti] = 0 + vi;
            trianglesBottom[ti + 1] = trianglesBottom[ti + 4] = 2 + vi;
            trianglesBottom[ti + 2] = trianglesBottom[ti + 3] = 1 + vi;
            trianglesBottom[ti + 5] = 3 + vi;
        }
        trianglesBottom[ti] = 0 + vi;
        trianglesBottom[ti + 1] = trianglesBottom[ti + 4] = 0;
        trianglesBottom[ti + 2] = trianglesBottom[ti + 3] = 1 + vi;
        trianglesBottom[ti + 5] = 1;
        Debug.Log("MeshData Created");
    }

    private void UpdateMesh()
    {
        meshTop.Clear();

        meshTop.vertices = allVert;
        meshTop.triangles = trianglesTop;
        meshTop.RecalculateNormals();

        meshBottom.Clear();

        meshBottom.vertices = allVert;
        meshBottom.triangles = trianglesBottom;
        meshBottom.RecalculateNormals();

        Vector3 scale = transform.localScale;
        Quaternion rot = transform.rotation;
        Vector3 pos = transform.position;

        transform.localScale = Vector3.one;
        transform.rotation = Quaternion.identity;
        transform.position = Vector3.zero;

        CombineInstance[] instances = new CombineInstance[2];
        instances[0].mesh = meshTop;
        instances[0].transform = transform.localToWorldMatrix;
        instances[1].mesh = meshBottom;
        instances[1].transform = transform.localToWorldMatrix;

        Mesh combinedMesh = new Mesh();
        combinedMesh.name = "CombinedMesh";
        combinedMesh.CombineMeshes(instances, true);
        boundaryContainer.GetComponent<MeshFilter>().mesh = combinedMesh;

        transform.localScale = scale;
        transform.rotation = rot;
        transform.position = pos;
    }

    private void UpdateBoundaryPosition()
    {
        boundaryContainer.transform.localPosition = new Vector3(-centerEyeAnchor.localPosition.x, boundaryContainer.transform.localPosition.y, -centerEyeAnchor.localPosition.z);
    }
}
