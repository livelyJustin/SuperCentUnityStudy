using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] float maxHp = 15;
    float hp;
    [SerializeField] float HP { get { return hp; } set { hp = value; } }
    [SerializeField] Animator enemyAnimator;
    private Rigidbody rb;
    private Rigidbody spineRb;
    private Transform spine;
    private Collider col;
    private int hash_Hit;
    private bool isDead = false;
    [SerializeField] ParticleSystem ps_Blood;
    [SerializeField] float power = 0.001f;
    // RagDoll
    private Rigidbody[] rbs;
    private Collider[] cols;
    // HP UI Field
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image img_HP;
    [SerializeField] private Image img_HP_Red; // 빌보드 처리 해주기
    WaitForSeconds wfs3sec = new WaitForSeconds(3);
    WaitForSeconds wfs05sec = new WaitForSeconds(0.5f);
    [SerializeField] float fadeTime = 2;
    // NavMesh
    [SerializeField] private NavMeshAgent agent;
    private Vector3 nextPos;

    void Start()
    {
        hp = maxHp;
        EnemyManager.instance.Enemy_Count++;
        spine = enemyAnimator.GetBoneTransform(HumanBodyBones.Spine);
        spineRb = spine.GetComponent<Rigidbody>();

        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(); // reset 함수로 받아오기
        rbs = GetComponentsInChildren<Rigidbody>(); // 꺼주지 않으면 레그돌이 안 되는 것 처럼 보여도 물리 연산이 모두 일어남

        SetChildRigidbody(true);
        SetChildColider(false);

        col = transform.GetComponent<Collider>();
        col.enabled = true;

        hash_Hit = Animator.StringToHash("Hit");
    }

    public void GotHit(float dmg, Vector3 dir)
    {
        if (!isDead)
        {
            HP -= dmg;
            // print("체력: " + HP);
            // print("dir: " + dir);
            enemyAnimator.SetTrigger(hash_Hit);

            agent.isStopped = true;
            if (HP <= 0)
                Die(dir);
            StartCoroutine(DownHP(dmg));
            // else
        }
    }

    IEnumerator DownHP(float dmg)
    {
        canvasGroup.alpha = 1;
        float tagetValue = (img_HP.fillAmount - (MathF.Abs(dmg) / maxHp)) <= 0 ? 0 : (img_HP.fillAmount - (MathF.Abs(dmg) / maxHp));
        img_HP.fillAmount = tagetValue;
        yield return wfs05sec;
        float timer = 0;

        // HP UI Red Fade Down
        while (timer < 1.5f)
        {
            yield return null;
            timer += Time.deltaTime;
            img_HP_Red.fillAmount = Mathf.Lerp(img_HP_Red.fillAmount, tagetValue, 0.005f); // deltatime 값이 고려되지 않아 프레임 따라 속도가 달라질 수 있음
        }
        // agent.isStopped = false;

        // HP UI Fade Down
        yield return null;
        if (isDead == true)
        {
            timer = 0;
            while (timer < 1f)
            {
                yield return null;
                timer += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, 0.02f);
            }
        }
    }


    public void Die(Vector3 dir)
    {
        isDead = true;
        agent.enabled = false;

        foreach (var item in rbs)
            item.gameObject.layer = 0;

        enemyAnimator.enabled = false;
        SetChildRigidbody(false);
        SetChildColider(true);
        spineRb.AddForce(dir * power);

        EnemyManager.instance.CountDownEnemy();
        StartCoroutine(FadeDown());
    }


    IEnumerator FadeDown()
    {
        yield return wfs3sec;
        float timer = 0;
        foreach (var item in rbs)
            item.useGravity = false;

        foreach (var item in cols)
            item.isTrigger = true;

        while (timer < 3)
        {
            yield return null;
            timer += Time.deltaTime;
            Vector3 originTr = transform.position;
            Vector3 tagetPos = new Vector3(originTr.x, originTr.y - 3f, originTr.z);
            transform.position = Vector3.Lerp(originTr, tagetPos, fadeTime * Time.deltaTime);
        }

        this.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isDead && !agent.pathPending && !agent.hasPath)
        {
            if (GetRandomPos(transform.position, 5f, out nextPos))
            {
                agent.SetDestination(nextPos);
            }
        }
    }

    bool GetRandomPos(Vector3 center, float range, out Vector3 randomPoint)
    {
        NavMeshHit hit;
        for (int i = 0; i < 20; i++)
        {
            Vector3 ranPos = center + UnityEngine.Random.insideUnitSphere * range;
            if (NavMesh.SamplePosition(ranPos, out hit, 1f, NavMesh.AllAreas))
            {
                randomPoint = hit.position;
                return true;
            }
        }
        randomPoint = Vector3.zero;
        return false;
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
