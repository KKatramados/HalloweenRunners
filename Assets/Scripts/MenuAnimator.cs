using UnityEngine;
using UnityEngine.UI;
using System.Collections;

// MenuAnimator.cs - Adds animations to menu elements
public class MenuAnimator : MonoBehaviour
{
    [Header("Title Animation")]
    public RectTransform titleObject;
    public float titleBounceHeight = 20f;
    public float titleBounceSpeed = 2f;

    [Header("Button Animation")]
    public Button[] animatedButtons;
    public float buttonScaleAmount = 1.1f;
    public float buttonScaleDuration = 0.2f;

    private Vector3 titleStartPos;

    void Start()
    {
        if (titleObject != null)
        {
            titleStartPos = titleObject.localPosition;
            StartCoroutine(AnimateTitle());
        }

        // Add hover effects to buttons
        foreach (var button in animatedButtons)
        {
            if (button != null)
            {
                AddButtonHoverEffect(button);
            }
        }
    }

    IEnumerator AnimateTitle()
    {
        while (true)
        {
            float newY = titleStartPos.y + Mathf.Sin(Time.time * titleBounceSpeed) * titleBounceHeight;
            titleObject.localPosition = new Vector3(titleStartPos.x, newY, titleStartPos.z);
            yield return null;
        }
    }

    void AddButtonHoverEffect(Button button)
    {
        var trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        // On pointer enter (hover)
        var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryEnter.eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter;
        entryEnter.callback.AddListener((data) => { OnButtonHover(button.transform, true); });
        trigger.triggers.Add(entryEnter);

        // On pointer exit
        var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry();
        entryExit.eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit;
        entryExit.callback.AddListener((data) => { OnButtonHover(button.transform, false); });
        trigger.triggers.Add(entryExit);
    }

    void OnButtonHover(Transform buttonTransform, bool isHovering)
    {
        StopAllCoroutines();
        StartCoroutine(ScaleButton(buttonTransform, isHovering ? buttonScaleAmount : 1f));
    }

    IEnumerator ScaleButton(Transform buttonTransform, float targetScale)
    {
        Vector3 startScale = buttonTransform.localScale;
        Vector3 endScale = Vector3.one * targetScale;
        float elapsed = 0f;

        while (elapsed < buttonScaleDuration)
        {
            elapsed += Time.deltaTime;
            buttonTransform.localScale = Vector3.Lerp(startScale, endScale, elapsed / buttonScaleDuration);
            yield return null;
        }

        buttonTransform.localScale = endScale;
    }
}