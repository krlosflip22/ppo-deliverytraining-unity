using UnityEngine;

namespace PathFinding.AStar
{
    public class AStarTarget : MonoBehaviour
    {
        private CountPath counter;

        private AStarDeliverySpotsManager manager;

        public int index = -1;

        public float carDistance;

        public void Initialize(AStarDeliverySpotsManager _manager, Transform car)
        {
            manager = _manager;
            index = -1;
            counter = car.GetComponent<CountPath>();
        }

        public void SetDistance(Transform t)
        {
            carDistance = Mathf.Abs((transform.position - t.position).magnitude);
            Debug.Log(carDistance);
        }

        public void MoveToCar()
        {
            counter.FindPath(new Vector2(transform.position.x, transform.position.y));
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            if(collider.tag == "Start")
            {
                manager.OnPointAchieved(index);
                gameObject.SetActive(false);
            }
        }
    }
}