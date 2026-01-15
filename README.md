# MornDebug

## 概要

ゲーム開発中に使用できるインゲーム・エディタ統合デバッグメニューUIフレームワーク。

## 依存関係

| 種別 | 名前 |
|------|------|
| Mornライブラリ | MornLib, MornEditor, MornGlobal, MornProcess |

## 使い方

### カスタムメニューの作成

```csharp
[CreateAssetMenu(menuName = "Morn/Debug/CustomMenu")]
public sealed class MyDebugMenu : MornDebugMenuBase
{
    public override IEnumerable<(string, Action)> GetMenuItems()
    {
        yield return ("カテゴリ/項目", () => {
            GUILayout.Button("操作ボタン");
        });
    }
}
```

### Runtime表示

```csharp
MornDebugUI.Show(); // UI表示
MornDebugUI.Hide(); // UI非表示
```

### 動的な項目追加・削除

```csharp
MornDebugCore.RegisterGUI("キー", () => { /* 処理 */ });
MornDebugCore.UnregisterGUI("キー");
```

### エディタウィンドウ

`Tools > MornDebugWindow` でエディタ上でもデバッグメニューを使用できます。

### 組み込みメニュー

- セーブマネージャ/データ削除
- サウンド/音量スライダー
- チート/時間操作
- リロード/シーン再読み込み
