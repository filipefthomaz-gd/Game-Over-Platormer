using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collision : MonoBehaviour
{

    [Header("Layers")]
    public LayerMask groundLayer;
    private Collision coll;

    [Space]

    public bool onGround;
    public bool onWall;
    public bool onRightWall;
    public bool onLeftWall;
    public int wallSide;


    [Space]

    [Header("Collision")]
    public Vector2 collisionGroundSize = new Vector2(0, 0);
    public Vector2 collisionWallSize = new Vector2(0, 0);
    public float collisionRadius = 0.25f;
    public Vector2 bottomOffset, rightOffset, leftOffset;
    private Color debugCollisionColor = Color.red;

    // Update is called once per frame
    void Update()
    {
        onGround = Physics2D.OverlapBox((Vector2)transform.position + bottomOffset, collisionGroundSize, 0, groundLayer);
        onWall = Physics2D.OverlapBox((Vector2)transform.position + rightOffset, collisionWallSize, 0, groundLayer)
            || Physics2D.OverlapBox((Vector2)transform.position + leftOffset, collisionWallSize, 0,groundLayer);

        onRightWall = Physics2D.OverlapBox((Vector2)transform.position + rightOffset, collisionWallSize,0, groundLayer);
        onLeftWall = Physics2D.OverlapBox((Vector2)transform.position + leftOffset, collisionWallSize,0, groundLayer);

        wallSide = onRightWall ? -1 : 1;
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        var positions = new Vector2[] { bottomOffset, rightOffset, leftOffset };

        Gizmos.DrawWireCube((Vector2)transform.position + bottomOffset, collisionGroundSize);
        Gizmos.DrawWireCube((Vector2)transform.position + rightOffset, collisionWallSize);
        Gizmos.DrawWireCube((Vector2)transform.position + leftOffset, collisionWallSize);
    }
}