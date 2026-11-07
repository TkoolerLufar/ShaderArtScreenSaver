using System;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif


#if ENABLE_INPUT_SYSTEM
using MousePosition = UnityEngine.Vector2;
#else
using MousePosition = UnityEngine.Vector3;
#endif

public class ScreenSaverController : MonoBehaviour
{
    /// <summary>
    /// prevMousePosition is initialized as this invalid position.
    /// </summary>
    private bool isMouseCaptured = false;

    /// <summary>
    /// True if this is preview.
    /// </summary>
    public bool IsPreview
    {
        get;
        private set;
    }
    private MousePosition prevMousePosition;
    private bool isQuitting = false;

    /// <summary>
    /// If false, this disables itself at starting with preview mode.
    /// </summary>
    [SerializeField] private bool StayEnabledEvenIfPreview;

    private void Awake()
    {
#if UNITY_STANDALONE && !UNITY_EDITOR
        this.IsPreview = PlayerPrefs.GetInt("isPreview") != 0;
#else
        this.IsPreview = false;
#endif
    }

    private void Start()
    {
        if (!this.StayEnabledEvenIfPreview && this.IsPreview)
        {
            this.enabled = false;
        }
    }

    void Update()
    {
        if (IsPreview)
        {
            return;
        }

        // Quit when the screen is touched
        if (IsTouched())
        {
            Quit();
            return;
        }

        // Quit when the mouse is moving
#if ENABLE_INPUT_SYSTEM
        var currentMousePosition = Mouse.current?.position?.value ?? prevMousePosition;
#else
        var currentMousePosition = Input.mousePosition;
#endif
        if (isMouseCaptured && prevMousePosition != currentMousePosition)
        {
            Quit();
            return;
        }
        prevMousePosition = currentMousePosition;
        isMouseCaptured = true;

        // Quit when any key or mouse button is pressed
        if (IsKeyboardOrMousePressed())
        {
            Quit();
        }
    }

    private static bool IsKeyboardOrMousePressed() =>
#if ENABLE_INPUT_SYSTEM
        (Keyboard.current?.anyKey?.isPressed ?? false) ||
        (Mouse.current?.leftButton?.isPressed ?? false) ||
        (Mouse.current?.rightButton?.isPressed ?? false) ||
        (Mouse.current?.middleButton?.isPressed ?? false)
#else
        Input.anyKey
#endif
        ;

    private static bool IsTouched() =>
#if ENABLE_INPUT_SYSTEM
        Touchscreen.current?.press?.isPressed ?? false
#else
        Input.touchCount > 0
#endif
        ;
    /// <summary>
    /// Try to quit this screen saver.
    /// This function does nothing after once success.
    /// </summary>
    void Quit()
    {
        if (this.isQuitting)
        {
            return;
        }
        QuitApplication();
        this.isQuitting = true;
        this.enabled = false;
    }

    /// <summary>
    /// Quit this screen saver. Throw something if it fails.
    /// </summary>
    void QuitApplication()
    {
#if UNITY_EDITOR
        // Only log and do nothing else in editor
        Debug.Log("Quit!");
#elif UNITY_ANDROID
        using var UnityPlayerDream = new AndroidJavaClass("jp.tkoolerlufar.madewithunity.screensaver.UnityPlayerDream");
        using var UnityPlayerDreamCompanion = UnityPlayerDream.GetStatic<AndroidJavaObject>("Companion");
        using var currentService = UnityPlayerDreamCompanion.Call<AndroidJavaObject>("getCurrentService");
        if (currentService is not null)
        {
            currentService.Call("finish");
            return;
        }
        using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        using var currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        if (currentActivity is not null)
        {
            currentActivity.Call<bool>("moveTaskToBack", true);
            return;
        }
        throw new SystemException("Failed to exit the screen saver.");
#else
        Application.Quit();
#endif
    }
}
