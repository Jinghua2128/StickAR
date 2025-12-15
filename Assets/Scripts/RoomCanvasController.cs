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
    public float raycastDistance = 100f;
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

        if (playerCamera == null)
        {
            Debug.LogError("RoomCanvasController: Camera.main is NULL!");
        }
        else
        {
            Debug.Log($"RoomCanvasController on {gameObject.name}: Camera found = {playerCamera.name}");
        }

        // Calculate grid positions (centered on canvas)
        CalculateGridPositions();

        // Load existing StickARs from Firebase
        LoadStickARsFromFirebase();

        // Listen for new StickARs in real-time
        ListenForNewStickARs();
    }

    void CalculateGridPositions()
    {
        float startX = -gridSpacing;
        float startY = -gridSpacing;

        for (int x = 0; x < 3; x++)
        {
            for (int y = 0; y < 3; y++)
            {
                gridPositions[x, y] = new Vector3(
                    startX + (x * gridSpacing),
                    startY + (y * gridSpacing),
                    0.01f
                );
            }
        }

        Debug.Log($"Canvas {canvasId}: Local grid calculated");
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

    Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red, 0.1f);

    if (Physics.Raycast(ray, out hit, raycastDistance, canvasLayer))
    {
        Debug.Log($"Raycast hit: {hit.collider.gameObject.name}");
        
        if (hit.collider.gameObject == this.gameObject)
        {
            if (!isLookingAtCanvas)
            {
                isLookingAtCanvas = true;
                Debug.Log($"Started looking at {gameObject.name}");
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
        if (gridParent == null)
        {
            Debug.LogError($"❌ GridParent is NULL on {gameObject.name}! StickARs will spawn incorrectly.");
            Debug.LogError("Fix: Assign GridParent in Inspector for each canvas!");
            return;
        }
        
        Debug.Log($"GridParent assigned: {gridParent.name}, Canvas: {gameObject.name}");
        
        if (spawnedStickARs.ContainsKey(stickAR.id))
        {
            return;
        }

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

        GameObject stickARObj = Instantiate(stickARPrefab, gridParent);
        stickARObj.transform.localPosition = gridPositions[stickAR.gridX, stickAR.gridY];
        stickARObj.transform.localRotation = Quaternion.identity;
        stickARObj.transform.localScale = Vector3.one;

        
        // Make sure it faces the camera (for world space canvas)
        stickARObj.transform.localRotation = Quaternion.identity;
        
        // Setup StickAR display
        StickARDisplay display = stickARObj.GetComponent<StickARDisplay>();
        if (display != null)
        {
            display.Setup(stickAR, this);
        }

        spawnedStickARs.Add(stickAR.id, stickARObj);
        
        Debug.Log($"Spawned StickAR '{stickAR.content}' at grid ({stickAR.gridX},{stickAR.gridY}) under {gridParent.name}");
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