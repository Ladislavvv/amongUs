using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using System.IO;

public class MyPhoton : MonoBehaviour
{
    PhotonView myPV;
    GameObject myPlayerAvatar;

    Player[] allPlayers;
    int myNumberInRoom;

    // Start is called before the first frame update
    void Start()
    {
        myPV = GetComponent<PhotonView>();
        // спавн игроков вокруг стола, присваивание индивидуального порядкового номера
        allPlayers = PhotonNetwork.PlayerList;
        
        foreach(Player p in allPlayers)
        {
            if(p != PhotonNetwork.LocalPlayer)
            {
                myNumberInRoom++;
            }
        }
        
        if (myPV.IsMine)
        {
            myPlayerAvatar = PhotonNetwork.Instantiate(Path.Combine("PhotonPrefabs", "AU_Player"), AU_SpawnPoints.instance.spawnPoints[myNumberInRoom].position, Quaternion.identity);
        }
    }
}
