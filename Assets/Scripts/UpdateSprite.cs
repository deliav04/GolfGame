﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using System;


public class UpdateSprite : NetworkBehaviour
{
    public Sprite cardFace;
    public Sprite cardBack;
    public Image image;
    public Selectable selectable;
    // private Card1 card1;
    public PlayerManager playerManager;
    public GameManager GameManager;
    public static string[] suits = new string[] {"C", "D", "H", "S"};
    public static string[] values = new string[] {"A", "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K"};
    int i = 0;


    // Start is called before the first frame update
    void Start()
    {
        SyncListString deck = CreateDeck();
        // NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        // playerManager = networkIdentity.GetComponent<PlayerManager>();
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();

        string s = this.name;
        if (s[0] == 'D') {
            i +=13;
        } else if (s[0] == 'H') {
            i += 26;
        } else if (s[0] == 'S') {
            i += 39;
        } else if (s[0] == 'J') {
            i += 52;
        }

        if (s[1] == 'A') {
            i +=0;
        } else if (s[1] == 'J') {
            i += 10;
        } else if (s[1] == 'Q') {
            i += 11;
        } else if (s[1] == 'K') {
            i += 12;
        } else {
            if (s.Length == 3) {
                i += 9;
            } else {
                i += ((int)Char.GetNumericValue(s[1]) - 1);
            }
        }
        // foreach(string card in deck) {
        //     if (this.name == card) {
        //         cardFace = playerManager.cardFaces[i];
        //         break;
        //     }
        //     i++;
        // }

        // Debug.Log(i);
            cardFace = GameManager.cardFaces[i];
    
        image = GetComponent<Image>();
        selectable = GetComponent<Selectable>();
    }
    public static SyncListString CreateDeck() {
        SyncListString newDeck = new SyncListString();
        foreach (string s in suits) {
            foreach (string v in values) {
                newDeck.Add(s + v);
            }
        }
        newDeck.Add("J1");
        newDeck.Add("J2");
        return newDeck;
    }

    // public void ShowFace() {
    //     if (cardFace == null && isClient) {
    //         Debug.Log("Card face not set. Index: " + i);

    //         i = 0;
    //         string s = this.name;
    //         if (s[0] == 'D') {
    //             i +=13;
    //         } else if (s[0] == 'H') {
    //             i += 26;
    //         } else if (s[0] == 'S') {
    //             i += 39;
    //         } else if (s[0] == 'J') {
    //             i += 52;
    //         }

    //         if (s[1] == 'A') {
    //             i +=0;
    //         } else if (s[1] == 'J') {
    //             i += 10;
    //         } else if (s[1] == 'Q') {
    //             i += 11;
    //         } else if (s[1] == 'K') {
    //             i += 12;
    //         } else {
    //             if (s.Length == 3) {
    //                 i += 9;
    //             } else {
    //                 i += ((int)Char.GetNumericValue(s[1]) - 1);
    //                 Debug.Log("Name: " + s + " int parsed: " + (int)Char.GetNumericValue(s[1]));
    //             }
    //         }
            
    //         cardFace = playerManager.cardFaces[i];
    //     }
    //     image.sprite = cardFace;
    // }
    
    
    // Update is called once per frame
    void Update()
    {
        if (selectable == null) {
            selectable = this.gameObject.GetComponent<Selectable>();
        }
        if (cardFace == null) {
            Debug.Log("Card face not set. Index: " + i);

            i = 0;
            string s = this.name;
            if (s[0] == 'D') {
                i +=13;
            } else if (s[0] == 'H') {
                i += 26;
            } else if (s[0] == 'S') {
                i += 39;
            } else if (s[0] == 'J') {
                i += 52;
            }

            if (s[1] == 'A') {
                i +=0;
            } else if (s[1] == 'J') {
                i += 10;
            } else if (s[1] == 'Q') {
                i += 11;
            } else if (s[1] == 'K') {
                i += 12;
            } else {
                if (s.Length == 3) {
                    i += 9;
                } else {
                    i += ((int)Char.GetNumericValue(s[1]) - 1);
                }
            }
            cardFace = GameManager.cardFaces[i];
        }

        if (cardBack == null) {
            Debug.Log("Card back not set");
        }

        if (image == null) {
            Debug.Log("Image component not set");
            image = this.gameObject.GetComponent<Image>();
        }
        if (selectable.faceUp) {
            image.sprite = cardFace;
        } else {
           image.sprite = cardBack;
        }
    }
}
