using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PrototypeSceneBuilder
{
    [MenuItem("Amane/プロトタイプシーンを作成", false, 1)]
    public static void CreatePrototypeScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

        var bootstrapObj = new GameObject("PrototypeBootstrap");
        bootstrapObj.AddComponent<Amane.Prototype.PrototypeBootstrap>();

        var mainCamera = GameObject.Find("Main Camera");
        if (mainCamera != null)
            mainCamera.GetComponent<Camera>().backgroundColor = new Color(0.106f, 0.165f, 0.29f);

        string scenePath = "Assets/_Project/Scenes/Prototype.unity";
        string fullDir = System.IO.Path.Combine(Application.dataPath, "_Project", "Scenes");
        System.IO.Directory.CreateDirectory(fullDir);
        EditorSceneManager.SaveScene(scene, scenePath);

        var buildScenes = new EditorBuildSettingsScene[]
        {
            new EditorBuildSettingsScene(scenePath, true)
        };
        EditorBuildSettings.scenes = buildScenes;

        Debug.Log("[Amane] プロトタイプシーンを作成しました。Playボタンを押してください！");
        EditorUtility.DisplayDialog("残響都市アマネ",
            "プロトタイプシーンを作成しました！\n\nPlayボタン(▶)を押すとゲームが始まります。\n\n" +
            "操作:\n" +
            "・タイトル → 「未読を開く」でゲーム開始\n" +
            "・フィールド → 行動を選択（AP消費）\n" +
            "・「未言界へ潜行する」→ 戦闘\n" +
            "・戦闘 → 攻撃/スキル/言継ぎ/総告白",
            "OK");
    }

    [MenuItem("Amane/ゲームをテストプレイ", false, 2)]
    public static void PlayPrototype()
    {
        string scenePath = "Assets/_Project/Scenes/Prototype.unity";
        if (!System.IO.File.Exists(
            System.IO.Path.Combine(Application.dataPath, "../", scenePath)))
        {
            CreatePrototypeScene();
        }
        EditorSceneManager.OpenScene(scenePath);
        EditorApplication.isPlaying = true;
    }
}
