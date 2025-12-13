using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class StickARDisplay : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text contentText;
    public TMP_Text authorText;
    public TMP_Text timestampText;
    public TMP_Text likesText;
    public Image backgroundImage;
    public Button likeButton;

    private StickAR stickARData;
    private RoomCanvasController parentCanvas;

    public void Setup(StickAR data, RoomCanvasController canvas)
    {
        stickARData = data;
        parentCanvas = canvas;

        // Display content
        if (contentText != null)
            contentText.text = data.content;

        // Display author with skin color
        if (authorText != null)
            authorText.text = $"{data.authorName} ({data.authorSkin})";

        // Display timestamp
        if (timestampText != null)
            timestampText.text = GetTimeAgo(data.timestamp);

        // Display likes
        if (likesText != null)
            likesText.text = $"❤ {data.likes}";

        // Set background color based on skin
        if (backgroundImage != null)
            backgroundImage.color = GetSkinColor(data.authorSkin);

        // Setup like button
        if (likeButton != null)
            likeButton.onClick.AddListener(OnLikeClicked);
    }

    void OnLikeClicked()
    {
        if (parentCanvas != null && stickARData != null)
        {
            parentCanvas.LikeStickAR(stickARData.id);
            
            // Update local display
            stickARData.likes++;
            if (likesText != null)
                likesText.text = $"❤ {stickARData.likes}";
        }
    }

    Color GetSkinColor(string skinName)
    {
        switch (skinName.ToLower())
        {
            case "blue": return new Color(0.3f, 0.6f, 1f);
            case "yellow": return new Color(1f, 0.9f, 0.3f);
            case "pink": return new Color(1f, 0.4f, 0.7f);
            case "red": return new Color(1f, 0.3f, 0.3f);
            case "green": return new Color(0.3f, 0.9f, 0.4f);
            case "purple": return new Color(0.7f, 0.3f, 0.9f);
            case "orange": return new Color(1f, 0.6f, 0.2f);
            case "black": return new Color(0.2f, 0.2f, 0.2f);
            case "white": return new Color(0.9f, 0.9f, 0.9f);
            default: return Color.white;
        }
    }

    string GetTimeAgo(long timestamp)
    {
        DateTime past = DateTimeOffset.FromUnixTimeSeconds(timestamp).DateTime;
        TimeSpan diff = DateTime.UtcNow - past;

        if (diff.TotalMinutes < 1)
            return "Just now";
        else if (diff.TotalMinutes < 60)
            return $"{(int)diff.TotalMinutes}m ago";
        else if (diff.TotalHours < 24)
            return $"{(int)diff.TotalHours}h ago";
        else if (diff.TotalDays < 7)
            return $"{(int)diff.TotalDays}d ago";
        else
            return past.ToString("MMM dd");
    }
}