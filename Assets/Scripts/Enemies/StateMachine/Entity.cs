using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public FiniteStateMachine stateMachine; // stateMachine
    public D_Entity entityData;
    public int facingDirection { get; private set; }
    public Rigidbody2D rb { get; private set; }
    public Animator animator { get; private set; }
    public GameObject aliveGO { get; private set; }
    public AnimationToStateMachine atsm { get; private set; }
    public int lastDamageDirection { get; private set; }

    [SerializeField] private Transform
        wallCheck, ledgeCheck, playerCheck, groundCheck;

    private float currentHealth, currentStunResistance, lastDamageTime;


    private Vector2 velocityWorkspace;

    protected bool isStunned, isDead;

    public virtual void Start()
    {
        facingDirection = 1;
        currentHealth = entityData.maxHealth;
        currentStunResistance = entityData.stunResistance;

        aliveGO = transform.Find("Alive").gameObject;
        rb = aliveGO.GetComponent<Rigidbody2D>();
        animator = aliveGO.GetComponent<Animator>();
        atsm = animator.GetComponent<AnimationToStateMachine>();

        stateMachine = new FiniteStateMachine(); // every entity will have its own stateMachine, that's an instance of a FiniteStateMachine
    }
    public virtual void Update()
    {
        stateMachine.currentState.LogicUpdate();

        if (Time.time >= lastDamageTime + entityData.stunRecoveryTime)
            ResetStunResistance();
    }
    public virtual void FixedUpdate() { stateMachine.currentState.PhysicsUpdate(); }
    public virtual void SetVelocity(float velocity)
    {
        velocityWorkspace.Set(facingDirection * velocity, rb.velocity.y);
        rb.velocity = velocityWorkspace;
    }
    public virtual void SetVelocity(float velocity, Vector2 angle, int direction)
    {
        angle.Normalize();
        velocityWorkspace.Set(angle.x * velocity * direction, angle.y * velocity);
        rb.velocity = velocityWorkspace;
    }
    public virtual bool CheckWall() { return Physics2D.Raycast(wallCheck.position, aliveGO.transform.right, entityData.wallCheckDistance, entityData.whatIsGround); }
    public virtual bool CheckLedge() { return Physics2D.Raycast(ledgeCheck.position, Vector2.down, entityData.ledgeCheckDistance, entityData.whatIsGround); }
    public virtual bool CheckGround() { return Physics2D.OverlapCircle(groundCheck.position, entityData.groundCheckRadius, entityData.whatIsGround); }
    public virtual bool CheckPlayerInMinAgroRange() { return Physics2D.Raycast(playerCheck.position, aliveGO.transform.right, entityData.minAgroDistance, entityData.whatIsPlayer); }
    public virtual bool CheckPlayerInMaxAgroRange() { return Physics2D.Raycast(playerCheck.position, aliveGO.transform.right, entityData.maxAgroDistance, entityData.whatIsPlayer); }
    public virtual bool CheckPlayerInCloseRangeAction() { return Physics2D.Raycast(playerCheck.position, aliveGO.transform.right, entityData.closeRangeActionDistance, entityData.whatIsPlayer); }
    public virtual void DamageHop(float velocity) {
        velocityWorkspace.Set(rb.velocity.x, velocity);
        rb.velocity = velocityWorkspace;
    }
    public virtual void ResetStunResistance()
    {
        isStunned = false;
        currentStunResistance = entityData.stunResistance;
    }
    public virtual void Damage(AttackDetails attackDetails)
    {
        lastDamageTime = Time.time;
        
        currentHealth -= attackDetails.damageAmount;
        currentStunResistance -= attackDetails.stunDamageAmount;

        DamageHop(entityData.damageHopSpeed);

        Instantiate(entityData.hitParticle, aliveGO.transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));

        if (attackDetails.position.x > aliveGO.transform.position.x)
            lastDamageDirection = -1;
        else
            lastDamageDirection = 1;

        if (currentStunResistance <= 0)
            isStunned = true;

        if (currentHealth <= 0)
            isDead = true;
    }
    public virtual void Flip()
    {
        facingDirection *= -1;
        aliveGO.transform.Rotate(0, 180, 0);
    }
    public virtual void OnDrawGizmos()
    {
        Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)(Vector2.right * facingDirection * entityData.wallCheckDistance));
        Gizmos.DrawLine(ledgeCheck.position, ledgeCheck.position + (Vector3)(Vector2.down * entityData.ledgeCheckDistance));
        Gizmos.DrawWireSphere(playerCheck.position + (Vector3)(Vector2.right * entityData.closeRangeActionDistance), .2f);
        Gizmos.DrawWireSphere(playerCheck.position + (Vector3)(Vector2.right * entityData.minAgroDistance), .2f);
        Gizmos.DrawWireSphere(playerCheck.position + (Vector3)(Vector2.right * entityData.maxAgroDistance), .2f);
    }
}