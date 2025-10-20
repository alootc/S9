using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Authentication;
using System.Collections.Generic;
using System.Threading.Tasks;

public class TestLobby : MonoBehaviour
{
    public Lobby hostLobby;
    public Lobby joinLobby;

    private float heartbeatTimer;
    private float heartbeatFrequency = 15f;

    private async void Start()
    {
        await InitializeUnityServices();
    }

    private async Task InitializeUnityServices()
    {
        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
                Debug.Log("Signed in as: " + AuthenticationService.Instance.PlayerId);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Error initializing Unity Services: " + e);
        }
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    private void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer <= 0f)
            {
                heartbeatTimer = heartbeatFrequency;
                SendHeartbeat();
            }
        }
    }

    private async void SendHeartbeat()
    {
        try
        {
            await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            Debug.Log("Heartbeat sent to lobby: " + hostLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogWarning("Heartbeat failed: " + e.Message);
        }
    }

    public async void CreateLobby(string lobbyName = "Test Lobby", int maxPlayers = 4, bool isPrivate = false)
    {
        try
        {
            Player player = await GetPlayer();

            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Player = player,
                Data = new Dictionary<string, DataObject>
                {
                    { "GameMode", new DataObject(DataObject.VisibilityOptions.Public, "Default") },
                    { "Map", new DataObject(DataObject.VisibilityOptions.Public, "Arena") }
                }
            };

            hostLobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, options);
            joinLobby = hostLobby;

            Debug.Log($"Lobby creado: {hostLobby.Name} | Código: {hostLobby.LobbyCode}");
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Error al crear lobby: " + e);
        }
    }

    public async void ListLobbies()
    {
        try
        {
            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync();
            Debug.Log("Lobbies encontrados: " + response.Results.Count);

            foreach (Lobby lobby in response.Results)
            {
                Debug.Log($"Lobby: {lobby.Name} | Jugadores: {lobby.Players.Count}/{lobby.MaxPlayers} | Código: {lobby.LobbyCode}");
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Error al listar lobbies: " + e);
        }
    }

    public async void JoinLobbyByCode(string lobbyCode)
    {
        try
        {
            Player player = await GetPlayer();

            JoinLobbyByCodeOptions options = new JoinLobbyByCodeOptions
            {
                Player = player
            };

            joinLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, options);
            Debug.Log("Unido al lobby con código: " + lobbyCode);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Error al unirse al lobby: " + e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            if (joinLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(joinLobby.Id, AuthenticationService.Instance.PlayerId);
                Debug.Log("Has salido del lobby.");
                joinLobby = null;
                hostLobby = null;
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError("Error al salir del lobby: " + e);
        }
    }

    public async Task<Player> GetPlayer()
    {
        string nickname = "Guest_" + Random.Range(1000, 9999);

        try
        {
            string playerName = await AuthenticationService.Instance.GetPlayerNameAsync();
            if (!string.IsNullOrEmpty(playerName))
                nickname = playerName;
        }
        catch
        {
            Debug.Log("Sin nombre personalizado, usando: " + nickname);
        }

        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
            {
                { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, nickname) }
            }
        };
    }

    public void PrintPlayers()
    {
        if (joinLobby == null)
        {
            Debug.Log("No estás en un lobby actualmente.");
            return;
        }

        foreach (Player player in joinLobby.Players)
        {
            if (player.Data != null && player.Data.ContainsKey("PlayerName"))
            {
                Debug.Log("Jugador: " + player.Data["PlayerName"].Value);
            }
        }
    }
}
