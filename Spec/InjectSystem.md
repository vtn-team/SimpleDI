# InjectSystemクラス概要

# 概要
- 注入用パラメータを保持しておくシングルトン

# 実装
- シーン読み込み時、またはゲーム起動時に注入用のパラメータを読み込んでおく

# 処理フロー
1. シーン読み込み時、またはゲーム起動時にDataAsset/ParamInjectSettingsを読み込む
	1. Addressablesで読み込む
2. 各注入クラスからアクセスされる

# 内部変数
- paramInjectSettings: 読み込んだParamInjectSettings

# 外部インタフェース
- ParamInjectSettingsを取得する

# 期待値
- ユースケースに応じて注入するパラメータを変化させることができる

# エッジケース
- なし
