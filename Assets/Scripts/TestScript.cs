using UnityEngine;

public class TestScript : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("AWAKE - TestScript is running!");
    }

    void Start()
    {
        Debug.Log("START - TestScript is running!");
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("SPACE PRESSED!");
        }
    }
}