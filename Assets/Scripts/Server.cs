using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class Server : NetworkBehaviour
{

    public Sprite[] cardFaces;
    public GameObject CardPrefab;
    public GameObject PlayerAreaPrefab;
    public GameObject Background;
    public GameObject Discard;

    public static string[] suits = new string[] {"C", "D", "H", "S"};
    public static string[] values = new string[] {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"};

    public SyncListGO playerAreas = new SyncListGO();
    public SyncListString deck = new SyncListString();


    [System.Serializable]
    public class SyncListGO : SyncList<GameObject> {}
   
    // Start is called before the first frame update
    public override void OnStartServer() {
        base.OnStartServer();

        if (isServer) {
            GenerateDeck();
        }
        
    }

    public override void OnStartClient() {
        base.OnStartClient();
        deck.Callback += DeckChanged;
        playerAreas.Callback += PlayersChanged;
      
        // GameObject playerArea = Instantiate(PlayerAreaPrefab, new Vector3(0,0,0), Quaternion.identity);
        // NetworkServer.Spawn(playerArea, connectionToClient);
        // playerAreas.Add(playerArea);
        // RpcSpawnArea(playerArea);
    }

    [ClientRpc]
    void RpcSpawnArea(GameObject playerArea) {
        playerAreas.Callback += PlayersChanged;
        playerArea.name = (playerAreas.Count).ToString();
        playerArea.transform.SetParent(Background.transform, false);
        Debug.Log("Area Spawned");
    }

    void PlayersChanged(SyncListGO.Operation op, int index, GameObject oldItem, GameObject newItem) {
        Debug.Log("Players changed " + op);
    }

    public void DeckChanged(SyncListString.Operation op, int index, string oldItem, string newItem) {
        //Debug.Log("Deck changed " + op);
    }

    public void GenerateDeck() {
        SyncListString newDeck = new SyncListString();
        foreach (string s in suits) {
            foreach (string v in values) {
                newDeck.Add(s + v);
            }
        }
        newDeck.Add("J1");
        newDeck.Add("J2");
        System.Random random = new System.Random();
        int n = newDeck.Count;
        while (n > 1)
        {
            int k = random.Next(n);
            n--;
            string temp = newDeck[k];
            newDeck[k] = newDeck[n];
            newDeck[n] = temp;   
        }
        deck = newDeck;
        
    }
   

    public void DrawCard() {
        if (isServer) { CmdDrawCard(); }
    }

    [Command]
    public void CmdDrawCard() {
        GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
        newCard.GetComponent<Selectable>().faceUp = true;
        newCard.name = deck.Last<string>();
        NetworkServer.Spawn(newCard);
        RpcDrawCard(newCard, deck.Last<string>());
        UpdateDeck();        
    }

    [ClientRpc]
    void RpcDrawCard(GameObject card, string n) {
        card.name = n;
        card.GetComponent<Selectable>().faceUp = true;
        card.transform.SetParent(Discard.transform, false);
    }

    public void UpdateDeck() {
        deck.RemoveAt(deck.Count - 1);
        deck.Callback += DeckChanged;
    }
}
