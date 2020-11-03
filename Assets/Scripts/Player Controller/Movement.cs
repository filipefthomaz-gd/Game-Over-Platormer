using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Collision coll;
    private SpriteRenderer spr;
    [HideInInspector]
    public Rigidbody2D rb;

    [Space]
    [Header("Basic Mov Stats")]
    public float speed = 10;
    public float accelerationFrames = 6;
    public float decelerationFrames = 3;

    [Space]
    [Header("Jump Stats")]
    public float maxHeight = 2;
    public float timeToMaxHeight = 0.5f;
    public float fallMultiplier = 1f;
    public float lowJumpMultiplier = 2f;


    [Space]
    [Header("Climb Stats")]
    public float slideSpeed = 5;
    public float climbSpeed = 3;
    public float wallJumpLerp = 10;

    [Space]
    [Header("Dash Stats")]
    public float dashSpeed = 20;


    [Space]
    [Header("Material")]
    public PhysicsMaterial2D material;

    [Space]
    [Header("Booleans")]
    public bool canMove;
    public bool wallGrab;
    public bool wallJumped;
    public bool wallSlide;
    public bool isDashing;

    [Space]

    private bool groundTouch;
    private bool hasDashed;
    private float jumpForce;
    private float playerSize;

    public int side = 1;

    public float testHighOrder = 2;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        coll = GetComponent<Collision>();
        spr = GetComponent<SpriteRenderer>();

        //Add material to RigidBody (NoFriction Material to bypass collision frictions)
        rb.sharedMaterial = material;

        //Maximum height jump will not be in Unity units, but player units (2 means it'll jump 200% + its height [300%])
        playerSize = spr.bounds.size.y;
    }

    private void FixedUpdate()
    {
        //////////
        ///WALK///
        //////////

        //Horizontal Movement happens on FixedUpdate to control frame acceleration and deceleration
        float horizontalRaw = Input.GetAxisRaw("Horizontal");
        float verticalRaw = Input.GetAxisRaw("Vertical");
        Vector2 dirRaw = new Vector2(horizontalRaw, verticalRaw);

        if (!wallGrab)
            Move(dirRaw);
    }

    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        float horizontalRaw = Input.GetAxisRaw("Horizontal");
        float verticalRaw = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(horizontal, vertical);
        Vector2 dirRaw = new Vector2(horizontalRaw, verticalRaw);

        //////////
        ///JUMP///
        //////////

        /*---

           Jump Force and Gravity come from y = y0 + v0*t + 1/2 a t^2

           Delta Y = jF * t - 1/2 g t^2
           tForYMax => Delta Y' = 0 => jF = g tForYMax => tForYMax = jF/g
           yMax = (jF - 1/2 g * tForYMax) * tForYMax

       ---*/

        jumpForce = 2 * maxHeight * playerSize / timeToMaxHeight;
        if (!wallGrab && !isDashing)
            rb.gravityScale = 2 * maxHeight * playerSize / (timeToMaxHeight * timeToMaxHeight * -Physics2D.gravity.y);
        else
            rb.gravityScale = 0;

        //Jump happens only when onGround (onGround is defined on Collision.cs)
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (coll.onGround)
                Jump(Vector2.up, false);
        }

        /*--- Jump special funcions

            First expression adds aditional gravity when descending from jump to more stability;
            Disable by setting fallMultiplier = 1

            Second expression adds extra gravity when ascending if Jump button is unpressed;
            This will reduce the jump height if Jump is unpressed, and maximize the height if Jump is pressed until max Height is reached;

         ---*/

        if (!isDashing)
        {
            if (rb.velocity.y < 0)
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;

            else if (rb.velocity.y > 0 && !Input.GetKey(KeyCode.C))
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        //////////
        ///DASH///
        //////////

        if (Input.GetKeyDown(KeyCode.X))
        {
            Dash(dir);
        }



        //////////
        ///WALL///
        //////////

        //Climbing on Wall (Activate and Deactivate)
        if (coll.onWall && Input.GetKey(KeyCode.Z))
            WallMove(dirRaw);

        if (Input.GetKeyUp(KeyCode.Z))
        {
            wallGrab = false;
            wallSlide = false;
        }


        if (coll.onWall && !coll.onGround)
        {
            if (horizontalRaw != 0 && !wallGrab && rb.velocity.y <= 0)
            {
                wallSlide = true;
                WallSlide();
            }
        }
    }

    //Gravity function; Not being used - instead RB Gravity is used with scale defined above;
    //Use this function if a custom Gravity is needed to more control;
    private void Gravity()
    {
        if (!coll.onGround)
            rb.velocity += Physics2D.gravity * Time.deltaTime;
    }

    //Move Function
    private void Move(Vector2 input)
    {
        //Input is -1, 1, or 0 (getting the Raw Input)

        /*---
         
            Movement, Acceleration and Deceleration
            Linear acceleration and decelaration implemented (during acceleration and decelaration Frame variables
         
         ---*/

        if (input.x != 0)
        {
            if (Mathf.Abs(rb.velocity.x) < speed)
                rb.velocity += new Vector2(input.x * speed / accelerationFrames, 0);

            if (Mathf.Abs(rb.velocity.x) >= speed)
                rb.velocity = new Vector2(input.x * speed, rb.velocity.y);
        }

        else
        {
            if (rb.velocity.x > 0)
            {
                rb.velocity -= new Vector2(speed / decelerationFrames, 0);
                if (rb.velocity.x <= 0)
                    rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else if (rb.velocity.x < 0)
            {
                rb.velocity += new Vector2(speed / decelerationFrames, 0);
                if (rb.velocity.x >= 0)
                    rb.velocity = new Vector2(0, rb.velocity.y);
            }
            else
                rb.velocity = new Vector2(0, rb.velocity.y);

        }
        //rb.velocity = new Vector2(dir.x * speed, rb.velocity.y);
    }

    //Jump Function where a certain velocity is added to the character
    private void Jump(Vector2 dir, bool smth)
    {
        rb.velocity = new Vector2(rb.velocity.x, 0);
        rb.velocity += dir * jumpForce;
    }

    private void Dash(Vector2 dir)
    {
        wallGrab = false;
        wallSlide = false;
        rb.velocity = new Vector2(0, 0);
        rb.velocity += dir.normalized * dashSpeed;
    }

    private void WallSlide()
    {
        /*if (coll.wallSide != side)
            anim.Flip(side * -1);*/

        /*if (!canMove)
            return;*/

        bool pushingWall = false;
        if ((rb.velocity.x > 0 && coll.onRightWall) || (rb.velocity.x < 0 && coll.onLeftWall))
        {
            pushingWall = true;
        }
        float push = pushingWall ? 0 : rb.velocity.x;

        rb.velocity = new Vector2(push, -slideSpeed);
    }

    private void WallMove(Vector2 dir)
    {
        wallGrab = true;
        wallSlide = false;

        float speedModifier = dir.y > 0 ? .5f : 1;

        rb.velocity = new Vector2(rb.velocity.x, dir.y * climbSpeed * speedModifier);
    }

}