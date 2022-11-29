using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Enemy : MonoBehaviour
{
    float hp;
    [SerializeField] float HP { get { return hp; } set { hp = value; } }
    [SerializeField] Animator enemyAnimator;
    [SerializeField] Rigidbody rb;
    [SerializeField] Rigidbody[] rbs;
    [SerializeField] Collider[] cols;
    [SerializeField] Rigidbody spineRb;
    [SerializeField] Transform spine;
    [SerializeField] Collider col;
    private int hash_Hit;
    void Start()
    {
        hp = 15;
        spine = enemyAnimator.GetBoneTransform(HumanBodyBones.Spine);
        spineRb = spine.GetComponent<Rigidbody>();

        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>();
        rbs = GetComponentsInChildren<Rigidbody>();

        SetChildRigidbody(true);
        SetChildColider(false);

        col = transform.GetComponent<Collider>();
        col.enabled = true;

        hash_Hit = Animator.StringToHash("Hit");
    }
    [SerializeField] float power = 0.001f;
    public void GotHit(float dmg, Vector3 dir)
    {
        if (!isDead)
        {
            HP -= dmg;
            print("체력: " + HP);
            print("dir: " + dir);
            enemyAnimator.SetTrigger(hash_Hit);
            if (HP <= 0)
                Die(dir);
        }
    }

    bool isDead = false;

    public void Die(Vector3 dir)
    {
        isDead = true;
        enemyAnimator.enabled = false;
        SetChildRigidbody(false);
        SetChildColider(true);
        spineRb.AddForce(dir * power);

        Destroy(this.gameObject, 3f);
    }

    void SetChildRigidbody(bool state)
    {
        rbs = GetComponentsInChildren<Rigidbody>();
        foreach (var item in rbs)
            item.isKinematic = state;
    }

    void SetChildColider(bool state)
    {
        cols = GetComponentsInChildren<Collider>();
        foreach (var item in cols)
            item.enabled = state;
    }
}
