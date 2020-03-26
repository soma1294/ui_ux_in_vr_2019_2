using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(VRUITransformOld2)), CanEditMultipleObjects]
public class VRUITransformOld2Editor : Editor
{
    //Context menu strings
    private const string STRING_RESET_POSITION_UNDO = "VRUITransformOld2 Reset Position";
    private const string STRING_RESET_ROTATION_UNDO = "VRUITransformOld2 Reset Rotation";
    //InspectorGUI strings
    private const string VRUITransformOld2_DISABLED_WARNING = "Element is child of an object with the VRUIScrollPanelComponent.\nDisabled the VRUITransformOld2 position field.";
        //Position
    private const string LOCAL_POSITION_LABEL = "Local Position";
    private const string LOCAL_POSITION_TOOLTIP = "The elements position on the parent VRUITransformOld2 object in relation to the chosen anchor.";
    private const string GLOBAL_POSITION_LABEL = "Position";
    private const string GLOBAL_POSITION_TOOLTIP = "The elements position in worldspace.";
    private const string STRING_POSITION_UNDO = "VRUI Position";
        //Rotation
    private const string ROTATION_LABEL = "Local Rotation";
    private const string ROTATION_TOOLTIP = "The elements local rotation (euler angles).";
    private const string STRING_ROTATION_UNDO = "VRUI Rotation";
        //Anchor
    private const string ANCHOR_LABEL = "Anchor";
    private const string ANCHOR_TOOLTIP = "The elements chosen anchor. At position (0, 0, 0) the element has the same position as the anchor.";
    private const string STRING_ANCHOR_UNDO = "Undo VRUI Anchor change";
    //Properties
    SerializedProperty positionOnPanelProp;
    SerializedProperty rotationOnPanelProp;
    SerializedProperty anchorProp;

    VRUITransformOld2 m_target;

    [MenuItem("CONTEXT/VRUITransformOld2/Reset Position")]
    static void ResetPosition(MenuCommand command)
    {
        VRUITransformOld2 target = (VRUITransformOld2)command.context;
        Undo.RecordObjects(new Object[] { target.GetComponent<VRUITransformOld2>(), target.transform }, STRING_RESET_POSITION_UNDO);
        target.PositionOnPanel = Vector3.zero;
        target.RepositionElement();
    }

    [MenuItem("CONTEXT/VRUITransformOld2/Reset Rotation")]
    static void ResetRotation(MenuCommand command)
    {
        VRUITransformOld2 target = (VRUITransformOld2)command.context;
        Undo.RecordObjects(new Object[] { target.GetComponent<VRUITransformOld2>(), target.transform }, STRING_RESET_ROTATION_UNDO);
        target.RotationOnPanel = Vector3.zero;
        target.ReorientElement();
    }

    private void OnEnable()
    {
        m_target = (VRUITransformOld2)target;

        positionOnPanelProp = serializedObject.FindProperty("posOnPanel");
        rotationOnPanelProp = serializedObject.FindProperty("rotOnPanel");
        anchorProp = serializedObject.FindProperty("anchor");

        //m_target.transform.hideFlags = HideFlags.NotEditable | HideFlags.HideInInspector;
        //Tools.hidden = true;
    }

    private void OnDisable()
    {
        Tools.hidden = false;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        //The position and rotation of the objects is controlled by the VRUIScrollPanelBehaviour, so it does not make sense to show the inspector controls.
        //TODO: Blocking the right choice?
        if (!m_target.gameObject.GetComponent<VRUIScrollPanelBehaviour>())
        {
            //We draw our custom transform fields.
            DrawCustomTransformPosition();
            DrawCustomTransformRotation();
            //Anchoring only works when the object has a VRUITransformOld2 on its parent object.
            if (m_target.HasParentVRUITransformOld2())
            {
                EditorGUILayout.Space();
                DrawAnchorDropdown();
            }
        } else
        {
            EditorGUILayout.HelpBox(VRUITransformOld2_DISABLED_WARNING, MessageType.Warning);
            DrawCustomTransformRotation();
        }

        if (serializedObject.ApplyModifiedProperties())
        {
            foreach (VRUITransformOld2 vrTrans in targets)
            {
                if ((PrefabUtility.GetPrefabAssetType(vrTrans) != PrefabAssetType.Regular) || (PrefabUtility.GetPrefabAssetType(vrTrans) != PrefabAssetType.Variant))
                {
                    //vrTrans.ReorientElement();
                    vrTrans.RepositionElement();
                    if(vrTrans.HasParentVRUITransformOld2())
                        vrTrans.SetupAnchor();
                }
            }
        }
    }

    private void DrawCustomTransformPosition()
    {
        string posLabel;
        string posTooltip;
        if (m_target.HasParentVRUITransformOld2())
        {
            posLabel = LOCAL_POSITION_LABEL;
            posTooltip = LOCAL_POSITION_TOOLTIP;
        }
        else
        {
            posLabel = GLOBAL_POSITION_LABEL;
            posTooltip = GLOBAL_POSITION_TOOLTIP;
        }
        positionOnPanelProp.vector3Value = EditorGUILayout.Vector3Field(new GUIContent(posLabel, posTooltip), positionOnPanelProp.vector3Value);
    }
    
    private void DrawCustomTransformRotation()
    {
        rotationOnPanelProp.vector3Value = EditorGUILayout.Vector3Field(new GUIContent(ROTATION_LABEL, ROTATION_TOOLTIP), rotationOnPanelProp.vector3Value);
    }

    private void DrawAnchorDropdown()
    {
        string[] choices = System.Enum.GetNames(typeof(VRUIAnchor));

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel(new GUIContent(ANCHOR_LABEL, ANCHOR_TOOLTIP));
        anchorProp.intValue = EditorGUILayout.Popup(anchorProp.intValue, choices);
        EditorGUILayout.EndHorizontal();
    }

    public void OnSceneGUI()
    {
        //DrawCustomPositionHandleGizmo();
    }
    /*
    private void DrawCustomPositionHandleGizmo()
    {
        Vector3 position = m_target.transform.position;
        Quaternion rotation = m_target.transform.rotation;
        EditorGUI.BeginChangeCheck();
        
        // right Axis
        GUI.SetNextControlName(name);
        Handles.color = Handles.xAxisColor;
        Vector3 newPos = Handles.Slider(position, rotation * Vector3.right);

        // Up Axis
        GUI.SetNextControlName(name);
        Handles.color = Handles.yAxisColor;
        newPos += Handles.Slider(position, rotation * Vector3.up) - position;

        // Forward Axis
        GUI.SetNextControlName(name);
        Handles.color = Handles.zAxisColor;
        newPos += Handles.Slider(position, rotation * Vector3.forward) - position;

        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObjects(new Object[] { m_target.GetComponent<VRUITransformOld2>(), m_target.transform }, STRING_POSITION_UNDO);
            m_target.PositionOnPanel = Vector3.one;
            m_target.RepositionElement();
        }
    }
    */
    private void DrawCustomPositionHandleGizmo()
    {
        Vector3 startPosition = m_target.transform.position;
        Quaternion startRotation = m_target.transform.rotation;
        //We need to check if something changed in the inspector so we can record the change for Unity's undo history.
        EditorGUI.BeginChangeCheck();
        Vector3 endPosition = Handles.PositionHandle(startPosition, startRotation);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObjects(new Object[] { m_target.GetComponent<VRUITransformOld2>(), m_target.transform }, STRING_POSITION_UNDO);
            Vector3 positionOnPanel = endPosition - startPosition - m_target.AnchorPosition;
            m_target.PositionOnPanel = positionOnPanel;
            m_target.RepositionElement();
        }
    }

    /*
    private void DrawCustomPositionHandleGizmo()
    {
        Vector3 positionOnPanel = m_target.PositionOnPanel;

        float size = HandleUtility.GetHandleSize(m_target.transform.position) * 6;

        float startX = positionOnPanel.x;

        //We need to check if something changed in the inspector so we can record the change for Unity's undo history.
        EditorGUI.BeginChangeCheck();
        Handles.color = Handles.xAxisColor;
        float xValue = Handles.ScaleValueHandle(startX, m_target.transform.position, Quaternion.LookRotation(m_target.transform.right, m_target.transform.up), size, Handles.ArrowHandleCap, 0.1f);
        float deltaX = xValue - startX;
        if(deltaX != 0)
            Debug.Log("delta=" + deltaX);

        if (positionOnPanel.x > 0)
            deltaX = -deltaX;

        if (deltaX != 0)
            Debug.Log("Changed delta=" + deltaX);
        //Handles.color = Handles.yAxisColor;
        //float yValue = Handles.ScaleValueHandle(positionOnPanel.y, m_target.transform.position, Quaternion.LookRotation(m_target.transform.up, -m_target.transform.forward), size, Handles.ArrowHandleCap, 0.1f);
        //Handles.color = Handles.zAxisColor;
        //float zValue = Handles.ScaleValueHandle(positionOnPanel.z, m_target.transform.position, Quaternion.LookRotation(m_target.transform.forward, m_target.transform.up), size, Handles.ArrowHandleCap, 0.1f);
        positionOnPanel = new Vector3(startX - deltaX, 0, 0);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObjects(new Object[] { m_target.GetComponent<VRUITransformOld2>(), m_target.transform }, STRING_POSITION_UNDO);

            m_target.PositionOnPanel = positionOnPanel;
            m_target.RepositionElement();
        }
    }
    */
}
