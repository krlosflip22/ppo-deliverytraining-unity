using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    DeliverySpotsManager manager;

    [SerializeField] bool isLast;

    public float carDistance;

    public int index;

    public void Initialize(DeliverySpotsManager _manager)
    {
        manager = _manager;
        index = -1;
    }

    public void SetDistance(Transform t)
    {
        carDistance = Mathf.Abs((transform.position - t.position).magnitude);
        //Debug.Log(carDistance);
    }

    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.tag.ToLower() == "player")
        {
            manager.SetReward(index, isLast);
            gameObject.SetActive(false);
        }
    }

}