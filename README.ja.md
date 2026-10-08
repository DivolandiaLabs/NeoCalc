# NeoCalc — 26 種類のテーマで自由にカスタマイズできる Windows 電卓

[Español](README.md) · [English](README.en.md) · [Português](README.pt.md) · [Français](README.fr.md) · [Deutsch](README.de.md) · [Italiano](README.it.md) · [Polski](README.pl.md) · [Русский](README.ru.md) · [한국어](README.ko.md) · **日本語**

**Windows 10 / 11** 向けの標準・関数電卓です。Windows の電卓と同じように動作し、さらに
**26 種類の内蔵テーマ**と自分だけのテーマを作れるエディターを備えています: 色、グラデーション、背景画像、フォント、
丸いキー、ネオンの輝き、液晶ディスプレイ… 約 200 KB の `.exe` ひとつだけで、インストールは不要です。

**Web サイト:** https://divolandialabs.github.io/neocalc/ · **ダウンロード:** [最新版](https://github.com/DivolandiaLabs/NeoCalc/releases/latest)

![サイバーネオンのテーマを適用した関数電卓モードの NeoCalc](docs/screenshots/en-cientifica.png)

## 主な機能

- **標準モード**は Windows の電卓と同じ動作です: 計算は順番につながり (3 + 5 × 2 = 16)、`=` で直前の計算を繰り返し、`%` も同じように働きます。
- **関数電卓モード**は演算子の優先順位とかっこに対応 (3 + 5 × 2 = 13)。`2nd`、DEG/RAD/GRAD、指数表記 (F-E)、三角関数、対数、べき乗、累乗根、`n!` (小数も可)、`mod`、`exp`。
- **履歴とメモリ** (MC、MR、M+、M−、MS): ウィンドウが広いときは横のパネルに、狭いときはキーの上に表示されます。
- **好きなサイズで**: 角をドラッグすると電卓全体が拡大・縮小し、**このサイズに固定**を押すと次回からそのサイズで開きます。
- **キーボード操作に完全対応**、コピー & 貼り付け、**常に手前に表示**ボタン。
- **26 種類のテーマ**: Windows ダーク / ライト、サイバーネオン、Synthwave、Matrix、レトロ液晶、アンバー端末、ドラキュラ、ノルディック、Game Boy、ホログラム、ガラス、丸型 (iPhone 風)、ゴールド&ブラック、Vaporwave…
- **テーマエディター**: その場でプレビュー、透明度付きのカラーピッカー、**ランダム**ボタン、テーマの**インポート / エクスポート** (`.neocalc.json`)。
- タスクバーにピン留めしてもテーマの色になる**タスクバーアイコン**。
- **10 言語**: スペイン語、英語、ポルトガル語、フランス語、ドイツ語、イタリア語、ポーランド語、ロシア語、韓国語、日本語。

![26 種類の内蔵テーマ](docs/screenshots/en-galeria.png)

![さまざまなサイズの電卓と「このサイズに固定」ボタン](docs/screenshots/en-tamanos.png)

## インストール

1. [リリースページ](https://github.com/DivolandiaLabs/NeoCalc/releases/latest)から `NeoCalc.exe` をダウンロードします。
2. 好きな場所 (例: `ドキュメント\NeoCalc`) に保存して開きます。インストールは不要です。
3. すぐ使えるように: タスクバーのアイコンを右クリック → **タスクバーにピン留めする**。

プログラムに署名がないため、初回は Windows SmartScreen の警告が出ることがあります: **詳細情報 → 実行** をクリックしてください。

アンインストールは `.exe` と、必要なら `%APPDATA%\NeoCalc` フォルダーを削除するだけです。

## テーマとカスタマイズ

パレットのボタン 🎨 または **Ctrl+T** でエディターを開きます。テーマを選ぶとすぐに適用されます。内蔵テーマを変更すると
**マイテーマ**にコピーが作られ、元のテーマはそのまま残ります。

![テーマエディター](docs/screenshots/en-temas.png)

変更できる項目: 背景色 (グラデーションと角度)、背景画像、ウィンドウの不透明度、角の丸み、ディスプレイの背景と数字、
液晶風の枠、キーの種類ごとの色、フォントと太さ、文字サイズ、キーの丸み (円形まで)、間隔、枠線、ネオンの輝き。

## キーボードショートカット

| キー | 操作 |
|---|---|
| `0`–`9`、`,` `.` | 数字の入力 |
| `+` `-` `*` `/` `Enter` | 演算子 / イコール |
| `Backspace` · `Delete` · `Esc` | 1 桁削除 · CE · C |
| `F9` · `R` · `@` · `Q` | +/− · 1/x · 平方根 · x² |
| `(` `)` `^` `!` `%` | かっこ、べき乗、階乗、mod (関数電卓) |
| `Alt+1` · `Alt+2` | 標準 · 関数電卓 |
| `Ctrl+M` `Ctrl+R` `Ctrl+P` `Ctrl+Q` `Ctrl+L` | MS · MR · M+ · M− · MC |
| `Ctrl+H` · `Ctrl+T` | 履歴 · テーマ |
| `Ctrl+C` · `Ctrl+V` | コピー · 貼り付け |

## ソースコードからビルド

何もインストールする必要はありません。Windows に標準で含まれる C# コンパイラー (.NET Framework 4.8) でビルドします。

```powershell
powershell -ExecutionPolicy Bypass -File build.ps1
```

アイコンはプログラム自身が描画します (`src\Icono.cs`)。計算エンジンの確認: `NeoCalc.exe /prueba result.txt`。

## プライバシー

NeoCalc はインターネットに接続せず、データを一切収集しません。設定、履歴、自作テーマはお使いのコンピューターの
`%APPDATA%\NeoCalc\config.json` にのみ保存されます。

## 動作環境

Windows 10 または 11 (.NET Framework 4.8 を含む)。

---

© 2026 Divolandia Labs · [divolandialabs.github.io](https://divolandialabs.github.io/) · ご質問やアイデアは [Issue を作成](https://github.com/DivolandiaLabs/NeoCalc/issues)してください。
