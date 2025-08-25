using UnityEngine;

// ========== Inject Param List (Manual Part) ==========

/// <summary>
/// 注入パラメータのリスト（手動作成部分）
/// 自動生成部分とpartial classで結合される
/// </summary>
[CreateAssetMenu(fileName = "InjectParamList", menuName = "SimpleDI/InjectParamList")]
public partial class InjectParamList : ScriptableObject
{
    [Header("Manual Parameters")]
    [SerializeField] private string _listName = "DefaultParamList";
    [SerializeField] private string _description = "";
    
    public string ListName => _listName;
    public string Description => _description;

    /// <summary>
    /// パラメータリストの初期化
    /// </summary>
    private void OnEnable()
    {
        if (string.IsNullOrEmpty(_listName))
        {
            _listName = name;
        }
    }

    /// <summary>
    /// パラメータリストの検証
    /// </summary>
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_listName))
        {
            _listName = name;
        }
    }
}