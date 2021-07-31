using UnityEngine;

namespace PathFinding.AStar {

    [RequireComponent(typeof(CountPath))]
    public class SeekerController : MonoBehaviour {

        private CountPath counter;

        void Start() {
            counter = GetComponent<CountPath>();

        }

        void Update() {
            if (Input.GetMouseButtonDown(0)) {
                counter.FindPath(transform, Camera.main.ScreenToWorldPoint(Input.mousePosition));
            }
        }


    }
}