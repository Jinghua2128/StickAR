using System;
using System.Collections.Generic;
using UnityEngine;

// StickAR Data Structure for Firebase
[System.Serializable]
public class StickAR
{
    public string id;
    public string authorUid;
    public string authorName;
    public string authorSkin;
    public string content;
    public long timestamp;
    public int gridX;  // 0-2 for 3x3 grid
    public int gridY;  // 0-2 for 3x3 grid
    public int likes;

    public StickAR()
    {
        id = Guid.NewGuid().ToString();
        timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        likes = 0;
    }
}

// Canvas Data - each room has 4 canvases
[System.Serializable]
public class CanvasData
{
    public string canvasId;  // "canvas_1", "canvas_2", "canvas_3", "canvas_4"
    public Dictionary<string, StickAR> stickARs = new Dictionary<string, StickAR>();
}

// Room data structure
[System.Serializable]
public class RoomFirebaseData
{
    public string roomName;
    public Dictionary<string, CanvasData> canvases = new Dictionary<string, CanvasData>();
}