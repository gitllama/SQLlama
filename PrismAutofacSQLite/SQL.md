# SQLアンチパタンを読みながらの要件整理

## 検討項目

### 複合主キー（PK: PRIMARY KEY）を使うか

- 一意にしたい場合は必要
  - PKがナチュラルキー（業務的にそのテーブルをユニークにするキー）
  - PKがサロゲートキー（業務情報とは直接関係ない連番）
- パフォーマンス影響ありそう


- サロゲート
  - 依存関係が薄くなる（テーブルの変更に強くなる）
  - アプリケーション/SQLの実装は簡潔
  - 容量は無駄
  - 参照側の容量は減らせる
  - 外部参照している場合、結合しないと関連が分からない

サロゲートキー + (複合)ユニーク制約でええんやないか

どうせ連番シリアルつけたらそうなる

ハッシュにしたら楽ちん？

### 履歴テーブルをどうするか



### 現状のデータベースが犯してるアンチパタン

- jaywalking
- Keyless Entry
- EAV（redmine）
- Multicolumn Attributes
- Phantom Files（redmine）
犯しそう

- ID Required


## アンチパタン

*******
**論理設計**

### 1.信号無視（Jaywalking）

- 1カラムにカンマ区切りでデータ入れるな
  - ソート/比較・検索しにくい/集約関数もつかえん
  - インデックスも聞かない
- パターンマッチで使えないことないけどダルい

|id|master_id|
|:--|:--|
|0|0,1,2|
|1|0,4|

#### 解決策

- 交差テーブル
  - 主テーブルのPKとマスタのPKをつなぐテーブル
  - joinもインデックスもつかえる
- （パフォーマンス優先なら使ってもよい）

|id|
|:--|
|0|
|0|

|id|master_id|
|:--|:--|
|0|0|
|0|1|
|0|2|
|1|0|
|1|4|

### 2.素朴な木（Naive Trees）

- コメント機能など親テーブルのidを持つテーブルでツリー構造になってる
  - ツリー構造が一発で見えない
  - 削除がつらい（整合のためにupdate処理必要

#### 解決策

- 経路列挙モデル
  - 親idじゃなくて親、爺まで含めたFullPath
  - Jaywalking
- 入れ子集合モデル
- 閉包テーブルモデル
  - 容量でかい

### 3.とりあえずid（ID Required）

- サロゲートをPKにするとおなじレコードを入れてしまいがち
- 冗長なキーが作成されてしまう
- UNIQUE制約が付与できる場合必要ない
- 主キーが複合キー場合、疑似キーを用いた場合では、クラスタ化インデックスの構成が変わるがパフォーマンスはほぼ変わらない
  - 自然キーがながすぎる場合(ex.メールアドレスを主キーにしてインデックスをつけるのは非効率
  - 連番じゃないと index 効率悪い

|id|id_A|id_B|
|:--|:--|:--|
|0|0|1|
|1|1|2|
|2|1|2|

id 1と2が同じ内容

### 4.外部キー嫌い（Keyless Entry）

- 参照整合性制約
- 外部キーはオーバーヘッドになる？

#### 解決策

- 外部キー制約を宣言

```SQL
CREATE TABLE Bugs (
 reported_by  BIGINT UNSIGNED NOT NULL,
 status       VARCHAR(20) NOT NULL DEFAULT 'NEW',
 FOREIGN KEY (reported_by) REFERENCES Accounts(account_id),
 FOREIGN KEY (status) REFERENCES BugStatus(status)
);
```

### 5.EAV（Entity-Attribute-Value）

- カラムを可変属性にしたい
  - Attribute（属性）とValue（値）をペアにして登録する
  - 列数減らせるし、新属性追加でも列数増えない
  - 集計ができない
  - 属性の制約も使えない
  - 参照整合性を強制できない

#### 解決策

- そもそも非リレーショナルなHadoop、Redis、Caddandra使え
- サブタイプの数が限られて、属性を良くわかっている際尾
  - シングルテーブル継承
  - 具象テーブル継承
  - クラステーブル継承
  - 半構造化データ
    - LOB（Large Object/ BLOB,TEXTなど）列を追加しXML,JSONとして格納
    - 拡張性高い
    - SQL直でのアクセス手段はほぼない

### 6.ポリモーフィック（Polymorphic Associations）

- 複数の親テーブルを参照したい
  - issue_typeみたいなカラムで種類を分類する

#### 解決策

- 参照を逆にする
- 交差テーブルの作成
- 交差点に交通信号を設置する
- 両方の「道」を見る
- 「道」を合流させる
- 共通の親テーブルの作成

### 7.複数列属性（Multicolumn Attributes）

- 複数の属性（電話番号やタグなど）を持たせたい
  - tag1, tag2, tag3のように複数カラム作りがち
  - 何個用意すればいいかわからない
  - OR,ANDでつないだ検索
  - 追加削除の際にロックしてるカラムがどれか見ないといけない

#### 解決策

- Jaywalkingにちかい
- 従属テーブルを作成

```SQL
CREATE TABLE `Tags` (
  bug_id BIGINT UNSIGNED NOT NULL,
  tag VARCHAR(20),
  PRIMARY KEY (bug_id, tag),
  FOREIGN KEY (bug_id) REFERENCES Bugs(bug_id)
);

SELECT * FROM `Bugs`
  INNER JOIN `Tags` AS t1 USING (bug_id)
  INNER JOIN `Tags` AS t2 USING (bug_id)
WHERE t1.tag = 'printing'
  AND t2.tag = 'performance'
;
```


### 8.メタデータ大増殖（Metadata Tribbles）

- 年ごとにテーブル（や列）を分割をしてしまったり
  - スケーラビリティを高めたい
  - 年マタギで入力処理するときにテーブル間違ったり
  - テーブル間で一意性保証できない
  - テーブルまたいだクエリが面倒（毎回コード書き換えないといけない
  - 別テーブルでカラム増えたときの同期の問題
  - 従属テーブルの参照整合性をテーブルをまたいで管理必要
  - （明確に分離してアーカイブしたい場合は良い）

#### 解決策

- パーティショニング
  - 水平パーティショニング(シャーディング)
    - 論理的には1テーブルだが、物理的には行でテーブルが分割される
    - Selectのパフォーマンスは落ちる
  - 垂直パーティショニング
    - 論理的には1テーブルだが、物理的には列でテーブルが分割される。
    - Large Objectがいるとき有効
    - めったに使用されない際メリット
- 従属テーブルの導入
  - Multicolumn Attributes
  - idとyearをPKに設定する

SQLiteはパーティショニングに対応していない（そもそもファイルサイズ制限もある）

*******
**物理設計**

### 9.丸め誤差（Rounding Errors）

- Floatは避けてDECIMALやNumericに有効桁数いれた方が良い
  - NUMERIC(9,2) : 9桁精度に小数点以下2桁

### 10.31のフレーバー（31 Flavors）
### 11.幻のファイル（Phantom Files）

- 画像ファイルをBLOBで格納する？
  - リンクは実体まで削除してくれない
  - ロールバックでもリンク切れてると困る
  - 容量に伴う問題は当然出る

### 12.闇雲インデックス（Index Shotgun）

- インデックスは検索速度を上げるために目次を作ることだけど
- やみくもにつければいいというものではない（棒）
  - 「MENTOR」の原則に基づいて検討しなさい
  - Measure（測定）
  - Explain（解析）
  - Nominate（指名）
  - Test（テスト）
  - Optimize（最適化）
  - Rebuild（再構築）



*******
**クエリ**

### 13.Fear of the Unknown(恐怖のunknown)

- Nullは空文字など値に置き換えておいたりなんてしない

```SQL
SELECT * FROM Bugs WHERE assigned_to IS NULL;
SELECT * FROM Bugs WHERE assigned_to IS NOT NULL;
SELECT * FROM Bugs WHERE assigned_to IS NULL ORassigned_to <> 1;
SELECT * FROM Bugs WHERE assigned_to IS DISTINCT FROM 1;
```

IS DISTINCT FROM ならプリペアドステートメントにも使える!

### 14.Ambiguous Groups(曖昧なグループ)

- 例えば連番idとdateの並びが一致するとは限らない
  - 最大値を得るクエリだとまちがう
- 曖昧なクエリ結果を避けるために、単一値の原則に従うこと
  - 関数従属性のある列のみにクエリを実行する
  - 相関サブクエリを使用する
  - 導出テーブルを使用する
  - JOINを使用する
  - 他の列に対しても集約関数を使用する
  - グループごとにすべての値を連結する

### 15.Random Selection
### 16.Poor Man's Search Engine(貧者のサーチエンジン)

- 全文検索したい
  - Like演算子
  - （MySQL）正規表現述語, WHERE description REGEXP 'hoge'
  - インデックス効かないので遅い
  - マッチミスしがち
  - （単純で遅くていいなら）

#### 解決策

- SQLつかうな
- 転置インデックス
- ベンダ拡張つかう
  - SQLiteならFTS

### 17.Spaghetti Query
### 18.Implicit Columns(暗黙の列)

- ```SELECT * FROM```みたいにワイルドカードで不要な列まで取得しない

*******
**アプリ開発**

### 19.Readable Passowrds(読み取り可能パスワード)
### 20.SQL Injection

- 動的なクエリ
  - プリペアドステートメント使え
  - 入力をフィルタリングしろ
  - 動的値を引用符で囲む

```C#
//C#+SQliteでprepared statement
using (var con = new SqliteConnection(connectionStr))
{
    con.Open();
    using (var tran = con.BeginTransaction())
    {
        using (var cmd = con.CreateCommand())
        {
            cmd.CommandText = insertSQL;
            var param = new SqliteParameter()
            {
                DbType = System.Data.DbType.String,
                Value = string.Format("user{0}", i),
            };
            cmd.Parameters.Clear();
            cmd.Parameters.Add(param);
            cmd.Prepare();
            cmd.ExecuteNonQuery();
        }
        tran.Commit();
    }
}
```

### 21.Pseudokey Neat-Freak(疑似キー潔癖症)
### 22.See No Evil(臭いものに蓋)
### 23.Diplomatic Immunity(外交特権)
### 24.Magic Beans
### 25.砂の城

*******
**t_wadaさんの**

### 26.とりあえず削除フラグ

- 企業システムにおいてのRDBMSは「発生した事実を余さず記録する」トランザクショナルで永続的なデータストアとしての側面が強い
- UpdateはDeleteより大概速い

#### 解決策

- せめて削除日に変更する
- フラグではなく状態にする
  - 有効/中止/キャンセル/破棄 etc
- 履歴テーブルに残す
  - トリガー
- 削除も更新もしない（CRUDにおけるCreate、ReadのみでUpdate、Deleteはつかわない）
  - テーブルに状態を持たせない

```SQL
CREATE TRIGGER トリガー名 UPDATE OF カラム名 ON テーブル名
 BEGIN
  SQL文1;
  SQL文2;
 END;
```
