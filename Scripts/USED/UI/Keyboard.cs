using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class KeyboardHandler : MonoBehaviour
{
    [Header("Keyboard")]
    [SerializeField] public TMP_InputField inputField;
    [SerializeField] private GameObject openKeyboardButton;
    private TouchScreenKeyboard keyboard = null;
    private System.Action<string> onKeyboardDone;

    private void Start()
    {
        if (openKeyboardButton != null)
        {
            openKeyboardButton.GetComponent<Button>().onClick.AddListener(OpenKeyboard);
        }
    }

    public void OpenKeyboard()
    {
        OpenKeyboard(inputField.text);
    }

    public void OpenKeyboard(string text = "", TouchScreenKeyboardType keyboardType = TouchScreenKeyboardType.Default, bool autocorrection = false, bool multiline = false, bool secure = false, bool alert = false, System.Action<string> onDone = null)
    {
        keyboard = TouchScreenKeyboard.Open(text, keyboardType, autocorrection, multiline, secure, alert);
        onKeyboardDone = onDone;
    }

    private void Update()
    {
        if (keyboard != null && keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            Debug.Log("Wprowadzony tekst: " + keyboard.text);
            inputField.text = keyboard.text;
            onKeyboardDone?.Invoke(keyboard.text);
            keyboard = null;
        }
    }
}