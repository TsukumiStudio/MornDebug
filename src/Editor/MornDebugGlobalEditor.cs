using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [CustomEditor(typeof(MornDebugGlobal))]
    internal sealed class MornDebugGlobalEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("Menusの変更はDomain Reload後に反映されます。すぐに反映したい場合は下のボタンを押してください。", MessageType.Info);
            if (GUILayout.Button("Reload Domain"))
            {
                EditorUtility.RequestScriptReload();
            }

            var global = (MornDebugGlobal)target;
            var menus = global.Menus;
            if (menus != null && menus.OfType<MornDebugBuiltinMenu>().Any()) return;
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("MornDebugBuiltinMenuが未登録です。", MessageType.Info);
            if (GUILayout.Button("MornDebugBuiltinMenuを作成して登録"))
            {
                var globalPath = AssetDatabase.GetAssetPath(global);
                var dir = System.IO.Path.GetDirectoryName(globalPath);
                var path = System.IO.Path.Combine(dir, $"{nameof(MornDebugBuiltinMenu)}.asset");
                path = AssetDatabase.GenerateUniqueAssetPath(path);
                var menu = CreateInstance<MornDebugBuiltinMenu>();
                AssetDatabase.CreateAsset(menu, path);
                var so = serializedObject;
                var prop = so.FindProperty("_menus");
                prop.arraySize++;
                prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = menu;
                so.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                MornDebugGlobal.Logger.Log("MornDebugBuiltinMenuを作成・登録しました。");
            }
        }
    }
}