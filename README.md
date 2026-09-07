# DRSForm (ダミー応答サーバー)

Watson Webserver をベースにした、Windows Forms 製のモック HTTP サーバです。  
`Response.json` に応答内容を定義しておくことで、任意の URL / HTTP メソッドに対して  
あらかじめ用意したレスポンス（ステータスコード・本文・ヘッダーなど）を返せます。  
API のクライアント開発や結合テストで、実サーバの代わりに使うことを想定しています。  

## 主な機能

- URL / HTTP メソッドごとの応答内容を JSON で定義
- ステータスコード、Content-Type、文字コード（Charset）の指定
- 応答までの待機時間の指定（遅延応答のシミュレート）
- 応答内容をファイルから読み込み（HTML / JSON などのファイルパス指定）
- DLL（プラグイン）を呼び出して動的に応答を生成
- リクエスト / レスポンスの内容を画面上にリアルタイム表示
- log4net によるログ出力
- 【テスト実装】リクエスト Body の内容による応答の出し分け（`CheckBody`）

## 使用ライブラリ

| ライブラリ | 用途 |
|-----------|------|
| [Watson](https://www.nuget.org/packages/Watson) | HTTP サーバ本体 |
| [log4net](https://www.nuget.org/packages/log4net) | ログ出力 |
| [Costura.Fody](https://www.nuget.org/packages/Costura.Fody) | 依存 DLL を実行ファイルへ埋め込み |


## ビルドと起動

1. `DRSFrom.exe` を起動
2. 画面上でポート番号を入力し `Start` ボタンでサーバを開始（既定ポート: 8085）
3. `Stop` ボタンでサーバを停止

起動中は背景がライトグリーンになります。停止中はライトグレー、エラー時は赤で表示されます。

## 応答定義 (Response.json)

実行ファイルと同じフォルダに置いた `Response.json` を起動時に読み込みます。
配列で複数の応答を定義できます。上から順に評価され、最初に一致したものが返されます。

```json
[
  {
    "Memo": "直接アクセス時の応答",
    "URL": "/",
    "Values": "<html lang=\"ja\"><head><meta charset=\"utf-8\"></head><body><h1>404 Not Found</h1></body></html>",
    "Type": "ANY",
    "Status": "404",
    "Enc": "UTF-8",
    "WaitTime": "100",
    "ContentType": "text/html"
  }
]
```

### 各フィールド

| キー | 説明 |
|------|------|
| `URL` | 対象の要求 URL（例: `/`, `/api/users`） |
| `Type` | HTTP メソッド。`ANY` / `GET` / `POST` など。`ANY` は全メソッド対象 |
| `Status` | 返却するステータスコード（文字列）。未指定・不正時は `500` |
| `Values` | 応答本文。文字列そのもの、ファイルパス、または DLL 指定（後述） |
| `Enc` | 応答の文字コード。`UTF-8` / `SJIS` / `JIS` / `ASCII` |
| `WaitTime` | 応答までの待機時間。`100`（ミリ秒）、`5s`（秒）、`100ms` などの形式 |
| `ContentType` | 応答の Content-Type ヘッダー（例: `text/html`, `application/json`） |
| `CheckBody` | 【テスト実装】リクエスト Body の一致条件（後述）。省略可 |
| `Memo` | 定義の説明用メモ。動作には影響しない |

### 応答本文をファイルから返す

`Values` に `\` を含むパスを指定し、そのファイルが存在する場合はファイルの内容を応答本文として返します。   
```json
{
  "URL":"/page",
  "Values": ".\\pages\\index.html",
  "Type": "GET",
  "Status": "200",
  "ContentType": "text/html"
}
```

### 【テスト実装】Body の内容で応答を返す (CheckBody)

`CheckBody` にキー・値の組をカンマ区切りで指定すると、リクエスト Body が
**すべてのキーで一致した場合のみ** その応答を返します。  
一致しない場合、404が返されます。
```json
{
  "URL": "/bodyCheck",
  "Values": "bodyCheck",
  "Type": "ANY",
  "Status": "200",
  "CheckBody": "id=1234,pass=5678"
}
```

`application/json` / `application/x-www-form-urlencoded` / `form-data` / `text/plain` の Body を
キー・値に変換して照合します。

## 追加応答フォルダ (EXResFiles)

`App.config` の `EXResFiles` に指定したフォルダ内の `.json` を起動時に追加で読み込みます。
複数フォルダはカンマ区切りで指定します。

```xml
<add key="EXResFiles" value=".\addRes" />
```

## DLL プラグインによる動的応答

`Values` を `ファイルパス@ネームスペース.クラス名` の形式で指定すると、  
指定した DLL をロードしてクラスをインスタンス化し、`Main(string[])` を呼び出し、その戻り値を応答本文として返します。

```json
{
  "URL": "/Dllsample",
  "Values": ".\\plugin\\DRS_DLL_Sample.dll@DRS_DLL_Sample.SampleClass",
  "Type": "ANY",
  "Status": "200",
  "ContentType": "application/json"
}
```

`Main` には `[リクエストBody, 応答値(DLLパス), 実行フォルダのフルパス]` が渡されます。

> **⚠️ セキュリティ上の注意**
>
> DLL プラグイン機能は`Response.json`に記載された任意のDLLをロードして実行します。  
> これは実質的に**任意コード実行**にあたります。  
> **信頼できない`Response.json`/DLLを読み込ませない運用を前提**とし、  
> 社内検証などの限定的な用途でのみ使用してください。  
> 不特定多数がアクセスできる環境や本番相当の環境では使用しないでください。  

## 注意事項

- 待ち受け IP は `127.0.0.1`（ローカルホスト）固定です。  
- DLL プラグイン機能はセキュリティリスクがあります。上記「セキュリティ上の注意」を必ず確認してください。
