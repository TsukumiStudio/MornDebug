using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [CustomEditor(typeof(MornDebugGlobal))]
    internal sealed class MornDebugGlobalEditor : Editor
    {
        private List<Type> _menuTypes;

        private void OnEnable()
        {
            _menuTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsSubclassOf(typeof(MornDebugMenuBase)) && !t.IsAbstract)
                .OrderBy(t => t.Name)
                .ToList();
        }

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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("メニュー追加", EditorStyles.boldLabel);

            var hasMissing = false;
            foreach (var type in _menuTypes)
            {
                var exists = menus != null && menus.Any(m => m != null && m.GetType() == type);
                if (exists) continue;
                hasMissing = true;
                if (GUILayout.Button($"{type.Name} を作成して登録"))
                {
                    CreateAndRegister(global, type);
                }
            }

            if (!hasMissing)
            {
                EditorGUILayout.HelpBox("全てのメニューが登録済みです。", MessageType.Info);
            }
        }

        private void CreateAndRegister(MornDebugGlobal global, Type type)
        {
            var globalPath = AssetDatabase.GetAssetPath(global);
            var dir = System.IO.Path.GetDirectoryName(globalPath);
            var path = System.IO.Path.Combine(dir, $"{type.Name}.asset");
            path = AssetDatabase.GenerateUniqueAssetPath(path);
            var menu = CreateInstance(type);
            AssetDatabase.CreateAsset(menu, path);
            var so = serializedObject;
            var prop = so.FindProperty("_menus");
            prop.arraySize++;
            prop.GetArrayElementAtIndex(prop.arraySize - 1).objectReferenceValue = menu;
            so.ApplyModifiedProperties();
            AssetDatabase.SaveAssets();
            MornDebugGlobal.Logger.Log($"{type.Name}を作成・登録しました。");
        }
    }
}
