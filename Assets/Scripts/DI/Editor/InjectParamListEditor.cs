using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// ========== InjectParamList Editor Extension ==========

#if UNITY_EDITOR

/// <summary>
/// InjectParamListのカスタムエディタ
/// InjectParamInfoを参照してグループ化とViewName表示を行う
/// </summary>
[CustomEditor(typeof(InjectParamList))]
public class InjectParamListEditor : Editor
{
    private InjectParamInfo _paramInfo;
    private Dictionary<string, bool> _groupFoldouts = new Dictionary<string, bool>();
    private Dictionary<string, List<FieldInfo>> _groupedFields = new Dictionary<string, List<FieldInfo>>();
    private List<FieldInfo> _ungroupedFields = new List<FieldInfo>();
    private bool _paramInfoLoaded = false;

    private void OnEnable()
    {
        LoadParamInfo();
        CacheFields();
    }

    /// <summary>
    /// InjectParamInfoを読み込む
    /// </summary>
    private void LoadParamInfo()
    {
        // Assets/DataAsset/Params から InjectParamInfo を検索
        string[] guids = AssetDatabase.FindAssets("t:InjectParamInfo", new[] { "Assets/DataAsset/Params" });
        
        if (guids.Length > 0)
        {
            string path = AssetDatabase.GUIDToAssetPath(guids[0]);
            _paramInfo = AssetDatabase.LoadAssetAtPath<InjectParamInfo>(path);
            _paramInfoLoaded = true;
            
            if (guids.Length > 1)
            {
                Debug.LogWarning("InjectParamListEditor: Multiple InjectParamInfo assets found. Using the first one.");
            }
        }
        else
        {
            Debug.LogWarning("InjectParamListEditor: No InjectParamInfo found in Assets/DataAsset/Params");
            _paramInfoLoaded = false;
        }
    }

    /// <summary>
    /// フィールドをキャッシュし、グループ化する
    /// </summary>
    private void CacheFields()
    {
        _groupedFields.Clear();
        _ungroupedFields.Clear();

        var targetType = target.GetType();
        var fields = targetType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                               .Where(f => f.GetCustomAttribute<SerializeField>() != null)
                               .ToArray();

        if (_paramInfoLoaded && _paramInfo != null)
        {
            // グループ化
            foreach (var field in fields)
            {
                string fieldName = GetFieldVarName(field.Name);
                var paramInfo = _paramInfo.GetParamInfo(fieldName);
                
                if (paramInfo != null && !string.IsNullOrEmpty(paramInfo.Group))
                {
                    string group = paramInfo.Group;
                    if (!_groupedFields.ContainsKey(group))
                    {
                        _groupedFields[group] = new List<FieldInfo>();
                        _groupFoldouts[group] = true; // デフォルトで開いた状態
                    }
                    _groupedFields[group].Add(field);
                }
                else
                {
                    _ungroupedFields.Add(field);
                }
            }
        }
        else
        {
            // ParamInfoがない場合はすべてungrouped
            _ungroupedFields.AddRange(fields);
        }
    }

    /// <summary>
    /// フィールド名から変数名を取得（アンダースコアを除去）
    /// </summary>
    private string GetFieldVarName(string fieldName)
    {
        if (fieldName.StartsWith("_"))
        {
            return fieldName.Substring(1);
        }
        return fieldName;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ヘッダー情報
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inject Parameter List", EditorStyles.boldLabel);
        
        if (!_paramInfoLoaded)
        {
            EditorGUILayout.HelpBox("InjectParamInfo not found in Assets/DataAsset/Params. Field names will be displayed without grouping or Japanese names.", MessageType.Warning);
        }

        EditorGUILayout.Space();

        // リロードボタン
        if (GUILayout.Button("Reload Param Info"))
        {
            LoadParamInfo();
            CacheFields();
        }

        EditorGUILayout.Space();

        // グループ化されたフィールドを表示
        foreach (var group in _groupedFields.Keys)
        {
            DrawGroupFields(group, _groupedFields[group]);
        }

        // グループ化されていないフィールドを表示
        if (_ungroupedFields.Count > 0)
        {
            DrawUngroupedFields(_ungroupedFields);
        }

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// グループ化されたフィールドを描画
    /// </summary>
    private void DrawGroupFields(string groupName, List<FieldInfo> fields)
    {
        if (!_groupFoldouts.ContainsKey(groupName))
        {
            _groupFoldouts[groupName] = true;
        }

        // グループのフォルダウト
        _groupFoldouts[groupName] = EditorGUILayout.Foldout(_groupFoldouts[groupName], $"【{groupName}】", true);

        if (_groupFoldouts[groupName])
        {
            EditorGUI.indentLevel++;
            
            foreach (var field in fields)
            {
                DrawFieldWithInfo(field);
            }
            
            EditorGUI.indentLevel--;
        }

        EditorGUILayout.Space();
    }

    /// <summary>
    /// グループ化されていないフィールドを描画
    /// </summary>
    private void DrawUngroupedFields(List<FieldInfo> fields)
    {
        EditorGUILayout.LabelField("その他のパラメータ", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        
        foreach (var field in fields)
        {
            DrawFieldWithInfo(field);
        }
        
        EditorGUI.indentLevel--;
    }

    /// <summary>
    /// フィールドを情報と共に描画
    /// </summary>
    private void DrawFieldWithInfo(FieldInfo field)
    {
        string fieldName = GetFieldVarName(field.Name);
        string displayName = fieldName;
        string tooltip = "";

        // ParamInfoから情報を取得
        if (_paramInfoLoaded && _paramInfo != null)
        {
            var paramInfo = _paramInfo.GetParamInfo(fieldName);
            if (paramInfo != null)
            {
                if (!string.IsNullOrEmpty(paramInfo.ViewName))
                {
                    displayName = $"{paramInfo.ViewName} ({fieldName})";
                }
                tooltip = $"Group: {paramInfo.Group}\nVariable: {paramInfo.VarName}\nView: {paramInfo.ViewName}";
            }
        }

        // SerializedPropertyを取得
        var property = serializedObject.FindProperty(field.Name);
        if (property != null)
        {
            var content = new GUIContent(displayName, tooltip);
            EditorGUILayout.PropertyField(property, content);
        }
        else
        {
            EditorGUILayout.LabelField($"Property not found: {field.Name}", EditorStyles.helpBox);
        }
    }
}

#endif