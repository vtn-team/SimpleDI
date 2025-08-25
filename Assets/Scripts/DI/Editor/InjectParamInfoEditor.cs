using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// ========== InjectParamInfo Editor Extension ==========

#if UNITY_EDITOR

/// <summary>
/// InjectParamInfoのカスタムエディタ
/// </summary>
[CustomEditor(typeof(InjectParamInfo))]
public class InjectParamInfoEditor : Editor
{
    private Vector2 _scrollPosition;

    public override void OnInspectorGUI()
    {
        InjectParamInfo paramInfo = (InjectParamInfo)target;
        
        serializedObject.Update();

        // ヘッダー
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Inject Parameter Information", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("This asset defines the display information for injection parameters.\nVariables are grouped and shown with Japanese names in the InjectParamList inspector.", MessageType.Info);

        EditorGUILayout.Space();

        // ツールバー
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add New Parameter Info"))
        {
            paramInfo.AddParamInfo("NewGroup", "newVarName", "新しいパラメータ");
            EditorUtility.SetDirty(paramInfo);
        }
        
        if (GUILayout.Button("Auto-Generate from Injectable Types"))
        {
            AutoGenerateParamInfo(paramInfo);
        }

        if (GUILayout.Button("Clear All"))
        {
            if (EditorUtility.DisplayDialog("Clear All", "Are you sure you want to clear all parameter info?", "Yes", "No"))
            {
                paramInfo.ClearAll();
                EditorUtility.SetDirty(paramInfo);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();

        // パラメータリスト表示
        SerializedProperty paramInfosProperty = serializedObject.FindProperty("_paramInfos");
        
        if (paramInfosProperty.arraySize == 0)
        {
            EditorGUILayout.HelpBox("No parameter information defined.", MessageType.Warning);
        }
        else
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            
            for (int i = 0; i < paramInfosProperty.arraySize; i++)
            {
                DrawParamInfoElement(paramInfosProperty.GetArrayElementAtIndex(i), i);
            }
            
            EditorGUILayout.EndScrollView();
        }

        // グループ統計情報
        DrawGroupStatistics(paramInfo);

        serializedObject.ApplyModifiedProperties();
    }

    /// <summary>
    /// パラメータ情報要素を描画
    /// </summary>
    private void DrawParamInfoElement(SerializedProperty element, int index)
    {
        EditorGUILayout.BeginVertical("box");
        
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Parameter {index + 1}", EditorStyles.boldLabel, GUILayout.Width(100));
        
        if (GUILayout.Button("×", GUILayout.Width(25)))
        {
            SerializedProperty paramInfosProperty = serializedObject.FindProperty("_paramInfos");
            paramInfosProperty.DeleteArrayElementAtIndex(index);
            return;
        }
        EditorGUILayout.EndHorizontal();

        // フィールド表示
        SerializedProperty groupProperty = element.FindPropertyRelative("_group");
        SerializedProperty varNameProperty = element.FindPropertyRelative("_varName");
        SerializedProperty viewNameProperty = element.FindPropertyRelative("_viewName");

        EditorGUILayout.PropertyField(groupProperty, new GUIContent("Group", "グループ名（分類用）"));
        EditorGUILayout.PropertyField(varNameProperty, new GUIContent("Variable Name", "注入対象の変数名"));
        EditorGUILayout.PropertyField(viewNameProperty, new GUIContent("Display Name (Japanese)", "日本語表示名"));

        EditorGUILayout.EndVertical();
        EditorGUILayout.Space();
    }

    /// <summary>
    /// グループ統計情報を表示
    /// </summary>
    private void DrawGroupStatistics(InjectParamInfo paramInfo)
    {
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Statistics", EditorStyles.boldLabel);
        
        var groups = paramInfo.GetAllGroups();
        if (groups.Count > 0)
        {
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Total Groups: {groups.Count}");
            EditorGUILayout.LabelField($"Total Parameters: {paramInfo.ParamInfos.Count}");
            
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Groups:", EditorStyles.miniBoldLabel);
            foreach (var group in groups.OrderBy(g => g))
            {
                int count = paramInfo.GetParamInfosByGroup(group).Count;
                EditorGUILayout.LabelField($"  • {group}: {count} parameters");
            }
            EditorGUILayout.EndVertical();
        }
        else
        {
            EditorGUILayout.HelpBox("No groups defined.", MessageType.Info);
        }
    }

    /// <summary>
    /// 注入可能な型から自動的にパラメータ情報を生成
    /// </summary>
    private void AutoGenerateParamInfo(InjectParamInfo paramInfo)
    {
        if (!EditorUtility.DisplayDialog("Auto Generate", 
            "This will automatically generate parameter info from injectable types. Continue?", 
            "Yes", "No"))
        {
            return;
        }

        try
        {
            var types = DICodeGenerator.FindInjectableTypes();
            int addedCount = 0;

            foreach (var type in types)
            {
                var fields = type.GetFields(System.Reflection.BindingFlags.Instance | 
                                          System.Reflection.BindingFlags.Public | 
                                          System.Reflection.BindingFlags.NonPublic);
                
                foreach (var field in fields)
                {
                    var injectAttr = field.GetCustomAttribute<InjectAttribute>();
                    if (injectAttr != null)
                    {
                        string key = injectAttr.Key ?? field.Name;
                        if (string.IsNullOrEmpty(injectAttr.Key) && key.StartsWith("_"))
                        {
                            key = key.Substring(1);
                        }

                        // 既存のものでなければ追加
                        if (paramInfo.GetParamInfo(key) == null)
                        {
                            string group = type.Name.Replace("Behaviour", "").Replace("Controller", "").Replace("Manager", "");
                            string viewName = ConvertToJapanese(key);
                            
                            paramInfo.AddParamInfo(group, key, viewName);
                            addedCount++;
                        }
                    }
                }
            }

            EditorUtility.SetDirty(paramInfo);
            EditorUtility.DisplayDialog("Auto Generate Complete", 
                $"Added {addedCount} parameter info entries.", "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", 
                $"Failed to auto-generate parameter info: {e.Message}", "OK");
        }
    }

    /// <summary>
    /// 変数名を日本語に変換（簡易版）
    /// </summary>
    private string ConvertToJapanese(string varName)
    {
        // 簡易的な変換ルール
        switch (varName.ToLower())
        {
            case "movespeed": return "移動速度";
            case "attacktime": return "攻撃時間";
            case "attackinterval": return "攻撃間隔";
            case "scrollspeedmin": return "スクロール最小速度";
            case "scrollspeedmax": return "スクロール最大速度";
            case "maxgear": return "最大ギア";
            case "speed": return "速度";
            case "curveinputdistance": return "カーブ入力距離";
            case "playermovespeed": return "プレイヤー移動速度";
            case "playerjumpheight": return "プレイヤージャンプ高度";
            case "lanewidth": return "レーン幅";
            case "stagegridsize": return "ステージグリッドサイズ";
            case "stagewidth": return "ステージ幅";
            case "stagelength": return "ステージ長";
            case "inputbuffertime": return "入力バッファ時間";
            case "inputqueuesize": return "入力キューサイズ";
            default: return varName; // そのまま返す
        }
    }
}

#endif