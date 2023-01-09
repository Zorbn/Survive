using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Game.Scripts
{
    public class GameNetworkManager : NetworkManager
    {
        public List<GameObject> serverPlayerGameObjects;

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
        
            serverPlayerGameObjects.Add(conn.identity.gameObject);
        }
    
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            serverPlayerGameObjects.Remove(conn.identity.gameObject);
        
            base.OnServerDisconnect(conn);
        }
    }
}
