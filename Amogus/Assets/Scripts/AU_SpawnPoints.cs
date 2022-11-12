using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AU_SpawnPoints : MonoBehaviour
{
    public static AU_SpawnPoints instance;

    public Transform[] spawnPoints;

    void Start()
    {
        instance = this;
    }
}
