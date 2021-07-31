using UnityEngine;
using System;

public enum TileType
{
    straight,
    corner,
    tform,
    cross
}

public class TrackTile : MonoBehaviour
{
    private Track owner;

    [SerializeField] Transform[] exitDirections;
    [SerializeField] GameObject[] invisibleWalls;

    public TileType tileType;

    public Vector3[] directions;

    public Vector3 Forward = Vector3.zero;

    public bool hasDestination = false;

    public int DirectionCount { get => directions.Length; }

    private void Awake()
    {
        directions = new Vector3[exitDirections.Length];
        for (int i = 0; i < directions.Length; i++)
        {
            directions[i] = (exitDirections[i].position - transform.position).normalized;
        }
    }

    public void Initialize(Track _owner)
    {
        owner = _owner;
    }

    public void Reset()
    {
        Forward = Vector3.zero;
        hasDestination = false;
        if(invisibleWalls != null)
        {
            foreach(GameObject o in invisibleWalls) o.SetActive(false);
        }
    }

    public Vector3 SetForward(Transform destination, Vector3 joinPosition, bool isFirstTile = false)
    {
        Vector3 newForward = Vector3.zero;
        float minDistance = 1000;
        float maxProjection = 0;
        float maxDestProjection = 0;
        int minIndex = -1;
        int carIndex = -1;

        bool hitsDestination = transform.position == destination.position;
        if(hitsDestination) Debug.Log("Has destination");

        for(int i = 0; i < directions.Length; i++)
        {
            if(!isFirstTile)
            {
                Vector3 directionToCar = Vector3.Project(joinPosition - transform.position, directions[i]);
                directionToCar.y = 0;
                if(directionToCar.normalized == directions[i].normalized && directionToCar.magnitude > maxProjection)
                {
                    maxProjection = directionToCar.magnitude;
                    carIndex = i;
                }
            }

            if(!hitsDestination)
            {
                float newDistance = (destination.position - (transform.position + directions[i] * 10)).magnitude;
                if(newDistance < minDistance)
                {
                    minDistance = newDistance;
                    minIndex = i;
                    newForward = directions[i];
                }
            }
            else
            {
                Vector3 carToDest = Vector3.Project(transform.position - joinPosition, directions[i]);
                carToDest.y = 0;
                if(carToDest.normalized == directions[i].normalized && carToDest.magnitude > maxDestProjection)
                {
                    maxDestProjection = carToDest.magnitude;
                    minIndex = i;
                    newForward = directions[i];
                }
            }
        }

        if(invisibleWalls != null)
        {
            for (int j = 0; j < invisibleWalls.Length; j++)
            {
                invisibleWalls[j].SetActive(j != minIndex && j != carIndex);
            }
        }

        Forward = newForward;
        return Forward;
    }

    void OnDrawGizmosSelected()
    {
        if(directions == null) return;

        Gizmos.color = Color.cyan;
            Gizmos.DrawLine(
                transform.position + Vector3.up * 2,
                transform.position + Vector3.up * 2 + Forward * 5
            );
        Gizmos.DrawSphere(transform.position + Vector3.up * 2 + Forward * 5, 0.3f);
    }
}
