# GluttonousCockroachSimulator
日本語 : 食いしん坊なゴキブリシミュレーター
2年次前期審査会に向けて開発しているゲーム

# 概要
- ゴキブリ側と人間側に分かれて対戦するゲーム
- ゴキブリ側は制限時間まで逃げ切れば勝利
- 人間側は制限時間内にゴキブリを倒せば勝利

# ゴキブリの仕様
- WASDで移動
- マウスで視点移動
- スペースキーでジャンプ
- 壁や天井などに張り付いて移動することが可能
- 空腹ゲージがある
- 食べ物を食べることで空腹ゲージを満たすことができる
- 空腹ゲージが0になると死んでしまう

# 人間の仕様
- WASDで移動
- マウスで視点移動
- 物をつかんで投げることができる
- 殺虫スプレーなどを使用することができる  

# TGS コンペまでにやること(7/26 18時まで)
### ゴキブリの動き
- ~~ジャンプ処理~~ [ 7 / 3 ]
- ~~落下処理~~ [ 7 / 3 ]
- ~~カメラ操作~~ [ 7 / 5 ]
- ~~満腹ゲージ~~ [ 7 / 8 ]
- ~~アイテム処理（食べて満腹ゲージが回復する ）~~ [ 7 / 8 ]
- ~~死亡処理（死んだフラグを立てる）~~ [ 7 / 8 ]
- ~~ダメージ処理（一定時間無敵、加速、ジャンプ力UP）~~[ 7 / 9 ]  

### 人間の動き
- ~~移動処理~~[ 7 / 10 ]
- ~~攻撃処理（殺虫スプレー？）~~  [ 7 / 12 ] サボってしまった...

### ゲームルールの適応 [ 7 / 11 までにここまでやる ]
- ゴキブリは制限時間殺されなければ勝利
- ニンゲンは資源時間内にゴキブリを殺せば勝利  

### ゲームの流れを作成
- タイトルシーン
- セレクトシーン（ゴキブリ or ニンゲン）
- マッチングシーン
- ゲームシーン
- リザルトシーン
- タイトル or マッチング へ遷移  

### ネットワークに必要なこと
- 動きはRigidbodyにまかせる
- 攻撃はニンゲンがゴキブリのHit()をRPCで呼ぶ
- 死亡判定はゴキブリがGameManagerのGameOver()をRPCで呼ぶ
- アイテムのスポーンはGameManagerがGeneraterのGenerate()をRPCで呼ぶ（同じ位置で生成されるように気を付けないといけない）
- 回復処理はゴキブリが回復したらRPCで更新する