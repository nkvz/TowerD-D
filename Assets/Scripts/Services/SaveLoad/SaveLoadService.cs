using System;
using System.Collections.Generic;
using UnityEngine;

public class SaveLoadService
{
    private const string SaveKey = "TowerD&D.Tower.v1";
    private const int CurrentVersion = 1;

    [Serializable]
    public class TowerData
    {
        public int version = CurrentVersion;
        public List<CubeData> cubes = new List<CubeData>();
    }

    [Serializable]
    public class CubeData
    {
        public string type;
        public Vector2 position;
    }

    public void Save(IReadOnlyList<Cube> cubes)
    {
        TowerData data = new TowerData();
        for (int i = 0; i < cubes.Count; i++)
        {
            Cube cube = cubes[i];
            if (cube == null || cube.Config == null)
                continue;

            data.cubes.Add(new CubeData
            {
                type = cube.Config.Type.ToString(),
                position = cube.RectTransform.anchoredPosition
            });
        }

        PlayerPrefs.SetString(SaveKey, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    public bool TryLoad(out TowerData data)
    {
        data = null;
        if (!PlayerPrefs.HasKey(SaveKey))
            return false;

        try
        {
            data = JsonUtility.FromJson<TowerData>(PlayerPrefs.GetString(SaveKey));
            return data != null && data.version == CurrentVersion && data.cubes != null;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"Could not load tower save: {exception.Message}");
            data = null;
            return false;
        }
    }

    public void ClearAll()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }
}
