using System;
using UnityEngine;

// ========== アトリビュート定義 ==========

/// <summary>
/// フィールドに値を注入するためのアトリビュート
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class InjectAttribute : Attribute
{
    public string Key { get; }

    public InjectAttribute(string key = null)
    {
        Key = key;
    }
}

/// <summary>
/// InjectMapperでシリアライズ可能なパラメータ用のアトリビュート
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class InjectParamAttribute : PropertyAttribute
{
}

// ========== 依存注入インターフェース ==========

public interface IInjectable
{
    void InjectDependencies();
}

// ========== 基底クラス ==========

public abstract class InjectableMonoBehaviour : MonoBehaviour, IInjectable
{
    protected virtual void Awake()
    {
        InjectDependencies();
    }

    public abstract void InjectDependencies();
}

public static class DIInjector
{
    // ジェネリックメソッドでタイプセーフな注入
    public static void InjectInto<T>(T target) where T : IInjectable
    {
        target.InjectDependencies();
    }
}

// ========== InjectSystem Singleton ==========

/// <summary>
/// 注入用パラメータを保持するシングルトン
/// </summary>
public class InjectSystem : MonoBehaviour
{
    private static InjectSystem _instance;
    private ParamInjectSettings _paramInjectSettings;
    private bool _isInitialized = false;

    public static InjectSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                // シーン内から既存のインスタンスを検索
                _instance = FindObjectOfType<InjectSystem>();
                
                if (_instance == null)
                {
                    // 新しいGameObjectを作成してInjectSystemをアタッチ
                    GameObject go = new GameObject("InjectSystem");
                    _instance = go.AddComponent<InjectSystem>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    /// <summary>
    /// ParamInjectSettingsを取得
    /// </summary>
    public ParamInjectSettings ParamInjectSettings 
    { 
        get 
        {
            if (!_isInitialized)
            {
                Initialize();
            }
            return _paramInjectSettings; 
        } 
    }

    private void Awake()
    {
        // 既に別のインスタンスが存在する場合は自身を破棄
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        Initialize();
    }

    /// <summary>
    /// シーン読み込み時またはゲーム起動時にParamInjectSettingsを読み込む
    /// </summary>
    private void Initialize()
    {
        if (_isInitialized) return;

        try
        {
            // まず直接的なパスから読み込みを試す
            _paramInjectSettings = UnityEditor.AssetDatabase.LoadAssetAtPath<ParamInjectSettings>("Assets/DataAsset/Params/ParamInjectSettings.asset");
            
            if (_paramInjectSettings == null)
            {
                // フォールバック: Resources フォルダから読み込み
                _paramInjectSettings = Resources.Load<ParamInjectSettings>("ParamInjectSettings");
            }

            if (_paramInjectSettings == null)
            {
                Debug.LogWarning("InjectSystem: ParamInjectSettings not found. Please ensure it exists at Assets/DataAsset/Params/ParamInjectSettings.asset");
            }
            else
            {
                Debug.Log($"InjectSystem: Loaded ParamInjectSettings successfully");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"InjectSystem: Failed to load ParamInjectSettings: {e.Message}");
        }

        _isInitialized = true;
    }

    /// <summary>
    /// パラメータ設定を手動で再読み込み
    /// </summary>
    [ContextMenu("Reload Parameters")]
    public void ReloadParameters()
    {
        _isInitialized = false;
        Initialize();
    }

    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }
}