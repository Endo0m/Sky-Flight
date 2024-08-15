using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using Firebase;
using Firebase.RemoteConfig;
using System;

public class LoadManager : MonoBehaviour
{
    public Slider loadingSlider;
    public Canvas loadCanvas;
    public WebViewManager webViewManager;
    public GameManager gameManager;

    private const string RemoteConfigKey = "RegistrationPath";

    private void Start()
    {
        Debug.Log("LoadManager Start method called");
        loadCanvas.enabled = true;
        StartCoroutine(InitializeFirebaseAndLoad());
    }

    private IEnumerator InitializeFirebaseAndLoad()
    {
        Debug.Log("Initializing Firebase");
        var task = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        var dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            Debug.Log("Firebase initialized successfully");
            yield return StartCoroutine(LoadingProcess());
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            // Здесь можно добавить обработку ошибки инициализации Firebase
            ShowErrorAndFallbackToGame();
        }
    }

    private IEnumerator LoadingProcess()
    {
        Debug.Log("Starting loading process");
        loadingSlider.value = 0f;

        yield return StartCoroutine(SimulateLoading());

        Debug.Log("Simulated loading completed");

        string savedUrl = PlayerPrefs.GetString(RemoteConfigKey, "");
        Debug.Log($"Saved URL from PlayerPrefs: {savedUrl}");

        if (string.IsNullOrEmpty(savedUrl))
        {
            Debug.Log("No saved URL, fetching from Remote Config");
            yield return StartCoroutine(FetchRemoteConfig());
        }
        else
        {
            Debug.Log("Using saved URL");
            HandleConfigValue(savedUrl);
        }

        Debug.Log("LoadingProcess completed");
    }

    private IEnumerator SimulateLoading()
    {
        Debug.Log("Starting simulated loading");
        float progress = 0f;
        while (progress < 1f)
        {
            progress += 0.01f;
            loadingSlider.value = progress;
            Debug.Log($"Loading progress: {progress:P0}");
            yield return new WaitForSeconds(0.05f);
        }
        Debug.Log("Simulated loading completed");
    }

    private IEnumerator FetchRemoteConfig()
    {
        Debug.Log("Fetching Remote Config");
        bool fetchComplete = false;
        Exception fetchException = null;

        FirebaseRemoteConfig.DefaultInstance.FetchAsync(System.TimeSpan.Zero).ContinueWith(task => {
            if (task.IsCompleted)
            {
                Debug.Log("Remote Config fetch completed");
                FirebaseRemoteConfig.DefaultInstance.ActivateAsync().ContinueWith(activateTask => {
                    try
                    {
                        string fetchedUrl = FirebaseRemoteConfig.DefaultInstance.GetValue(RemoteConfigKey).StringValue;
                        Debug.Log($"Fetched URL: {fetchedUrl}");
                        PlayerPrefs.SetString(RemoteConfigKey, fetchedUrl);
                        PlayerPrefs.Save();
                        HandleConfigValue(fetchedUrl);
                    }
                    catch (Exception e)
                    {
                        fetchException = e;
                        Debug.LogError($"Error activating Remote Config: {e.Message}\n{e.StackTrace}");
                    }
                    finally
                    {
                        fetchComplete = true;
                    }
                });
            }
            else
            {
                fetchException = task.Exception;
                Debug.LogError($"Remote Config fetch failed: {task.Exception}");
                fetchComplete = true;
            }
        });

        while (!fetchComplete)
        {
            Debug.Log("Waiting for Remote Config fetch to complete...");
            yield return new WaitForSeconds(0.5f);
        }

        if (fetchException != null)
        {
            Debug.LogError($"Error occurred during Remote Config fetch: {fetchException.Message}");
            ShowErrorAndFallbackToGame();
        }

        Debug.Log("FetchRemoteConfig completed");
    }

    private void HandleConfigValue(string url)
    {
        Debug.Log($"Handling config value: {url}");
        loadCanvas.enabled = false;

        if (string.IsNullOrEmpty(url))
        {
            Debug.Log("URL is empty, showing game");
            gameManager.SetMainMenuState();
        }
        else
        {
            Debug.Log("URL is not empty, showing WebView");
            webViewManager.ShowUrlFullScreen(url);
        }

        Debug.Log("LoadManager process completed");
    }

    private void ShowErrorAndFallbackToGame()
    {
        Debug.Log("Showing error and falling back to game");
        loadCanvas.enabled = false;
        gameManager.SetMainMenuState();
    }
}