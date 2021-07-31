using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class MLStatsManager : MonoBehaviour
{
    private static MLStatsManager instance;

    public const string REPEATED_TILE="RepeatedTile";
    public const string END_COLLISION="EndCollision";
    public const string BAD_DIRECTION="BadDirection";
    public const string INCORRECT_DESTINATION="IncorrectDestination";
    public const string OUT_OF_TIME="OutOfTime";

    float collisionValue = 0;
    float tileRepeatedValue = 0;
    float badDestinationValue = 0;
    float badDirectionValue = 0;
    float outOfTimeValue = 0;

    private void Awake() 
    {
        instance = this;
    }

    void AddMetric(string metric, float value)
    {
        Academy.Instance.StatsRecorder.Add(metric,value);
    }

    public static void SendMetric(string metric)
    {
        instance.SendCollisionMetric(metric == END_COLLISION ? 1 : 0);
        instance.SendRepeatedTileMetric(metric == REPEATED_TILE ? 1 : 0);
        instance.SendIncorrectDestinationMetric(metric == INCORRECT_DESTINATION ? 1 : 0);
        instance.SendBadDirectionMetric(metric == BAD_DIRECTION ? 1 : 0);
        instance.SendOutOfTimeMetric(metric == OUT_OF_TIME ? 1 : 0);
    }

    public void SendCollisionMetric(int value)
    {
        collisionValue += value;
        AddMetric(END_COLLISION, collisionValue);
    }

    public void SendRepeatedTileMetric(int value)
    {
        tileRepeatedValue += value;
        AddMetric(REPEATED_TILE, tileRepeatedValue);
    }

    public void SendIncorrectDestinationMetric(int value)
    {
        badDestinationValue += value;
        AddMetric(INCORRECT_DESTINATION, badDestinationValue);
    }

    public void SendBadDirectionMetric(int value)
    {
        badDirectionValue += value;
        AddMetric(BAD_DIRECTION, badDirectionValue);
    }

    public void SendOutOfTimeMetric(int value)
    {
        outOfTimeValue += value;
        AddMetric(OUT_OF_TIME, outOfTimeValue);
    }
}
