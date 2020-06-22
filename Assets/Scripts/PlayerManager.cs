using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;

public class PlayerManager : NetworkBehaviour

{
    public int numPlayers;
    public Sprite[] cardFaces;
    public GameObject CardPrefab;
    public GameObject PlayerAreaPrefab;
    public GameObject Background;
    public GameObject Discard;
    public GameObject Server;
    public Server server;
    public GameObject deckButtonPrefab;
    public GameObject startButton;
    public GameObject mainCanvas;

    //public GameObject Button;

    public static string[] suits = new string[] {"C", "D", "H", "S"};
    public static string[] values = new string[] {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"};
    //public Dictionary<string, GameObject> hand = new Dictionary<string, GameObject>();

    public string[] cardStrings;
    public GameObject[] cardObjects;


    public List<GameObject> playerAreas = new List<GameObject>();

    // public GameObject[,] playerPos = new GameObject[NUMPLAYERS, 6]; 
    // public GameObject[] p1Pos;
    // public GameObject[] p2Pos;
    // public GameObject[] p3Pos;
    // public GameObject[] p4Pos;
    // public GameObject[] playerPos = new GameObject[NUMPLAYERS*6]; 
    // public List<string> p1;
    // public List<string> p2;
    // public List<string> p3;
    // public List<string> p4;

    // sync deck
    // Sync newDeck of players
    // Upon connecting each player adds their netId to the newDeck and stores their index
    // each player creates player areas somehow and has the player number attached to it
    // Cards are spawned as a child of the correct area based on the index of the current client
        // RPC method takes as an argument the index of the owner
   
    public SyncListString deck;
    public SyncListString players = new SyncListString();
    public int playerNum;

    // void Start() {
    //     GameObject playerArea = Instantiate(PlayerAreaPrefab, new Vector3(0,0,0), Quaternion.identity);
    //     NetworkServer.Spawn(playerArea, connectionToClient);
    //     RpcSpawnArea(playerArea);
    // }
     

    [Server]
    public override void OnStartServer() {
        
        Background = GameObject.Find("Background");
        Discard = GameObject.Find("Discard");
        Server = GameObject.Find("Server");
        server = Server.GetComponent<Server>();
        startButton = GameObject.Find("StartButton");
        mainCanvas = GameObject.Find("Main Canvas");
    }

    [ClientRpc]
    void RpcUpdateDeck(int n) {
        for (int i = 0; i < n; i++) {
            server.UpdateDeck();
        }
    }
    
    [Command]
    void CmdAddPlayer() {
        int n = players.Count;
        players.Add(n.ToString());
        players.Callback += PlayersChanged;
        // RpcAddPlayer();
    }

    [ClientRpc]
    void RpcAddPlayer() {
        int n = players.Count;
        players.Add(n.ToString());
    }


    void PlayersChanged(SyncListString.Operation op, int index, string oldItem, string newItem) {
        Debug.Log("list changed " + op);
    }

    
    public void DeckChanged(SyncListString.Operation op, int index, string oldItem, string newItem) {
        //Debug.Log("Deck changed " + op);
    }

    public override void OnStartClient() {
        base.OnStartClient();
        
       
    
        Background = GameObject.Find("Background");
        Discard = GameObject.Find("Discard");
        Server = GameObject.Find("Server");
        server = Server.GetComponent<Server>();
        startButton = GameObject.Find("StartButton");
        mainCanvas = GameObject.Find("Main Canvas");

        
        // players.Callback += PlayersChanged;
        //CmdAddPlayer();

        deck = server.deck;
        numPlayers = NetworkServer.connections.Count;
        Debug.Log("Type: " + NetworkServer.connections.GetType());
        //if (isLocalPlayer) { 
            playerNum = numPlayers - 1; //}
        // if (isLocalPlayer) { GolfSort(); }
        //CmdSpawnArea();
        // GameObject playerArea = Instantiate(PlayerAreaPrefab, new Vector3(0,0,0), Quaternion.identity);
        // NetworkServer.Spawn(playerArea, connectionToClient);
        // RpcSpawnArea(playerArea);

    }

    



    // public void GolfDeal() {
    //     if (isServer) { CmdGolfDeal(); }
    // }

    // [Command]
    // void CmdGolfDeal() {
    //     int i = 0;
    //     foreach (string card in hand) {
    //         GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
    //         // newCard.transform.SetParent(playerAreas[i].transform, false);
    //         // newCard.name = card;
    //         // //newCard.GetComponent<Selectable>().faceUp = false
    //         // NetworkServer.Spawn(card, connectionToClient);
    //     }
    //     i++;
    // }

    // void GolfSort() {
    //     for (int j = 0; j < 6; j++) {
    //         hand.Add(deck.Last<string>());
    //         RpcUpdateDeck(6);
    //     }
    // }

    [Command]
    public void CmdDrawCard() {
        server.deck.Callback += DeckChanged;
        deck = server.deck;
        GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
        newCard.GetComponent<Selectable>().faceUp = true;
        newCard.name = deck.Last<string>();
        NetworkServer.Spawn(newCard);
        newCard.transform.SetParent(Discard.transform, false);
        RpcDrawCard(newCard, deck.Last<string>());
        server.UpdateDeck();
        //RpcUpdateDeck(1);        
    }

    [ClientRpc]
    void RpcDrawCard(GameObject card, string n) {
        card.name = n;
        card.GetComponent<Selectable>().faceUp = true;
        card.transform.SetParent(Discard.transform, false);
    }


    [Command]
    public void CmdStartGame() {
        // Create deck button
        GameObject deckButton = Instantiate(deckButtonPrefab, new Vector3(-37,0,0), Quaternion.identity);
        deckButton.name = "DeckButton";
        NetworkServer.Spawn(deckButton);
        deckButton.transform.SetParent(mainCanvas.transform, false);
        startButton.SetActive(false);
        RpcStartGame(deckButton);

        // Create player areas
        numPlayers = NetworkServer.connections.Count;

        cardStrings = new string[numPlayers];
        cardObjects =  new GameObject[numPlayers];

        for (int i = 0; i < numPlayers; i++) {
            GameObject playerArea = Instantiate(PlayerAreaPrefab, new Vector3(0,0,0), Quaternion.identity);
            NetworkServer.Spawn(playerArea);
            RpcSpawnArea(playerArea);        
        }

        //wait();

        // Deal cards CURRENTLY DEALING WRONG - SHOULD BE ONE CARD PER PLAYER
        
        for (int j = 0; j < 6; j++) {
            for (int i = 0; i < numPlayers; i++) {
                server.deck.Callback += DeckChanged;
                deck = server.deck;
                GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
                newCard.GetComponent<Selectable>().faceUp = true;
                newCard.name = deck.Last<string>();
                cardStrings[i] = deck.Last<string>();
                cardObjects[i] = newCard;
                NetworkServer.Spawn(newCard);
                server.UpdateDeck();

            }
            int index = 0;
            foreach (Mirror.NetworkConnectionToClient conn in NetworkServer.connections.Values) {
                TargetDealCards(conn, cardStrings, cardObjects, index);
                index++;
            }
    
            //RpcDealCards(cardStrings, cardObjects, i);
        }

    }

    [ClientRpc]
    void RpcStartGame(GameObject deckButton) {
        deckButton.transform.SetParent(mainCanvas.transform, false);
        startButton.SetActive(false);
    }



    // [Command]
    // public void CmdSpawnArea() {
    //     GameObject playerArea = Instantiate(PlayerAreaPrefab, new Vector3(0,0,0), Quaternion.identity);
    //     NetworkServer.Spawn(playerArea);
    //     RpcSpawnArea(playerArea);
    // }

    [ClientRpc]
    void RpcSpawnArea(GameObject playerArea) {
        playerArea.name = (playerAreas.Count + 1).ToString();
        playerAreas.Add(playerArea);
        playerArea.transform.SetParent(Background.transform, false);
        playerArea.transform.SetAsFirstSibling();
        
    }
        
    [ClientRpc]
    void RpcDealCards(string[] names, GameObject[] cards, int player) {
        // int area = player - playerNum;
        // if (area >= numPlayers) { area -= numPlayers; } else if (area < 0 ) {--area;}
        // Debug.Log(area);
        for (int i = 0; i < 6; i++) {
            (cards[i]).name = names[i];
            (cards[i]).GetComponent<Selectable>().faceUp = true;
            (cards[i]).transform.SetParent((playerAreas[player]).transform, false);
       }
    }

    [TargetRpc]
    void TargetDealCards(NetworkConnection conn, string[] names, GameObject[] cards, int playerNum) {
        int num = names.Length;
        int index = num - playerNum;
        for (int i = 0; i < num; i++ ) {
            if (index == playerAreas.Count) { index = 0;}
            Debug.Log(index);
            cards[i].name = names[i];
            cards[i].GetComponent<Selectable>().faceUp = true;
            (cards[i]).transform.SetParent((playerAreas[index]).transform, false);
            index++;
        }
    }
}


