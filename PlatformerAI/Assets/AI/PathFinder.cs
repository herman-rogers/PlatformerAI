using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PathFinder : MonoBehaviour {
    public GameObject pathingGameObject;
    public GameObject graphGameObject;
    public GameObject[] listOfTargets;
    public List< Vector3 > aiPath;
    public float depthGraphSearch;
    public bool debugGraph;
    public bool debugPath;
    public float debugTime = 0.2f;
    Transform targetGameObject;
    PathFinderNodes targetNode;
    PathFinderNodes checkingNode;
    Vector3 startNode;
    PriorityQueue< float, PathFinderNodes > openQueue;
    Dictionary< PathFinderNodes, Vector3 > storedNodeList;
    List< PathFinderNodes > closedNodeList;
    AIController aiController;
    bool foundTarget;
    bool reachedTarget;
    float incrementY;
    float raycastSize = 1.0f;
    const float searchGridSize = 1.0f;
    int counter = 1;
    int startTarget = 0;

	void Start( ) {
        aiController = this.gameObject.GetComponent< AIController >( ); 
        startNode = this.gameObject.transform.position;
        incrementY = startNode.y - depthGraphSearch;
        InitializeAIVariables( );
        targetGameObject = listOfTargets[startTarget].transform;
        StartAI( );
	}

    void Update( ){
        if( reachedTarget ){
            reachedTarget = false;
            startTarget++;
            if( startTarget < listOfTargets.Length ){
                Start( );
            }
        }
    }

    void InitializeAIVariables( ){
        openQueue = new PriorityQueue< float, PathFinderNodes >( );
        storedNodeList = new Dictionary< PathFinderNodes, Vector3 >( );
        closedNodeList = new List< PathFinderNodes >( );
        aiPath = new List< Vector3 >( );
        counter = 1;
        foundTarget = false;
    }

    void StartAI( ){
        CreateGraph( );
        CalculateGraphNodeValues( );
        StartAStarSearch( );
    }

    void CreateGraph( ){
        //Add x nodes to graph
        for (float i = startNode.x; i < (  targetGameObject.position.x ); i++ ){
            Vector3 nodeTransformPosition = new Vector3( i, incrementY, 0 );
            Vector3 inverseNodePosition = new Vector3(i, incrementY, 0);
            PathFinderNodes nodeObject = new PathFinderNodes( nodeTransformPosition);
            if( SmartGraph( nodeObject ) ){
                continue;
            }
            if( counter == 1 ){
                checkingNode = nodeObject;
            }
            targetNode = nodeObject;
            counter++;
            storedNodeList.Add( nodeObject, nodeTransformPosition );

        }
        if( incrementY <  ( ( targetGameObject.position.y ) ) ){
            incrementY ++;
            CreateGraph( ); //Recursively add new vertical rows
        }
    }

    bool SmartGraph( PathFinderNodes node ){
        RaycastHit2D upHit = Physics2D.Raycast( node.objectPosition,  Vector3.up, raycastSize );
        node.extraCostWeight = 10;
        if ( upHit.collider != null ){
            if (upHit.collider.gameObject.layer == 10  ){
                return true;
            }
        }
        return false;
    }

    void CalculateGraphNodeValues( ){
        CalculateAllHValues( );
        CreateNodeParentChildren( );
        checkingNode.CalculateFValues( );
        Debug.Log("Graph Size " + counter);
    }

    void CalculateAllHValues( ){
        foreach( KeyValuePair< PathFinderNodes, Vector3 > node in storedNodeList ){
            node.Key.heuristicValue = ManhattenHeuristic( node.Key );
            FindBaseMovementCost( node.Key );
        }
    }

    float ManhattenHeuristic( PathFinderNodes currenNodePosition ){
        Vector3 newPos = currenNodePosition.objectPosition;
        Vector3 endPos = targetGameObject.transform.position;
        float calculateHeuristic = ( Mathf.Abs( newPos.x - endPos.x ) + 
                                     Mathf.Abs( newPos.y - endPos.y ) );
        return calculateHeuristic;
    }

    void CreateNodeParentChildren( ){
        foreach ( KeyValuePair< PathFinderNodes, Vector3 > parentObject in storedNodeList ){
            foreach ( KeyValuePair< PathFinderNodes, Vector3 > childrenObjects in storedNodeList){
                float distanceBetweenObjects = ParentChildNodeDistance(childrenObjects.Key.objectPosition,
                                               parentObject.Key.objectPosition);
                if ( distanceBetweenObjects == 1 ){
                    if (parentObject.Key.objectPosition.x > childrenObjects.Key.objectPosition.x)
                    {
                        parentObject.Key.leftNode = childrenObjects.Key;
                    }
                    if (parentObject.Key.objectPosition.x < childrenObjects.Key.objectPosition.x){
                        parentObject.Key.rightNode = childrenObjects.Key;
                    }
                    if (parentObject.Key.objectPosition.y > childrenObjects.Key.objectPosition.y){
                        parentObject.Key.downNode = childrenObjects.Key;
                    }
                    if (parentObject.Key.objectPosition.y < childrenObjects.Key.objectPosition.y){
                        parentObject.Key.upNode = childrenObjects.Key;
                    }
                }
            }
        }
    }

    void FindBaseMovementCost( PathFinderNodes node ){
        RaycastHit2D rightHit = Physics2D.Raycast( node.objectPosition,  Vector3.right, raycastSize );
        RaycastHit2D leftHit = Physics2D.Raycast(node.objectPosition, Vector3.left, raycastSize );
        RaycastHit2D downHit = Physics2D.Raycast(node.objectPosition, Vector3.down, raycastSize );
        node.extraCostWeight = 10;
        if ( rightHit.collider != null ){
            if (rightHit.collider.gameObject.layer == 9  ){
                node.extraCostWeight = 20;
            }
        }
        else if ( leftHit.collider != null ){
            if( leftHit.collider.gameObject.layer == 9 ){
                node.extraCostWeight = 20;
            }
        }
        else if ( downHit.collider  ){
            if( downHit.collider.gameObject.layer == 8 ){
                node.extraCostWeight = -10;
            }
            if( downHit.collider.gameObject.layer == 9 ){
                node.extraCostWeight = 20;
            }
        }        
    }

    float ParentChildNodeDistance(Vector3 firstVector, Vector3 secondVector){
        float radius = Vector3.Distance(firstVector, secondVector);
        return radius;
    }

    void StartAStarSearch( ){
        FindPath( );
        while( openQueue != null && !openQueue.IsEmpty ){
            FindPath( );
        }
    }

    void FindPath( ){
        if ( checkingNode.upNode != null ){
            DetermineNodeValues(checkingNode, checkingNode.upNode);
        }
        if ( checkingNode.downNode != null ){
           DetermineNodeValues(checkingNode, checkingNode.downNode);
        }
        if ( checkingNode.rightNode != null ){
            DetermineNodeValues(checkingNode, checkingNode.rightNode);
        }
        if ( checkingNode.leftNode != null ){
            DetermineNodeValues(checkingNode, checkingNode.leftNode);
        }
        if( !foundTarget ){
            closedNodeList.Add( checkingNode );
            checkingNode.isClosed = true;
            checkingNode.onOpenList = false;
            if( checkingNode == openQueue.Peek( ).Value ){
                openQueue.DequeueValue( );
            }
            if( openQueue.IsEmpty ){
                Debug.LogError( "AI Path Cannot be found: Invalid Path" );
            } else {
                checkingNode = openQueue.Peek( ).Value;
            }
        } else {
            TraceBackPath( );
        }
    }

    void DetermineNodeValues( PathFinderNodes currentNode, PathFinderNodes testing ){
        if ( testing == null ) {
            return;
        }
        if ( testing == targetNode ) {
            targetNode.parentNode = currentNode;
            openQueue = null;
            foundTarget = true;
            return;
        }
        if( !testing.isClosed && openQueue != null){
            if( !testing.onOpenList ){
                testing.parentNode = currentNode;
                testing.movementCost = currentNode.movementCost + testing.extraCostWeight;
                testing.CalculateFValues( );
                testing.onOpenList = true;
                openQueue.Enqueue( testing.totalCost, testing );
            }
        }
    }

    void TraceBackPath( ){
        CreateAIPath( );
        StartCoroutine(DebugGraph());
    }

    void CreateAIPath( ){
        PathFinderNodes nodes = targetNode;
        do{
            aiPath.Add( nodes.objectPosition );
            nodes = nodes.parentNode;    
        } while ( nodes != null );
        aiPath.Reverse( );
        aiController.StartAIMovement( );
    }

    //Debugger
    IEnumerator DebugGraph( ){
        foreach ( KeyValuePair< PathFinderNodes, Vector3 > node in storedNodeList ){
            if( aiPath.Contains( node.Key.objectPosition ) && debugPath ){
                Instantiate( pathingGameObject, node.Key.objectPosition, Quaternion.identity );
            }
            if( debugGraph ){
                Instantiate( graphGameObject, node.Key.objectPosition, Quaternion.identity );
            }
            yield return new WaitForSeconds(0.009f);
        }
    }
}
