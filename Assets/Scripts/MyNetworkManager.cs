using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;


public class MyNetworkManager : NetworkManager
{
    public override void OnServerDisconnect(NetworkConnection conn) {
        base.OnServerDisconnect(conn);

        GameManager GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        if (GameManager.PlayerConnections.Exists(x => x == conn)) {
            StopServer();
        }
    }

    
}
