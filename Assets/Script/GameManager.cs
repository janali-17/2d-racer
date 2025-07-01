using UnityEngine;
using Playroom;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using System;
using System.Linq;
using System.Xml.Schema;
using static Playroom.PlayroomKit;
using System.Collections;

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
    private static readonly List<string> playersIds = new();
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
            _playroomKit.OnPlayerJoin(AddPlayer);


            Debug.LogWarning("IS HOST: " + _playroomKit.IsHost());

            // register an rpc event called SpawnPlayers, call this rpc for all players when there are 2 playere in the list.
            _playroomKit.RpcRegister("SpawnPlayers", SpawnPlayers);
            _playroomKit.RpcRegister("UpdatePlayerList", UpdateLocalPlayers);
            _playroomKit.RpcRegister("GameOver", HandleGameOver);
            print($"[Unity Log] isHost: {_playroomKit.IsHost()}");
            Time.timeScale = 1.0f;
        });
    }



    private void UpdateLocalPlayers(string data, string hostId)
    {
        Debug.LogWarning($"player ids from host: {data}");
        List<string> newOrderIds = data.Split(',').Select(id => id.Trim()).Where(id => !string.IsNullOrEmpty(id)).ToList();

        playersIds.Clear();
        playersIds.AddRange(newOrderIds);

        Debug.Log($"Updated playersIds list: {string.Join(", ", playersIds)}");

        Debug.LogWarning("Player ids count: " + playersIds.Count);

        if (playersIds.Count == 2)
        {
            _playroomKit.RpcCall("SpawnPlayers", "");
        }
    }



    private void SpawnPlayers(string data, string hostId)
    {
        var reorderedPlayers = playersIds.Select(id => players.FirstOrDefault(p => p.id == id)).Where(p => p != null).ToList();
        players.Clear();
        players.AddRange(reorderedPlayers);

        if (players.Count < 2)
        {
            Debug.LogError("2 players not in game");
            return;
        }

        playerOne = players[0];
        playerTwo = players[1];


        GameObject playerOneObject = Instantiate(playerPrefabs, spawnPosPlayerOne, Quaternion.identity);
        playerOneObject.GetComponent<SpriteRenderer>().color = playerOne.GetProfile().color;


        GameObject playerTwoObject = Instantiate(playerPrefabs, spawnPosPlayerTwo, Quaternion.identity);
        playerTwoObject.GetComponent<SpriteRenderer>().color = playerTwo.GetProfile().color;


        PlayerDict.Add(playerOne.id, playerOneObject);
        PlayerDict.Add(playerTwo.id, playerTwoObject);
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

    private void Update()
    {
        if (PlayerDict.Count > 0)
        {
            LocalPlayerSet();
            GetOtherPlayers();
        }
    }

    private void LocalPlayerSet()
    {
        if (playerJoined)
        {
            if (PlayerDict.TryGetValue(_playroomKit.MyPlayer().id, out GameObject p))
            {
                p.GetComponent<Player>().PlayerJumpInput();
                _playroomKit.MyPlayer().SetState("pos", p.transform.position);
            }
        }
    }

    private void GetOtherPlayers()
    {
        //for (var i = 0; i < players.Count; i++)
        //{
        //    if (players[i] != null)
        //    {
        //        var pos = players[i].GetState<Vector3>("pos");
        //        if (playerGameObjects != null)
        //        {
        //            playerGameObjects[i].GetComponent<Transform>().position = pos;
        //        }
        //    }
        //}

        foreach (KeyValuePair<string, GameObject> player in PlayerDict)
        {
            if (player.Key == _playroomKit.MyPlayer().id) continue;

            Vector3 pos = _playroomKit.GetPlayer(player.Key).GetState<Vector3>("pos");
            player.Value.GetComponent<Transform>().position = pos;
        }
    }

    private void AddPlayer(PlayroomKit.Player player)
    {
        if (_playroomKit.IsHost())
        {
            playersIds.Add(player.id);
            string data = string.Join(",", playersIds);
            Debug.Log($"host sending player ids: {data}");
            _playroomKit.RpcCall("UpdatePlayerList", data, PlayroomKit.RpcMode.OTHERS);
        }

        players.Add(player);

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
    [SerializeField] private UnityEngine.Color gizmoColor = Color.green;

    private void OnDrawGizmos()
    {
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(spawnPosPlayerOne, gizmoRadius);
        Gizmos.DrawSphere(spawnPosPlayerTwo, gizmoRadius);
    }
}
