using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    [SerializeField] Transform gunPos;
    [SerializeField] float speed;
    [SerializeField] float maxTimer = 1;
    [SerializeField] ParticleSystem[] ps_Hits;
    [SerializeField] ParticleSystem ps_Bullet;
    WaitForSeconds wfs05sec = new WaitForSeconds(0.5f);
    WaitForSeconds wfs01sec = new WaitForSeconds(0.1f);

    bool isGoing = false;
    bool isEffectOn = false;
    public Collider myColider;
    public Action stopBullet;

    private void Awake()
    {
        myColider = GetComponent<Collider>();

    }
    private void OnEnable()
    {
        myColider.enabled = true;
    }

    public IEnumerator GoToEnemy(Transform target) // Trasform을 주는게 좋다.
    {
        if (isGoing == false)
        {
            isGoing = true;
            transform.position = gunPos.position;

            float timer = 0;
            ps_Bullet.Play();
            SoundManager.Instance.PlaySound(SoundList.Shot);
            while (timer < maxTimer)
            {
                yield return null;
                timer += Time.deltaTime;
                transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
            }
            yield return null;

            isGoing = false;
        }
    }

    IEnumerator Effect_EnemyHitON(Transform enemyTr)
    {
        yield return wfs01sec;


        Vector3 rotdir = enemyTr.position - transform.position; // get angle 함수 제작
        float angle = Mathf.Atan2(rotdir.x, rotdir.z) * Mathf.Rad2Deg;
        Quaternion targetRot = Quaternion.AngleAxis(angle - 180, Vector3.up);
        Vector3 pos = new Vector3(enemyTr.position.x, enemyTr.position.y + 0.5f, enemyTr.position.z);
        SoundManager.Instance.PlayOneShot(SoundList.Blood);
        SoundManager.Instance.PlayOneShot(SoundList.Explosion);

        foreach (var item in ps_Hits)
        {
            if (item.isPlaying == false)
            {
                item.gameObject.SetActive(true);
                item.transform.position = pos;
                item.transform.rotation = targetRot;
                item.Play();
            }
        }

        yield return wfs05sec;
        // ps_Bullet.trails.
        foreach (var item in ps_Hits)
        {
            item.transform.position = gunPos.position;
            // item.gameObject.SetActive(false);
        }
    }


    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.layer == 8)
        {
            myColider.enabled = false;
            var enemy = other.transform.root;

            StartCoroutine(Effect_EnemyHitON(other.transform));
            var weaponDmg = UnityEngine.Random.Range(10, 21);
            Vector3 rotdir = enemy.position - transform.position;
            enemy.GetComponent<Enemy>().GotHit(weaponDmg, rotdir);
            transform.position = gunPos.position;
            // this.enabled = false;
            // this.gameObject.SetActive(false);
        }
    }
}
