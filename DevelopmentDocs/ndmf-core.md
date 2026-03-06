# NDMF基礎: Plugin / Pass / Build Pipeline

NDMF (Non-Destructive Modular Framework) は、VRChatアバターのビルド工程を拡張するためのモジュラーフレームワークです。ここではViewPoint Scalerで利用している要素に絞って解説します。

## なぜNDMFを使うのか
- **非破壊性:** ビルド対象のアバターを複製し、複製側にのみ処理を適用するため元Prefabを汚染しません。
- **順序制御:** `BuildPhase` と `AfterPlugin` で他ツールとの実行順を厳密に制御できます (FloorAdjuster後に実行したい等)。
- **依存関係の明示:** プラグイン名を宣言しておけば、別作者の処理とも安全に共存できます。

## 主要コンセプト
### Plugin<T>
- 1つのアセンブリからエクスポートされるNDMF拡張の単位。
- `Configure()` 内で「どのフェーズで、どのPassを、どの順序で動かすか」を宣言します。
- ViewPoint Scalerでは `ViewPointScalerPlugin : Plugin<ViewPointScalerPlugin>` が該当。

### Pass<T>
- 実際の処理本体。`Execute(BuildContext ctx)` でアバター複製にアクセスします。
- 1つのPlugin内で複数のPassを組み合わせられます (設定キャプチャ用、実変更用など)。
- `DisplayName` はNDMF UIのログ表示に使われるため、処理内容が分かる名前を付けるとデバッグが容易です。

### BuildPhase
- NDMFがアバターを料理する際の「工程」。主なフェーズ:
  - `Resolving`: 依存解決や初期データ収集向き。IEditorOnlyがまだ削除されていないタイミング。
  - `Transforming`: 実際にTransformを書き換える処理向き。FloorAdjusterもここで動作。
  - `Optimizing` など他フェーズもありますが、ViewPoint Scalerでは上記2つを利用。

### BuildContext
- Pass実行時に渡されるコンテキスト。
- `AvatarRootObject`, `AssetContainer`, `GetOrAddComponent<T>()` 等を通じて複製アバターへアクセス。
- 今回は `context.AvatarRootObject` から `VRCAvatarDescriptor` やカスタムコンポーネントを探索しています。

## 処理順序の宣言方法
```csharp
InPhase(BuildPhase.Transforming)
    .AfterPlugin("net.narazaka.vrchat.floor_adjuster")
    .AfterPlugin("nadena.dev.ndmf.floor_adjuster")
    .AfterPlugin("nadena.dev.modular-avatar")
    .Run(ScaleAvatarPass.Instance);
```
- `AfterPlugin` は「対象プラグインが存在していればその後に実行」する宣言。存在しない場合はスキップされるため互換性を壊しません。
- FloorAdjusterは `net.narazaka.vrchat.floor_adjuster` というQualifiedNameを持つため、この文字列を指定しています。

## IEditorOnlyコンポーネントとNDMF
- VRChat SDKは `IEditorOnly` を実装したコンポーネントをビルド前に除去します。
- NDMF Resolvingフェーズは除去前の状態にアクセスできるため、ここでユーザー設定を読み取り、専用のRuntimeデータにコピーする実装がよく使われます。
- ViewPoint Scalerの `CaptureSettingsPass` がこのパターンで、`ViewPointScalerRuntimeData` へ値を退避しています。

## よくある落とし穴
1. **フェーズの選択ミス:** FloorAdjuster前にスケーリングすると床合わせが狂うなど、結果が不定に。`AfterPlugin` の指定とフェーズ選択を常にセットで考える。
2. **ログ不足:** NDMFビルドは一瞬で終わることもあるため、`Debug.Log` を仕込んでおくと原因調査が楽。
3. **IEditorOnlyの副作用:** RootにEditorOnlyタグを付けるとNDMFの`ClearEditorOnlyTags`でルートが削除されるケースがあり危険。コンポーネント側で警告を出すなど対策を推奨。

このドキュメントを踏まえて、`viewpoint-scaler.md` では実際のコードがどのようにPlug-in/Pass構造に落とし込まれているかを追っていきます。