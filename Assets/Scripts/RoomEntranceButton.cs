using UnityEngine;
using UnityEngine.UI; 

public class RoomEntranceButton : MonoBehaviour
{
    [Tooltip("The index of the room in the RoomManager's list that this button should spawn.")]
    public int targetRoomIndex; 

    private RoomManager manager;

    void Start()
    {
        // --- UPDATED METHOD ---
        // FindFirstObjectByType is the modern replacement for FindObjectOfType for finding a single instance.
        manager = FindFirstObjectByType<RoomManager>();
        // --- END UPDATED METHOD ---

        if (manager == null)
        {
            Debug.LogError("RoomManager not found! Cannot link room button. Ensure the manager exists in the scene.");
            return;
        }
        
        // Ensure the script is on a Button component and link the action
        Button button = GetComponent<Button>();
        if (button != null)
        {
            // Add a listener that calls the OnButtonClicked function when the button is pressed
            button.onClick.AddListener(OnButtonClicked);
        }
        else
        {
            Debug.LogWarning("RoomEntranceButton requires a Button component or needs to be adapted for a Collider/Trigger.");
        }
    }

    // Called when the button is pressed
    public void OnButtonClicked()
    {
        if (manager != null)
        {
            // Call the manager's function, passing the target index defined in the Inspector
            manager.SpawnSpecificRoom(targetRoomIndex);
        }
    }
}