using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum AppState
{
    Start,
    Scene1,
    Scene2,
    Scene3
}

// It works as state machine. You can add more states to expand scenario.
// NextState() can be called from other script to provide scenario action control.
// Switching sceens are seamless: hide objects -> destroy objects -> additional load scene -> unload old scene -> show objects  

public class ApplicationManager : MonoBehaviour
{
    public static ApplicationManager Instance;

    [SerializeField] private GameObject[] circles;

    private AppState _currentState = AppState.Start;

    private Color _defaultCircleColor = new Color(1f, 1f, 1f, 98f / 255f);
    private Color _currentCircleColor = new Color(217f / 255f, 192f / 255f, 119f / 255f);

    private Dictionary<AppState, string> _stateSceneMap = new Dictionary<AppState, string>()
    {
        { AppState.Scene1, "Scene1" },
        { AppState.Scene2, "Scene2" },
        { AppState.Scene3, "Scene3" }
    };

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    private async void Start()
    {
        CameraController.Instance.enabled = false;
        await NextState(_currentState + 1);
    }

    public async void NextState()
    {
        await NextState(_currentState + 1);
    }

    private async Task NextState(AppState nextState)
    {
        await FadeObjectsOnSwitchScene(false);
        DestroyObjectsOnSwitchScene();
        await LoadAsyncScene(_stateSceneMap[nextState]);

        if (_currentState != AppState.Start)
            await UnloadAsyncScene(_stateSceneMap[_currentState]);

        switch (nextState)
        {
            case AppState.Scene1:
                CameraController.Instance.enabled = false;
                SetCircleColor(0);
                _currentState = AppState.Scene1;
                break;
            case AppState.Scene2:
                CameraController.Instance.enabled = true;
                SetCircleColor(1);
                _currentState = AppState.Scene2;
                break;
            case AppState.Scene3:
                CameraController.Instance.enabled = true;
                SetCircleColor(2);
                _currentState = AppState.Scene3;
                break;
        }

        await FadeObjectsOnSwitchScene(true);
    }

    private async Task LoadAsyncScene(string name)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name, LoadSceneMode.Additive);
        while (!asyncLoad.isDone)
        {
            await Task.Yield();
        }
    }

    private async Task UnloadAsyncScene(string name)
    {
        AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(name);
        while (!asyncUnload.isDone)
        {
            await Task.Yield();
        }
    }

    private async Task FadeObjectsOnSwitchScene(bool fadeIn)
    {
        ObjectFader[] faders = FindObjectsOfType<ObjectFader>();
        foreach (ObjectFader fader in faders)
        {
            if (fader.IgnoreFader)
                continue;

            if (fadeIn)
                fader.FadeIn();
            else
                fader.FadeOut();
        }

        bool allFadersFinished = false;
        int maxAttempts = 10000;
        int attempts = 0;
        while (!allFadersFinished && attempts < maxAttempts)
        {
            allFadersFinished = true;
            foreach (ObjectFader fader in faders)
            {
                if (fader.IsFading)
                {
                    allFadersFinished = false;
                    break;
                }
            }
            attempts++;
            await Task.Yield();
        }
    }

    private void DestroyObjectsOnSwitchScene()
    {
        ObjectFader[] faders = FindObjectsOfType<ObjectFader>();
        foreach (ObjectFader fader in faders)
        {
            if (!fader.IgnoreFader)
                Destroy(fader.gameObject);
        }
    }

    public async void OnObjectClick(GameObject obj)
    {
        obj.transform.SetParent(null);
        obj.transform.DetachChildren();
        obj.GetComponent<ObjectFader>().IgnoreFader = true;
        DontDestroyOnLoad(obj);
        await NextState(_currentState + 1);
        obj.GetComponent<ObjectFader>().IgnoreFader = false;
    }

    public async void ResetState()
    {
        await NextState(AppState.Scene1);
        CameraController.Instance.ReturnToDefault = true;
    }

    private void SetCircleColor(int index)
    {
        RawImage targetImage = circles[index].GetComponent<RawImage>();

        foreach (GameObject circle in circles)
        {
            RawImage image = circle.GetComponent<RawImage>();

            if (image != targetImage)
                StartCoroutine(ChangeColorSmoothly(image, image.color, _defaultCircleColor));
        }
        
        StartCoroutine(ChangeColorSmoothly(targetImage, targetImage.color, _currentCircleColor));
    }

    private IEnumerator ChangeColorSmoothly(RawImage image, Color startColor, Color targetColor)
    {
        float elapsedTime = 0f;

        while (elapsedTime < 1f)
        {
            float t = elapsedTime / 1f;
            image.color = Color.Lerp(startColor, targetColor, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        image.color = targetColor;
    }
}

