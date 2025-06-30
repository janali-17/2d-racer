using UnityEngine;
using Playroom;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private PlayroomKit _playroomKit = new();

    public bool isGameOver = false;

    [SerializeField] private GameObject playerPrefabs;
    [SerializeField] private Vector3 spawnPosPlayerOne = new Vector3(-1f, -3f, 0f);
    [SerializeField] private Vector3 spawnPosPlayerTwo = new Vector3(-1f, 3f, 0f);
    [SerializeField] private Image playerOneDied;
    [SerializeField] private Image playerTwoDied;
    [SerializeField] private GameObject playerObj;
    // [SerializeField] private TextMeshProUGUI restartText;

    private PlayroomKit.Player playerOne;
    private PlayroomKit.Player playerTwo;

    private static bool playerJoined;

    /// <summary>
    /// List of players and their gameObjects.
    /// </summary>
    private static readonly List<PlayroomKit.Player> players = new();
    private static Dictionary<string, GameObject> PlayerDict = new();
    private static readonly List<GameObject> playerGameObjects = new();

    private void Awake()
    {
        Instance = this;
        playerOneDied.gameObject.SetActive(false);
        playerTwoDied.gameObject.SetActive(false);
        Time.timeScale = 0.0f;
    }

    private void Start()
    {
        _playroomKit.InsertCoin(new InitOptions()
        {
            maxPlayersPerRoom = 2,
            defaultPlayerStates = new() {
            {"score", 0},
            {"isDead", false}
        },
        }, () =>
        {
            // Game launch logic here
            _playroomKit.OnPlayerJoin(AddPlayer);
            _playroomKit.RpcRegister("GameOver", HandleGameOver);
            print($"[Unity Log] isHost: {_playroomKit.IsHost()}");
            Time.timeScale = 1.0f;
        });
    }

    private void HandleGameOver(string data, string senderId)
    {
        bool gameOver = bool.Parse(data);

        Debug.LogWarning($"Game over for {senderId} : gameOver: {gameOver}");

        if (_playroomKit.IsHost())
        {
            playerOneDied.gameObject.SetActive(gameOver);
        }
        else
        {
            playerTwoDied.gameObject.SetActive(gameOver);
        }
    }

    //private void HandleGameOver(string data, string senderId)
    //{
    //    if (bool.Parse(data) == true) 
    //    {
    //        // show game over for player one
    //        playerOneDied.gameObject.SetActive(true);
    //    }
    //    else
    //    {
    //        // show for player 2
    //        playerTwoDied.gameObject.SetActive(true);
    //    }
    //}

    private void Update()
    {
        LocalPlayerSet();
        GetOtherPlayers();
    }


    //public void CheckForGameOver()
    //{
    //    if (playerObj.GetComponent<Player>().gameOver)
    //    {
    //        if (_playroomKit.IsHost())
    //        {
    //            _playroomKit.MyPlayer().SetState("isDead", true);
    //        }
    //        else
    //        {
    //            _playroomKit.MyPlayer().SetState("isDead", true);
    //        }
    //    }

    //    // Check other players' death states
    //    foreach (var player in players)
    //    {
    //        if (player.id != _playroomKit.MyPlayer().id)
    //        {
    //            bool isOtherPlayerDead = player.GetState<bool>("isDead");
    //            if (isOtherPlayerDead)
    //            {
    //                if (_playroomKit.IsHost())
    //                {
    //                    playerOneDied.gameObject.SetActive(true);
    //                }
    //                else
    //                {
    //                    playerTwoDied.gameObject.SetActive(true);
    //                }
    //            }
    //        }
    //    }
    //}

    private void LocalPlayerSet()
    {
        if (playerJoined)
        {
            var myPlayer = _playroomKit.MyPlayer();
            var index = players.IndexOf(myPlayer);

            playerGameObjects[index].GetComponent<Player>().PlayerJumpInput();

            players[index].SetState("pos", playerGameObjects[index].transform.position);


        }
    }
    //private void CheckForDeaths()
    //{
    //    var myPlayer = _playroomKit.MyPlayer();
    //    if (myPlayer == null) return;

    //    foreach (var player in players)
    //    {
    //        var deadState = player.GetState<string>("dead");
    //        if (string.IsNullOrEmpty(deadState)) continue;

    //        bool isDead = deadState.ToLower() == "true";
    //        if (!isDead) continue;

    //        bool isMe = player.id == myPlayer.id;

    //        if (isMe)
    //        {
    //            // Show MY death image
    //            if (_playroomKit.IsHost())
    //                playerOneDied.gameObject.SetActive(true);
    //            else
    //                playerTwoDied.gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            // Show OTHER player's death image
    //            if (_playroomKit.IsHost())
    //                playerTwoDied.gameObject.SetActive(true);
    //            else
    //                playerOneDied.gameObject.SetActive(true);
    //        }
    //    }
    //}





    private void GetOtherPlayers()
    {

        for (var i = 0; i < players.Count; i++)
        {
            if (players[i] != null)
            {
                var pos = players[i].GetState<Vector3>("pos");
                var color = players[i].GetState<Color>("color");
                if (playerGameObjects != null)
                {
                    playerGameObjects[i].GetComponent<Transform>().position = pos;
                    playerGameObjects[i].GetComponent<SpriteRenderer>().color = color;
                }

            }
        }
    }


    private void AddPlayer(PlayroomKit.Player player)
    {
        playerObj = Instantiate(playerPrefabs, spawnPosPlayerOne, Quaternion.identity);

        Debug.Log($"{player.GetProfile().name} with {player.id} joined the game. IS HOST: {_playroomKit.IsHost()}");

        if (_playroomKit.IsHost())
        {
            _playroomKit.SetState("PlayerOneId", player.id);
        }
        else
        {
            _playroomKit.SetState("PlayerTwoId", player.id);
        }

        if (_playroomKit.IsHost())
        {
            playerObj.transform.position = spawnPosPlayerOne;
        }
        else
        {
            playerObj.transform.position = spawnPosPlayerTwo;
        }

        player.SetState("color", player.GetProfile().color);

        PlayerDict.Add(player.id, playerObj);
        players.Add(player);
        playerGameObjects.Add(playerObj);


        playerJoined = true;
        player.OnQuit(RemovePlayer);
    }

    private static void RemovePlayer(string playerID)
    {
        if (PlayerDict.TryGetValue(playerID, out GameObject player))
        {
            PlayerDict.Remove(playerID);
            players.Remove(players.Find(p => p.id == playerID));
            playerGameObjects.Remove(player);
            Destroy(player);
        }
        else
        {
            Debug.LogWarning("Player is not in dictionary");
        }
    }

    public PlayroomKit GetPlayerRoomKit()
    {
        return _playroomKit;
    }
    public List<PlayroomKit.Player> GetAllPlayers()
    {
        return players;
    }

    //Draw Gizmos
    [SerializeField] private float gizmoRadius = 0.3f;
    [SerializeField] private Color gizmoColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(spawnPosPlayerOne, gizmoRadius);
        Gizmos.DrawSphere(spawnPosPlayerTwo, gizmoRadius);
    }
}
