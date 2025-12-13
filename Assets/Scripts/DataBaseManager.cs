using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Auth;
using Firebase.Extensions;
using TMPro;
using System.Collections.Generic;

[System.Serializable]
public class Player
{
    public string id;
    public string uid;
    public string email;
    public string playerName;
    public int level;
    public int experience;
    public int AR_tokens;
    public string selectedSkin;
    public UnlockedItems unLocked = new UnlockedItems();
}

[System.Serializable]
public class UnlockedItems
{
    // Starter skins - new players get these by default
    public List<string> skins = new List<string> { "Blue", "Yellow", "Pink" };
    // Starter rooms - new players get these by default
    public List<string> rooms = new List<string> { "Library", "Grass Patch" };
}

// Define ALL available skins in your game (put this in a separate manager if you prefer)
public static class GameSkins
{
    public static readonly List<string> AllSkins = new List<string>
    {
        // Starter skins (unlocked by default)
        "Blue",
        "Yellow", 
        "Pink",
        
        // Premium/Unlockable skins (locked by default)
        "Red",
        "Green",
        "Purple",
        "Orange",
        "Black",
        "White",
        "Rainbow"
        // Add more skins here as you create them
    };
}

public class DataBaseManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField userName;
    public TMP_InputField email;
    public TMP_InputField password;
    public TMP_Text userStats;
    public TMP_Text errorText;
    public TMP_Text playerNameText;

    [Header("Panel References")]
    public GameObject signUpPanel;
    public GameObject userPanel;

    private DatabaseReference db;
    private FirebaseAuth auth;

    void Start()
    {
        // Initialize Firebase
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                db = FirebaseDatabase.DefaultInstance.RootReference;
                Debug.Log("Firebase initialized successfully.");
            }
            else
            {
                Debug.LogError($"Could not resolve Firebase dependencies: {task.Result}");
            }
        });
    }

    public void SignUp()
    {
        errorText.text = "";

        // Validate inputs
        if (string.IsNullOrEmpty(userName.text))
        {
            errorText.text = "Please enter a username!";
            return;
        }

        if (string.IsNullOrEmpty(email.text))
        {
            errorText.text = "Please enter an email address!";
            return;
        }

        if (string.IsNullOrEmpty(password.text))
        {
            errorText.text = "Please enter a password!";
            return;
        }

        // Create user with email and password
        var signUpTask = auth.CreateUserWithEmailAndPasswordAsync(email.text, password.text);

        signUpTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                HandleAuthError(task.Exception);
                return;
            }

            if (task.IsCanceled)
            {
                errorText.text = "User creation cancelled!";
                return;
            }

            if (task.IsCompletedSuccessfully)
            {
                var user = task.Result.User;
                Debug.Log($"User created successfully: {user.UserId}");

                // Initialize player data for new user
                InitializeNewPlayer(user.UserId, user.Email, userName.text);
            }
        });
    }

    public void SignIn()
    {
        errorText.text = "";

        if (string.IsNullOrEmpty(email.text) || string.IsNullOrEmpty(password.text))
        {
            errorText.text = "Please enter email and password!";
            return;
        }

        var signInTask = auth.SignInWithEmailAndPasswordAsync(email.text, password.text);

        signInTask.ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted)
            {
                HandleAuthError(task.Exception);
                return;
            }

            if (task.IsCanceled)
            {
                errorText.text = "Sign in cancelled!";
                return;
            }

            if (task.IsCompletedSuccessfully)
            {
                var user = task.Result.User;
                Debug.Log($"User signed in successfully: {user.UserId}");

                // Load player data
                LoadPlayerData(user.UserId);
            }
        });
    }

    private void InitializeNewPlayer(string uid, string userEmail, string playerName)
    {
        // Create new player data with email and selected skin
        Player newPlayer = new Player
        {
            id = GeneratePlayerId(),
            uid = uid,
            email = userEmail,
            playerName = playerName,
            level = 1,
            experience = 0,
            AR_tokens = 20,
            selectedSkin = "Blue", // Default skin
            unLocked = new UnlockedItems()
        };

        // Convert to JSON
        string playerJson = JsonUtility.ToJson(newPlayer);

        // Save to Firebase
        db.Child("players").Child(uid).SetRawJsonValueAsync(playerJson).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to initialize player data in Firebase.");
                errorText.text = "Failed to create player profile!";
            }
            else
            {
                Debug.Log("Player data initialized successfully.");
                errorText.text = "Account created! Please sign in.";
            }
        });
    }

    private void LoadPlayerData(string uid)
    {
        db.Child("players").Child(uid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to retrieve player data from Firebase.");
                errorText.text = "Failed to load player data!";
                return;
            }

            DataSnapshot snapshot = task.Result;

            if (!snapshot.Exists)
            {
                Debug.LogWarning("Player data doesn't exist. Creating new profile...");
                InitializeNewPlayer(uid, auth.CurrentUser.Email, "Player");
                return;
            }

            // Parse player data
            Player player = JsonUtility.FromJson<Player>(snapshot.GetRawJsonValue());

            // Update UI
            UpdateUI(player);

            // Switch to user panel
            signUpPanel.SetActive(false);
            userPanel.SetActive(true);
        });
    }

    private void UpdateUI(Player player)
    {
        userStats.text = $"Name: {player.playerName}\n" +
                        $"Level: {player.level}\n" +
                        $"ID: {player.id}\n" +
                        $"Email: {player.email}\n" +
                        $"Experience: {player.experience}\n" +
                        $"Selected Skin: {player.selectedSkin}";

        playerNameText.text = $"{player.playerName}";

        Debug.Log("UI updated with player data.");
    }

    private void HandleAuthError(System.AggregateException exception)
    {
        var baseException = exception.GetBaseException();

        if (baseException is FirebaseException firebaseException)
        {
            var errorCode = (AuthError)firebaseException.ErrorCode;

            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    errorText.text = "Please enter an email address!";
                    break;
                case AuthError.MissingPassword:
                    errorText.text = "Please enter a password!";
                    break;
                case AuthError.WeakPassword:
                    errorText.text = "Password must be at least 6 characters!";
                    break;
                case AuthError.EmailAlreadyInUse:
                    errorText.text = "This email is already registered!";
                    break;
                case AuthError.InvalidEmail:
                    errorText.text = "Invalid email address!";
                    break;
                case AuthError.WrongPassword:
                    errorText.text = "Incorrect password!";
                    break;
                case AuthError.UserNotFound:
                    errorText.text = "No account found with this email!";
                    break;
                default:
                    errorText.text = $"Authentication error: {errorCode}";
                    break;
            }
        }
        else
        {
            errorText.text = $"Error: {baseException.Message}";
        }

        Debug.LogError($"Auth Error: {baseException}");
    }

    private string GeneratePlayerId()
    {
        // Generate an 8-digit player ID
        return Random.Range(10000000, 99999999).ToString();
    }

    public void SignOut()
    {
        if (auth.CurrentUser != null)
        {
            auth.SignOut();
            userPanel.SetActive(false);
            signUpPanel.SetActive(true);
            Debug.Log("User signed out.");
        }
    }

    // Update player's selected skin
    public void UpdateSelectedSkin(string skinName)
    {
        if (auth.CurrentUser == null) return;

        string uid = auth.CurrentUser.UserId;

        db.Child("players").Child(uid).Child("selectedSkin")
            .SetValueAsync(skinName).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to update selected skin");
            }
            else
            {
                Debug.Log($"Selected skin updated to: {skinName}");
            }
        });
    }

    // Get current player's unlocked skins
    public void GetPlayerSkins(System.Action<List<string>> callback)
    {
        if (auth.CurrentUser == null) return;

        string uid = auth.CurrentUser.UserId;

        db.Child("players").Child(uid).GetValueAsync().ContinueWithOnMainThread(task =>
        {
            if (task.IsCompletedSuccessfully)
            {
                Player player = JsonUtility.FromJson<Player>(task.Result.GetRawJsonValue());
                callback?.Invoke(player.unLocked.skins);
            }
        });
    }
}