# ゴキブリVSニンゲン
2年次前期審査会とTGSに向けて開発しているゲーム

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

# 次回  
カメラを模索していたが、ゴキブリの挙動はシネマシーンと相性が悪いため、カメラを直接いじる方がよさそう。
ゴキブリが向きを変える挙動は、開店した直後に何本か下に向かってRayを飛ばし、それぞれの法線を持ってきて多数決を取って多い方へ向きを変えるようにすればいけそう。
