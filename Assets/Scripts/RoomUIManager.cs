using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class RoomUIManager : MonoBehaviour
{
    [Header("Room Page UI")]
    public GameObject roomPage;                 // Main room page UI
    public GameObject createStickARPanel;       // Panel for creating StickAR
    public TMP_InputField stickARContentInput;  // Input field for StickAR text
    public GameObject gridSelectionUI;          // UI showing 3x3 grid selection

    [Header("Skin Selection")]
    public GameObject skinSelectionPanel;       // Panel for choosing skin
    public Transform skinButtonsParent;         // Parent for skin buttons
    public GameObject skinButtonPrefab;         // Prefab for skin button

    [Header("Grid Selection")]
    public Button[] gridButtons;                // 9 buttons for 3x3 grid (assign in inspector)

    [Header("Other UI")]
    public Button createButton;
    public Button cancelButton;
    public Button skinButton;                   // Button to open skin selection
    public TMP_Text currentSkinText;            // Shows current selected skin

    private RoomCanvasController currentCanvas;
    private int selectedGridX = -1;
    private int selectedGridY = -1;
    private List<string> playerUnlockedSkins = new List<string>();

    void Start()
    {
        // Hide all panels initially
        if (roomPage != null) roomPage.SetActive(false);
        if (createStickARPanel != null) createStickARPanel.SetActive(false);
        if (skinSelectionPanel != null) skinSelectionPanel.SetActive(false);

        // Setup button listeners
        if (createButton != null)
            createButton.onClick.AddListener(OnCreateStickAR);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelCreate);

        if (skinButton != null)
            skinButton.onClick.AddListener(OnOpenSkinSelection);

        // Setup grid buttons
        SetupGridButtons();
    }

    void SetupGridButtons()
    {
        if (gridButtons == null || gridButtons.Length != 9) return;

        for (int i = 0; i < 9; i++)
        {
            int x = i % 3;
            int y = i / 3;
            int index = i; // Capture for lambda

            gridButtons[i].onClick.AddListener(() => OnGridPositionSelected(x, y, index));
        }
    }

    public void ShowRoomPage()
    {
        if (roomPage != null)
            roomPage.SetActive(true);

        // Load player's unlocked skins
        LoadPlayerSkins();
    }

    public void HideRoomPage()
    {
        if (roomPage != null)
            roomPage.SetActive(false);
    }

    public void ShowCreateStickARUI(RoomCanvasController canvas)
    {
        currentCanvas = canvas;

        if (createStickARPanel != null)
            createStickARPanel.SetActive(true);

        if (gridSelectionUI != null)
            gridSelectionUI.SetActive(true);

        // Reset selection
        selectedGridX = -1;
        selectedGridY = -1;
        ResetGridButtonColors();
    }

    public void HideCreateStickARUI()
    {
        if (createStickARPanel != null)
            createStickARPanel.SetActive(false);

        currentCanvas = null;
    }

    void OnGridPositionSelected(int x, int y, int buttonIndex)
    {
        selectedGridX = x;
        selectedGridY = y;

        // Highlight selected button
        ResetGridButtonColors();
        if (gridButtons != null && buttonIndex < gridButtons.Length)
        {
            ColorBlock colors = gridButtons[buttonIndex].colors;
            colors.normalColor = Color.green;
            gridButtons[buttonIndex].colors = colors;
        }

        Debug.Log($"Selected grid position: ({x}, {y})");
    }

    void ResetGridButtonColors()
    {
        if (gridButtons == null) return;

        foreach (Button btn in gridButtons)
        {
            ColorBlock colors = btn.colors;
            colors.normalColor = Color.white;
            btn.colors = colors;
        }
    }

    void OnCreateStickAR()
    {
        if (currentCanvas == null)
        {
            Debug.LogWarning("No canvas selected!");
            return;
        }

        if (selectedGridX < 0 || selectedGridY < 0)
        {
            Debug.LogWarning("Please select a grid position!");
            return;
        }

        string content = stickARContentInput != null ? stickARContentInput.text : "";

        if (string.IsNullOrEmpty(content))
        {
            Debug.LogWarning("Please enter some text!");
            return;
        }

        // Create the StickAR
        currentCanvas.CreateStickAR(content, selectedGridX, selectedGridY);

        // Clear input and hide UI
        if (stickARContentInput != null)
            stickARContentInput.text = "";

        HideCreateStickARUI();
    }

    void OnCancelCreate()
    {
        // Clear and hide
        if (stickARContentInput != null)
            stickARContentInput.text = "";

        HideCreateStickARUI();
    }

    void LoadPlayerSkins()
    {
        DataBaseManager dbManager = FindFirstObjectByType<DataBaseManager>();
        if (dbManager == null) return;

        dbManager.GetPlayerSkins((skins) =>
        {
            playerUnlockedSkins = skins;
            if (currentSkinText != null && skins.Count > 0)
            {
                // Get current skin from Firebase
                // For now just show first unlocked skin
                currentSkinText.text = $"Skin: {skins[0]}";
            }
        });
    }

    void OnOpenSkinSelection()
    {
        if (skinSelectionPanel != null)
            skinSelectionPanel.SetActive(true);

        PopulateSkinButtons();
    }

    void PopulateSkinButtons()
    {
        if (skinButtonsParent == null || skinButtonPrefab == null) return;

        // Clear existing buttons
        foreach (Transform child in skinButtonsParent)
        {
            Destroy(child.gameObject);
        }

        DataBaseManager dbManager = FindFirstObjectByType<DataBaseManager>();
        if (dbManager == null) return;

        dbManager.GetPlayerSkins((unlockedSkins) =>
        {
            // Create buttons only for unlocked skins
            foreach (string skin in unlockedSkins)
            {
                GameObject btnObj = Instantiate(skinButtonPrefab, skinButtonsParent);
                Button btn = btnObj.GetComponent<Button>();
                TMP_Text btnText = btnObj.GetComponentInChildren<TMP_Text>();

                if (btnText != null)
                    btnText.text = skin;

                if (btn != null)
                {
                    string skinName = skin; // Capture for lambda
                    btn.onClick.AddListener(() => OnSelectSkin(skinName));
                }
            }
        });
    }

    void OnSelectSkin(string skinName)
    {
        DataBaseManager dbManager = FindFirstObjectByType<DataBaseManager>();
        if (dbManager != null)
        {
            dbManager.UpdateSelectedSkin(skinName);

            if (currentSkinText != null)
                currentSkinText.text = $"Skin: {skinName}";
        }

        if (skinSelectionPanel != null)
            skinSelectionPanel.SetActive(false);
    }
}