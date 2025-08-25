# 主要な特徴

## **1. コード生成によるリフレクション回避**

ビルド前に注入コードを自動生成し、リフレクションを使わずに値を注入します。

## **2. 使い方**

### ステップ1: Injectするパラメータがあるクラスを`partial`として定義
- 注入したい値を、以下のように[Inject]指定します。
- 必ず初期化時に値を参照する前に`InjectDependencies()`を呼び出してください。
```csharp
public partial class Player : MonoBehaviour
{
    [Inject] private float moveSpeed;
    [Inject] private int maxHealth;
    
    void Awake()
    {
        InjectDependencies(); // 生成されたメソッドを呼ぶ
    }
}
```

### ステップ2: コード生成を実行
- 自動実行されます
- メニューから: `Tools > DI > Generate Injection Code`でも実行可能です

### ステップ3: 生成されたコード
```csharp
// Assets/Scripts/Generated/DIGenerated.cs
public partial class Player : IInjectable
{
    public void InjectDependencies()
    {
        var locator = ServiceLocator.Instance;
        this.moveSpeed = locator.Get<float>("moveSpeed");
        this.maxHealth = locator.Get<int>("maxHealth");
    }
}
```


# データ設定
## 1. DataAsset/Paramsフォルダ内にあるDI管理ファイルの説明
- InjectParamInfo (命名固定)
	- DIするパラメータの命名を補足する補助ファイルです
- ParamInjectSettings (命名固定)
	- DIにどの値を注入するかを選択するアセットです
- InjectParamList
	- (メニューから生成可能な設定ファイルです)
	- DIするパラメータを定義するアセットです
	- DIする環境ごとにアセットを生成してください

## 2. InjectParamInfoの「Auto-Generate from Injectable Types」
- このボタンを押すことで、Injectしたパラメータを全検索してそれぞれのカテゴリや説明のテンプレートを自動設定してくれる。デフォルトはクラス名と変数名が入る。
- カテゴリ別にパラメータが並び変えられるので、クラスをまたいで値を設定する際や、別の意味合いを持つ変数を分けておきたい場合などに便利
