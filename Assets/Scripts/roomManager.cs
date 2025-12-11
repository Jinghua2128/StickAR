using UnityEngine;

public class roomManager : MonoBehaviour
{   
    public GameObject currentRoom;

    void currentRoomSet(GameObject newRoom)
    {
        currentRoom = newRoom;
        spawnNewRoom(newRoom);
        newRoom.SetActive(true);
    }

    public void spawnNewRoom(GameObject newRoom)
    {
        Instantiate(newRoom, Vector3.zero, Quaternion.identity);
    }
}
