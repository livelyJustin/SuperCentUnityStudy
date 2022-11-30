using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class Enemy : MonoBehaviour
{
    [SerializeField] float maxHp = 15;
    float hp;
    [SerializeField] float HP { get { return hp; } set { hp = value; } }
    [SerializeField] Animator enemyAnimator;
    private Rigidbody rb;
    private Rigidbody[] rbs;
    private Collider[] cols;
    private Rigidbody spineRb;
    private Transform spine;
    private Collider col;
    private int hash_Hit;
    bool isDead = false;
    [SerializeField] float power = 0.001f;
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Image img_HP;
    [SerializeField] private Image img_HP_Red; // 빌보드 처리 해주기
    WaitForSeconds wfs3sec = new WaitForSeconds(3);
    [SerializeField] float fadeTime = 2;

    private void Awake()
    {

    }
    void Start()
    {
        hp = maxHp;
        EnemyManager.instance.Enemy_Count++;
        spine = enemyAnimator.GetBoneTransform(HumanBodyBones.Spine);
        spineRb = spine.GetComponent<Rigidbody>();

        rb = GetComponent<Rigidbody>();
        cols = GetComponentsInChildren<Collider>(); // reset 함수로 받아오기
        rbs = GetComponentsInChildren<Rigidbody>();

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
            print("체력: " + HP);
            // print("dir: " + dir);
            enemyAnimator.SetTrigger(hash_Hit);
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
        yield return new WaitForSeconds(0.5f);
        float timer = 0;

        while (timer < 1.5f)
        {
            yield return null;
            timer += Time.deltaTime;
            img_HP_Red.fillAmount = Mathf.Lerp(img_HP_Red.fillAmount, tagetValue, 0.005f); // deltatime 값이 고려되지 않아 프레임 따라 속도가 달라질 수 있음
        }

        yield return null;
        timer = 0;
        while (timer < 1f)
        {
            yield return null;
            timer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, 0.02f);
        }
    }


    public void Die(Vector3 dir)
    {
        isDead = true;
        // canvasGroup.alpha = 0;
        enemyAnimator.enabled = false;
        SetChildRigidbody(false);
        SetChildColider(true);
        spineRb.AddForce(dir * power);

        // Destroy(this.gameObject, 3f);
        EnemyManager.instance.CountDownEnemy();
        StartCoroutine(FadeDown());
    }


    IEnumerator FadeDown()
    {
        yield return wfs3sec;
        float timer = 0;
        // SetChildColider(false);
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
