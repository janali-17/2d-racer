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

    private bool P1,P2;

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
            gameId = "fUnqBN5GmfFD6CTUykzK",
            maxPlayersPerRoom = 2,
            defaultPlayerStates = new() {
            {"score", 0},
            {"isDead", false}

        },
        }, () =>
        {
            _playroomKit.OnPlayerJoin(AddPlayer);
            _playroomKit.RpcRegister("SpawnPlayers", SpawnPlayers);
            _playroomKit.RpcRegister("UpdatePlayerList", UpdateLocalPlayers);
            _playroomKit.RpcRegister("OnPlayerDied", OnPlayerDied);
            print($"[Unity Log] isHost: {_playroomKit.IsHost()}");
            Time.timeScale = 1.0f;
        });
    }

    private void OnPlayerDied(string data, string arg2)
    {
        if (data == playerOne.id)
        {
            P1 = true;
            playerOneDied.gameObject.SetActive(true);
        }
        if (data == playerTwo.id)
        {
            P2 = true;
            playerTwoDied.gameObject.SetActive(true);
        }

        if(P1 && P2)
        {
            Time.timeScale = 0.0f;
        }
    }

    private void UpdateLocalPlayers(string data, string hostId)
    {
        List<string> newOrderIds = data.Split(',').Select(id => id.Trim()).Where(id => !string.IsNullOrEmpty(id)).ToList();

        playersIds.Clear();
        playersIds.AddRange(newOrderIds);

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

        playerOneObject.GetComponent<Player>().Initialize(playerOne.id);
        playerTwoObject.GetComponent<Player>().Initialize(playerTwo.id);
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
