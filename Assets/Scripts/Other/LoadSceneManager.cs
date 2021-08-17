using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneManager : MonoBehaviour
{
    public void LoadLauncherScene()
    {
        if (SceneManager.GetActiveScene().name == "TitleScene")
        {
            StartCoroutine(nameof(Load), "LauncherScene");
        }
        else
        {
            Debug.LogError($"ロードできませんでした : 現在のシーン{SceneManager.GetActiveScene().name}");
        }
    }

    public void LoadTitleScene()
    {
        if (SceneManager.GetActiveScene().name == "LauncherScene")
        {
            StartCoroutine(nameof(Load), "TitleScene");
        }
        else
        {
            Debug.LogError($"ロードできませんでした : 現在のシーン{SceneManager.GetActiveScene().name}");
        }
    }

    IEnumerator Load(string sceneName)
    {
        yield return new WaitForSeconds(0.2f);
        SceneManager.LoadScene(sceneName);
    }
}
