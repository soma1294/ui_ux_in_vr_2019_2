using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

//[CustomEditor(typeof(VRUIPanelBehaviour), true)]
public class VRUIPanelBehaviourEditorOld : Editor
{
    VRUIPanelBehaviour m_target;

    private void OnEnable()
    {
        m_target = (VRUIPanelBehaviour)target;
        if(PrefabUtility.GetPrefabAssetType(m_target) != PrefabAssetType.Regular)
            m_target.RedrawPanel();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        DrawSizeFields();
    }

    private void DrawSizeFields()
    {
        float panelSizeX = m_target.PanelSizeX;
        float panelSizeY = m_target.PanelSizeY;

        //We need to check if something changed in the inspector so we can record the change for Unity's undo history.
        EditorGUI.BeginChangeCheck();
        panelSizeX = EditorGUILayout.FloatField("Panel Size X", panelSizeX);
        panelSizeY = EditorGUILayout.FloatField("Panel Size Y", panelSizeY);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RegisterCompleteObjectUndo(new Object[] { m_target.GetComponent<VRUIPanelBehaviour>(), m_target.transform }, "VRUI Panel Size");

            if (panelSizeX < 0)
                panelSizeX = 0;
            if (panelSizeY < 0)
                panelSizeY = 0;

            m_target.PanelSizeX = panelSizeX;
            m_target.PanelSizeY = panelSizeY;
            m_target.RedrawPanel();
        }
    }

    public void OnSceneGUI()
    {
        m_target = (VRUIPanelBehaviour)target;

        DrawSizeChooserGizmo();
    }

    private void DrawSizeChooserGizmo()
    {
        float panelSizeX = m_target.PanelSizeX;
        float panelSizeY = m_target.PanelSizeY;

        float size = HandleUtility.GetHandleSize(m_target.gameObject.transform.position) * 0.7f;

        //We need to check if something changed in the inspector so we can record the change for Unity's undo history.
        EditorGUI.BeginChangeCheck();
        Handles.color = Color.magenta;
        panelSizeX = Handles.ScaleSlider(panelSizeX, m_target.gameObject.transform.position, m_target.gameObject.transform.right, m_target.gameObject.transform.rotation, size, 0.5f);
        Handles.color = Color.yellow;
        panelSizeY = Handles.ScaleSlider(panelSizeY, m_target.gameObject.transform.position, m_target.gameObject.transform.up, m_target.gameObject.transform.rotation, size, 0.5f);
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObjects(new Object[] { m_target.GetComponent<VRUIPanelBehaviour>(), m_target.transform}, "Undo VRUI Panel Size");

            m_target.PanelSizeX = panelSizeX;
            m_target.PanelSizeY = panelSizeY;
            m_target.RedrawPanel();
            
        }
    }
}
