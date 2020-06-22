using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using Mirror;

public class Golf : NetworkBehaviour
{
    public int numPlayers;
    // public Sprite[] cardFaces;
    // public GameObject cardPrefab;
    // public GameObject Player1Area;
    // public GameObject Background;
    // public GameObject Discard;
    // public GameObject[,] playerPos = new GameObject[NUMPLAYERS, 6]; 
    // public GameObject[] p1Pos;
    // public GameObject[] p2Pos;
    // public GameObject[] p3Pos;
    // public GameObject[] p4Pos;
    // public GameObject[] playerPos = new GameObject[NUMPLAYERS*6]; 



    // public static string[] suits = new string[] {"C", "D", "H", "S"};
    // public static string[] values = new string[] {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"};
    // public List<string>[] players = new List<string>[NUMPLAYERS];
    // public List<string> p1;
    // public List<string> p2;
    // public List<string> p3;
    // public List<string> p4;
    // GameObject[] playerAreas = new GameObject[NUMPLAYERS];
    
    public PlayerManager playerManager;


    //public List<string> deck;


    // void Start() {
    //     // for (int i = 0; i < NUMPLAYERS; i++) {
    //     //     GameObject playerArea = Instantiate(Player1Area, new Vector3(0,0,0), Quaternion.identity);
    //     //     playerArea.transform.SetParent(Background.transform, false);
    //     //     playerAreas[i] = playerArea;
    //     // }
    //     for (int i = 0; i < NUMPLAYERS; i++) {
    //         players[i] = new List<string>();
    //     }
    //      //PlayCards();
    // }

    // public override void OnStartClient() {
    //     base.OnStartClient();

    //     Discard = GameObject.Find("Discard");
    // }

    // [Server]
    // public override void OnStartServer() {

    // }

    // public void PlayCards() {
    //     deck = GenerateDeck();
    //     Shuffle(deck);

    //     // //To be removed
    //     // foreach (string card in deck)
    //     // {
    //     //     print(card);
    //     // }

    //     GolfSort();
    //     GolfDeal();
    // }


    // public static List<string> GenerateDeck() {
    //     List<string> newDeck = new List<string>();
    //     foreach (string s in suits) {
    //         foreach (string v in values) {
    //             newDeck.Add(s + v);
    //         }
    //     }
    //     newDeck.Add("J1");
    //     newDeck.Add("J2");
    //     return newDeck;
    // }

    // void Shuffle<T>(List<T> list) {
    //     System.Random random = new System.Random();
    //     int n = list.Count;
    //     while (n > 1)
    //     {
    //         int k = random.Next(n);
    //         n--;
    //         T temp = list[k];
    //         list[k] = list[n];
    //         list[n] = temp;         
    //     }
    // }


    // void GolfDeal() {
    //     int i = 0;
    //     foreach (List<string> player in players) {
    //         foreach (string card in player) {
    //             GameObject newCard = Instantiate(cardPrefab, new Vector3(0,0,0), Quaternion.identity);
    //             newCard.transform.SetParent(playerAreas[i].transform, false);
    //             newCard.name = card;
    //             //newCard.GetComponent<Selectable>().faceUp = false;
    //         }
    //         i++;
    //     }
    // }

    // void GolfSort() {
    //     foreach (List<string> player in players) {
    //         for (int j = 0; j < 6; j++) {
    //             player.Add(deck.Last<string>());
    //             deck.RemoveAt(deck.Count - 1);
    //         }
    //     }
    // }

    public void DrawCard() {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.CmdDrawCard();
    }
    
    public void StartGame() {
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        playerManager = networkIdentity.GetComponent<PlayerManager>();
        playerManager.CmdStartGame();
    }
 }
