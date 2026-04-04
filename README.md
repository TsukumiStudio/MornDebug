# MornDebug

<p align="center">
  <img src="https://img.shields.io/github/license/TsukumiStudio/MornDebug" alt="License" />
</p>

## 概要

階層型デバッグメニューフレームワーク。EditorWindow・ランタイムUIの両方に対応し、ScriptableObjectベースで拡張可能。

## 導入方法

Unity Package Manager で以下の Git URL を追加:

```
https://github.com/TsukumiStudio/MornDebug.git?path=src#1.0.0
```

`Window > Package Manager > + > Add package from git URL...` に貼り付けてください。

### 依存パッケージ

- [MornGlobal](https://github.com/TsukumiStudio/MornGlobal) (`com.tsukumistudio.mornglobal`)
- [UniTask](https://github.com/Cysharp/UniTask) (`com.cysharp.unitask`)

## 機能

- **階層メニュー** — パス区切り（`/`）でフォルダ風に整理されたデバッグメニュー
- **動的登録** — `MornDebugCore.RegisterGUI` でランタイムからメニューを追加・削除
- **EditorWindow** — `Tools > MornDebug` から開けるエディタウィンドウ
- **ランタイムUI** — `MornDebugUI` コンポーネントでゲーム内にデバッグメニューを表示
- **ScriptableObject拡張** — `MornDebugMenuBase` を継承してカスタムメニューを作成可能

### ビルトインメニュー

- **セーブ/データ削除** — PlayerPrefsリセット
- **サウンド** — AudioMixerの全公開パラメータをスライダーで制御
- **チート/時間操作** — Time.timeScaleの調整
- **リロード** — シーン再読み込み、Domain/Scene Reload（Editor）
- **シーン一覧** — BuildSettingsのシーンをツリー表示（Editor）

## 使い方

### セットアップ

1. `Tools > MornDebug` でEditorWindowを開く
2. MornDebugGlobalアセットが自動作成される
3. InspectorでBuiltinMenuの作成ボタンを押す

### カスタムメニューの追加

```csharp
[CreateAssetMenu(menuName = "Morn/Debug/MyMenu")]
public sealed class MyDebugMenu : MornDebugMenuBase
{
    public override IEnumerable<(string key, Action action)> GetMenuItems()
    {
        yield return ("カスタム/ボタン", () =>
        {
            if (GUILayout.Button("実行"))
            {
                // 処理
            }
        });
    }
}
```

### ランタイムから動的に登録

```csharp
MornDebugCore.RegisterGUI("動的メニュー/情報", () =>
{
    GUILayout.Label($"FPS: {1f / Time.deltaTime:F0}");
}, destroyCancellationToken);
```

## ライセンス

[The Unlicense](LICENSE)
