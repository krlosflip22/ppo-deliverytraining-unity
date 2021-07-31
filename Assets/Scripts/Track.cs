using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Track : MonoBehaviour
{
    public TrackTile currentTile;
    TrackTile firstTile;

    [SerializeField] TrackTile[] tiles;

    [SerializeField] List<TrackTile> ignoredTiles;

    [SerializeField] List<TrackTile> coveredTracks;

    public event AddRewardDelegate OnCorrectMovement;
    public event RemoveRewardDelegate OnIncorrectMovement;

    public int TilesCount { get => tiles.Length; }

    void Awake()
    {
        firstTile = currentTile;
        coveredTracks = new List<TrackTile>();
        foreach(TrackTile t in tiles)
        {
            t.Initialize(this);
        }
    }

    public void Setup()
    {
        ClearCovered();
        foreach(TrackTile t in tiles)
        {
            t.Reset();
        }
        currentTile = firstTile;
    }

    public void ClearCovered()
    {
        coveredTracks.Clear();
    }

    public TrackTile GetTile(int index)
    {
        return ignoredTiles.Contains(tiles[index]) ? null : tiles[index];
    }

    public TrackTile GetRandomTile()
    {
        var rnd = new System.Random();
        return tiles.Where(x => !ignoredTiles.Contains(x) && x.tileType == TileType.straight)
            .OrderBy(x => rnd.Next())
            .First();
    }

    public void AddTileToCoveredList(TrackTile tile)
    {
        if(coveredTracks.Contains(tile))
        {
            Debug.Log("End: Re enter to previous tile");
            ClearCovered();
            OnIncorrectMovement(-1f, MLStatsManager.REPEATED_TILE);
            return;
        }
        coveredTracks.Add(tile);
        TrackTile prevTile = currentTile;
        prevTile.Reset();
        currentTile = tile;
    }
}
