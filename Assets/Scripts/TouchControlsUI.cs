using UnityEngine;
using UnityEngine.UI;

// TouchControlsUI.cs - Attach to Canvas GameObject
// Creates visual touch controls for mobile gameplay
public class TouchControlsUI : MonoBehaviour
{
    [Header("Touch Control Sprites")]
    public Sprite joystickBaseSprite;
    public Sprite joystickHandleSprite;
    public Sprite jumpButtonSprite;
    public Sprite attackButtonSprite;

    [Header("Joystick Settings")]
    public float joystickSize = 150f;
    public float handleSize = 80f;
    public Color joystickColor = new Color(1f, 1f, 1f, 0.5f);

    [Header("Button Settings")]
    public float buttonSize = 100f;
    public Color buttonColor = new Color(1f, 1f, 1f, 0.6f);

    [Header("Visual Feedback")]
    public Color pressedColor = new Color(1f, 1f, 0f, 0.8f);
    public float pressedScale = 1.1f;

    // UI Elements
    private GameObject joystickBase;
    private GameObject joystickHandle;
    private GameObject jumpButton;
    private GameObject attackButton;

    private Image joystickBaseImage;
    private Image joystickHandleImage;
    private Image jumpButtonImage;
    private Image attackButtonImage;

    // Touch tracking
    private int joystickTouchId = -1;
    private Vector2 joystickStartPos;
    private Vector2 joystickCurrentPos;

    private bool isJumpPressed = false;
    private bool isAttackPressed = false;

    void Start()
    {
        // Only create controls on mobile
        if (!Application.isMobilePlatform)
        {
            gameObject.SetActive(false);
            return;
        }

        CreateTouchControls();
    }

    void CreateTouchControls()
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        // Match PlayerController touch zones:
        // Left side: Movement joystick area (left 40% of screen)
        // Jump button: 75% x-position, 10% y-position (lower right)
        // Attack button: 75% x-position, 35% y-position (above jump)

        // Create Joystick Base (positioned in left 40% zone)
        joystickBase = new GameObject("JoystickBase");
        joystickBase.transform.SetParent(transform, false);
        
        RectTransform joystickBaseRect = joystickBase.AddComponent<RectTransform>();
        joystickBaseRect.sizeDelta = new Vector2(joystickSize, joystickSize);
        joystickBaseRect.anchorMin = new Vector2(0, 0);
        joystickBaseRect.anchorMax = new Vector2(0, 0);
        joystickBaseRect.pivot = new Vector2(0.5f, 0.5f);
        // Position at 20% of screen width (center of left 40% zone) and 15% screen height
        joystickBaseRect.anchoredPosition = new Vector2(screenWidth * 0.2f, screenHeight * 0.15f);

        joystickBaseImage = joystickBase.AddComponent<Image>();
        joystickBaseImage.sprite = joystickBaseSprite;
        joystickBaseImage.color = joystickColor;
        
        // If no sprite, create a circle
        if (joystickBaseSprite == null)
        {
            joystickBaseImage.sprite = CreateCircleSprite();
        }

        // Create Joystick Handle
        joystickHandle = new GameObject("JoystickHandle");
        joystickHandle.transform.SetParent(joystickBase.transform, false);
        
        RectTransform joystickHandleRect = joystickHandle.AddComponent<RectTransform>();
        joystickHandleRect.sizeDelta = new Vector2(handleSize, handleSize);
        joystickHandleRect.anchoredPosition = Vector2.zero;

        joystickHandleImage = joystickHandle.AddComponent<Image>();
        joystickHandleImage.sprite = joystickHandleSprite;
        joystickHandleImage.color = joystickColor;
        
        if (joystickHandleSprite == null)
        {
            joystickHandleImage.sprite = CreateCircleSprite();
        }

        // Create Jump Button (matches PlayerController: 75% x, 10% y from bottom-left)
        jumpButton = new GameObject("JumpButton");
        jumpButton.transform.SetParent(transform, false);
        
        RectTransform jumpButtonRect = jumpButton.AddComponent<RectTransform>();
        jumpButtonRect.sizeDelta = new Vector2(buttonSize, buttonSize);
        jumpButtonRect.anchorMin = new Vector2(0, 0);
        jumpButtonRect.anchorMax = new Vector2(0, 0);
        jumpButtonRect.pivot = new Vector2(0.5f, 0.5f);
        // Position at 75% + 10% (center of button zone) x, and 20% y (center of 10-30% zone)
        jumpButtonRect.anchoredPosition = new Vector2(screenWidth * 0.85f, screenHeight * 0.2f);

        jumpButtonImage = jumpButton.AddComponent<Image>();
        jumpButtonImage.sprite = jumpButtonSprite;
        jumpButtonImage.color = buttonColor;
        
        if (jumpButtonSprite == null)
        {
            jumpButtonImage.sprite = CreateCircleSprite();
        }

        // Add "JUMP" text label
        CreateButtonLabel(jumpButton, "JUMP");

        // Create Attack Button (matches PlayerController: 75% x, 35% y from bottom-left)
        attackButton = new GameObject("AttackButton");
        attackButton.transform.SetParent(transform, false);
        
        RectTransform attackButtonRect = attackButton.AddComponent<RectTransform>();
        attackButtonRect.sizeDelta = new Vector2(buttonSize, buttonSize);
        attackButtonRect.anchorMin = new Vector2(0, 0);
        attackButtonRect.anchorMax = new Vector2(0, 0);
        attackButtonRect.pivot = new Vector2(0.5f, 0.5f);
        // Position at 75% + 10% x, and 45% y (center of 35-55% zone)
        attackButtonRect.anchoredPosition = new Vector2(screenWidth * 0.85f, screenHeight * 0.45f);

        attackButtonImage = attackButton.AddComponent<Image>();
        attackButtonImage.sprite = attackButtonSprite;
        attackButtonImage.color = buttonColor;
        
        if (attackButtonSprite == null)
        {
            attackButtonImage.sprite = CreateCircleSprite();
        }

        // Add "ATTACK" text label
        CreateButtonLabel(attackButton, "ATTACK");
    }

    void CreateButtonLabel(GameObject button, string labelText)
    {
        GameObject label = new GameObject("Label");
        label.transform.SetParent(button.transform, false);
        
        RectTransform labelRect = label.AddComponent<RectTransform>();
        labelRect.sizeDelta = new Vector2(buttonSize, 30f);
        labelRect.anchoredPosition = Vector2.zero;

        Text text = label.AddComponent<Text>();
        text.text = labelText;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.fontSize = 16;
        text.alignment = TextAnchor.MiddleCenter;
        text.color = Color.white;
        
        // Add outline for better visibility
        var outline = label.AddComponent<Outline>();
        outline.effectColor = Color.black;
        outline.effectDistance = new Vector2(1, -1);
    }

    Sprite CreateCircleSprite()
    {
        // Create a simple circle texture
        int size = 128;
        Texture2D texture = new Texture2D(size, size);
        Color[] pixels = new Color[size * size];
        
        Vector2 center = new Vector2(size / 2f, size / 2f);
        float radius = size / 2f;
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 pos = new Vector2(x, y);
                float distance = Vector2.Distance(pos, center);
                
                if (distance <= radius)
                {
                    float alpha = 1f;
                    // Soft edge
                    if (distance > radius - 5f)
                    {
                        alpha = (radius - distance) / 5f;
                    }
                    pixels[y * size + x] = new Color(1f, 1f, 1f, alpha);
                }
                else
                {
                    pixels[y * size + x] = Color.clear;
                }
            }
        }
        
        texture.SetPixels(pixels);
        texture.Apply();
        
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (!Application.isMobilePlatform) return;

        ProcessTouchInput();
        UpdateJoystickVisuals();
        UpdateButtonVisuals();
    }

    void ProcessTouchInput()
    {
        // Reset button states
        isJumpPressed = false;
        isAttackPressed = false;

        // Get screen rectangles for each control
        Rect joystickRect = GetScreenRect(joystickBase.GetComponent<RectTransform>());
        Rect jumpRect = GetScreenRect(jumpButton.GetComponent<RectTransform>());
        Rect attackRect = GetScreenRect(attackButton.GetComponent<RectTransform>());

        // Process all touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            // Handle Joystick
            if (joystickRect.Contains(touch.position))
            {
                if (touch.phase == TouchPhase.Began)
                {
                    joystickTouchId = touch.fingerId;
                    joystickStartPos = touch.position;
                    joystickCurrentPos = touch.position;
                }
                else if (touch.fingerId == joystickTouchId)
                {
                    if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    {
                        joystickCurrentPos = touch.position;
                    }
                    else if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
                    {
                        joystickTouchId = -1;
                        joystickCurrentPos = joystickStartPos;
                    }
                }
            }

            // Handle Jump Button
            if (jumpRect.Contains(touch.position))
            {
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary)
                {
                    isJumpPressed = true;
                }
            }

            // Handle Attack Button
            if (attackRect.Contains(touch.position))
            {
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Stationary)
                {
                    isAttackPressed = true;
                }
            }
        }

        // Reset joystick if no touch
        if (joystickTouchId == -1)
        {
            joystickCurrentPos = joystickStartPos;
        }
    }

    void UpdateJoystickVisuals()
    {
        if (joystickHandle == null) return;

        RectTransform handleRect = joystickHandle.GetComponent<RectTransform>();
        
        if (joystickTouchId != -1)
        {
            // Calculate handle position based on touch
            Vector2 delta = joystickCurrentPos - joystickStartPos;
            float maxDistance = joystickSize / 2f - handleSize / 2f;
            delta = Vector2.ClampMagnitude(delta, maxDistance);
            
            // Convert screen space to local space
            RectTransform joystickRect = joystickBase.GetComponent<RectTransform>();
            Vector2 localPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                joystickRect, 
                joystickStartPos + delta, 
                null, 
                out localPos
            );
            
            handleRect.anchoredPosition = localPos;
            
            // Visual feedback
            joystickHandleImage.color = pressedColor;
        }
        else
        {
            // Reset to center
            handleRect.anchoredPosition = Vector2.zero;
            joystickHandleImage.color = joystickColor;
        }
    }

    void UpdateButtonVisuals()
    {
        // Jump button feedback
        if (jumpButtonImage != null)
        {
            if (isJumpPressed)
            {
                jumpButtonImage.color = pressedColor;
                jumpButton.transform.localScale = Vector3.one * pressedScale;
            }
            else
            {
                jumpButtonImage.color = buttonColor;
                jumpButton.transform.localScale = Vector3.one;
            }
        }

        // Attack button feedback
        if (attackButtonImage != null)
        {
            if (isAttackPressed)
            {
                attackButtonImage.color = pressedColor;
                attackButton.transform.localScale = Vector3.one * pressedScale;
            }
            else
            {
                attackButtonImage.color = buttonColor;
                attackButton.transform.localScale = Vector3.one;
            }
        }
    }

    Rect GetScreenRect(RectTransform rectTransform)
    {
        Vector3[] corners = new Vector3[4];
        rectTransform.GetWorldCorners(corners);
        
        float xMin = corners[0].x;
        float xMax = corners[2].x;
        float yMin = corners[0].y;
        float yMax = corners[2].y;
        
        return new Rect(xMin, yMin, xMax - xMin, yMax - yMin);
    }

    // Public method to check if controls are being used (optional, for debugging)
    public bool IsJoystickActive()
    {
        return joystickTouchId != -1;
    }

    public Vector2 GetJoystickDirection()
    {
        if (joystickTouchId == -1) return Vector2.zero;
        
        Vector2 delta = joystickCurrentPos - joystickStartPos;
        float maxDistance = joystickSize / 2f;
        return delta / maxDistance;
    }
}