using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Mirror;
using UnityEngine.Animations;
using UnityEngine.UI;
using System;

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
    public string[] PlayerNames;
    public GameObject[] PlayerAreas;


    public List<GameObject> PlayerScores = new List<GameObject>();

    public SyncListString deck;
    public SyncListString players = new SyncListString();
    public int playerNum;

    public int NumCardsFlipped = 0;

    bool RoundScored = false;
    List<int> PlayersRemoved = new List<int>();


    [Server]
    public override void OnStartServer() {
        Background = GameObject.Find("Background");
        Discard = GameObject.Find("Discard");
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        startButton = GameObject.Find("StartButton");
        mainCanvas = GameObject.Find("Main Canvas");

        Debug.Log("Starting coroutine 2");
        StartCoroutine(StartNewRound());
    }


    
    [Command]
    void CmdAddPlayer() {
        int n = players.Count;
        players.Add(n.ToString());
        players.Callback += PlayersChanged;
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
        // Debug.Log("Deck changed " + op);
    }

    public override void OnStartClient() {
        base.OnStartClient();
      
    
        Background = GameObject.Find("Background");
        Discard = GameObject.Find("Discard");
        startButton = GameObject.Find("StartButton");
        mainCanvas = GameObject.Find("Main Canvas");
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();


        deck = GameManager.deck;
    }

    
    [Command]
    public void CmdDrawCard() {
        if (isServer) {
            if (connectionToClient == GameManager.CurrPlayer && !GameManager.NewCardFlipped) {
                GameManager.deck.Callback += DeckChanged;
                deck = GameManager.deck;

                TargetDeselectCard(connectionToClient);

                GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
                newCard.GetComponent<Selectable>().faceUp = true;
                newCard.name = deck.Last<string>();
                NetworkServer.Spawn(newCard);
                newCard.transform.SetParent(Discard.transform, false);
                RpcDrawCard(newCard, deck.Last<string>());
                GameManager.UpdateDeck();
                GameManager.NewCardFlipped = true;
                
            }
        }
    }

    [TargetRpc]
    void TargetDeselectCard(NetworkConnection conn) {
        if (GameManager.NewCard != null) {
            GameManager.NewCard.GetComponent<Image>().color = Color.white;
            GameManager.NewCard = null;
        }
    }

    [ClientRpc]
    void RpcDrawCard(GameObject card, string n) {
        card.name = n;
        card.GetComponent<Selectable>().faceUp = true;
        card.transform.SetParent(Discard.transform, false);
        card.gameObject.tag = "DiscardCard";
    }

// ____________________________________ Game Setup ____________________________________ 
    [Command]
    public void CmdStartGame() {
        // ____________________________________ Create deck button ____________________________________ 
        GameObject deckButton = Instantiate(deckButtonPrefab, new Vector3(-37,-49,0), Quaternion.identity);
        deckButton.name = "DeckButton";
        NetworkServer.Spawn(deckButton);
        deckButton.transform.SetParent(mainCanvas.transform, false);
        startButton.SetActive(false);
        RpcStartGame(deckButton);

        // ____________________________________ Create player areas ____________________________________ 
        numPlayers = NetworkServer.connections.Count;

        GameManager.NumPlayers = numPlayers;
        GameManager.Scores = new int[numPlayers];
        GameManager.AddPlayers();

        cardStrings = new string[numPlayers];
        cardObjects =  new GameObject[numPlayers];
        PlayerNames = new string[numPlayers];
        PlayerAreas = new GameObject[numPlayers];

        // TODO: Let users input names
        for (int i = 0; i < numPlayers; i++) {
            PlayerNames[i] = "Player " + (i + 1).ToString();
        }

        for (int i = 0; i < numPlayers; i++) {
            GameObject playerArea = Instantiate(PlayerAreaPrefab, new Vector3(0,0,0), Quaternion.identity);
            NetworkServer.Spawn(playerArea);
            playerArea.transform.GetChild(0).tag = "PlayerArea";
            if (isServer) {
                playerArea.name = i.ToString();
                PlayerAreas[i] = (playerArea.transform.GetChild(0).gameObject);
                playerArea.transform.SetParent(Background.transform, false);
                playerArea.transform.SetAsFirstSibling();
            }

            int playerIndex = 0;
            foreach (Mirror.NetworkConnectionToClient conn in GameManager.PlayerConnections) {
                if (conn != null) {
                    TargetSpawnArea(conn, playerArea, PlayerNames, i, playerIndex, GameManager.NumPlayers);        
                    playerIndex++; 
                }
            }
        }

        



        // ____________________________________ Deal cards ____________________________________         
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

             if (isServer) {
                int num = cardStrings.Length;
                for (int i = 0; i < num; i++ ) {
                    cardObjects[i].name = cardStrings[i];
                    (cardObjects[i]).transform.SetParent((PlayerAreas[i]).transform, false);
                    }
             }

            Debug.Log("Num players: " + GameManager.NumPlayers);
            int index = 0;
            foreach (Mirror.NetworkConnectionToClient conn in GameManager.PlayerConnections) {
                if (conn != null) {
                    TargetDealCards(conn, cardStrings, cardObjects, index);
                    index++; 
                }
            }
        }

        // ____________________________________ Initial Discard Card ____________________________________         
        GameManager.deck.Callback += DeckChanged;
        deck = GameManager.deck;

        GameObject FirstDiscard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
        FirstDiscard.GetComponent<Selectable>().faceUp = true;
        FirstDiscard.name = deck.Last<string>();
        NetworkServer.Spawn(FirstDiscard);
        FirstDiscard.transform.SetParent(Discard.transform, false);
        RpcDrawCard(FirstDiscard, deck.Last<string>());
        GameManager.UpdateDeck();


        // ____________________________________ Score Board Setup ____________________________________         
        for (int i = 1; i <= GameManager.NumPlayers; i++) {
            GameObject scoreText = Instantiate(GameManager.ScoreTextPrefab, new Vector3(0,0,0), Quaternion.identity);
            NetworkServer.Spawn(scoreText);
            PlayerScores.Add(scoreText);
            scoreText.GetComponent<Text>().text = "Player " + i.ToString() + "\n 0";
            scoreText.transform.SetParent(GameManager.ScoresObject.transform, false);
            RpcScoreRound(scoreText, i);
        }

        GameManager.ChangeGameState("InitialCardFlip");
    }

    [TargetRpc]
    void TargetFixAreas(NetworkConnection conn) {
        Background.transform.GetChild(0).gameObject.SetActive(false);
        Background.transform.GetChild(0).gameObject.SetActive(true);
    }

    [ClientRpc]
    void RpcStartGame(GameObject deckButton) {
        deckButton.transform.SetParent(mainCanvas.transform, false);
        startButton.SetActive(false);
    }

    [TargetRpc]
    void TargetSpawnArea(NetworkConnection conn, GameObject playerArea, string[] playerNames, int toName, int thisPlayer, int players) {
        playerArea.transform.GetChild(0).tag = "PlayerArea";
        playerArea.transform.SetParent(Background.transform, false);
        playerArea.transform.SetAsLastSibling();

        int index = toName + thisPlayer;
        Debug.Log("Players: " + players + " index: " + index);
        if (index >= players) { 
            index -= players;
            Debug.Log("index after subtraction " + index);
        }
        Debug.Log("This player: " + thisPlayer + " naming: " + toName + " index: " + index);
        playerArea.transform.GetChild(1).gameObject.GetComponent<Text>().text = playerNames[index];     

    }
        
  
    [TargetRpc]
    void TargetDealCards(NetworkConnection conn, string[] names, GameObject[] cards, int playerNum) {
        GameObject[] areas = GameObject.FindGameObjectsWithTag("PlayerArea");
        Debug.Log("Player areas on client: " + areas.Length);
        int num = names.Length;
        int index = num - playerNum;
        for (int i = 0; i < num; i++ ) {
            if (index == areas.Length) { index = 0;}
            
            if (index == 0) {
                cards[i].gameObject.tag = "PlayerCard";
            }
            Debug.Log(cards[i]);
            cards[i].name = names[i];
            (cards[i]).transform.SetParent((areas[index]).transform, false);
            index++;
        }
    }


// ____________________________________ Initial Card Flips ____________________________________ 
    public void FlipCards(GameObject card1, GameObject card2) {
        NumCardsFlipped = 2;
        CmdFlipCards(card1, card2);
    }

    [Command]
    void CmdFlipCards(GameObject card1, GameObject card2) {    
        RpcFlipCards(card1, card2);
        GameManager.numReady++; 
    }

    [ClientRpc]
    void RpcFlipCards(GameObject card1, GameObject card2) {
        card1.GetComponent<Selectable>().faceUp = true;
        card2.GetComponent<Selectable>().faceUp = true;
    }

// ____________________________________ Swap Cards ____________________________________
    public void SwapCards(GameObject OldCard, GameObject NewCard, bool LastRound, int LastPlayer) {
        if (!OldCard.GetComponent<Selectable>().faceUp) {
            NumCardsFlipped++;
            CmdSwapCards(OldCard, NewCard, false, NumCardsFlipped, LastRound, LastPlayer);
        } else {
            CmdSwapCards(OldCard, NewCard, true, NumCardsFlipped, LastRound, LastPlayer);
        }

        Debug.Log("Num flipped: " + NumCardsFlipped);
    }

    [Command]
    void CmdSwapCards(GameObject OldCard, GameObject NewCard, bool FaceUp, int NumFlipped, bool LastRound, int LastPlayer) {
        if (connectionToClient == GameManager.CurrPlayer) {
            Debug.Log("Here 3");
            string NewName = NewCard.name;
            Sprite NewFace = NewCard.GetComponent<UpdateSprite>().cardFace;
            NewCard.name = OldCard.name;
            NewCard.GetComponent<UpdateSprite>().cardFace = OldCard.GetComponent<UpdateSprite>().cardFace;

            OldCard.name = NewName;
            OldCard.GetComponent<UpdateSprite>().cardFace = NewFace;

            NewCard.GetComponent<Selectable>().faceUp = true;
            OldCard.GetComponent<Selectable>().faceUp = true;

            RpcSwapCards(OldCard, NewCard);
            Debug.Log("Here 1");
            GameManager.deck.Callback += DeckChanged;
            deck = GameManager.deck;
            if ((LastRound && GameManager.CurrPlayerIndex == LastPlayer) || deck.Count == 0) {
                RpcFlipAllCards();
                Debug.Log("All cards flipped?: " + Time.time);
                GameManager.ChangeGameState("Scoring");
                ScoreRound();
            } else {
                Debug.Log("Here 2");
                GameManager.CurrPlayerIndex++;
                if (GameManager.CurrPlayerIndex == GameManager.PlayerConnections.Count) { 
                    GameManager.CurrPlayerIndex = 0; 
                }
                GameManager.CurrPlayer = GameManager.PlayerConnections[GameManager.CurrPlayerIndex];

                while (GameManager.CurrPlayer == null) {
                    GameManager.CurrPlayerIndex++;
                    if (GameManager.CurrPlayerIndex == GameManager.PlayerConnections.Count) {
                        GameManager.CurrPlayerIndex = 0;
                    }
                }

                HighlightPlayer();

                GameManager.NewCardFlipped = false;
                
                if (NumFlipped > GameManager.MaxFlipped) {
                    GameManager.MaxFlipped = NumFlipped;
                }
                Debug.Log("Here 3");
            }

        } else {
            if (!FaceUp) {
                TargetDecrementFipped(connectionToClient);
            }
        }
    }

    [TargetRpc]
    void TargetDecrementFipped(NetworkConnection conn) {
        NumCardsFlipped--;
    }

    [ClientRpc]
    void RpcSwapCards(GameObject OldCard, GameObject NewCard) {
        string NewName = NewCard.name;
        Sprite NewFace = NewCard.GetComponent<UpdateSprite>().cardFace;
        NewCard.name = OldCard.name;
        NewCard.GetComponent<UpdateSprite>().cardFace = OldCard.GetComponent<UpdateSprite>().cardFace;

        OldCard.name = NewName;
        OldCard.GetComponent<UpdateSprite>().cardFace = NewFace;

        NewCard.GetComponent<Selectable>().faceUp = true;
        OldCard.GetComponent<Selectable>().faceUp = true;

    }
    [Server] 
    void HighlightPlayer() {
        GameManager.PlayerHighlighted = true;
        int playerIndex = 0;
        foreach (Mirror.NetworkConnectionToClient conn in GameManager.PlayerConnections) {
            if (conn != null) {
                TargetHighlightPlayer(conn, GameManager.CurrPlayerIndex, playerIndex );
                playerIndex++;
            }
        }
    }

    [Command]
    public void CmdHighlightCurrPlayer() {
        HighlightPlayer();
    }

    [TargetRpc]
    void TargetHighlightPlayer(NetworkConnection conn, int toHighlight, int thisPlayer) {
        GameObject[] areas = GameObject.FindGameObjectsWithTag("PlayerArea");
        int index = toHighlight - thisPlayer;
        if (index < 0) { index += GameManager.NumPlayers;}
        areas[index].transform.parent.GetChild(1).gameObject.GetComponent<Text>().color = Color.white;
        index--;
        if (index < 0) { index += GameManager.NumPlayers;}
        areas[index].transform.parent.GetChild(1).gameObject.GetComponent<Text>().color = Color.black;

    }

    [Command]
    public void CmdUnhighlightAll() {
        RpcUnhighlightAll();
    }

    [ClientRpc]
    void RpcUnhighlightAll() {
        GameObject[] areas = GameObject.FindGameObjectsWithTag("PlayerArea");
        foreach (GameObject area in areas) {
            area.transform.parent.GetChild(1).gameObject.GetComponent<Text>().color = Color.black;
        }
    }

// ____________________________________ End Round ____________________________________

    [Command] 
    public void CmdEndRound() {
        if (!GameManager.RoundOver) {
            Debug.Log("Here 5. Current Player: " + GameManager.CurrPlayerIndex);

            GameManager.RoundOver = true;
            GameManager.LastPlayer = GameManager.CurrPlayerIndex - 2; //index already incremented at this point
            Debug.Log("Last player set: " + GameManager.LastPlayer);
            if (GameManager.LastPlayer == -2) { GameManager.LastPlayer = GameManager.NumPlayers - 2; }
            if (GameManager.LastPlayer == -1) { GameManager.LastPlayer = GameManager.NumPlayers - 1; }
        }
    }


    [ClientRpc]
    void RpcFlipAllCards() {
        foreach (Transform area in Background.transform) {
            Transform playerArea = area.GetChild(0);
            foreach (Transform card in playerArea) {
                card.gameObject.GetComponent<Selectable>().faceUp = true;
            }
        }        
    }
    [Server] 
    public void ScoreRound() {
        Debug.Log("Scoring now");        

        GameObject[] areas = GameObject.FindGameObjectsWithTag("PlayerArea");
        string[] names = new string[6];
        int [] values = new int[6];
        for (int i = 0; i < GameManager.Scores.Length; i++) {
            for (int k = 0; k < 6; k++) {
                names[k] = areas[i].transform.GetChild(k).gameObject.name;
            }

            int j = 0;
            foreach (string s in names) {
                if (s[0] == 'J') {
                    values[j] = 0;
                } else if (s[1] == 'A') {
                    values[j] = 1;
                } else if (s[1] == 'J') {
                    values[j] = 11;
                } else if (s[1] == 'Q') {
                    values[j] = 12;
                } else if (s[1] == 'K') {
                    values[j] = 13;
                } else {
                    if (s.Length == 3) {
                        values[j] = 10;
                    } else {
                        values[j] = (int)Char.GetNumericValue(s[1]);
                    }
                }
                j++;
            }

            int score = 0;
            if (values[0] == values[3]) {
                if (values[0] == values[1] && values[1] == values[4]) {
                    if (values[0] > 10) {
                        score -= 40;
                    } else {
                        score -= values[0] * 4;
                    }
                    score -= 20;

                } else {
                    if (values[0] > 10) {
                        score -= 20;
                    } else {
                        score -= values[0] * 2;
                    }
                }
                
            } if (values[1] == values[4] && values[1] != values[0]) {
                if (values[1] == values[2] && values[2] == values[5]) {
                    if (values[1] > 10) {
                        score -= 40;
                    } else {
                        score -= values[1] * 4;
                    }
                    score -= 20;

                } else {
                    if (values[1] > 10) {
                        score -= 20;
                    } else {
                        score -= values[1] * 2;
                    }
                }
            } if (values[2] == values[5] && values[2] != values[1]) {
                if (values[2] > 10) {
                    score -= 20;
                } else {
                    score -= values[2] * 2;
                }
            }


            foreach (int value in values) {
                if (value > 10) { 
                    score += 10; 
                } else {
                    score += value;
                }
            }

            if (!GameManager.SETROUNDS && GameManager.Scores[i] < GameManager.MAXSCORE) { //What if score = maxscore??
                GameManager.Scores[i] += score;
            }
        }        


        int c = 0;
        GameObject[] ScoreTexts = GameObject.FindGameObjectsWithTag("ScoreText");

        Debug.Log("Score texts found: " + ScoreTexts.Length);       
        foreach (GameObject text in ScoreTexts) {
                int score = GameManager.Scores[c];
                string[] old = text.GetComponent<Text>().text.Split('\n');
                string name = old[0];
                text.GetComponent<Text>().text = name + "\n" + score.ToString();
                RpcUpdateScore(text, score);
            c++;
        }
        
        PlayersRemoved.Clear();
        for (int n = 0; n < GameManager.Scores.Length; n++) {
            if (!GameManager.SETROUNDS && GameManager.Scores[n] > GameManager.MAXSCORE) {
                if (ScoreTexts[n].GetComponent<Text>().color != Color.red) {
                    Debug.Log("Removing player. Score: " + GameManager.Scores[n]);
                    ScoreTexts[n].GetComponent<Text>().color = Color.red;
                    GameManager.PlayerConnections[n] = null;
                    PlayersRemoved.Add(n);
                }
            }
        }

        GameManager.RoundNumber++;
        RoundScored = true;
    }


    [Server]
    public IEnumerator StartNewRound() {
        while(true) {
            if (RoundScored) {
                RoundScored = false;
                yield return new WaitForSeconds(5.0f);
        
                Debug.Log("Players Remaining: " + (GameManager.NumPlayers - PlayersRemoved.Count));

                if (GameManager.SETROUNDS) {
                    if (GameManager.NUMROUNDS == GameManager.RoundNumber) {
                        // TODO: End Game
                    } else {
                        RestartRound();
                    }
                } else {
                    if ((GameManager.NumPlayers - PlayersRemoved.Count) > 1) {
                        RestartRound();
                    } else {
                        // TODO: End Game
                    }
                }
            }
            yield return null;
        }
    }

    [ClientRpc] 
    void RpcScoreRound(GameObject text, int i) {
        GameManager.ScoresObject.transform.SetAsLastSibling();
        text.transform.SetParent(GameManager.ScoresObject.transform, false);
        text.GetComponent<Text>().text = "Player " + i.ToString() + "\n 0";
    }

    [ClientRpc] 
    void RpcUpdateScore(GameObject text, int score) {
        if (score > GameManager.MAXSCORE) {
            text.GetComponent<Text>().color = Color.red;
        }
        string[] old = text.GetComponent<Text>().text.Split('\n');
        string name = old[0];
        text.GetComponent<Text>().text = name + "\n" + score.ToString();
    }

// ____________________________________ Start New Round ____________________________________

    [Server]
    void RestartRound() {
        RpcUnhighlightAll();
        RpcReset();

        GameObject[] areas = GameObject.FindGameObjectsWithTag("PlayerArea");

        foreach (GameObject area in areas) {
            foreach (Transform card in area.transform) {
                Destroy(card.gameObject);
            }
        }

        foreach (Transform card in Discard.transform) {
            Destroy(card.gameObject);
        }

        Debug.Log("Old Cards Destroyed");

        Debug.Log("Areas to remove: " + PlayersRemoved.Count);
        foreach (int toRemove in PlayersRemoved) {
            areas[toRemove].transform.parent.gameObject.SetActive(false);
            int thisPlayer = 0;
            foreach (NetworkConnection conn in GameManager.PlayerConnections) {
                if (conn != null) {
                    TargetRemoveArea(conn, toRemove, thisPlayer);
                    thisPlayer++;
                }
            }
            GameManager.NumPlayers--;
        }

        GameManager.GenerateDeck();

        cardStrings = new string[GameManager.NumPlayers];
        cardObjects =  new GameObject[GameManager.NumPlayers];

        GameObject[] updatedAreas = GameObject.FindGameObjectsWithTag("PlayerArea");

        Debug.Log("Player areas found: " + updatedAreas.Length);

        Debug.Log("Number of players: " + GameManager.NumPlayers);

         // ____________________________________ Deal cards ____________________________________         
        for (int j = 0; j < 6; j++) {
            for (int i = 0; i < GameManager.NumPlayers; i++) {
                GameManager.deck.Callback += DeckChanged;
                deck = GameManager.deck;
                GameObject newCard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
                // newCard.GetComponent<Selectable>().faceUp = true;
                newCard.name = deck.Last<string>();
                cardStrings[i] = deck.Last<string>();
                cardObjects[i] = newCard;
                NetworkServer.Spawn(newCard);
                newCard.transform.SetParent(updatedAreas[i].transform, false);
                GameManager.UpdateDeck();
            }



            int index = 0;
            foreach (Mirror.NetworkConnectionToClient conn in GameManager.PlayerConnections) {
                if (conn != null) {
                    TargetDealCards(conn, cardStrings, cardObjects, index);
                    index++; 
                }
            }
        }

        // ____________________________________ Initial Discard Card ____________________________________         
        GameManager.deck.Callback += DeckChanged;
        deck = GameManager.deck;

        GameObject FirstDiscard = Instantiate(CardPrefab, new Vector3(0,0,0), Quaternion.identity);
        FirstDiscard.GetComponent<Selectable>().faceUp = true;
        FirstDiscard.name = deck.Last<string>();
        NetworkServer.Spawn(FirstDiscard);
        FirstDiscard.transform.SetParent(Discard.transform, false);
        RpcDrawCard(FirstDiscard, deck.Last<string>());
        GameManager.UpdateDeck();


        // ____________________________________ Reset Values ____________________________________
        GameManager.numReady = 0;
        GameManager.RoundOver = false;
        GameManager.MaxFlipped = 2;
        GameManager.NewCardFlipped = false;

        GameManager.ChangeGameState("InitialCardFlip");
    }

    [TargetRpc]
    void TargetRemoveArea(NetworkConnection conn, int toRemove, int thisPlayer) {
        int index = toRemove - thisPlayer;
        if (index < 0) { index += GameManager.NumPlayers; }
        Background.transform.GetChild(index).gameObject.SetActive(false);
    }

    [ClientRpc]
    void RpcReset() {
        GameManager.numReady = 0;
        GameManager.RoundOver = false;
        GameManager.NewCardFlipped = false;
        NumCardsFlipped = 0;
    }

}


