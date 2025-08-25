using System;
using System.IO;
using UnityEditor;
using UnityEngine;

// ========== Auto Injection Processor ==========

#if UNITY_EDITOR

/// <summary>
/// ドメインリロード後の自動処理
/// </summary>
[InitializeOnLoad]
public static class InjectAutoProcessor
{
    private const string SETTINGS_PATH = "Assets/DataAsset/Params/ParamInjectSettings.asset";
    private const string LAST_GENERATION_TIME_KEY = "InjectAutoProcessor.LastGenerationTime";

    static InjectAutoProcessor()
    {
        EditorApplication.delayCall += OnEditorReady;
    }

    private static void OnEditorReady()
    {
        // 設定ファイルの存在確認
        var settings = LoadSettings();
        if (settings == null)
        {
            Debug.LogWarning("InjectAutoProcessor: ParamInjectSettings not found. Creating default settings.");
            CreateDefaultSettings();
            return;
        }

        // 自動生成が有効な場合のみ実行
        if (settings.AutoGenerateOnReload)
        {
            CheckAndGenerateIfNeeded();
        }
    }

    private static ParamInjectSettings LoadSettings()
    {
        return AssetDatabase.LoadAssetAtPath<ParamInjectSettings>(SETTINGS_PATH);
    }

    private static void CreateDefaultSettings()
    {
        var settings = ScriptableObject.CreateInstance<ParamInjectSettings>();
        
        // デフォルトの設定ディレクトリを作成
        string directory = Path.GetDirectoryName(SETTINGS_PATH);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        AssetDatabase.CreateAsset(settings, SETTINGS_PATH);
        AssetDatabase.SaveAssets();
        
        Debug.Log($"InjectAutoProcessor: Created default settings at {SETTINGS_PATH}");
    }

    private static void CheckAndGenerateIfNeeded()
    {
        try
        {
            // スクリプトの変更時刻をチェック
            string lastGenerationTimeString = EditorPrefs.GetString(LAST_GENERATION_TIME_KEY, "");
            DateTime lastGenerationTime = DateTime.MinValue;
            
            if (!string.IsNullOrEmpty(lastGenerationTimeString))
            {
                DateTime.TryParse(lastGenerationTimeString, out lastGenerationTime);
            }

            // スクリプトファイルの最新更新時刻を取得
            DateTime latestScriptTime = GetLatestScriptModificationTime();
            
            // 生成が必要かチェック
            bool shouldGenerate = lastGenerationTime < latestScriptTime;
            
            if (shouldGenerate)
            {
                Debug.Log("InjectAutoProcessor: Script changes detected. Generating injection code...");
                DICodeGenerator.GenerateInjectionCode();
                
                // 最終生成時刻を更新
                EditorPrefs.SetString(LAST_GENERATION_TIME_KEY, DateTime.Now.ToString("O"));
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"InjectAutoProcessor: Error during auto generation: {e.Message}");
        }
    }

    private static DateTime GetLatestScriptModificationTime()
    {
        DateTime latestTime = DateTime.MinValue;
        
        // Scripts フォルダ内のC#ファイルをチェック
        string[] scriptFiles = Directory.GetFiles("Assets/Scripts", "*.cs", SearchOption.AllDirectories);
        
        foreach (string file in scriptFiles)
        {
            // Generated フォルダは除外
            if (file.Contains("/Generated/") || file.Contains("\\Generated\\"))
                continue;
                
            DateTime fileTime = File.GetLastWriteTime(file);
            if (fileTime > latestTime)
            {
                latestTime = fileTime;
            }
        }
        
        return latestTime;
    }


    /// <summary>
    /// 手動で再生成を実行
    /// </summary>
    [MenuItem("Tools/DI/Force Regenerate All")]
    public static void ForceRegenerate()
    {
        Debug.Log("InjectAutoProcessor: Force regenerating all injection code...");
        DICodeGenerator.GenerateInjectionCode();
        EditorPrefs.SetString(LAST_GENERATION_TIME_KEY, DateTime.Now.ToString("O"));
    }

    /// <summary>
    /// 自動生成の有効/無効を切り替え
    /// </summary>
    [MenuItem("Tools/DI/Toggle Auto Generation")]
    public static void ToggleAutoGeneration()
    {
        var settings = LoadSettings();
        if (settings != null)
        {
            // リフレクションを使ってprivateフィールドにアクセス
            var field = typeof(ParamInjectSettings).GetField("_autoGenerateOnReload", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            
            if (field != null)
            {
                bool currentValue = (bool)field.GetValue(settings);
                field.SetValue(settings, !currentValue);
                EditorUtility.SetDirty(settings);
                AssetDatabase.SaveAssets();
                
                Debug.Log($"InjectAutoProcessor: Auto generation {(!currentValue ? "enabled" : "disabled")}");
            }
        }
    }
}

#endif