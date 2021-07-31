using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.UI;
using System;

public delegate void AddRewardDelegate(float reward, bool isFinal = false);
public delegate void RemoveRewardDelegate(float reward = -1f, string failType = "");

public class CarAgent : Agent
{
    [Header("Managers")]
    [SerializeField] DeliverySpotsManager deliverySpots;
    [SerializeField] Track track;

    [Header("Car parameters")]
    [SerializeField] float speed = 10f;
    [SerializeField] float torque = 10f;
    [SerializeField] int score = 0;
    [SerializeField] bool resetOnCollision = true;

    [Header("UI")]
    [SerializeField] Text epCompletedText;
    [SerializeField] Text epSuccededText;
    [SerializeField] Text epFailedText;
    [SerializeField] Text endCollisionText;
    [SerializeField] Text endBadTileText;
    [SerializeField] Text endBadDestinationText;
    [SerializeField] Text endBadDirectionText;
    [SerializeField] Text endTimeoutText;

    int epCompleted = 0;
    int epSucceded = 0;
    int epFailed = 0;
    int endCollision = 0;
    int endTile = 0;
    int endDest = 0;
    int endDir = 0;
    int endTimeout = 0;

    float parallelDistance;
    float episodeInitTime;


    public override void Initialize()
    {
        deliverySpots.OnCorrectMovement += GivePoints;
        deliverySpots.OnIncorrectMovement += TakeAwayPoints;
        track.OnCorrectMovement += GivePoints;
        track.OnIncorrectMovement += TakeAwayPoints;
    }

    private void MoveCar(float horizontal, float vertical, float dt)
    {
        float distance = speed * vertical;
        transform.Translate(distance * dt * Vector3.forward);

        float rotation = horizontal * torque * 90f;
        transform.Rotate(0f, rotation * dt, 0f);
    }

    public override void OnActionReceived(float[] vectorAction)
    {
        float horizontal = vectorAction[0];
        float vertical = vectorAction[1];

        var lastPos = transform.position;
        MoveCar(horizontal, vertical, Time.fixedDeltaTime);

        int reward = GetTrackIncrement();

        var moveVec = transform.position - lastPos;
        float angle = Vector3.Angle(moveVec, track.currentTile.Forward);
        float bonus = (1f - angle / 90f) * Mathf.Clamp01(vertical) * Time.fixedDeltaTime;
        AddReward(bonus);

        score += reward;

        deliverySpots.CheckAgentDistance();
    }

    public void GivePoints(float amount = 1.0f, bool isFinal = false)
    {
        AddReward(amount);

        if(isFinal)
        {
            Debug.Log("Episode finished successfuly");
            epSucceded++;
            epSuccededText.text = epSucceded.ToString();
            epCompleted++;
            epCompletedText.text = epCompleted.ToString();
            EndEpisode();
        }
    }

    public override void Heuristic(float[] action)
    {
        action[0] = Input.GetAxis("Horizontal");
        action[1] = Input.GetAxis("Vertical");
    }

    public override void CollectObservations(VectorSensor vectorSensor)
    {
        float angle = Vector3.SignedAngle(track.currentTile.Forward, transform.forward, Vector3.up);

        vectorSensor.AddObservation(angle / 180f);
        vectorSensor.AddObservation(ObserveRay(2f, .5f, 25f));
        vectorSensor.AddObservation(ObserveRay(2f, 0f, 0f));
        vectorSensor.AddObservation(ObserveRay(2f, -.5f, -25f));
        vectorSensor.AddObservation(ObserveRay(-2f, 0, 180f));

    }

    const float RAY_DIST = 5f;
    private float ObserveRay(float z, float x, float angle)
    {
        var tf = transform;

        // Get the start position of the ray
        var raySource = tf.position + Vector3.up / 2f;
        var position = raySource + tf.forward * z + tf.right * x;

        // Get the angle of the ray
        var eulerAngle = Quaternion.Euler(0, angle, 0f);
        var dir = eulerAngle * tf.forward;

        // See if there is a hit in the given direction
        Physics.Raycast(position, dir, out var hit, RAY_DIST);
        return hit.distance >= 0 ? hit.distance / RAY_DIST : -1f;
    }

    bool outOfTimeSend = false;
    void Update()
    {
        if(Time.time - episodeInitTime > 5f * 60f && !outOfTimeSend)
        {
            TakeAwayPoints(-1, MLStatsManager.OUT_OF_TIME);
            outOfTimeSend = true;
        }
    }

    public override void OnEpisodeBegin()
    {
        if (resetOnCollision)
        {
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
        episodeInitTime = Time.time;
        outOfTimeSend = false;
        track.Setup();
        deliverySpots.Setup(transform);
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("wall"))
        {
            Debug.Log("End: Collision");
            TakeAwayPoints(-1, MLStatsManager.END_COLLISION);
        }
    }

    public void TakeAwayPoints(float reward = -1f, string failType = "")
    {
        MLStatsManager.SendMetric(failType);
        switch(failType)
        {
            case MLStatsManager.BAD_DIRECTION:  endDir++; endBadDirectionText.text = endDir.ToString(); break;
            case MLStatsManager.INCORRECT_DESTINATION:  endDest++; endBadDestinationText.text = endDest.ToString(); break;
            case MLStatsManager.OUT_OF_TIME:  endTimeout++; endTimeoutText.text = endTimeout.ToString(); break;
            case MLStatsManager.END_COLLISION:  endCollision++; endCollisionText.text = endCollision.ToString(); break;
            case MLStatsManager.REPEATED_TILE:  endTile++; endBadTileText.text = endTile.ToString(); break;
        }

        SetReward(reward);
        epFailed++;
        epFailedText.text = epFailed.ToString();
        epCompleted++;
        epCompletedText.text = epCompleted.ToString();
        EndEpisode();
    }

    private int GetTrackIncrement()
    {
        var carCenter = transform.position + Vector3.up + Vector3.forward;

        // Find what tile I'm on
        if (Physics.Raycast(carCenter, Vector3.down, out RaycastHit hit, 2f))
        {
            var newHit = hit.transform.GetComponent<TrackTile>();
            if(newHit != null && newHit != track.currentTile)
            {
                newHit.SetForward(deliverySpots.currentDestination.transform, hit.point);
                track.AddTileToCoveredList(newHit);
            }
        }

        return 0;
    }

    void OnDrawGizmosSelected()
    {
        if(track.currentTile == null) return;

        // Draws a blue line from this transform to the target
        Gizmos.color = Color.magenta;
        Gizmos.DrawLine(
            transform.position + Vector3.up * 2,
            transform.position + Vector3.up * 2 + track.currentTile.Forward * 3
            );
        Gizmos.DrawSphere(transform.position + Vector3.up * 2 + track.currentTile.Forward * 3, 0.2f);
    }
}