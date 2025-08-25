using System;
using System.Collections.Generic;
using UnityEngine;

// ========== InjectParamInfo ==========

/// <summary>
/// パラメータ情報を定義するクラス
/// </summary>
[Serializable]
public class ParamInfo
{
    [Header("Parameter Info")]
    [SerializeField] private string _group;
    [SerializeField] private string _varName;
    [SerializeField] private string _viewName;

    public string Group => _group;
    public string VarName => _varName;
    public string ViewName => _viewName;

    public ParamInfo(string group, string varName, string viewName)
    {
        _group = group;
        _varName = varName;
        _viewName = viewName;
    }

    public ParamInfo()
    {
        _group = "";
        _varName = "";
        _viewName = "";
    }
}

/// <summary>
/// 注入先の変数名の説明を記載するアセット
/// </summary>
[CreateAssetMenu(fileName = "InjectParamInfo", menuName = "SimpleDI/InjectParamInfo")]
public class InjectParamInfo : ScriptableObject
{
    [Header("Parameter Information List")]
    [SerializeField] private List<ParamInfo> _paramInfos = new List<ParamInfo>();

    public List<ParamInfo> ParamInfos => _paramInfos;

    /// <summary>
    /// 指定した変数名の情報を取得
    /// </summary>
    public ParamInfo GetParamInfo(string varName)
    {
        return _paramInfos.Find(info => info.VarName == varName);
    }

    /// <summary>
    /// 指定したグループの情報をすべて取得
    /// </summary>
    public List<ParamInfo> GetParamInfosByGroup(string group)
    {
        return _paramInfos.FindAll(info => info.Group == group);
    }

    /// <summary>
    /// すべてのグループ名を取得
    /// </summary>
    public List<string> GetAllGroups()
    {
        var groups = new HashSet<string>();
        foreach (var info in _paramInfos)
        {
            if (!string.IsNullOrEmpty(info.Group))
            {
                groups.Add(info.Group);
            }
        }
        return new List<string>(groups);
    }

    /// <summary>
    /// パラメータ情報を追加
    /// </summary>
    public void AddParamInfo(string group, string varName, string viewName)
    {
        var newInfo = new ParamInfo(group, varName, viewName);
        _paramInfos.Add(newInfo);
    }

    /// <summary>
    /// 指定した変数名の情報を削除
    /// </summary>
    public bool RemoveParamInfo(string varName)
    {
        var info = GetParamInfo(varName);
        if (info != null)
        {
            _paramInfos.Remove(info);
            return true;
        }
        return false;
    }

    /// <summary>
    /// すべての情報をクリア
    /// </summary>
    public void ClearAll()
    {
        _paramInfos.Clear();
    }

    /// <summary>
    /// デバッグ用：すべての情報を表示
    /// </summary>
    [ContextMenu("Show All Param Info")]
    private void ShowAllParamInfo()
    {
        Debug.Log("=== InjectParamInfo Contents ===");
        foreach (var info in _paramInfos)
        {
            Debug.Log($"Group: {info.Group}, VarName: {info.VarName}, ViewName: {info.ViewName}");
        }
    }
}