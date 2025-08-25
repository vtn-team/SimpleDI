using UnityEngine;

// ========== Param Inject Settings ==========

/// <summary>
/// パラメータ注入設定
/// 使用するInjectParamListを選択する
/// </summary>
[CreateAssetMenu(fileName = "ParamInjectSettings", menuName = "SimpleDI/ParamInjectSettings")]
public class ParamInjectSettings : ScriptableObject
{
    [Header("Inject Settings")]
    [SerializeField] private InjectParamList _selectedParamList;
    
    [Header("Generation Settings")]
    [SerializeField] private bool _autoGenerateOnReload = true;
    [SerializeField] private string _generatedCodePath = "Assets/Scripts/Inject/Generated/";

    public InjectParamList SelectedParamList => _selectedParamList;
    public bool AutoGenerateOnReload => _autoGenerateOnReload;
    public string GeneratedCodePath => _generatedCodePath;

    /// <summary>
    /// パラメータリストを設定
    /// </summary>
    public void SetParamList(InjectParamList paramList)
    {
        _selectedParamList = paramList;
    }

    /// <summary>
    /// 自動生成設定を変更
    /// </summary>
    public void SetAutoGenerate(bool enabled)
    {
        _autoGenerateOnReload = enabled;
    }

    /// <summary>
    /// 生成コードパスを設定
    /// </summary>
    public void SetGeneratedCodePath(string path)
    {
        _generatedCodePath = path;
    }

    private void OnValidate()
    {
        // パスの正規化
        if (!string.IsNullOrEmpty(_generatedCodePath))
        {
            _generatedCodePath = _generatedCodePath.Replace("\\", "/");
            if (!_generatedCodePath.EndsWith("/"))
            {
                _generatedCodePath += "/";
            }
        }
    }

    /// <summary>
    /// 設定の有効性をチェック
    /// </summary>
    public bool IsValid()
    {
        return _selectedParamList != null && !string.IsNullOrEmpty(_generatedCodePath);
    }

    /// <summary>
    /// 設定情報を表示（デバッグ用）
    /// </summary>
    [ContextMenu("Show Settings Info")]
    private void ShowSettingsInfo()
    {
        Debug.Log("=== ParamInjectSettings Info ===");
        Debug.Log($"Selected Param List: {(_selectedParamList != null ? _selectedParamList.name : "None")}");
        Debug.Log($"Auto Generate: {_autoGenerateOnReload}");
        Debug.Log($"Code Path: {_generatedCodePath}");
        Debug.Log($"Is Valid: {IsValid()}");
    }
}