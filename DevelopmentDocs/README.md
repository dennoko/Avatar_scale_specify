# NDMF Learning Notes for VRC ViewPoint Scaler

この `Docs/` 以下は、NDMF(Non-Destructive Modular Framework) を使ったアバター改変パイプラインを学ぶための補助資料です。本プロジェクトの実装を題材に、以下のトピックに分けて解説します。

| ファイル | 内容 |
| --- | --- |
| `ndmf-core.md` | NDMFの基本概念 (Plugin / Pass / BuildPhase / コンテキストの扱い) |
| `viewpoint-scaler.md` | ViewPoint Scalerツールの設計と実装詳細 |
| `workflow-and-debug.md` | FloorAdjuster等との組み合わせ運用、デバッグ手順、トラブルシューティング |

## 使い方
1. まず `ndmf-core.md` でNDMFの構成要素とライフサイクルを把握します。
2. 次に `viewpoint-scaler.md` で本ツールのアーキテクチャを追い、実際のコードとの対応関係を確認します。
3. `workflow-and-debug.md` では、FloorAdjusterとの連携やテストビルドログの読み方を学び、実プロジェクトへの応用を想定したチェックリストを参照できます。

各ドキュメントは独立して読めますが、順番に進めると理解がスムーズです。