# ワークフロー / デバッグ / FAQ

ViewPoint Scalerと他ツール (特にFloorAdjuster) を組み合わせる際の実務的な手順と、発生しやすい問題の対処方法をまとめました。

## 推奨ビルド手順
1. **FloorAdjusterセットアップ**
   - by skeleton モード: アバターを右クリックして `Setup FloorAdjuster` → Gizmoで床位置を合わせる。
   - by scale モード: Armatureに `Floor Adjuster (by scale)` を追加し、`Height` を設定。
2. **ViewPointScalerの配置**
   - アバタールートに `ViewPointScaler` を追加し、`Target Eye Height` を指定。
   - ルートに `EditorOnly` タグを付けないこと (NDMFで削除される)。
3. **NDMF Test Build**
   - NDMFウィンドウで `Test Build` を実行し、エラーウィンドウでログを確認。
   - `[ViewPointScaler]` ログと `FloorAdjuster` のログが両方出ていることを確認。
4. **VRChat Build & Publish**
   - 問題がなければ通常のVRChatアップロードに進む。

## 代表的なログキャプチャ
```
[FloorAdjuster] Processed pass ...
[ViewPointScaler] Captured target height: 1.60
[ViewPointScaler] Starting processing for Avatar(Clone)
[ViewPointScaler] Measured Current Height: 1.45
[ViewPointScaler] Calculated Scale Factor: 1.103
[ViewPointScaler] Updating ViewPosition (Direct Y/Z Scale): (0.00, 1.25, 0.05) -> (0.00, 1.38, 0.06)
```
- FloorAdjuster後にViewPointScalerが走っているかが確認ポイント。
- `Updating ViewPosition` のログでY/Zが広がっていれば、VRChat内でも視点高さが変わる。

## トラブルシューティング
| 症状 | 原因 | 対処 |
| --- | --- | --- |
| `ViewPointScaler component not present` | アバターにコンポーネントが無い / IEditorOnlyにより早期削除 | Resolvingフェーズで値が退避されるよう `ViewPointScalerRuntimeData` を確認。必要ならコンポーネントを付け直す。 |
| FloorAdjusterより先にViewPointScalerが実行される | `AfterPlugin` 名称が一致していない or FloorAdjuster未導入 | `ViewPointScalerPlugin` の `AfterPlugin` 列挙に `net.narazaka.vrchat.floor_adjuster` が含まれていることを確認。 |
| ViewPositionが変化しない | VRChatはRoot ScaleをViewPositionに反映しない | `UpdateDescriptorView` でY/Zを直接スケールする実装を確認。ログに`Direct Y/Z Scale`が出ているか見る。 |
| ルートがビルド時に削除 | ルートに `EditorOnly` タグ | `ViewPointScaler` の `OnValidate` 警告に従いタグを外す。 |
| NullReferenceException (CaptureSettings) | ResolverでRuntimeData生成に失敗 | スクリプトリロード・Unity再起動、または `ViewPointScalerRuntimeData.cs` が正しくEditorアセンブリに含まれているか確認。 |

## FloorAdjusterとの高度な使い分け
- **視点基準の順序:** 先にFloorAdjusterで「地面位置」を確定してからViewPointScalerで「目標高さ」を計算する。逆にすると、FloorAdjusterの高さが目標値を上書きする恐れがある。
- **by scaleモードの前後ズレ:** FloorAdjuster (by scale) はViewPositionのZ方向も補正するため、本ツールもZ値を同倍率で拡大して整合性を保っている。もし独自カメラ位置を使う場合は、Z補正をカスタマイズしてもよい。
- **by skeletonモード:** Humanoid AvatarのScale書き換え後にViewPositionを再評価する必要がある。必要に応じて、`ScaleAvatarPass` の前に `Animator.avatar` の更新が完了しているか確認するログを入れると安心。

## デバッグTips
- `Assets/Editor/Avatar_scale_specify/Editor/ViewPointScaleProcessor.cs` の `Debug.Log` 出力を活用。必要に応じて `#if VRC_DEVELOPMENT_BUILD` などでログレベルを切り替え可能。
- 問題が再現しない場合は、`ViewPointScalerBuildHook` (VRCSDKフック) を一時的に有効化してNDMF依存をバイパスする方法も有効。両者の出力を比較すると、NDMFフェーズの差分を素早く把握できます。

## チェックリスト
- [ ] ViewPointScalerコンポーネントがRoot階層に存在する
- [ ] FloorAdjuster (いずれかのモード) を適用済みで、`FloorAdjuster`コンポーネントがScene上に存在
- [ ] NDMF TestBuildでPassの順序が `Capture -> FloorAdjuster -> Scale` になっている
- [ ] ビルドログに `Updating ViewPosition (Direct Y/Z Scale)` が表示されている
- [ ] VRChat上で実際の視点高さが目標値に近い

このドキュメントは運用メモとして追加・改訂していくことを想定しています。新しいTipsや問題事例を見つけたら追記してください。