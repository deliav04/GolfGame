using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

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
                if (GameManager.InitialCard1 == null) {
                    GameManager.InitialCard1 = this.gameObject;
                } else if (GameManager.InitialCard2 == null) {
                    if (this.gameObject != GameManager.InitialCard1) {
                        GameManager.InitialCard2 = this.gameObject;
                    }
                }
            }
        }    
    }
    
}
