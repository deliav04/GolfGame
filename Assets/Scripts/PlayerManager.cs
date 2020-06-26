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
    public GameManager GameManager;
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
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        startButton = GameObject.Find("StartButton");
        mainCanvas = GameObject.Find("Main Canvas");
    }

    [ClientRpc]
    void RpcUpdateDeck(int n) {
        for (int i = 0; i < n; i++) {
            GameManager.UpdateDeck();
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
        startButton = GameObject.Find("StartButton");
        mainCanvas = GameObject.Find("Main Canvas");
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();


        
        // players.Callback += PlayersChanged;
        //CmdAddPlayer();

        deck = GameManager.deck;
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
    //     if (isgameManager) { CmdGolfDeal(); }
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
        if (isServer) {
            if (connectionToClient == GameManager.CurrPlayer) {
                GameManager.deck.Callback += DeckChanged;
                deck = GameManager.deck;
                GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
                newCard.GetComponent<Selectable>().faceUp = true;
                newCard.name = deck.Last<string>();
                NetworkServer.Spawn(newCard);
                newCard.transform.SetParent(Discard.transform, false);
                RpcDrawCard(newCard, deck.Last<string>());
                GameManager.UpdateDeck();
            }
        }
        //RpcUpdateDeck(1);        
    }

    [ClientRpc]
    void RpcDrawCard(GameObject card, string n) {
        card.name = n;
        card.GetComponent<Selectable>().faceUp = true;
        card.transform.SetParent(Discard.transform, false);
        card.gameObject.tag = "DiscardCard";
    }


    [Command]
    public void CmdStartGame() {
        // Create deck button
        GameObject deckButton = Instantiate(deckButtonPrefab, new Vector3(-37,0,0), Quaternion.identity);
        deckButton.name = "DeckButton";
        NetworkServer.Spawn(deckButton);
        deckButton.transform.SetParent(mainCanvas.transform, false);
        //startButton.SetActive(false);
        RpcStartGame(deckButton);

        // Create player areas
        numPlayers = NetworkServer.connections.Count;

        GameManager.Scores = new int[numPlayers];
        GameManager.AddPlayers();

        cardStrings = new string[numPlayers];
        cardObjects =  new GameObject[numPlayers];

        for (int i = 0; i < numPlayers; i++) {
            GameObject playerArea = Instantiate(PlayerAreaPrefab, new Vector3(0,0,0), Quaternion.identity);
            NetworkServer.Spawn(playerArea);
            RpcSpawnArea(playerArea);        
        }


        // Deal cards        
        for (int j = 0; j < 6; j++) {
            for (int i = 0; i < numPlayers; i++) {
                GameManager.deck.Callback += DeckChanged;
                deck = GameManager.deck;
                GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
                // newCard.GetComponent<Selectable>().faceUp = true;
                newCard.name = deck.Last<string>();
                cardStrings[i] = deck.Last<string>();
                cardObjects[i] = newCard;
                NetworkServer.Spawn(newCard);
                GameManager.UpdateDeck();

            }
            int index = 0;
            foreach (Mirror.NetworkConnectionToClient conn in NetworkServer.connections.Values) {
                TargetDealCards(conn, cardStrings, cardObjects, index);
                index++;
            }
        }

        Debug.Log("Set up complete changing GameState to InitialCardFlip");
        GameManager.ChangeGameState("InitialCardFlip");
    }

    [ClientRpc]
    void RpcStartGame(GameObject deckButton) {
        deckButton.transform.SetParent(mainCanvas.transform, false);
        startButton.SetActive(false);
    }

    [ClientRpc]
    void RpcSpawnArea(GameObject playerArea) {
        playerArea.name = (playerAreas.Count + 1).ToString();
        playerAreas.Add(playerArea);
        playerArea.transform.SetParent(Background.transform, false);
        playerArea.transform.SetAsFirstSibling();
        
    }
        
  
    [TargetRpc]
    void TargetDealCards(NetworkConnection conn, string[] names, GameObject[] cards, int playerNum) {
        int num = names.Length;
        int index = num - playerNum;
        for (int i = 0; i < num; i++ ) {
            if (index == playerAreas.Count) { index = 0;}
            
            if (index == 0) {
                cards[i].gameObject.tag = "PlayerCard";
            }
            cards[i].name = names[i];
            (cards[i]).transform.SetParent((playerAreas[index]).transform, false);
            index++;
        }
    }

    [Command]
    public void CmdFlipCards(GameObject card1, GameObject card2) {    
        RpcFlipCards(card1, card2);
        Debug.Log(GameManager.numReady + " players ready to play");
        if (isServer) { GameManager.numReady++; }
        Debug.Log(GameManager.numReady + " players ready to play");
    }

    [ClientRpc]
    void RpcFlipCards(GameObject card1, GameObject card2) {
        card1.GetComponent<Selectable>().faceUp = true;
        card2.GetComponent<Selectable>().faceUp = true;
    }



    
    // private IEnumerator SelectCards() {
    //     GameObject card1 = null;
    //     GameObject card2 =  null;
    //     Debug.Log("Waiting for click");
    //     while (card1 == null) {
    //         if (Input.GetMouseButtonDown(0)) {
    //             Debug.Log("Clicked");
    //             Debug.Log(Input.mousePosition);
    //             // Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 10));
    //             // RaycastHit2D hit = Physics2D.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
                

    //             RaycastHit2D hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
    //             // Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
    //             // Vector3 mousePos2D = new Vector2(mousePos.x, mousePos.y);

    //             // RaycastHit2D hit = Physics2D.Raycast(mousePos2D, -Vector2.up);
    //             // RaycastHit  hit;
    //             // Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                  

    //             // Vector2 rayPos = new Vector2(Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
    //             // RaycastHit2D hit=Physics2D.Raycast(rayPos, -Vector2.zero, 0f);


    //             if (hit.collider) {

    //                 Debug.Log("Hit");
    //                 if (hit.collider.CompareTag("PlayerCard")) {
    //                     card1 = hit.collider.gameObject;
    //                     Debug.Log("Player card clicked");
    //                 } else if (hit.collider.CompareTag("DiscardCard")) {
    //                     Debug.Log("Discard card clicked");
    //                 }

    //             }           
    //         }
    //     yield return null;
    //     }
        
    // }
}


