using UnityEngine;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using System.Collections.Generic;

public class RoomCanvasController : MonoBehaviour
{
    [Header("Canvas Setup")]
    public string canvasId;  // "canvas_1", "canvas_2", "canvas_3", "canvas_4"
    public string roomName;  // Set by RoomManager when entering room

    [Header("3x3 Grid Setup")]
    public GameObject stickARPrefab;  // Prefab for displaying a StickAR
    public Transform gridParent;      // Parent for all StickAR objects
    public float gridSpacing = 0.33f; // Distance between grid positions (1.0 / 3)

    [Header("Raycast Detection")]
    public float raycastDistance = 5f;
    public LayerMask canvasLayer;

    private DatabaseReference db;
    private Dictionary<string, GameObject> spawnedStickARs = new Dictionary<string, GameObject>();
    private Camera playerCamera;
    private bool isLookingAtCanvas = false;

    // 3x3 grid positions (0,0 to 2,2)
    private Vector3[,] gridPositions = new Vector3[3, 3];

    void Start()
    {
        db = FirebaseDatabase.DefaultInstance.RootReference;
        playerCamera = Camera.main;

        // Calculate grid positions (centered on canvas)
        CalculateGridPositions();

        // Load existing StickARs from Firebase
        LoadStickARsFromFirebase();

        // Listen for new StickARs in real-time
        ListenForNewStickARs();
    }

    void CalculateGridPositions()
    {
        // Create 3x3 grid centered on canvas
        Vector3 startPos = transform.position - new Vector3(gridSpacing, gridSpacing, 0);

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                gridPositions[x, y] = startPos + new Vector3(x * gridSpacing, y * gridSpacing, 0);
            }
        }
    }

    void Update()
    {
        // Check if player is looking at this canvas
        CheckIfLookingAtCanvas();
    }

    void CheckIfLookingAtCanvas()
    {
        if (playerCamera == null) return;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, raycastDistance, canvasLayer))
        {
            if (hit.collider.gameObject == this.gameObject)
            {
                if (!isLookingAtCanvas)
                {
                    isLookingAtCanvas = true;
                    OnStartLookingAtCanvas();
                }
            }
            else
            {
                if (isLookingAtCanvas)
                {
                    isLookingAtCanvas = false;
                    OnStopLookingAtCanvas();
                }
            }
        }
        else
        {
            if (isLookingAtCanvas)
            {
                isLookingAtCanvas = false;
                OnStopLookingAtCanvas();
            }
        }
    }

    void OnStartLookingAtCanvas()
    {
        // Notify UI that player is looking at this canvas
        RoomUIManager roomUI = FindFirstObjectByType<RoomUIManager>();
        if (roomUI != null)
        {
            roomUI.ShowCreateStickARUI(this);
        }
    }

    void OnStopLookingAtCanvas()
    {
        // Hide create UI when not looking
        RoomUIManager roomUI = FindFirstObjectByType<RoomUIManager>();
        if (roomUI != null)
        {
            roomUI.HideCreateStickARUI();
        }
    }

    void LoadStickARsFromFirebase()
    {
        db.Child("rooms").Child(roomName).Child("canvases").Child(canvasId)
            .Child("stickARs").GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError($"Failed to load StickARs for {roomName}/{canvasId}");
                return;
            }

            DataSnapshot snapshot = task.Result;

            foreach (DataSnapshot childSnapshot in snapshot.Children)
            {
                string json = childSnapshot.GetRawJsonValue();
                StickAR stickAR = JsonUtility.FromJson<StickAR>(json);
                SpawnStickAR(stickAR);
            }

            Debug.Log($"Loaded {snapshot.ChildrenCount} StickARs for {canvasId}");
        });
    }

    void ListenForNewStickARs()
    {
        db.Child("rooms").Child(roomName).Child("canvases").Child(canvasId)
            .Child("stickARs").ChildAdded += (object sender, ChildChangedEventArgs e) =>
        {
            if (e.DatabaseError != null)
            {
                Debug.LogError(e.DatabaseError.Message);
                return;
            }

            // Only spawn if not already spawned
            if (!spawnedStickARs.ContainsKey(e.Snapshot.Key))
            {
                string json = e.Snapshot.GetRawJsonValue();
                StickAR stickAR = JsonUtility.FromJson<StickAR>(json);
                SpawnStickAR(stickAR);
            }
        };
    }

    void SpawnStickAR(StickAR stickAR)
    {
        if (spawnedStickARs.ContainsKey(stickAR.id))
        {
            return; // Already spawned
        }

        // Check if grid position is valid
        if (stickAR.gridX < 0 || stickAR.gridX > 2 || stickAR.gridY < 0 || stickAR.gridY > 2)
        {
            Debug.LogWarning($"Invalid grid position: ({stickAR.gridX}, {stickAR.gridY})");
            return;
        }

        // Get grid position
        Vector3 position = gridPositions[stickAR.gridX, stickAR.gridY];

        // Spawn StickAR prefab
        GameObject stickARObj = Instantiate(stickARPrefab, position, transform.rotation, gridParent);
        
        // Setup StickAR display
        StickARDisplay display = stickARObj.GetComponent<StickARDisplay>();
        if (display != null)
        {
            display.Setup(stickAR, this);
        }

        spawnedStickARs.Add(stickAR.id, stickARObj);
    }

    public void CreateStickAR(string content, int gridX, int gridY)
    {
        if (FirebaseAuth.DefaultInstance.CurrentUser == null)
        {
            Debug.LogError("User not authenticated!");
            return;
        }

        // Create new StickAR
        StickAR newStickAR = new StickAR
        {
            authorUid = FirebaseAuth.DefaultInstance.CurrentUser.UserId,
            content = content,
            gridX = gridX,
            gridY = gridY
        };

        // Get player data to fill in author info
        string uid = FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        db.Child("players").Child(uid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Player player = JsonUtility.FromJson<Player>(task.Result.GetRawJsonValue());
                newStickAR.authorName = player.playerName;
                newStickAR.authorSkin = player.selectedSkin;

                // Save to Firebase
                SaveStickARToFirebase(newStickAR);
            }
        });
    }

    void SaveStickARToFirebase(StickAR stickAR)
    {
        string json = JsonUtility.ToJson(stickAR);

        db.Child("rooms").Child(roomName).Child("canvases").Child(canvasId)
            .Child("stickARs").Child(stickAR.id)
            .SetRawJsonValueAsync(json).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to save StickAR to Firebase");
            }
            else
            {
                Debug.Log($"StickAR created successfully on {canvasId}");
            }
        });
    }

    public void LikeStickAR(string stickARId)
    {
        db.Child("rooms").Child(roomName).Child("canvases").Child(canvasId)
            .Child("stickARs").Child(stickARId).Child("likes")
            .RunTransaction(mutableData =>
        {
            int currentLikes = mutableData.Value != null ? int.Parse(mutableData.Value.ToString()) : 0;
            mutableData.Value = currentLikes + 1;
            return TransactionResult.Success(mutableData);
        });
    }

    public bool IsLookingAtThisCanvas()
    {
        return isLookingAtCanvas;
    }
}