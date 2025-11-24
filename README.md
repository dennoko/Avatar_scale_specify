# VRC ViewPoint Scaler

## 概要
VRChatアバターの接地調整（例: FloorAdjuster）後に、目標とする視点の高さへ自動でスケールを合わせるUnityエディタ拡張です。アバターに `ViewPointScaler` コンポーネントで目標の高さ(m)を設定するだけで、ビルド時に複製されたアバターへ非破壊で倍率が適用されます。

## 必要環境 / 依存関係
- Unity 2021.3 以降想定
- VRChat SDK3 - Avatars
- nadena.dev NDMF（推奨）
  - FloorAdjuster等の接地ツールより後に実行されるよう `AfterPlugin` で順序制御済み
- NDMFが存在しない環境では `IVRCSDKPreprocessAvatarCallback` によるフォールバックが自動で動作します

## 導入手順
1. `Assets/Avatar_scale_specify` 配下を任意のUnityプロジェクトにコピー（`Runtime`, `Editor` 共に必要）。
2. Unityを再起動またはスクリプトリロードして、メニュー `VRChat Utility/ViewPoint Scaler` が追加されたことを確認します。

## 使い方
1. スケール調整したいアバターのルート（`VRCAvatarDescriptor` と同じ階層）を選択し、`VRChat Utility/ViewPoint Scaler` メニューから `ViewPointScaler` を追加。
2. インスペクターで `Target Eye Height (m)` を設定。
   - ギズモ表示で現在高さ（赤）と目標高さ（水色）が可視化されます。
3. FloorAdjusterなどの接地ツールを実行し、アバターをY=0に合わせます。
4. VRChatのビルド/アップロードを開始すると、NDMF Transformingフェーズで `ViewPointScaleProcessor` が倍率を計算し、複製アバターのRoot ScaleとDescriptorのViewPositionを自動調整します。
5. 成功するとコンソールに `[ViewPointScaler] Scaled avatar by ...` が出力され、元のシーンには変更が残りません。

## 詳細仕様
- スケール倍率 = `Target Eye Height / 現在高さ`
- Root Transformに一律スケールを掛け、必要に応じて `VRCAvatarDescriptor.ViewPosition` のY値を再計算
- `VRCAvatarDescriptor.ViewPosition` も同じ倍率でスケーリングし、カメラ位置がアバターの新しい頭の高さに追従
- ビルド完了後、`ViewPointScaler` コンポーネントは複製側から削除
- 極端に小さい/大きい値に対するクランプは行わないため、入力値に注意

## 実行順序とフォールバック
- `ViewPointScalerPlugin` が `BuildPhase.Transforming` で `nadena.dev.ndmf.floor_adjuster` と `nadena.dev.modular-avatar` の後に実行されるよう指定
- NDMFが無効な場合は `ViewPointScalerBuildHook` (callbackOrder: `int.MaxValue - 512`) が同等処理を実行

## トラブルシューティング
| 症状 | 対処 |
| --- | --- |
| ログが出ずスケールされない | FloorAdjuster/NDMFが古い場合、`ViewPointScalerPlugin` の `AfterPlugin` 名称が一致しているか確認。またはVRCSDKビルドフックで動作するかチェック。 |
| 目標値と差が残る | 接地前にスケール処理が走っている可能性。必ずFloorAdjuster等でY=0に揃えてからビルドする。 |
| アバタールートが丸ごと削除される | アバターのルートに `EditorOnly` タグが付いていると、NDMFの `ClearEditorOnlyTags` でアバター全体が破棄されます。`ViewPointScaler` のGameObjectにはタグを付けないでください。旧バージョンで自動付与されていた場合は `Default` などに戻してください。 |

## ファイル構成
```
Assets/
├── Avatar_scale_specify/Runtime/ViewPointScaler.cs
└── Editor/Avatar_scale_specify/
    ├── README.md (本ファイル)
    ├── Editor/ViewPointScaleProcessor.cs
    ├── Editor/ViewPointScalerMenu.cs
    ├── Editor/ViewPointScalerPlugin.cs
    └── Editor/ViewPointScalerBuildHook.cs
```

## ライセンス
リポジトリ全体のライセンス方針に従ってください。
