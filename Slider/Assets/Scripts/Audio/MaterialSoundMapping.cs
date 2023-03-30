using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(menuName = "Scriptable Objects/Material Sound Mapping")]
public class MaterialSoundMapping : ScriptableObject
{
    [SerializeField]
    private MaterialSoundMappingEntry[] mappingList;
    private Dictionary<Tile, FMODUnity.EventReference> _CachedMapping;

    public FMODUnity.EventReference this[TileBase t]
    {
        get {
            if (_CachedMapping == null)
            {
                _CachedMapping = new Dictionary<Tile, FMODUnity.EventReference>();
                foreach (var m in mappingList)
                {
                    if (_CachedMapping.ContainsKey(m.materialTile))
                    {
                        Debug.LogWarning($"{m.materialTile.name} is duplicated in preset {name}");
                    }
                    else
                    {
                        _CachedMapping.Add(m.materialTile, m.eventRef);
                    }
                }
            }

            if (t == null || t is not Tile)
            {
                Debug.LogWarning($"Not a proper tile: {t}");
                return fallbackMapping;
            } else if (!_CachedMapping.ContainsKey(t as Tile))
            {
                Debug.LogWarning($"Not in mapping: {t.name}");
                return fallbackMapping;
            } else
            {
                return _CachedMapping[t as Tile];
            }
        }
    }

    [SerializeField]
    private FMODUnity.EventReference fallbackMapping;

    [System.Serializable]
    public struct MaterialSoundMappingEntry
    {
        public Tile materialTile;
        public FMODUnity.EventReference eventRef;
    }
}