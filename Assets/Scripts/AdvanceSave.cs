using NUnit.Framework;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using Unity.Services.Authentication;
using Unity.Services.CloudSave;
using Unity.Services.Core;
using UnityEngine;

public class AdvanceSave : MonoBehaviour
{
    [SerializeField]private PlayerData playerData;
    private async void Start()
    {
        await UnityServices.InitializeAsync();

      
    }
    [Button]
    public async void Test()
    {
        var name = await AuthenticationService.Instance.GetPlayerNameAsync();
        print("Advance save" + name);
    }
    [Button]
    public async void SavePlayerData()
    {
        var playerDataJson = JsonUtility.ToJson(playerData);
        //var data = new Dictionary<string, object>() { {"player.profile", playerDataJson } };

        await CloudSaveService.Instance.Data.Player.SaveAsync(
            new Dictionary<string, object> { { "player_profile", playerDataJson } });
    }
    [Button]
    public async void LoadPlayerData()
    {
        var key = new HashSet<string>() { "player_profile" };
        var data = await CloudSaveService.Instance.Data.Player.LoadAsync(key);

        if(data.TryGetValue("player_profile", out var profile))
        {
            string json = profile.Value.GetAs<string>();
            PlayerData loadPlayerData = JsonUtility.FromJson<PlayerData>(json);
            SetData(loadPlayerData);
        }
    }

    public void SetData(PlayerData newData)
    {
        playerData = newData;
    }
}

[Serializable]
public class PlayerData
{
    public string characterName;
    public int level;
    public int gold;
    public int schemaVersion = 1;
    public List<ItemData> Inventory = new();

}
[Serializable]
public class ItemData
{
    public string ItemId;
    public int quantity;
}
