# Floor Adjuster 自動設定の実装計画（NDMF対応版）

この計画は、`ViewPoint Scaler` ツールに「Floor Adjuster」（Narazaka製）の自動設定オプションを追加する方法について記述します。
Modular Avatar (MA) との互換性を確保するため、Editor上の処理ではなく、**NDMF (Non-Destructive Modular Framework)** のビルドプロセス中に計算と適用を行います。

## 目的
NDMFビルドパイプライン内でアバターの足の頂点を解析し、`FloorAdjuster` の高さを自動設定する機能を実装します。これにより、MAによって結合されたメッシュ（靴など）も正しく考慮されます。

## ユーザーレビューが必要な項目
> [!WARNING]
> **Floor Adjuster への依存とリフレクション**: `net.narazaka.vrchat.floor_adjuster` のセットアップメソッドを呼び出すために、C#のリフレクションを使用します。APIが変更された場合、この機能は動作しなくなる可能性があります。

## 設計上の決定事項

### Q&Aへの回答に基づく方針
1.  **FloorAdjusterの自動追加**: 
    - 可能です。Narazaka Floor Adjusterのエディタ拡張クラスをリフレクションで検索し、セットアップメソッド（例: `Setup`）を呼び出すことで、標準的な手順でオブジェクトを生成します。
2.  **位置合わせのタイミング**: 
    - **NDMFビルド時 (`Building` または `Transforming` フェーズ)** に行います。これにより、エディタ上では存在しないがビルド時に生成・結合されるメッシュ（MA Merge Armatureなど）を考慮できます。
3.  **NDMFでの頂点検索**: 
    - はい、NDMFプラグインとして実装し、アバターの全てのレンダラー（結合済み含む）を対象に頂点検索を行います。
4.  **実行順序**: 
    - `Modular Avatar` (Mesh生成) **→** `ViewPoint Scaler` (高さ計算 & FloorAdjuster位置更新) **→** `Floor Adjuster` (適用) の順序を確保します。

## 提案される変更

### Editor / Runtime
#### [MODIFY] [ViewPointScaler.cs](file:///c:/Users/dennn/programming/Unity/Avatar_scale_specify/Runtime/ViewPointScaler.cs) (※ファイルが存在しない場合作成)
- インスペクター設定用のプロパティを追加:
    - `public bool AutoConfigureFloorAdjuster = false;`

#### [MODIFY] [ViewPointScaleProcessor.cs](file:///c:/Users/dennn/programming/Unity/Avatar_scale_specify/Editor/ViewPointScaleProcessor.cs)
- `ApplyFloorAdjuster` メソッドを追加。
    - **頂点検索ロジック**:
        - アバター内の全ての `SkinnedMeshRenderer` を取得。
        - Humanoid Bone（LeftFoot, RightFoot, LeftToes, RightToes）にウェイトを持つ頂点をフィルタリング。
        - ワールド座標（またはAvatar Rootローカル座標）での最小Y値を計算。
    - **Floor Adjuster 制御**:
        - "FloorAdjuster" コンポーネント/オブジェクトを探す。
        - 存在しない場合、リフレクションを用いて作成を試みる。
        - オブジェクトのTransformを計算した高さに合わせて移動させる。

#### [MODIFY] [ViewPointScalerPlugin.cs](file:///c:/Users/dennn/programming/Unity/Avatar_scale_specify/Editor/ViewPointScalerPlugin.cs)
- 新しいパス `AutoSetFloorAdjusterPass` を追加し、登録します。
- 実行順序定義:
    ```csharp
    InPhase(BuildPhase.Transforming)
        .AfterPlugin("nadena.dev.modular-avatar")
        .BeforePlugin("net.narazaka.vrchat.floor_adjuster")
        .Run(AutoSetFloorAdjusterPass.Instance);
    ```

## 検証計画

### 自動テスト（ビルド検証）
1.  **MA環境でのテスト**:
    - アバターに靴のPrefabをMAで着せる。
    - `ViewPointScaler` の `AutoConfigureFloorAdjuster` をONにする。
    - ビルド実行（PlayMode突入）。
    - 生成されたアバターを確認し、FloorAdjusterが正しく機能して接地しているか確認。
    - Consoleログで "Found lowest foot vertex at Y=..." を確認。

### 手動検証
1.  **自動生成未済のケース**:
    - FloorAdjusterがない状態でビルドし、自動追加されるか確認。
