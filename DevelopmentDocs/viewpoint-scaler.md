# ViewPoint Scaler 実装ガイド

NDMFを使って「FloorAdjuster後に視点高さを合わせる」処理を実装する際の着眼点とコード対応をまとめます。

## アーキテクチャ概要
```
ViewPointScaler (MonoBehaviour, Runtime, IEditorOnly)
        │ TargetEyeHeight (ユーザー入力)
        ▼
CaptureSettingsPass (NDMF / Resolving phase)
        │ ViewPointScalerRuntimeData(TargetEyeHeight)
        ▼
ScaleAvatarPass (NDMF / Transforming phase)
        ├─ 計測: VRCAvatarDescriptor + ViewPosition
        ├─ Root Scale = Target / Current
        └─ Descriptor.ViewPosition.Y/Z *= ScaleFactor
```

### 主要コンポーネント
| ファイル | 役割 |
| --- | --- |
| `Runtime/ViewPointScaler.cs` | ユーザー設定用。`IEditorOnly`によりビルド対象から除去。値をNDMF Resolvingフェーズで読み取る。 |
| `Editor/ViewPointScaleProcessor.cs` | 実際のスケーリングロジックをまとめた静的クラス。 |
| `Editor/ViewPointScalerPlugin.cs` | NDMFプラグイン宣言。FloorAdjuster後に処理を走らせる。 |
| `Editor/ViewPointScalerRuntimeData.cs` | IEditorOnly除去前に設定値をコピーして保持するための隠しコンポーネント。 |

#### Separate Scaler Object メニュー
`GameObject/VRChat Utility/Add ViewPoint Scaler Child` を実行すると、選択中のアバター直下に `AvatarScaler` という子オブジェクトを自動生成し、その子に `ViewPointScaler` を付与できます。設定済みの子オブジェクトごとコピーして別アバターへ貼り付けられるため、複数アバター間で統一したスケール設定を共有したい場合に便利です。

## 処理フロー詳細
1. **ユーザー設定:** アバターRootに `ViewPointScaler` を追加し、`Target Eye Height (m)` を設定。
2. **Resolvingフェーズ:** `CaptureSettingsPass` が `ViewPointScaler` を探し、内部コンポーネント `ViewPointScalerRuntimeData` に `TargetEyeHeight` を退避。IEditorOnlyはこの後の工程で自動削除されるが、退避データは残る。
3. **FloorAdjuster処理:** `net.narazaka.vrchat.floor_adjuster` / `nadena.dev.ndmf.floor_adjuster` が Transformingフェーズで床合わせを実行。
4. **Transformingフェーズ後段:** `ScaleAvatarPass` が `ViewPointScaleProcessor.TryApply()` を呼び出し、以下の手順でスケーリング:
   - `MeasureViewHeight`: DescriptorのViewPositionをワールド座標に変換し、Ground(Y=AvatarRoot.position.y)からの高さを算出。
   - `scaleFactor = targetHeight / currentHeight`
   - `avatarRoot.transform.localScale *= scaleFactor`
   - `descriptor.ViewPosition.y/z *= scaleFactor` (Xはそのまま)
   - `CleanupScalers`: 退避データと元コンポーネントを複製側から削除。
5. **ログ:** `[ViewPointScaler] ...` として途中経過を `Debug.Log` に出力。NDMF ErrorReportWindowから確認可能。

## コード断片
### 設定値の退避
```csharp
var scaler = FindScaler(avatarRoot);
var data = avatarRoot.GetOrAddComponent<ViewPointScalerRuntimeData>();
data.TargetEyeHeight = scaler.TargetEyeHeight;
```
※ 実際は独自ヘルパーではなく `GetComponent` + `AddComponent` で実装。

### FloorAdjuster後のスケーリング
```csharp
var currentHeight = MeasureViewHeight(descriptor, avatarRoot.transform);
var scaleFactor = targetHeight / currentHeight;
avatarRoot.transform.localScale *= scaleFactor;
UpdateDescriptorView(descriptor, scaleFactor);
```

### ViewPosition補正
```csharp
var oldView = descriptor.ViewPosition;
var newView = new Vector3(oldView.x, oldView.y * scaleFactor, oldView.z * scaleFactor);
descriptor.ViewPosition = newView;
```
VRChatはRoot ScaleをViewPositionに反映しないため、FloorAdjuster(by scale)と同様にY/Zのみ直接拡大している点がポイントです。

## テストのポイント
- **NDMF TestBuild** で `Capture ViewPointScaler settings` → `Scale avatar root to target view height` の順にパスが走るか確認。
- ログに `Measured Current Height` と `Calculated Scale Factor` が表示されているかをチェック。異常値の場合はFloorAdjuster前に処理されていない可能性大。
- 失敗時は `report` が `ViewPointScaler component (or captured data) not present.` 等の文言になるため、ログフィルタで `[ViewPointScaler]` を検索すれば手掛かりを得られます。

## 追加アイデア
- `ViewPointScaler` の値をScriptableObject化して複数アバターで共有する。
- `ScaleAvatarPass` 内で `AnimationClip` のルートモーション修正など、追加の後処理をチェーンする。
- FloorAdjusterの `by skeleton` モードと組み合わせる場合は、Humanoid AvatarのScale書換後に再度 `VRCAvatarDescriptor.ViewPosition` を再計測する拡張も検討できます。
