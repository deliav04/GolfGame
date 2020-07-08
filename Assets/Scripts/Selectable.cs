using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI; 

public class Selectable : NetworkBehaviour
{
    public bool faceUp = false;

    public GameManager GameManager;

    // public UpdateSprite UpdateSprite1;
    // public UpdateSprite UpdateSprite2;
    // public GameObject card1 =  null;
    // string card1Name;
    // Sprite card1Face;
    

    void Start() {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public void OnPointerClick() {
        if (GameManager.GameState == "InitialCardFlip") {
            if (this.gameObject.tag == "PlayerCard") {
                if (this.gameObject == GameManager.InitialCard1) {
                    GameManager.InitialCard1 = null;
                    this.gameObject.GetComponent<Image>().color = Color.white;
                } else if (this.gameObject == GameManager.InitialCard2) {
                    GameManager.InitialCard2 = null;
                    this.gameObject.GetComponent<Image>().color = Color.white;
                } else if (GameManager.InitialCard1 == null) {
                    GameManager.InitialCard1 = this.gameObject;
                    this.gameObject.GetComponent<Image>().color = Color.yellow;
                } else if (GameManager.InitialCard2 == null) {
                    GameManager.InitialCard2 = this.gameObject;
                    this.gameObject.GetComponent<Image>().color = Color.yellow;
                }
            }
        } else if (GameManager.GameState == "Turns") {
            if (this.gameObject.tag == "PlayerCard") {
                if (GameManager.OldCard == null) {
                    GameManager.OldCard = this.gameObject;
                    this.gameObject.GetComponent<Image>().color = Color.yellow;
                } else if (GameManager.OldCard == this.gameObject) {
                    GameManager.OldCard = null;
                    this.gameObject.GetComponent<Image>().color = Color.white;
                }
            } else if (this.gameObject.tag == "DiscardCard") {
                if (GameManager.NewCard == null) {
                    GameManager.NewCard = this.gameObject;
                    this.gameObject.GetComponent<Image>().color = Color.yellow;
                } else if (GameManager.NewCard == this.gameObject) {
                    GameManager.NewCard = null;
                    this.gameObject.GetComponent<Image>().color = Color.white;
                }
            }
        }    
    }
    
}
