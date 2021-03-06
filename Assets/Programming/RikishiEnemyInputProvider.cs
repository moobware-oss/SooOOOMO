﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(RikishiController))]
[RequireComponent(typeof(ParameterizedFlabbiness))]
public class RikishiEnemyInputProvider : MonoBehaviour
{
    public Transform Dohyo;
    public float DohyoRadius = 1f;

    public float PersonalSpaceBubbleRadius = 1f;
    public float TrackSpeed = 1f;
    public float StrikeDelay = 1f;
    private Transform trackingTransform;
    private float trackingTransformStartY;

    Transform mainCameraTransform;
    Transform playerTransform;

    RikishiController rikishiController;
    ParameterizedFlabbiness zeFlabben;
    Scoreboard scoreboard;

    Animator animator;

    Root behaviorTree = BT.Root();

    private Vector3 fromPlayerToMe;
    private float distanceFromPlayerToMeSquared;
    private float distanceFromMeToCenterOfDohyo;

    void Start()
    {
        mainCameraTransform = Camera.main.transform;
        playerTransform = GameObject.FindWithTag("Player").transform;
        rikishiController = GetComponent<RikishiController>();
        zeFlabben = GetComponent<ParameterizedFlabbiness>();
        scoreboard = FindObjectOfType<Scoreboard>();
        animator = GetComponent<Animator>();
        zeFlabben.Flabbiness = scoreboard.GetEnemyScore() * 34f;
        rikishiController.shoveForce += zeFlabben.Flabbiness * 3;

        trackingTransform = new GameObject().transform;
        trackingTransform.position = playerTransform.position;
        trackingTransformStartY = trackingTransform.position.y;

        // BT class from the 2D Game Kit
        // https://learn.unity.com/tutorial/2d-game-kit-advanced-topics#5c7f8528edbc2a002053b77a
        //behaviorTree.OpenBranch(

        //    //BT.If(() => { return m_EnemyBehaviour.Target != null; }).OpenBranch(
        //    //    BT.Call(m_EnemyBehaviour.CheckTargetStillVisible),
        //    //    BT.Call(m_EnemyBehaviour.OrientToTarget),
        //    //    BT.Trigger(m_Animator, "Shooting"),
        //    //    BT.Call(m_EnemyBehaviour.RememberTargetPos),
        //    //    BT.WaitForAnimatorState(m_Animator, "Attack")
        //    //),

        //    BT.If(TestPlayerInStrikingDistance).OpenBranch(
        //        BT.Trigger(animator, "Stop Dancing"),
        //        BT.Call(Attack),
        //        BT.Wait(StrikeDelay)
        //    ),

        //    BT.Call(LockOntoTarget)
        //);

        behaviorTree.OpenBranch(
            BT.RandomSequence(new int[] { 3, 1 }).OpenBranch(
                BT.Sequence().OpenBranch(
                    //BT.Log("Chasing Player"),
                    BT.RunCoroutine(GetWithinStrikingDistanceOfThePlayer),
                    BT.Call(Attack),
                    BT.RunCoroutine(GetReadyForNextAttack)
                ),
                BT.Sequence().OpenBranch(
                    BT.While(() => PlayerIsInStrikingDistance() == false).OpenBranch(
                        //BT.Log("Avoiding Player"),
                        BT.Call(() => ImOnTheEdgeOfTheDohyo()),
                        BT.Call(SitStill),
                        BT.Call(LockOntoTarget)
                    )
                )
            )
        );
    }

    //void OnDrawGizmos()
    //{
    //    if (trackingTransform == null)
    //    {
    //        return;
    //    }
    //    // Draw a semitransparent blue cube at the transforms position
    //    Gizmos.color = Color.black;
    //    Gizmos.DrawCube(trackingTransform.position, Vector3.one / 4);
    //}

    IEnumerator<BTState> GetWithinStrikingDistanceOfThePlayer()
    {
        while (PlayerIsInStrikingDistance() == false)
        {
            LockOntoTarget();

            if (ImOnTheEdgeOfTheDohyo() == false)
            {
                MoveToWardTarget();
            } else
            {
                SitStill();
            }
            yield return BTState.Continue;
        }

        yield return BTState.Success;
    }

    IEnumerator<BTState> GetReadyForNextAttack()
    {
        var canAttackAgainSeconds = Time.time + 0.8333; // 5/6th of a second
        while (Time.time < canAttackAgainSeconds)
        {
            LockOntoTarget();

            if (ImOnTheEdgeOfTheDohyo() == false)
            {
                MoveToWardTarget();
            } else
            {
                SitStill();
            }
            yield return BTState.Continue;
        }

        yield return BTState.Success;
    }


    void SitStill()
    {
        rikishiController.Move(Vector3.zero);
    }

    bool PlayerIsInStrikingDistance()
    {
        return distanceFromPlayerToMeSquared <= PersonalSpaceBubbleRadius;
    }

    bool ImOnTheEdgeOfTheDohyo()
    {
        return distanceFromMeToCenterOfDohyo >= DohyoRadius;
    }

    void Attack()
    {
        rikishiController.AttemptShove();
    }

    void LockOntoTarget()
    {
        var idealTarget = new Vector3(playerTransform.position.x, trackingTransformStartY, playerTransform.position.z);
        trackingTransform.position = Vector3.Lerp(trackingTransform.position, idealTarget, Time.deltaTime * TrackSpeed);
        rikishiController.SetDesiredAimTarget(trackingTransform.position);
    }

    void MoveToWardTarget()
    {
        rikishiController.Move(fromPlayerToMe.normalized);
    }

    private void Update()
    {
        fromPlayerToMe = playerTransform.position - transform.position;
        distanceFromPlayerToMeSquared = fromPlayerToMe.sqrMagnitude;

        var fromMeToCenterOfDohyo = transform.position - Dohyo.position;
        distanceFromMeToCenterOfDohyo = fromMeToCenterOfDohyo.sqrMagnitude;
        behaviorTree.Tick();
    }
}

/*
 *
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BTAI;

namespace Gamekit2D
{
    public class SpitterBT : MonoBehaviour
    {
        Animator m_Animator;
        Damageable m_Damageable;
        Root m_Ai = BT.Root();
        EnemyBehaviour m_EnemyBehaviour;

        private void OnEnable()
        {
            m_EnemyBehaviour = GetComponent<EnemyBehaviour>();
            m_Animator = GetComponent<Animator>();

            m_Ai.OpenBranch(

                BT.If(() => { return m_EnemyBehaviour.Target != null; }).OpenBranch(
                    BT.Call(m_EnemyBehaviour.CheckTargetStillVisible),
                    BT.Call(m_EnemyBehaviour.OrientToTarget),
                    BT.Trigger(m_Animator, "Shooting"),
                    BT.Call(m_EnemyBehaviour.RememberTargetPos),
                    BT.WaitForAnimatorState(m_Animator, "Attack")
                ),

                BT.If(() => { return m_EnemyBehaviour.Target == null; }).OpenBranch(
                    BT.Call(m_EnemyBehaviour.ScanForPlayer)
                )
            );
        }

        private void Update()
        {
            m_Ai.Tick();
        }
    }
}
*/

/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BTAI;
using UnityEngine.Events;
using System;
using Gamekit2D;

public class MissileGolem : MonoBehaviour
#if UNITY_EDITOR
, BTAI.IBTDebugable
#endif
{
[System.Serializable]
public class BossRound
{
    public float platformSpeed = 1;
    public MovingPlatform[] platforms;
    public GameObject[] enableOnProgress;
    public int bossHP = 10;
    public int shieldHP = 10;
}

public Transform target;

public int laserStrikeCount = 2;
public float laserTrackingSpeed = 30.0f;
public float delay = 2;
public float beamDelay, grenadeDelay, lightningDelay, cleanupDelay, deathDelay;

public GameObject shield, beamLaser;
public GunnerProjectile projectile;
public Grenade grenade;
public GameObject lightning;
public Damageable damageable;
public float lightningTime = 1;
public Transform grenadeSpawnPoint;
public Vector2 grenadeLaunchVelocity;
[Space]
public BossRound[] rounds;
[Space]
public GameObject[] disableOnDeath;
public UnityEvent onDefeated;

[Header("Audio")]
public AudioClip bossDeathClip;
public AudioClip playerDeathClip;
public AudioClip postBossClip;
public AudioClip bossMusic;
[Space]
public RandomAudioPlayer stepAudioPlayer;
public RandomAudioPlayer laserFireAudioPlayer;
public RandomAudioPlayer grenadeThrowAudioPlayer;
public RandomAudioPlayer lightingAttackAudioPlayer;
public RandomAudioPlayer takingDamage;
public RandomAudioPlayer shieldUpAudioPlayer;
public RandomAudioPlayer shieldDownAudioPlayer;
[Space]
public AudioSource roundDeathSource;
public AudioClip startRound2Clip;
public AudioClip startRound3Clip;
public AudioClip deathClip;

[Header("UI")]
public Slider healthSlider;
public Slider shieldSlider;

bool onFloor = false;
int round = 0;

private int m_TotalHealth = 0;
private int m_CurrentHealth = 0;

//used to track target movement, to correct for it.
private Vector2 m_PreviousTargetPosition;


public void SetEllenFloor(bool onFloor)
{
    this.onFloor = onFloor;
}

Animator animator;
Root ai;
Vector3 originShieldScale;


void OnEnable()
{
    if (PlayerCharacter.PlayerInstance != null)
    {
        PlayerCharacter.PlayerInstance.damageable.OnDie.AddListener(PlayerDied);
    }
    originShieldScale = shield.transform.localScale;
    animator = GetComponent<Animator>();

    round = 0;

    ai = BT.Root();
    ai.OpenBranch(
        //First Round
        BT.SetActive(beamLaser, false),
        BT.Repeat(rounds.Length).OpenBranch(
            BT.Call(NextRound),
            //grenade enabled is true only on 2 and 3 round, so allow to just test if it's the 1st round or not here
            BT.If(GrenadeEnabled).OpenBranch(
                BT.Trigger(animator, "Enabled")
                ),
            BT.Wait(delay),
            BT.Call(ActivateShield),
            BT.Wait(delay),
            BT.While(ShieldIsUp).OpenBranch(
                BT.RandomSequence(new int[] { 1, 6, 4, 4 }).OpenBranch(
                    BT.Root().OpenBranch(
                        BT.Trigger(animator, "Walk"),
                        BT.Wait(0.2f),
                        BT.WaitForAnimatorState(animator, "Idle")
                        ),
                    BT.Repeat(laserStrikeCount).OpenBranch(
                        BT.SetActive(beamLaser, true),
                        BT.Trigger(animator, "Beam"),
                        BT.Wait(beamDelay),
                        BT.Call(FireLaser),
                        BT.WaitForAnimatorState(animator, "Idle"),
                        BT.SetActive(beamLaser, false),
                        BT.Wait(delay)
                    ),
                    BT.If(EllenOnFloor).OpenBranch(
                        BT.Trigger(animator, "Lightning"),
                        BT.Wait(lightningDelay),
                        BT.Call(ActivateLightning),
                        BT.Wait(lightningTime),
                        BT.Call(DeactivateLighting),
                        BT.Wait(delay)
                    ),
                    BT.If(GrenadeEnabled).OpenBranch(
                        BT.Trigger(animator, "Grenade"),
                        BT.Wait(grenadeDelay),
                        BT.Call(ThrowGrenade),
                        BT.WaitForAnimatorState(animator, "Idle")
                    )
                )
            ),
            BT.SetActive(beamLaser, false),
            BT.Trigger(animator, "Grenade", false),
            BT.Trigger(animator, "Beam", false),
            BT.Trigger(animator, "Lightning", false),
            BT.Trigger(animator, "Disabled"),
            BT.While(IsAlive).OpenBranch(BT.Wait(0))
        ),
        BT.Trigger(animator, "Death"),
        BT.SetActive(damageable.gameObject, false),
        BT.Wait(cleanupDelay),
        BT.Call(Cleanup),
        BT.Wait(deathDelay),
        BT.Call(Die),
        BT.Terminate()
    );

    BackgroundMusicPlayer.Instance.ChangeMusic(bossMusic);
    BackgroundMusicPlayer.Instance.Play();
    BackgroundMusicPlayer.Instance.Unmute(2.0f);

    //we aggregate the total health to set the slider to the proper value
    //(as the boss is actually "killed" every round and regenerated, we can't use directly its current health)
    for(int i = 0; i < rounds.Length; ++i)
    {
        m_TotalHealth += rounds[i].bossHP;
    }
    m_CurrentHealth = m_TotalHealth;

    healthSlider.maxValue = m_TotalHealth;
    healthSlider.value = m_TotalHealth;

    if (target != null)
        m_PreviousTargetPosition = target.transform.position;
}

private void OnDisable()
{
    if (PlayerCharacter.PlayerInstance != null)
    {
        PlayerCharacter.PlayerInstance.damageable.OnDie.RemoveListener(PlayerDied);
    }
}

void PlayerDied(Damager d, Damageable da)
{
    BackgroundMusicPlayer.Instance.PushClip(playerDeathClip);
}

void ActivateShield()
{
    shieldUpAudioPlayer.PlayRandomSound();

    shield.SetActive(true);
    shield.transform.localScale = Vector3.one * 0.01f;

    shieldSlider.GetComponent<Animator>().Play("BossShieldActivate");

    Damageable shieldDamageable = shield.GetComponent<Damageable>();

    //need to be set after enabled happen, otherwise enable reset health. That why we use round - 1, round was already advance at that point
    shieldDamageable.SetHealth(rounds[round - 1].shieldHP);
    shieldSlider.maxValue = rounds[round - 1].shieldHP;
    shieldSlider.value = shieldSlider.maxValue;
}

void FireLaser()
{
    laserFireAudioPlayer.PlayRandomSound();

    var p = Instantiate(projectile);
    var dir = -beamLaser.transform.right;
    p.transform.position = beamLaser.transform.position;
    p.initialForce = new Vector3(dir.x, dir.y) * 1000;
}

void ThrowGrenade()
{
    grenadeThrowAudioPlayer.PlayRandomSound();

    var p = Instantiate(grenade);
    p.transform.position = grenadeSpawnPoint.position;
    p.initialForce = grenadeLaunchVelocity;
}

bool GrenadeEnabled()
{
    return round > 1;
}

void ActivateLightning()
{
    lightingAttackAudioPlayer.PlayRandomSound();

    var p = Instantiate(lightning) as GameObject;
    p.transform.position = transform.position;
    Destroy(p, lightningTime);
}

void DeactivateLighting()
{
    lightingAttackAudioPlayer.Stop();
}

private void FixedUpdate()
{
    if (target != null)
    {
        m_PreviousTargetPosition = target.position;
    }
}

void Update()
{
    ai.Tick();
    if (beamLaser != null && target != null)
    {
        Vector2 targetMovement = (Vector2)target.position - m_PreviousTargetPosition;
        targetMovement.Normalize();
        Vector3 targetPos = target.position + Vector3.up * (1.0f + targetMovement.y * 0.5f);

        beamLaser.transform.rotation = Quaternion.RotateTowards(beamLaser.transform.rotation, Quaternion.Euler(0, 0, Vector3.SignedAngle(Vector3.left, targetPos - beamLaser.transform.position, Vector3.forward)), laserTrackingSpeed * Time.deltaTime);
    }

    shield.transform.localScale = Vector3.Lerp(shield.transform.localScale, originShieldScale, Time.deltaTime);
}

void Cleanup()
{
    shieldSlider.GetComponent<Animator>().Play("BossShieldDefeat");
    healthSlider.GetComponent<Animator>().Play("BossHealthDefeat");

    BackgroundMusicPlayer.Instance.ChangeMusic(postBossClip);
    BackgroundMusicPlayer.Instance.PushClip(bossDeathClip);

    roundDeathSource.clip = deathClip;
    roundDeathSource.loop = false;
    roundDeathSource.Play();

    foreach (var g in disableOnDeath)
        g.SetActive(false);
    shield.SetActive(false);
    beamLaser.SetActive(false);
}

void Die()
{
    onDefeated.Invoke();
}

bool EllenOnPlatform()
{
    return !onFloor;
}

bool EllenOnFloor()
{
    return onFloor;
}

bool IsAlive()
{
    var alive = damageable.CurrentHealth > 0;
    return alive;
}

bool IsNotAlmostDead()
{
    var alive = damageable.CurrentHealth > 1;
    return alive;
}

bool ShieldIsUp()
{
    return shield.GetComponent<Damageable>().CurrentHealth > 0;
}

void NextRound()
{
    damageable.SetHealth(rounds[round].bossHP);
    damageable.EnableInvulnerability(true);
    foreach (var p in rounds[round].platforms)
    {
        p.gameObject.SetActive(true);
        p.speed = rounds[round].platformSpeed;
    }
    foreach (var g in rounds[round].enableOnProgress)
    {
        g.SetActive(true);
    }
    round++;

    if (round == 2)
    {
        roundDeathSource.clip = startRound2Clip;
        roundDeathSource.loop = true;
        roundDeathSource.Play();
    }
    else if (round == 3)
    {
        roundDeathSource.clip = startRound3Clip;
        roundDeathSource.loop = true;
        roundDeathSource.Play();
    }
}

void Disabled()
{

}

void Enabled()
{

}

public void Damaged(Damager damager, Damageable damageable)
{
    takingDamage.PlayRandomSound();

    m_CurrentHealth -= damager.damage;
    healthSlider.value = m_CurrentHealth;
}

public void ShieldDown()
{
    shieldDownAudioPlayer.PlayRandomSound();
    damageable.DisableInvulnerability();
}

public void ShieldHit()
{
    shieldSlider.value = shield.GetComponent<Damageable>().CurrentHealth;
}

public void PlayStep()
{
    stepAudioPlayer.PlayRandomSound();
}

#if UNITY_EDITOR
public BTAI.Root GetAIRoot()
{
    return ai;
}
#endif
}

    */
