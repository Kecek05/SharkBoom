using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.CloudSave;
using QFSW.QC;
using System;

public static class Save
{
    private static readonly string PLAYER_PEARLS = "PlayerPearls";

    public static event Action<int> OnPlayerPearlsChanged;

    [Command("save-savePlayerPeals")]
    public static async Task SavePlayerPearls(int PlayerPearls)
    {
        //Save to cloud
        var data = new Dictionary<string, object> { { PLAYER_PEARLS, PlayerPearls } };
        await CloudSaveService.Instance.Data.Player.SaveAsync(data);

        OnPlayerPearlsChanged?.Invoke(PlayerPearls);
        Debug.Log($"Saved {PLAYER_PEARLS} with value {PlayerPearls}");
    }

    public static async Task<int> LoadPlayerPearls()
    {
        //Load from cloud
        var loadData = await CloudSaveService.Instance.Data.Player.LoadAsync(new HashSet<string>() { PLAYER_PEARLS});

        if (loadData.TryGetValue(PLAYER_PEARLS, out var playerPearls))
        {
            return playerPearls.Value.GetAs<int>();
        }
        else
        {
            Debug.Log($"No value found for {PLAYER_PEARLS}, returning 0");
            return 0;
        }
    }

}
