using UnityEngine;
using System.Collections;

public class SimpleEnemyAnimator : MonoBehaviour
{
    [Header("Animation Clips")]
    public string walkAnimationName;
    public string attackAnimationName;
    public string hurtAnimationName;
    public string deathAnimationName;
    
    [Header("Animation Settings")]
    [Range(0.01f, 2f)]
    public float walkAnimationRefreshRate = 0.05f;
    
    [Header("Debug")]
    public bool showDebugLogs = false;
    public bool isBigEnemy = false; // Set this to true for Big Enemy

    private Animator animator;
    private Coroutine walkingCoroutine;
    private bool isDead = false;
    private bool isAttacking = false;
    
    // Animation state tracking
    private string currentAnimationName = "";
    private float animationStartTime = 0f;
    
    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator found on " + gameObject.name);
            enabled = false;
        }
    }
    
    void Start()
    {
        // Force animations to loop
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips)
            {
                if (clip != null)
                {
                    clip.wrapMode = WrapMode.Loop;
                    if (showDebugLogs)
                        Debug.Log("Set " + clip.name + " to loop mode");
                }
            }
        }
        
        // Start with walking animation
        PlayWalkAnimation();
    }
    
    void Update()
    {
        // Extra check to ensure animations keep playing
        if (isBigEnemy && !isDead && !isAttacking)
        {
            // If walking animation has been playing for more than 1 second, refresh it
            if (currentAnimationName == walkAnimationName && Time.time - animationStartTime > 1.0f)
            {
                PlayWalkAnimation();
            }
        }
    }
    
    public void PlayWalkAnimation()
    {
        if (isDead || animator == null) return;
        
        // Stop any existing walking refresh coroutine
        if (walkingCoroutine != null)
        {
            StopCoroutine(walkingCoroutine);
        }
        
        // Play the walk animation
        if (!string.IsNullOrEmpty(walkAnimationName))
        {
            animator.Play(walkAnimationName, 0, 0);
            currentAnimationName = walkAnimationName;
            animationStartTime = Time.time;
            
            if (showDebugLogs)
                Debug.Log("[" + Time.time + "] Playing walk animation: " + walkAnimationName);
        }
        
        // Start a new walking coroutine
        walkingCoroutine = StartCoroutine(KeepWalkingAnimationPlaying());
    }
    
    private IEnumerator KeepWalkingAnimationPlaying()
    {
        while (!isDead && !isAttacking)
        {
            // Wait before refreshing
            yield return new WaitForSeconds(walkAnimationRefreshRate);
            
            // Play the walk animation again to ensure it keeps playing
            if (!string.IsNullOrEmpty(walkAnimationName))
            {
                animator.Play(walkAnimationName, 0, 0.1f);  // Start slightly into the animation to avoid visual "pops"
                currentAnimationName = walkAnimationName;
                animationStartTime = Time.time;
                
                if (showDebugLogs)
                    Debug.Log("[" + Time.time + "] Refreshing walk animation: " + walkAnimationName);
            }
        }
    }
    
    public void PlayAttackAnimation(float duration = 0.5f)
    {
        if (isDead || animator == null) return;
        
        isAttacking = true;
        
        // Stop walking animation while attacking
        if (walkingCoroutine != null)
        {
            StopCoroutine(walkingCoroutine);
            walkingCoroutine = null;
        }
        
        // Play the attack animation
        if (!string.IsNullOrEmpty(attackAnimationName))
        {
            animator.Play(attackAnimationName, 0, 0);
            currentAnimationName = attackAnimationName;
            animationStartTime = Time.time;
            
            if (showDebugLogs)
                Debug.Log("[" + Time.time + "] Playing attack animation: " + attackAnimationName);
        }
        
        // Resume walking after attack completes
        StartCoroutine(ResumeWalkingAfterDelay(duration));
    }
    
    private IEnumerator ResumeWalkingAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        isAttacking = false;
        
        if (!isDead)
        {
            PlayWalkAnimation();
        }
    }
    
    public void PlayHurtAnimation()
    {
        if (isDead || animator == null) return;
        
        // Stop walking animation temporarily
        if (walkingCoroutine != null)
        {
            StopCoroutine(walkingCoroutine);
            walkingCoroutine = null;
        }
        
        // Play the hurt animation
        if (!string.IsNullOrEmpty(hurtAnimationName))
        {
            animator.Play(hurtAnimationName, 0, 0);
            currentAnimationName = hurtAnimationName;
            animationStartTime = Time.time;
            
            if (showDebugLogs)
                Debug.Log("[" + Time.time + "] Playing hurt animation: " + hurtAnimationName);
        }
        else if (!string.IsNullOrEmpty(attackAnimationName))
        {
            // Fall back to attack animation if no hurt animation
            animator.Play(attackAnimationName, 0, 0);
            currentAnimationName = attackAnimationName;
            animationStartTime = Time.time;
            
            if (showDebugLogs)
                Debug.Log("[" + Time.time + "] Playing attack as hurt animation: " + attackAnimationName);
        }
        
        // Resume walking after hurt completes
        StartCoroutine(ResumeWalkingAfterDelay(0.5f));
    }
    
    public void PlayDeathAnimation()
    {
        if (isDead || animator == null) return;
        
        isDead = true;
        isAttacking = false;
        
        // Stop any existing walking refresh coroutine
        if (walkingCoroutine != null)
        {
            StopCoroutine(walkingCoroutine);
            walkingCoroutine = null;
        }
        
        // Play the death animation
        if (!string.IsNullOrEmpty(deathAnimationName))
        {
            animator.Play(deathAnimationName, 0, 0);
            currentAnimationName = deathAnimationName;
            animationStartTime = Time.time;
            
            if (showDebugLogs)
                Debug.Log("[" + Time.time + "] Playing death animation: " + deathAnimationName);
        }
        else if (!string.IsNullOrEmpty(attackAnimationName))
        {
            // Fall back to attack animation if no death animation
            animator.Play(attackAnimationName, 0, 0);
            currentAnimationName = attackAnimationName;
            animationStartTime = Time.time;
            
            if (showDebugLogs)
                Debug.Log("[" + Time.time + "] Playing attack as death animation: " + attackAnimationName);
        }
    }
}