using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PathFinding.AStar {
    public interface Pathfinding {
        void OnPathFound(Vector2[] newPath);

    }
}

