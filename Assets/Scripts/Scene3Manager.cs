using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Scenario manager for scene3

public class Scene3Manager : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Button resetButton;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text radiusText;

    private void Start()
    {
        resetButton.onClick.AddListener(ResetApp);

        SphereController selectedSphere = FindObjectOfType<SphereController>();

        if (selectedSphere != null)
        {
            nameText.text = selectedSphere.PlanetName;
            radiusText.text = selectedSphere.PlanetRadius;
        }

        FadeIn();
    }

    private void ResetApp()
    {
        FadeOut();
        ApplicationManager.Instance.ResetState();
    }

    private async Task FadeIn()
    {
        await FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 1f, 1f);
    }

    private async Task FadeOut()
    {
        await FadeCanvasGroup(canvasGroup, canvasGroup.alpha, 0f, 1f);
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
