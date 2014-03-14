using UnityEngine;
using System.Collections;

public class AIController : MonoBehaviour {
    public float  aiSpeed = 0.13f;
    public int currentWayPoint = 0;
    //private SkeletonAnimation skeletonAnimation;
    private PathFinder aiPathFinder;
    private Vector3 velocity;
    private bool loop = true;
    private float jumpTimer = 0.0f;
    private float animationTimer = 0.0f;
    Vector3 cachedTransform;
    float startTime = 0.0f;
    float journeyLength;

    public void RestartAI( ){
        StopAllCoroutines( );
        this.gameObject.transform.position = cachedTransform;
        StartAIMovement( );
    }

	public void StartAIMovement( ){
        cachedTransform = this.gameObject.transform.position;
        //skeletonAnimation = this.GetComponentInChildren< SkeletonAnimation >( );
        aiPathFinder = this.GetComponent< PathFinder >( );
        StartCoroutine( MoveAI( ) );
        startTime = Time.time;
        journeyLength = Vector3.Distance( this.transform.position, 
                                          aiPathFinder.listOfTargets[0].transform.position );
	}
	
	IEnumerator MoveAI( ){
        for( int i = 1; i < aiPathFinder.aiPath.Count; i += 1 ){
            jumpTimer = 0.0f;
            for( float n = this.transform.position.x; n < aiPathFinder.aiPath[i].x; n += aiSpeed ){
                if( aiPathFinder.aiPath[i].y > this.transform.position.y && jumpTimer == 0.0f ){
                    this.gameObject.GetComponent< Rigidbody2D >( ).AddForce( new Vector2( 0, 200) );
                    jumpTimer += Time.deltaTime;
                    PlayJumpAnimation( );
                }
                Vector3 newPosition = new Vector3( n, this.transform.position.y, 0 );
                this.transform.position = newPosition;
                yield return new WaitForSeconds( 0.009f );
            }
        }
    }

    void PlayJumpAnimation( ){
        if ( animationTimer == 0.0f ){
            //skeletonAnimation.animationName = "Jumping";
            animationTimer += Time.deltaTime;
        } else {
            animationTimer += Time.deltaTime;
        }
        if ( animationTimer > 0.1f ){
            //skeletonAnimation.animationName = "";
            animationTimer = 0.0f;
        }
    }
}
