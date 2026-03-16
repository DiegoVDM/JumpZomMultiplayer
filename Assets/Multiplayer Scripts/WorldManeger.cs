using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;
using UnityEngine.UIElements;

namespace HelloWorld
{
    public class WorldManeger : MonoBehaviour
    {
        [Header("LAN Connection")]
        [SerializeField] private string hostIp = "192.168.1.23"; // client should target the host machine's LAN IP
        [SerializeField] private ushort port = 7777;

        private VisualElement rootVisualElement;
        private Button hostButton;
        private Button clientButton;
        private Button serverButton;
        private Button moveButton;
        private Label statusLabel;

        private NetworkManager nm;
        private UnityTransport transport;

        private void OnEnable()
        {
            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument == null)
            {
                Debug.LogError("WorldManeger: UIDocument component not found on this GameObject.");
                return;
            }

            rootVisualElement = uiDocument.rootVisualElement;

            hostButton = CreateButton("HostButton", "Host");
            clientButton = CreateButton("ClientButton", "Client");
            serverButton = CreateButton("ServerButton", "Server");
            moveButton = CreateButton("MoveButton", "Move");
            statusLabel = CreateLabel("StatusLabel", "Not Connected");

            rootVisualElement.Clear();
            rootVisualElement.Add(hostButton);
            rootVisualElement.Add(clientButton);
            rootVisualElement.Add(serverButton);
            rootVisualElement.Add(moveButton);
            rootVisualElement.Add(statusLabel);

            hostButton.clicked += OnHostButtonClicked;
            clientButton.clicked += OnClientButtonClicked;
            serverButton.clicked += OnServerButtonClicked;
            moveButton.clicked += SubmitNewPosition;

            nm = NetworkManager.Singleton;

            if (nm != null)
            {
                transport = nm.GetComponent<UnityTransport>();

                nm.OnServerStarted += OnServerStarted;
                nm.OnClientConnectedCallback += OnClientConnected;
                nm.OnClientDisconnectCallback += OnClientDisconnected;
            }
            else
            {
                Debug.LogError("WorldManeger: NetworkManager.Singleton is null.");
            }
        }

        private void Update()
        {
            UpdateUI();
        }

        private void OnDisable()
        {
            if (hostButton != null) hostButton.clicked -= OnHostButtonClicked;
            if (clientButton != null) clientButton.clicked -= OnClientButtonClicked;
            if (serverButton != null) serverButton.clicked -= OnServerButtonClicked;
            if (moveButton != null) moveButton.clicked -= SubmitNewPosition;

            if (nm != null)
            {
                nm.OnServerStarted -= OnServerStarted;
                nm.OnClientConnectedCallback -= OnClientConnected;
                nm.OnClientDisconnectCallback -= OnClientDisconnected;
            }
        }

        private void OnHostButtonClicked()
        {
            if (nm == null || transport == null)
            {
                Debug.LogError("Host start failed: NetworkManager or UnityTransport missing.");
                return;
            }

            transport.SetConnectionData("0.0.0.0", port);
            bool started = nm.StartHost();

            Debug.Log($"StartHost() returned {started}. Listening on 0.0.0.0:{port}");
        }

        private void OnClientButtonClicked()
        {
            if (nm == null || transport == null)
            {
                Debug.LogError("Client start failed: NetworkManager or UnityTransport missing.");
                return;
            }

            transport.SetConnectionData(hostIp, port);
            bool started = nm.StartClient();

            Debug.Log($"StartClient() returned {started}. Attempting to connect to {hostIp}:{port}");
        }

        private void OnServerButtonClicked()
        {
            if (nm == null || transport == null)
            {
                Debug.LogError("Server start failed: NetworkManager or UnityTransport missing.");
                return;
            }

            transport.SetConnectionData("0.0.0.0", port);
            bool started = nm.StartServer();

            Debug.Log($"StartServer() returned {started}. Listening on 0.0.0.0:{port}");
        }

        private void OnServerStarted()
        {
            Debug.Log("SERVER STARTED YAYY SUCESS #1");
        }

        private void OnClientConnected(ulong clientId)
        {
            Debug.Log($"CLIENT CONNECTEDDD: {clientId} AFTER CLIENT BUTTEN HAS BEN PRESS SHOULD THIS BE HERE!");

            var obj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            Debug.Log(obj != null
                ? $"PLAYER OBJECT FOUND FOR {clientId}: {obj.name}"
                : $"NO PLAYER OBJECT FOUND FOR {clientId}");
        }

        private void OnClientDisconnected(ulong clientId)
        {
            Debug.LogWarning($"CLIENT DISCONNECTED!!!! BYEEEEE !!!!: {clientId}");
        }

        private Button CreateButton(string name, string text)
        {
            var button = new Button();
            button.name = name;
            button.text = text;
            button.style.width = 240;
            button.style.backgroundColor = Color.white;
            button.style.color = Color.black;
            button.style.unityFontStyleAndWeight = FontStyle.Bold;
            return button;
        }

        private Label CreateLabel(string name, string content)
        {
            var label = new Label();
            label.name = name;
            label.text = content;
            label.style.color = Color.black;
            label.style.fontSize = 18;
            return label;
        }

        private void UpdateUI()
        {
            if (NetworkManager.Singleton == null)
            {
                SetStartButtons(false);
                SetMoveButton(false);
                SetStatusText("NetworkManager not found");
                return;
            }

            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                SetStartButtons(true);
                SetMoveButton(false);
                SetStatusText("Not connected");
            }
            else
            {
                SetStartButtons(false);
                SetMoveButton(true);
                UpdateStatusLabels();
            }
        }

        private void SetStartButtons(bool state)
        {
            hostButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            clientButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
            serverButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void SetMoveButton(bool state)
        {
            moveButton.style.display = state ? DisplayStyle.Flex : DisplayStyle.None;

            if (state)
            {
                moveButton.text = NetworkManager.Singleton.IsServer ? "Move" : "Request Position Change";
            }
        }

        private void SetStatusText(string text)
        {
            if (statusLabel != null)
                statusLabel.text = text;
        }

        private void UpdateStatusLabels()
        {
            var mode = NetworkManager.Singleton.IsHost ? "Host"
                     : NetworkManager.Singleton.IsServer ? "Server"
                     : "Client";

            string transportName = NetworkManager.Singleton.NetworkConfig.NetworkTransport != null
                ? NetworkManager.Singleton.NetworkConfig.NetworkTransport.GetType().Name
                : "None";

            string transportText = "Transport: " + transportName;
            string modeText = "Mode: " + mode;
            string ipText = $"Target IP: {hostIp}:{port}";

            SetStatusText($"{transportText}\n{modeText}\n{ipText}");
        }

        private void SubmitNewPosition()
        {
            // Tutorial-only code path.
            // Keeping this as a reference, but it is NOT part of your real player movement system.
            // Your actual player movement should come from PlayerController + NetworkTransform.

            if (NetworkManager.Singleton == null)
            {
                Debug.LogWarning("SubmitNewPosition: NetworkManager.Singleton is null.");
                return;
            }

            if (NetworkManager.Singleton.IsServer && !NetworkManager.Singleton.IsClient)
            {
                foreach (ulong uid in NetworkManager.Singleton.ConnectedClientsIds)
                {
                    var playerObject = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(uid);

                    if (playerObject == null)
                    {
                        Debug.LogWarning($"SubmitNewPosition: no player object found for client {uid}.");
                        continue;
                    }

                    var player = playerObject.GetComponent<MultiPScript>();

                    if (player == null)
                    {
                        Debug.LogWarning(
                            $"SubmitNewPosition: MultiPScript not found on player object for client {uid}. " +
                            "This is expected if you removed the tutorial script."
                        );
                        continue;
                    }

                    player.Move();
                }
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                var playerObject = NetworkManager.Singleton.SpawnManager.GetLocalPlayerObject();

                if (playerObject == null)
                {
                    Debug.LogWarning("SubmitNewPosition: local player object not found.");
                    return;
                }

                var player = playerObject.GetComponent<MultiPScript>();

                if (player == null)
                {
                    Debug.LogWarning(
                        "SubmitNewPosition: MultiPScript not found on local player object. " +
                        "This is expected if you switched to PlayerController."
                    );
                    return;
                }

                player.Move();
            }
        }



        //Start Debug code
        private void Start()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted += OnServerStarted;
            }
        }

        private void OnDestroy()
        {
            if (NetworkManager.Singleton != null)
            {
                NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
                NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
                NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
            }
        }
        //end Debug code

    }
}