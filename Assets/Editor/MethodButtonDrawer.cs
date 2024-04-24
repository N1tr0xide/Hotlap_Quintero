using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.Reflection;

[CanEditMultipleObjects]
[CustomEditor(typeof(MonoBehaviour), true)]
public class MethodButtonDrawer : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var type = target.GetType(); //target is the MonoBehaviour

        foreach (var method in type.GetMethods(BindingFlags.Public| BindingFlags.Instance | BindingFlags.NonPublic))
        {
            var attributes = method.GetCustomAttributes(typeof(ExposeMethodInEditor), true);
            if (attributes.Length <= 0) continue;
            
            if (GUILayout.Button("Run:" + method.Name))
            {
                ((MonoBehaviour)target).Invoke(method.Name, 0f);
            }
        }
    }
}
