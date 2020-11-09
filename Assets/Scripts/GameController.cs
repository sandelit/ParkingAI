using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameController : MonoBehaviour
{

    public static bool isQuitting = false;

    void OnApplicationQuit()
    {
        isQuitting = true;
    }
}
