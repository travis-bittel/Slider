using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class MirageSTileManager : Singleton<MirageSTileManager>, ISavable
{
    [SerializeField] private List<GameObject> mirageSTiles;
    [SerializeField] private List<ArtifactTBPluginMirage> mirageButtons;
    //public static Vector2Int mirageTailPos;
    public static EventHandler OnMirageSTilesEnabled;
    public List<STileTilemap> MirageMaterialTileMaps;

    private bool mirageEnabled;
    public bool MirageEnabled => mirageEnabled;

    /// <summary>
    /// The scale factor from the position of a tile on the grid to the transform.position of the tile.
    /// </summary>
    private const int GRID_POSITION_TO_WORLD_POSITION = 17;

    private const string MIRAGE_ENABLED_SAVE_STRING = "DesertMirageEnabled";

    public void Save()
    {
        if(mirageEnabled)
        {
            UnSubscribeMirageEvents();
        }
        SaveSystem.Current.SetBool(MIRAGE_ENABLED_SAVE_STRING, mirageEnabled);
    }

    public void Load(SaveProfile profile)
    {
        mirageEnabled = profile.GetBool(MIRAGE_ENABLED_SAVE_STRING);
        if(mirageEnabled)
        {
            EnableMirage();
        }
    }

    public void Awake()
    {
        InitializeSingleton();
   //     mirageTailPos = new Vector2Int(-1, -1);
        mirageButtons = UIArtifact._instance.transform.parent.GetComponentsInChildren<ArtifactTBPluginMirage>().ToList();
    }

    private void SubscibeMirageEvents()
    {
        SGridAnimator.OnSTileMoveStart += RemovePlayerOnMirageSTile;
        SGridAnimator.OnSTileMoveEndLate += EnableMirageTilesAfterSMove;
    }

    private void UnSubscribeMirageEvents()
    {
        SGridAnimator.OnSTileMoveStart -= RemovePlayerOnMirageSTile;
        SGridAnimator.OnSTileMoveEndLate -= EnableMirageTilesAfterSMove;
    }


    public void EnableMirage()
    {
        mirageEnabled = true;
        SubscibeMirageEvents();
        EnableMirageVFX();
    }

    public void EnableMirageVFX() {}

    public void DisableMirage()
    {
        mirageEnabled = false;
        UnSubscribeMirageEvents();
        DisableMirageVFX();
        DisableMirage(-1);
        //remove and disable lasers;
    }

    public void DisableMirageVFX() {}

    public void EnableMirageTile(int islandId, int x, int y)
    {
        if (islandId > 7 || islandId < 1) return;
        mirageSTiles[islandId - 1].transform.position = new Vector2(x * GRID_POSITION_TO_WORLD_POSITION, y * GRID_POSITION_TO_WORLD_POSITION);
        mirageSTiles[islandId - 1].gameObject.SetActive(true);
    }
    
    /// <summary>
    /// Function that disables mirages either from selecting or from making an artifact move
    /// </summary>
    /// <param name="islandId">0 means disable all mirages</param>
    public void DisableMirage(int islandId = -1)
    {
        //Insert disable effect
        if (islandId == 0 || islandId > 7) return;
        if (islandId < 0) foreach (GameObject o in mirageSTiles) o.SetActive(false);
        else mirageSTiles[islandId - 1].gameObject.SetActive(false);
    }

    private void EnableMirageTilesAfterSMove(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        if (!mirageEnabled) return; 
        if (UIArtifact.GetInstance().MoveQueueEmpty())
        {
            //No new moves should be queued before mirage tiles are enabled
            foreach (ArtifactTBPluginMirage button in mirageButtons)
            {
                var buttonBase = button.GetComponent<ArtifactTileButton>();
                EnableMirageTile(button.mirageIslandId, buttonBase.x, buttonBase.y);
            }

        }

        OnMirageSTilesEnabled?.Invoke(this, null);
    }

    private void RemovePlayerOnMirageSTile(object sender, SGridAnimator.OnTileMoveArgs e)
    {
        RemovePlayerOnMirageSTile();
    }

    private void RemovePlayerOnMirageSTile()
    {
        int mirageIsland;
        bool playerOnMirage = IsPlayerOnMirage(out mirageIsland);

        if (playerOnMirage)
        {
            STile realSTile = DesertGrid.Current.GetStile(mirageIsland);
            Vector3 relativePos = Player._instance.transform.position - mirageSTiles[mirageIsland - 1].transform.position;
            realSTile.SetBorderColliders(false);
            Player.SetParent(realSTile.transform);
            Player.SetPosition(realSTile.transform.position + relativePos);

            AudioManager.Play("Hurt");
            UIEffects.FadeFromBlack(null, 1.5f);
        }
    }

    /// <summary>
    /// Returns a dictionary of each grid position, represented by Vector2Int ,
    /// to the ID of the mirage tile currently active on that grid position.
    /// If no mirage tile is active on that position, that position does not
    /// appear in the dictionary.
    /// </summary>
    public static Dictionary<Vector2Int, char> GetActiveMirageTileIdsByPosition()
    {
        List<GameObject> mirageTiles = _instance.mirageSTiles;

        Dictionary<Vector2Int, char> tileIdToPosition = new();

        for (int tileId = 0; tileId < mirageTiles.Count; tileId++)
        {
            GameObject mirageTile = mirageTiles[tileId];
            if (mirageTile.activeInHierarchy)
            {
                Vector2Int mirageTileGridPosition = GridPositionFromWorldPosition(mirageTile.transform.position);
                tileIdToPosition[mirageTileGridPosition] = (char)('A' + tileId);
            }
        }

        return tileIdToPosition;
    }

    private static Vector2Int GridPositionFromWorldPosition(Vector2 worldPosition)
    {
        int x = (int)(worldPosition.x / GRID_POSITION_TO_WORLD_POSITION);
        int y = (int)(worldPosition.y / GRID_POSITION_TO_WORLD_POSITION);
        return new Vector2Int(x, y);
    }

    public bool IsPlayerOnMirage(out int islandId)
    {
        Vector2 pos = Player.GetInstance().transform.position;
        float offset = 8.5f;
        for (int i = 0; i < 7; i++)
        {
            if (!mirageSTiles[i].activeSelf) continue;
            Vector3 stilePos = mirageSTiles[i].transform.position;
            if (stilePos.x - offset < pos.x && pos.x < stilePos.x + offset &&
            (stilePos.y - offset < pos.y && pos.y < stilePos.y + offset))
            {
               islandId = i+1;
               return true;
            }
        }
        islandId = -1;
        return false;
    }

    public static MirageSTileManager GetInstance()
    {
        return _instance;
    }

    public STileTilemap GetMaterialTileMap(int mirageIslandId)
    {
        return MirageMaterialTileMaps[mirageIslandId - 1];
    }

    public void IsMirageEnabled(Condition c)
    {
        c.SetSpec(mirageEnabled);
    }

   
}
