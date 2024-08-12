using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class StartScreenManager : MonoBehaviour
{
    public Canvas startCanvas;
    public Button continueButton;
    public Toggle privacyToggle;
    public Toggle termsToggle;
    public TextMeshProUGUI privacyText;
    public TextMeshProUGUI termsText;
    public Sprite toggleOnSprite;
    public Sprite toggleOffSprite;

    private WebViewManager webViewManager;
    private GameManager gameManager;
    private readonly string linkColor = "#E4053B";

    private void Start()
    {
        webViewManager = FindObjectOfType<WebViewManager>();
        gameManager = GameManager.Instance;
        SetupToggles();
        SetupHyperlinks();
        SetupContinueButton();
        CheckFirstLaunch();
    }

    private void SetupToggles()
    {
        SetupToggle(privacyToggle);
        SetupToggle(termsToggle);

        privacyToggle.onValueChanged.AddListener((_) => UpdateContinueButtonState());
        termsToggle.onValueChanged.AddListener((_) => UpdateContinueButtonState());

        UpdateContinueButtonState();
    }

    private void SetupToggle(Toggle toggle)
    {
        toggle.isOn = false;
        UpdateToggleSprite(toggle);
        toggle.onValueChanged.AddListener((isOn) => UpdateToggleSprite(toggle));
    }

    private void UpdateToggleSprite(Toggle toggle)
    {
        Image toggleImage = toggle.transform.Find("Background")?.GetComponent<Image>();
        if (toggleImage != null)
        {
            toggleImage.sprite = toggle.isOn ? toggleOnSprite : toggleOffSprite;
        }
        else
        {
            Debug.LogWarning($"Background Image not found for Toggle {toggle.name}");
        }
    }

    private void SetupContinueButton()
    {
        continueButton.onClick.AddListener(OnContinueClicked);
        UpdateContinueButtonState();
    }

    private void SetupHyperlinks()
    {
        privacyText.text = $"I agree to the <link=\"privacy\"><color={linkColor}>Privacy Policy</color></link>";
        termsText.text = $"I agree to the <link=\"terms\"><color={linkColor}>Terms of Use</color></link>";

        AddClickEvent(privacyText.gameObject, (data) => HandleTextClick(privacyText, data));
        AddClickEvent(termsText.gameObject, (data) => HandleTextClick(termsText, data));
    }

    private void AddClickEvent(GameObject go, UnityEngine.Events.UnityAction<PointerEventData> action)
    {
        EventTrigger trigger = go.GetComponent<EventTrigger>() ?? go.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((data) => { action((PointerEventData)data); });
        trigger.triggers.Add(entry);
    }

    private void HandleTextClick(TextMeshProUGUI tmpText, PointerEventData eventData)
    {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(tmpText, eventData.position, null);
        if (linkIndex != -1)
        {
            string linkId = tmpText.textInfo.linkInfo[linkIndex].GetLinkID();
            switch (linkId)
            {
                case "privacy":
                    webViewManager.OpenPrivacyPolicy();
                    break;
                case "terms":
                    webViewManager.OpenTermsOfUse();
                    break;
            }
        }
    }

    private void UpdateContinueButtonState()
    {
        bool isInteractable = privacyToggle.isOn && termsToggle.isOn;
        continueButton.interactable = isInteractable;

        // Обновляем прозрачность кнопки и её текста
        CanvasGroup buttonCanvasGroup = continueButton.GetComponent<CanvasGroup>();
        if (buttonCanvasGroup == null)
        {
            buttonCanvasGroup = continueButton.gameObject.AddComponent<CanvasGroup>();
        }
        buttonCanvasGroup.alpha = isInteractable ? 1f : 0.2f;
    }

    private void OnContinueClicked()
    {
        PlayerPrefs.SetInt("FirstLaunch", 1);
        PlayerPrefs.Save();
        startCanvas.enabled = false;
        gameManager.SetMainMenuState();
    }

    private void CheckFirstLaunch()
    {
        if (PlayerPrefs.GetInt("FirstLaunch", 0) == 0)
        {
            startCanvas.enabled = true;
        }
        else
        {
            startCanvas.enabled = false;
            gameManager.SetMainMenuState();
        }
    }
}