using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MornLib
{
    [CustomEditor(typeof(MornDebugGlobal))]
    internal sealed class MornDebugGlobalEditor : Editor
    {
        private static readonly (string label, Type type)[] BuiltinMenuTypes =
        {
            ("セーブ", typeof(MornDebugSaveMenu)),
            ("サウンド", typeof(MornDebugSoundMenu)),
            ("時間操作", typeof(MornDebugTimeScaleMenu)),
            ("リロード", typeof(MornDebugReloadMenu)),
            ("シーン一覧", typeof(MornDebugSceneListMenu)),
        };

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
            EditorGUILayout.LabelField("ビルトインメニュー", EditorStyles.boldLabel);

            var anyCreated = false;
            foreach (var (label, type) in BuiltinMenuTypes)
            {
                var exists = menus != null && menus.Any(m => m != null && m.GetType() == type);
                if (exists) continue;

                if (GUILayout.Button($"{label}（{type.Name}）を作成して登録"))
                {
                    CreateAndRegister(global, type);
                    anyCreated = true;
                }
            }

            if (!anyCreated)
            {
                var allExists = BuiltinMenuTypes.All(t => menus != null && menus.Any(m => m != null && m.GetType() == t.type));
                if (allExists)
                {
                    EditorGUILayout.HelpBox("全てのビルトインメニューが登録済みです。", MessageType.Info);
                }
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
