using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;
using TMPro;

public class RoomManager : MonoBehaviour
{
    [System.Serializable]
    public class RoomData
    {
        public string roomName;
        public GameObject roomPrefab;
    }

    [Header("AR Setup")]
    [SerializeField] private ARTrackedImageManager trackedImageManager;
    
    [Header("Room Configuration")]
    [SerializeField] private List<RoomData> rooms = new List<RoomData>();

    [Header("Existing UI Elements")]
    [SerializeField] private GameObject popup;              // Your existing popup under Canvas/ScanPage
    [SerializeField] private Button enterButton;            // Your existing button in the popup
    [SerializeField] private TMP_Text popupText;            // Text in popup to show room name (optional)
    [SerializeField] private GameObject exitButton;         // Exit button (can be new or existing)
    [SerializeField] private RoomUIManager roomUIManager;   // Room UI Manager

    [Header("Cameras")]
    [SerializeField] private Camera arCamera;               // Main AR camera
    [SerializeField] private Camera roomCamera;             // Camera for inside room view

    private Dictionary<string, GameObject> spawnedRooms = new Dictionary<string, GameObject>();
    private Dictionary<string, Vector3> trackedImagePositions = new Dictionary<string, Vector3>();
    
    private string currentTrackedRoom = null;  // Which room marker is currently being tracked
    private string currentInsideRoom = null;   // Which room player is currently inside
    private bool isInsideRoom = false;

    private void Start()
    {
        Debug.Log("RoomManager Start - AR Camera enabled: " + arCamera.enabled);
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.AddListener(OnImageChanged);
        }

        // Setup all rooms
        SetupRooms();

        // Initially show AR camera, hide room camera
        if (arCamera != null) arCamera.enabled = true;
        if (roomCamera != null) roomCamera.enabled = false;
        
        // Hide popup and exit button initially
        if (popup != null) popup.SetActive(false);
        if (exitButton != null) exitButton.SetActive(false);

        // Setup enter button click listener
        if (enterButton != null)
        {
            enterButton.onClick.RemoveAllListeners();
            enterButton.onClick.AddListener(OnEnterButtonClicked);
        }
    }

    void SetupRooms()
    {
        foreach (RoomData room in rooms)
        {
            if (room.roomPrefab != null)
            {
                GameObject roomInstance = Instantiate(room.roomPrefab, Vector3.zero, Quaternion.identity);
                roomInstance.name = room.roomName;
                roomInstance.SetActive(false);
                spawnedRooms.Add(room.roomName, roomInstance);
            }
        }
    }

    void OnImageChanged(ARTrackablesChangedEventArgs<ARTrackedImage> eventArgs)
    {
        // Only respond to tracking if NOT inside a room
        if (isInsideRoom) return;

        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateTrackedImage(trackedImage);
        }

        foreach (ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateTrackedImage(trackedImage);
        }

        foreach (KeyValuePair<TrackableId, ARTrackedImage> removed in eventArgs.removed)
        {
            OnImageLost(removed.Value.referenceImage.name);
        }
    }

    void UpdateTrackedImage(ARTrackedImage trackedImage)
    {
        if (trackedImage == null) return;
        if (isInsideRoom) return;

        string imageName = trackedImage.referenceImage.name;

        if (trackedImage.trackingState == TrackingState.Tracking)
        {
            // Store position
            trackedImagePositions[imageName] = trackedImage.transform.position;

            // Show popup with room name
            if (popup != null)
            {
                popup.SetActive(true);
                
                // Update text to show which room can be entered (optional)
                if (popupText != null)
                {
                    popupText.text = $"Enter {imageName}?";
                }
            }

            currentTrackedRoom = imageName;
        }
        else if (trackedImage.trackingState == TrackingState.Limited || 
                trackedImage.trackingState == TrackingState.None)
        {
            OnImageLost(imageName);
        }
    }

    void OnImageLost(string roomName)
    {
        // Only hide popup if this was the tracked room
        if (currentTrackedRoom == roomName)
        {
            if (popup != null) popup.SetActive(false);
            currentTrackedRoom = null;
        }
    }

    public void OnEnterButtonClicked()
    {
        if (currentTrackedRoom != null)
        {
            EnterRoom(currentTrackedRoom);
        }
    }

    public void EnterRoom(string roomName)
    {
        if (!spawnedRooms.ContainsKey(roomName))
        {
            Debug.LogWarning($"Room {roomName} not found!");
            return;
        }

        // Hide popup
        if (popup != null) popup.SetActive(false);

        // Activate the room
        GameObject room = spawnedRooms[roomName];
        
        // Position room at tracked location if available
        if (trackedImagePositions.ContainsKey(roomName))
        {
            room.transform.position = trackedImagePositions[roomName];
        }
        
        room.SetActive(true);

        // Setup canvases in the room with room name
        RoomCanvasController[] canvases = room.GetComponentsInChildren<RoomCanvasController>();
        foreach (var canvas in canvases)
        {
            canvas.roomName = roomName;
        }

        // DON'T switch cameras - just keep using Main Camera
        // The room will be positioned where the marker was tracked

        // Show room UI and exit button
        if (roomUIManager != null) roomUIManager.ShowRoomPage();
        if (exitButton != null) exitButton.SetActive(true);

        currentInsideRoom = roomName;
        isInsideRoom = true;

        Debug.Log($"Entered room: {roomName}");
    }

    public void ExitRoom()
    {
        if (currentInsideRoom == null) return;

        // Deactivate current room
        if (spawnedRooms.ContainsKey(currentInsideRoom))
        {
            spawnedRooms[currentInsideRoom].SetActive(false);
        }

        // DON'T switch cameras - already using Main Camera

        // Hide room UI and exit button
        if (roomUIManager != null) roomUIManager.HideRoomPage();
        if (exitButton != null) exitButton.SetActive(false);

        isInsideRoom = false;
        currentInsideRoom = null;

        Debug.Log("Exited room");
    }
    public bool IsInsideRoom()
    {
        return isInsideRoom;
    }

    public string GetCurrentRoom()
    {
        return currentInsideRoom;
    }

    // Method for external scripts to call (like RoomEntranceButton)
    public void SpawnSpecificRoom(string roomName)
    {
        EnterRoom(roomName);
    }

    // Overload method to accept room index (for RoomEntranceButton compatibility)
    public void SpawnSpecificRoom(int roomIndex)
    {
        if (roomIndex >= 0 && roomIndex < rooms.Count)
        {
            string roomName = rooms[roomIndex].roomName;
            EnterRoom(roomName);
        }
        else
        {
            Debug.LogError($"Room index {roomIndex} is out of range! Total rooms: {rooms.Count}");
        }
    }

    private void OnDestroy()
    {
        if (trackedImageManager != null)
        {
            trackedImageManager.trackablesChanged.RemoveListener(OnImageChanged);
        }
    }
}