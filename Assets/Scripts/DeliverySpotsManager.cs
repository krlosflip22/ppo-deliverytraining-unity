using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DeliverySpotsManager : MonoBehaviour
{
    [SerializeField] List<DeliveryPoint> Milestones;
    [SerializeField] DeliveryPoint Goal;
    [SerializeField] Track track;

    int currentPoint = 0;

    Transform agentTransform;
    public DeliveryPoint currentDestination;
    float minDestinationDistance = -1000;
    float totalDestinationDistance = -1000;

    List<TrackTile> destinationTiles;

    public event AddRewardDelegate OnCorrectMovement;
    public event RemoveRewardDelegate OnIncorrectMovement;

    void Awake()
    {
        destinationTiles = new List<TrackTile>();

        for (int i = 0; i < Milestones.Count; i++)
        {
            Milestones[i].Initialize(this);
        }

        Goal.Initialize(this);
    }

    public void Setup(Transform _agentTransform)
    {
        if(agentTransform == null) agentTransform = _agentTransform;

        destinationTiles.Clear();
        currentPoint = 0;

        for(int i = 0; i < Milestones.Count; i++)
        {
            Milestones[i].Initialize(this);
        }

        Goal.Initialize(this);
        Goal.index = Milestones.Count;

        for (int i = 0; i < Milestones.Count; i++)
        {
            SetPointPosition(Milestones[i].transform);
        }

        for (int i = 0; i < Milestones.Count; i++)
        {
            List<DeliveryPoint> tempList = Milestones.Where(x => x.index == -1).OrderBy(x => x.carDistance).ToList();
            if(tempList.Count > 0)
            {
                DeliveryPoint newTarget = tempList[0];
                newTarget.index = i;
                if(i == 0) SetDestination(newTarget);

                foreach(DeliveryPoint target in tempList) target.SetDistance(newTarget.transform);
            }
        }

        Goal.gameObject.SetActive(false);
    }

    public void SetDestination(DeliveryPoint newDestination)
    {
        currentDestination = null;
        minDestinationDistance = Mathf.Abs((agentTransform.position - newDestination.transform.position).magnitude);
        totalDestinationDistance = minDestinationDistance;
        currentDestination = newDestination;
        track.currentTile.SetForward(newDestination.transform, Vector3.zero, true);
    }

    public void CheckAgentDistance()
    {
        if(currentDestination != null)
        {
            float newDestinationDistance = Mathf.Abs((agentTransform.position - currentDestination.transform.position).magnitude);

            if(newDestinationDistance < minDestinationDistance)
            {
                minDestinationDistance = newDestinationDistance;
                OnCorrectMovement(10f * (1 - minDestinationDistance/totalDestinationDistance));
            }
            else if(Mathf.Abs(newDestinationDistance - minDestinationDistance) > 3f)
            {
                Debug.Log("End: Moved to other direction");
                OnIncorrectMovement(-1f, MLStatsManager.BAD_DIRECTION);
            }
        }
    }

    void SetPointPosition(Transform p)
    {
        TrackTile tt = null;

        while(tt == null || destinationTiles.Contains(tt))
        {
            tt = track.GetRandomTile();
        }

        destinationTiles.Add(tt);
        p.position = tt.transform.position;
        p.rotation = tt.transform.rotation;
        p.gameObject.SetActive(true);
    }


    public void SetReward(int _id, bool isLast)
    {
        if (currentPoint == _id)
        {
            Debug.Log($"Spot achieved {_id}");
            OnCorrectMovement(20, isLast);
            if (isLast) return;
            if (currentPoint == Milestones.Count - 1)
            {
                SetDestination(Goal);
                Goal.gameObject.SetActive(true);
            }
            else
            {
                SetDestination(Milestones.Where(x => x.index == _id + 1).First());
            }

            track.ClearCovered();

            currentPoint++;
        }
        else
        {
            Debug.Log($"End: Incorrect spot achieved {_id}");
            OnIncorrectMovement(-1f, MLStatsManager.INCORRECT_DESTINATION);
        }
    }
}