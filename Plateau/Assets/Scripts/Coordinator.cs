﻿using UnityEngine;
using System.Collections;
using System;

public class Coordinator : MonoBehaviour {

    public static int nbPlayer = 4;
    public static int nbBonus = 3;
    public GameObject Dice;
    public GameObject Map;
    public GameObject[] Players = new GameObject[nbPlayer];
    public int[] playerPos = new int[nbPlayer];
    public GameObject[] bonusPrefabs = new GameObject[nbBonus];
    public Sprite[] playerSprites = new Sprite[4];

    delegate void BonusBehavior(int playerWhoActivate);

    
    private Dice d;
    private Map m;
    private int currentPlayer = 0;
    private bool rolled = false;
    private GameObject[][] bonus = new GameObject[nbPlayer][];
    private BonusBehavior[] bonusesBehavior = new BonusBehavior[nbBonus];
    private bool beginOfTurn = true;
    private bool beforeDiceThrow = false;
    private bool warping = false;
    private bool bonusEnd = true;
    private GameObject RPS;
    private GameObject RPSTemp;
    private bool bm2Init = true;
    private int bm2playerToMove;

    // Use this for initialization
    void Start () {
        m = Map.GetComponent<Map>();
        m.PrepareMap();
        Vector3 pos = m.tiles[0].transform.position + new Vector3(0, 0, 0);
        GameObject player = (GameObject)Resources.Load("Player", typeof(GameObject));
        RPS = (GameObject)Resources.Load("RandomPlayerSelector", typeof(GameObject));

        for (int i = 0; i < nbPlayer; ++i)
        {
            Players[i] = (GameObject)Instantiate(player);
            Players[i].transform.position = pos;
            Players[i].GetComponent<SpriteRenderer>().sprite = playerSprites[i];
            Players[i].SetActive(true);
        }  
        d = Dice.GetComponent<Dice>();
        for (int i = 0; i < nbPlayer; ++i)
        {
            bonus[i] = new GameObject[nbBonus];
            for (int j = 0; j < nbBonus; ++j)
            {
                bonus[i][j] = (GameObject)Instantiate(bonusPrefabs[j]);
                bonus[i][j].SetActive(false);
                bonus[i][j].transform.parent = transform;
            }
        }
        for (int i = 0; i < nbPlayer; ++i)
            playerPos[i] = -1;
        for (int i = 0; i < nbPlayer; ++i)
        {
            playerPos[i] = 0;
            SetSecondaryPlayer(Players[i], i);
        } 
        SetMainPlayer(Players[currentPlayer], currentPlayer);


        //Setting Bonus Behaviors
        bonusesBehavior[0] = BonusMoins1;
        bonusesBehavior[1] = BonusMoins2;
        bonusesBehavior[2] = BonusPlus3;
         
    }
	
	// Update is called once per frame
	void Update () {

        
        if(beginOfTurn)
        {
            AddBonus();
            beginOfTurn = false;
            beforeDiceThrow = true;
        }

        if(beforeDiceThrow)
        {
            for (int i = 0; i < nbBonus; ++i)
                if (bonus[currentPlayer][i].GetComponent<Bonus>().wasUsed)
                {
                    bonusEnd = false;
                    bonusesBehavior[i](currentPlayer);
                    if (bonusEnd)
                    {
                        bonus[currentPlayer][i].GetComponent<Bonus>().wasUsed = false;
                    }
                }    
        }

        Move move = Players[currentPlayer].GetComponent<Move>();
        //Happen after the dice was rolled
        if (d.hasBeenRolled)
        {
            beforeDiceThrow = false;
            Move(move, d.currentValue, currentPlayer);
            d.hasBeenRolled = false;
            rolled = true;
        }

        //Happen when player move is over
        if(!move.moving && rolled)
        {
            TileBehavior();
        }    
    }

    void SetMainPlayer(GameObject player, int place)
    {
        player.GetComponent<Animator>().enabled = true;
        player.transform.position = m.tiles[playerPos[place]].transform.position;
        player.transform.GetChild(0).gameObject.SetActive(true);
        player.transform.localScale = new Vector3(1, 1, 1);
        for (int i = 0; i < nbBonus; ++i)
            bonus[place][i].SetActive(true);
    }

    void SetSecondaryPlayer(GameObject player, int place)
    {
        player.transform.GetChild(0).gameObject.SetActive(false);
        player.GetComponent<Animator>().enabled = false;
        Vector3 pos = player.transform.position;
        int nbPlayerOnSameTile = -1;
        foreach(int i in playerPos)
            if (i == playerPos[place])
                nbPlayerOnSameTile++;
        player.transform.position = new Vector3(pos.x - 0.3f + 0.3f * nbPlayerOnSameTile, pos.y, pos.z - 0.3f);
        player.transform.localScale = new Vector3(0.5f, 0.5f, 1);
        for (int i = 0; i < nbBonus; ++i)
            bonus[place][i].SetActive(false);
    }

    void Move(Move move, int nbTile, int player)
    {
        int temp = playerPos[player];
        if(nbTile >= 0)
            playerPos[player] = Math.Min((playerPos[player] + nbTile),m.nbTiles - 1);
        if (nbTile < 0)
            playerPos[player] = Math.Max((playerPos[player] + nbTile), 0);
        move.startPosition.Add(Players[player].transform.position);
        for (int i = temp; i != (playerPos[player] + nbTile / Math.Abs(nbTile)); i = (i + nbTile / Math.Abs(nbTile)))
        {
            move.startPosition.Add(m.tiles[i].transform.position);
            move.endPosition.Add(m.tiles[i].transform.position);
        }
        if(player != currentPlayer)
        {
            Vector3 pos = move.endPosition[move.endPosition.Count - 1];
            int nbPlayerOnSameTile = -1;
            foreach (int i in playerPos)
                if (i == playerPos[player])
                    nbPlayerOnSameTile++;
            move.endPosition[move.endPosition.Count- 1] = new Vector3(pos.x - 0.3f + 0.3f * nbPlayerOnSameTile, pos.y, pos.z - 0.3f);
        }
        move.moving = true;
    }

    void AddBonus()
    {
        bool full = true;
        for (int i = 0; i < nbBonus; ++i)
            full = bonus[currentPlayer][i].GetComponent<Bonus>().active && full;
        if (full)
            return;
        bool done = false;
        while(!done)
        {
            int draw = UnityEngine.Random.Range(0, nbBonus);
            if (!bonus[currentPlayer][draw].GetComponent<Bonus>().active)
            {
                bonus[currentPlayer][draw].GetComponent<Bonus>().Switch();
                done = true;
            }  
        }
    }

    void TileBehavior()
    {
        Tile.TileType tileType = m.tiles[playerPos[currentPlayer]].GetComponent<Tile>().type;
        if (tileType == Tile.TileType.Dice)
        {
            rolled = false;
            beginOfTurn = true;
        }
        else if (tileType == Tile.TileType.Event)
        {
            SetSecondaryPlayer(Players[currentPlayer], currentPlayer);
            currentPlayer = (currentPlayer + 1) % nbPlayer;
            SetMainPlayer(Players[currentPlayer], currentPlayer);
            rolled = false;
            beginOfTurn = true;
        }
        else if (tileType == Tile.TileType.Warp && !warping)
        {
            int warp = -1;
            for (int i = 0; i < m.nbTiles; ++i)
            {
                Tile.TileType type = m.tiles[i].GetComponent<Tile>().type;
                if (type == Tile.TileType.Warp && i != playerPos[currentPlayer])
                    warp = i;
            }
            // TO CHANGE 
            Move(Players[currentPlayer].GetComponent<Move>(), warp - playerPos[currentPlayer], currentPlayer);
            //
            warping = true;
        }
        else
        {
            SetSecondaryPlayer(Players[currentPlayer], currentPlayer);
            currentPlayer = (currentPlayer + 1) % nbPlayer;
            SetMainPlayer(Players[currentPlayer], currentPlayer);
            rolled = false;
            beginOfTurn = true;
            warping = false;
        }
    }

    // BONUS BEHAVIORS


    void BonusPlus3(int player)
    {
        Move move = Players[player].GetComponent<Move>();
        Move(move, 3, player);
        bonusEnd = true;
    }

    void BonusMoins2(int player)
    {
        if (bm2Init)
        {
            int k = 0;
            RPSTemp = Instantiate(RPS);
            RPS rps = RPSTemp.GetComponent<RPS>();
            rps.Players = new GameObject[nbPlayer - 1];
            int count = 0;
            for (int i = 0; i < nbPlayer; ++i)
                if (i != currentPlayer)
                    rps.Players[count++] = Players[i];
            rps.nbPlayer = nbPlayer - 1;
            RPSTemp.gameObject.SetActive(true);
            bm2playerToMove = -1;
            bm2Init = false;
        }

        if (RPSTemp == null)
        {
            Move move = Players[bm2playerToMove].GetComponent<Move>();
            Move(move, -2, bm2playerToMove);
            bonusEnd = true;
            bm2Init = true;
            return;
        }

        if (RPSTemp.GetComponent<RPS>().end && bm2playerToMove == -1)
        {
            bm2playerToMove = RPSTemp.GetComponent<RPS>().currentArrowPos;
            if (bm2playerToMove >= currentPlayer)
                bm2playerToMove++;
            Debug.Log(bm2playerToMove);
        }
    }

    void BonusMoins1(int player)
    {
        for(int i = 0; i < nbPlayer; ++i)
            if(i != player)
            { 
                Move move = Players[i].GetComponent<Move>();
                Move(move, -1, i);
            }
        bonusEnd = true;
    }

    
}
