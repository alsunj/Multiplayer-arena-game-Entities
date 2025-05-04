using TMPro;
using Unity.Entities;
using Unity.NetCode;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class ConnectionManager : MonoBehaviour
{
    [SerializeField] private TMP_InputField _addressField;
    [SerializeField] private TMP_InputField _portField;
    [SerializeField] private TMP_Dropdown _connectionModeDropdown;
    [SerializeField] private TMP_InputField _playerAmountField;
    [SerializeField] private TMP_InputField _RogueEnemyAmountField;
    [SerializeField] private TMP_InputField _SlimeEnemyAmountField;
    [SerializeField] private GameObject LobbyAmountContainer;
    [SerializeField] private GameObject RangerAmountContainer;
    [SerializeField] private GameObject SlimeAmountContainer;
    [SerializeField] private Button _connectButton;
    [SerializeField] private int _gameStartCountDownTime;
    [SerializeField] private float _slimeSpawnCooldownTime;
    [SerializeField] private float _rogueSpawnCooldownTime;


    private ushort Port => ushort.Parse(_portField.text);
    private int PlayerAmount => int.Parse(_playerAmountField.text);
    private int RogueEnemyAmount => int.Parse(_RogueEnemyAmountField.text);
    private int SlimeEnemyAmount => int.Parse(_SlimeEnemyAmountField.text);
    private string Address => _addressField.text;

    private void OnEnable()
    {
        _connectionModeDropdown.onValueChanged.AddListener(OnConnectionModeChanged);
        _connectButton.onClick.AddListener(OnButtonConnect);
        OnConnectionModeChanged(_connectionModeDropdown.value);
    }

    private void OnDisable()
    {
        _connectionModeDropdown.onValueChanged.RemoveAllListeners();
        _connectButton.onClick.RemoveAllListeners();
    }

    private void OnConnectionModeChanged(int connectionMode)
    {
        string buttonLabel;
        _connectButton.enabled = true;

        switch (connectionMode)
        {
            case 0:
                buttonLabel = "Start Host";
                LobbyAmountContainer.SetActive(true);
                RangerAmountContainer.SetActive(true);
                SlimeAmountContainer.SetActive(true);
                break;
            case 1:
                buttonLabel = "Start Client";
                LobbyAmountContainer.SetActive(false);
                RangerAmountContainer.SetActive(false);
                SlimeAmountContainer.SetActive(false);
                break;
            default:
                buttonLabel = "<ERROR>";
                _connectButton.enabled = false;
                break;
        }

        var buttonText = _connectButton.GetComponentInChildren<TextMeshProUGUI>();
        buttonText.text = buttonLabel;
    }


    private void OnButtonConnect()
    {
        DestroyLocalSimulationWorld();
        SceneManager.LoadScene(1);

        switch (_connectionModeDropdown.value)
        {
            case 0:
                StartServer();
                StartClient();
                break;
            case 1:
                StartClient();
                break;
            default:
                Debug.LogError("Error: Unknown connection mode", gameObject);
                break;
        }
    }

    private static void DestroyLocalSimulationWorld()
    {
        foreach (var world in World.All)
        {
            if (world.Flags == WorldFlags.Game)
            {
                world.Dispose();
                break;
            }
        }
    }

    private void StartServer()
    {
        var serverWorld = ClientServerBootstrap.CreateServerWorld("Game Server World");

        var serverEndpoint = NetworkEndpoint.AnyIpv4.WithPort(Port);
        {
            using var networkDriverQuery =
                serverWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW.Listen(serverEndpoint);
        }

        var spawnerEntity = serverWorld.EntityManager.CreateEntity();
        serverWorld.EntityManager.AddComponentData(spawnerEntity, new GameStartProperties
        {
            CountdownTime = _gameStartCountDownTime,
            PlayerAmount = PlayerAmount,
            RogueEnemyAmount = RogueEnemyAmount,
            SlimeEnemyAmount = SlimeEnemyAmount
        });
        serverWorld.EntityManager.AddComponentData(spawnerEntity, new EnemySpawnTimer
        {
            SlimeSpawnCooldown = _slimeSpawnCooldownTime,
            RogueSpawnCooldown = _rogueSpawnCooldownTime
        });
        serverWorld.EntityManager.AddComponentData(spawnerEntity, new SpawnableEnemiesCounter
        {
            SlimeEnemyCounter = 0,
            RogueEnemyCounter = 0
        });
        serverWorld.EntityManager.AddComponentData(spawnerEntity, new PlayerCounter { Value = 0 });
    }

    private void StartClient()
    {
        var clientWorld = ClientServerBootstrap.CreateClientWorld("Game Client World");

        var connectionEndpoint = NetworkEndpoint.Parse(Address, Port);
        {
            using var networkDriverQuery =
                clientWorld.EntityManager.CreateEntityQuery(ComponentType.ReadWrite<NetworkStreamDriver>());
            networkDriverQuery.GetSingletonRW<NetworkStreamDriver>().ValueRW
                .Connect(clientWorld.EntityManager, connectionEndpoint);
        }

        World.DefaultGameObjectInjectionWorld = clientWorld;
    }
}