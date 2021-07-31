using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarTileDetector : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // if(other.tag.ToLower() == "limit")
        // {
        //     other.GetComponent<TrackTileLimit>().TriggerLimit();
        // }
    }
}
