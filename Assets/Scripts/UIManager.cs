using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("Auth Panels")]
    public GameObject loginPanel;
    public GameObject signUpPanel;

    [Header("Login Password")]
    public TMP_InputField loginPasswordField;
    public GameObject loginShowIcon;
    public GameObject loginHideIcon;

    [Header("Sign Up Password")]
    public TMP_InputField signUpPasswordField;
    public GameObject signUpShowIcon;
    public GameObject signUpHideIcon;

    [Header("Sign Up Confirm Password")]
    public TMP_InputField signUpConfirmPasswordField;
    public GameObject signUpConfirmShowIcon;
    public GameObject signUpConfirmHideIcon;

    [Header("Message UI (optional)")]
    public GameObject messagePanel;
    public TextMeshProUGUI messageText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        ShowLogin(); // default screen
    }

    // ───────────────── PANEL SWITCHING ─────────────────

    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        signUpPanel.SetActive(false);
    }

    public void ShowSignUp()
    {
        loginPanel.SetActive(false);
        signUpPanel.SetActive(true);
    }

    // ───────────── PASSWORD TOGGLE HELPERS ─────────────

    private void TogglePasswordField(TMP_InputField field, GameObject showIcon, GameObject hideIcon)
    {
        bool currentlyHidden = (field.contentType == TMP_InputField.ContentType.Password);

        if (currentlyHidden)
        {
            field.contentType = TMP_InputField.ContentType.Standard;
            showIcon.SetActive(false);
            hideIcon.SetActive(true);
        }
        else
        {
            field.contentType = TMP_InputField.ContentType.Password;
            showIcon.SetActive(true);
            hideIcon.SetActive(false);
        }

        field.ForceLabelUpdate();
    }

    // These are the ones you’ll hook up in the Button OnClick (no parameters):

    public void ToggleLoginPasswordVisibility()
    {
        TogglePasswordField(loginPasswordField, loginShowIcon, loginHideIcon);
    }

    public void ToggleSignUpPasswordVisibility()
    {
        TogglePasswordField(signUpPasswordField, signUpShowIcon, signUpHideIcon);
    }

    public void ToggleSignUpConfirmPasswordVisibility()
    {
        TogglePasswordField(signUpConfirmPasswordField, signUpConfirmShowIcon, signUpConfirmHideIcon);
    }

    // ───────────── OPTIONAL MESSAGE POPUP ─────────────

    public void ShowMessage(string msg)
    {
        if (messageText != null)
            messageText.text = msg;

        if (messagePanel != null)
            messagePanel.SetActive(true);
    }

    public void HideMessage()
    {
        if (messagePanel != null)
            messagePanel.SetActive(false);
    }
}