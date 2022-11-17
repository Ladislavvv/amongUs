using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class AU_GameController : MonoBehaviour
{
    PhotonView myPV;
    int whichPlayerIsImposter;
    
    void Start()
    {
        myPV = GetComponent<PhotonView>();
        if (PhotonNetwork.IsMasterClient)
        {
            PickImposter();
        }
    }
    void PickImposter()
    {
        whichPlayerIsImposter = Random.Range(1, PhotonNetwork.CurrentRoom.PlayerCount + 1);
        //Debug.Log("CurrentRoom.PlayerCount: ", PhotonNetwork.CurrentRoom.PlayerCount);
        myPV.RPC("RPC_SyncImposter", RpcTarget.AllBuffered, whichPlayerIsImposter);
        Debug.Log("Imposter: " + whichPlayerIsImposter);
    }

    [PunRPC]
    public void RPC_SyncImposter(int playerNumber)
    {
        whichPlayerIsImposter = playerNumber;
        AU_PlayerController.localPlayer.BecomeImposter(whichPlayerIsImposter);
    }
}
