using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PathFinding.AStar
{
    public class AStarDeliverySpotsManager : MonoBehaviour
    {

        public List<AStarTarget> Milestones;

        public AStarTarget Goal;

        public Transform car;
        Vector3 carInitPosition;
        Quaternion carInitRotation;


        int currentPoint = 0;

        void Awake()
        {
            carInitPosition = car.transform.position;
            carInitRotation = car.transform.rotation;
            Setup();
        }

        public void Setup()
        {
            currentPoint = 0;

            for(int i = 0; i < Milestones.Count; i++)
            {
                Milestones[i].Initialize(this, car);
            }

            Goal.Initialize(this, car);
            Goal.index = Milestones.Count;

            currentPoint = 0;
            for (int i = 0; i < Milestones.Count; i++)
            {
                SetPointRandomPosition(Milestones[i].transform, i > 0 ? Milestones[i - 1].transform : null);
            }

            for (int i = 0; i < Milestones.Count; i++)
            {
                List<AStarTarget> tempList = Milestones.Where(x => x.index == -1).OrderBy(x => x.carDistance).ToList();
                if(tempList.Count > 0)
                {
                    AStarTarget newTarget = tempList[0];
                    newTarget.index = i;
                    foreach(AStarTarget target in tempList)
                    {
                        target.SetDistance(newTarget.transform);
                    }
                }
            }

            //Milestones[i].gameObject.SetActive(true);
            Goal.gameObject.SetActive(false);
            car.transform.position = carInitPosition;
            car.transform.rotation = carInitRotation;
        }

        void SetPointRandomPosition(Transform p, Transform previous = null)
        {
            bool horizontal = Random.Range(0, 2) == 0;

            float randomPosition = Random.Range(50.0f, 200.0f);
            int randomLine = 50 * Random.Range(1, 5);

            p.transform.localPosition = horizontal ? new Vector3(randomPosition, 0, randomLine) : new Vector3(randomLine, 0, randomPosition);

            if (previous != null)
            {
                while (p.transform.localPosition == previous.transform.localPosition)
                {
                    SetPointRandomPosition(p);
                }
            }

            p.GetComponent<AStarTarget>().SetDistance(car);
            p.gameObject.SetActive(true);
        }

        public void MoveToPoint(int index)
        {
            if(index < Milestones.Count)
                Milestones.FirstOrDefault(x => x.index == index).MoveToCar();
            else
            {
                Goal.gameObject.SetActive(true);
                Goal.MoveToCar();
            }
        }

        public void OnPointAchieved(int _id)
        {
            if (currentPoint == _id && _id < Milestones.Count)
            {
                currentPoint++;
                MoveToPoint(currentPoint);
            }
        }
    }
}