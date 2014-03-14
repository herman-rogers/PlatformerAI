using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinderNodes {
    public float heuristicValue; //h value
    public float movementCost; //g value
    public float totalCost; //f value
    public float extraCostWeight; //for ignoring obstacles or following the ground
    public bool isClosed;
    public bool onOpenList;
    public PathFinderNodes parentNode;
    public PathFinderNodes upNode;
    public PathFinderNodes downNode;
    public PathFinderNodes rightNode;
    public PathFinderNodes leftNode;
    public Vector3 objectPosition;

    //void Awake( ){
    //    DetectAdjacentNodes();
    //}

    public PathFinderNodes( Vector3 position ){
        objectPosition = position;
        isClosed = false;
        onOpenList = false;
    }

    public void CalculateFValues( ){
        totalCost = movementCost + heuristicValue;
    }
}
