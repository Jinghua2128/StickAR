using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SimpleSkinSelector : MonoBehaviour
{
    [Header("UI")]
    public Button prevButton;
    public Button nextButton;
    public TMP_Text skinText;

    [Header("Skins")]
    public string[] skins = { "Blue", "Yellow", "Pink", "Red", "Green", "Purple" };
    
    private int currentIndex = 0;
    private DataBaseManager dbManager;

    void Start()
    {
        dbManager = FindFirstObjectByType<DataBaseManager>();
        
        // Setup buttons
        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(PreviousSkin);
        }
        
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextSkin);
        }
        
        UpdateDisplay();
    }

    void PreviousSkin()
    {
        currentIndex--;
        if (currentIndex < 0)
            currentIndex = skins.Length - 1;
        
        UpdateDisplay();
        SaveToFirebase();
    }

    void NextSkin()
    {
        currentIndex++;
        if (currentIndex >= skins.Length)
            currentIndex = 0;
        
        UpdateDisplay();
        SaveToFirebase();
    }

    void UpdateDisplay()
    {
        if (skinText != null)
            skinText.text = skins[currentIndex];
            
        Debug.Log("Current skin: " + skins[currentIndex]);
    }

    void SaveToFirebase()
    {
        if (dbManager != null)
        {
            dbManager.UpdateSelectedSkin(skins[currentIndex]);
            Debug.Log("Saved skin: " + skins[currentIndex]);
        }
    }

    public string GetCurrentSkin()
    {
        return skins[currentIndex];
    }
}