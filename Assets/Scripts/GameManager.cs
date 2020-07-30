using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using Mirror;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class GameManager : NetworkBehaviour
{
    public PlayerManager playerManager;

    public Sprite[] cardFaces;
    public GameObject CardPrefab;
    public GameObject PlayerAreaPrefab;
    public GameObject Background;
    public GameObject Discard;
    public GameObject ScoreTextPrefab;

    public static string[] suits = new string[] {"C", "D", "H", "S"};
    public static string[] values = new string[] {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"};

    // public SyncListGO playerAreas = new SyncListGO();
    public SyncListString deck = new SyncListString();
    public List<NetworkConnection> PlayerConnections = new List<NetworkConnection>();

    [SyncVar]
    public string GameState;
   
    [SyncVar]
    public int NumPlayers;

    [SyncVar]
    public int MaxFlipped = 2;
    
    [SyncVar]
    public int LastPlayer = -1;


    public int[] Scores;

    public NetworkConnection CurrPlayer;
    public int CurrPlayerIndex = 0;
    int FirstPlayerIndex = 0;

    [SyncVar]
    public bool RoundOver = false;
    public bool GameOver = false;

    public GameObject InitialCard1;
    public GameObject InitialCard2;
    public GameObject OldCard;
    public GameObject NewCard;
    bool CardsFlipped = false;
    public bool NewCardFlipped = false;
    [SyncVar]
    public bool PlayerHighlighted = false;
    public GameObject ScoresObject;


    [SyncVar]
    public int numReady = 0;

    public bool SETROUNDS = false;
    public int NUMROUNDS = 9;
    public int MAXSCORE = 30;
    public int RoundNumber = 0;

   
    public override void OnStartServer() {
        base.OnStartServer();

        if (isServer) {
            GenerateDeck();
        }
        
        ScoresObject = GameObject.Find("Scores");
    }

    public override void OnStartClient() {
        base.OnStartClient();
        deck.Callback += DeckChanged;
        // playerAreas.Callback += PlayersChanged;
        ScoresObject = GameObject.Find("Scores");

      
        // GameObject playerArea = Instantiate(PlayerAreaPrefab, new Vector3(0,0,0), Quaternion.identity);
        // NetworkServer.Spawn(playerArea, connectionToClient);
        // playerAreas.Add(playerArea);
        // RpcSpawnArea(playerArea);
    }

    void Start() {

        // Initialize the Google Mobile Ads SDK.
        MobileAds.Initialize(initStatus => { });

        Debug.Log("Starting coroutine 1");
        StartCoroutine(PlayGame());


    }


    // [ClientRpc]
    // void RpcSpawnArea(GameObject playerArea) {
    //     playerAreas.Callback += PlayersChanged;
    //     playerArea.name = (playerAreas.Count).ToString();
    //     playerArea.transform.SetParent(Background.transform, false);
    //     Debug.Log("Area Spawned");
    // }

    // void PlayersChanged(SyncListGO.Operation op, int index, GameObject oldItem, GameObject newItem) {
    //     Debug.Log("Players changed " + op);
    // }

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
   

    public void UpdateDeck() {
        deck.RemoveAt(deck.Count - 1);
        deck.Callback += DeckChanged;
    } 

    public IEnumerator PlayGame() {
        while (!GameOver) {
            if (GameState == "InitialCardFlip") {
                PlayerHighlighted = false;
                if (InitialCard1 != null && InitialCard2 != null) {
                    if (!CardsFlipped) {
                        InitialCard1.GetComponent<Image>().color = Color.white;
                        InitialCard2.GetComponent<Image>().color = Color.white;

                        FlipCards(InitialCard1, InitialCard2);
                        //CardsFlipped = true;
                        InitialCard1 = null;
                        InitialCard2 = null;
                    }
                }  
                if (numReady == NumPlayers) { 
                    ChangeGameState("Turns");
                }
            }
            if (GameState == "Turns") {
                if (isClient && !PlayerHighlighted) { playerManager.CmdHighlightCurrPlayer(); }
                if (OldCard != null && NewCard != null) {
                    
                    NewCard.GetComponent<Image>().color = Color.white;
                    OldCard.GetComponent<Image>().color = Color.white;

                    if (RoundOver == true) {
                        SwapCards(OldCard, NewCard, true, LastPlayer);
                    } else {
                        SwapCards(OldCard, NewCard, false, -1);
                    }

                    OldCard = null;
                    NewCard = null;

                }
                
                if (MaxFlipped == 6 && !RoundOver && isClient) {
                    EndRound();                            
                }
                

            } 
            
            // if (GameState == "Scoring") {
            //     if (isClient) {
            //         if (!RoundScored) {
            //             RoundOver = true;
            //             RoundScored = true;
            //             Debug.Log("Scoring round now");
            //             playerManager.ScoreRound();
            //         }
            //     }
            // }

        yield return null;
        }
        

    }

    
    void FlipCards(GameObject card1, GameObject card2) {
        Debug.Log("Cards selected, flipping now");
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.FlipCards(card1, card2);
    }

    void SwapCards(GameObject OldCard, GameObject NewCard, bool LastRound, int LastPlayer) {
        Debug.Log("Cards selected, swapping now");
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.SwapCards(OldCard, NewCard, LastRound, LastPlayer);
    }

    void EndRound() {
        playerManager.CmdEndRound();
    }
    
    public void ChangeGameState(string state) {
        if (isServer) {
            GameState = state;
        
            if (state == "Turns") {


                CurrPlayerIndex = FirstPlayerIndex;
                CurrPlayer = PlayerConnections[FirstPlayerIndex];
                while (CurrPlayer == null) {
                    FirstPlayerIndex++;
                    if (FirstPlayerIndex == PlayerConnections.Count) {
                        FirstPlayerIndex = 0;
                    }
                    CurrPlayer = PlayerConnections[FirstPlayerIndex];
                }

                FirstPlayerIndex++;
                if (FirstPlayerIndex == PlayerConnections.Count) {
                    FirstPlayerIndex = 0;
                }
            } 
        } else {
            if (state == "Turns") {
                playerManager.CmdHighlightCurrPlayer();
            }
        }
        Debug.Log("GameState now " + GameState);

    }


    public void AddPlayers() {
        if (isServer) {
            // Debug.Log("Adding Players");
            foreach (Mirror.NetworkConnectionToClient conn in NetworkServer.connections.Values) {
                PlayerConnections.Add(conn);
            }
        }
    }

    

}
