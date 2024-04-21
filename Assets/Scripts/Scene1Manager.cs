using System.Threading.Tasks;
using UnityEngine;

// Scenario manager for scene1

public class Scene1Manager : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    private async void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        await FadeIn();
        await Task.Delay(1000);
        await FadeOut();

        ApplicationManager.Instance.NextState();
    }

    private async Task FadeIn()
    {
        await FadeCanvasGroup(_canvasGroup, _canvasGroup.alpha, 1f, 1f);
    }

    private async Task FadeOut()
    {
        await FadeCanvasGroup(_canvasGroup, _canvasGroup.alpha, 0f, 1f);
    }

    private async Task FadeCanvasGroup(CanvasGroup cg, float start, float end, float duration)
    {
        float startTime = Time.time;
        while (Time.time < startTime + duration)
        {
            cg.alpha = Mathf.Lerp(start, end, (Time.time - startTime) / duration);
            await Task.Yield();
        }
        cg.alpha = end;
    }
}