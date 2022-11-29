using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    [SerializeField] CameraShake camShake;
    [SerializeField] GunManager gun;
    [SerializeField] int bullet;
    [SerializeField] int max_bullet = 7;
    [SerializeField] float weaponDmg = 0;
    // [SerializeField] Transform cam => Camera.main.transform;


    // Vector3 camPos;
    void Start()
    {
        StartCoroutine(CallRendonAnimaition());
        bullet = max_bullet;
        // camPos = cam.position;
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        wallLayer = 1 << LayerMask.NameToLayer("Wall");

        spineTr = transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.UpperChest);
        //var hash_Shoot = playerAnimator.StringToHash("RunSpeed");
        //var statinfo = playerAnimator.GetCurrentAnimatorClipInfo(0); // 실시간 갱신, 애니메이션은 한 프레임 늦게 됨
        //statinfo.isTag; // 
        //if(statinfo.shortNameHash == hash_Shoot) // hash의 이름만. full path( .idle 까지)
        //{
        //    // stateinfo.normalizedTime -> 0~1까지 재생한 시간
        //}
    }

    Vector3 dir;
    bool moveOn = false;
    [SerializeField] float speed = 1;
    [SerializeField] float timer = 0;
    Ray ray;
    RaycastHit hit;
    [SerializeField] private Vector3 FirstTouch;
    [SerializeField] private Vector3 LastTouch;
    bool isShooting, isReloading = false;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            isActiveIdle = false ? true : false;
            ResetAnimation();
        }

        // move
        if (Input.GetKey(KeyCode.Mouse0))
            moveOn = true;
        else
            moveOn = false;

        if (Input.GetMouseButtonDown(0))
        {
            FirstTouch.x = Input.mousePosition.x;
            FirstTouch.y = Input.mousePosition.y;
        }
        if (Input.GetMouseButton(0))
        {
            LastTouch.x = Input.mousePosition.x;
            LastTouch.y = Input.mousePosition.y;

            float angle = Mathf.Atan2(LastTouch.x - FirstTouch.x, LastTouch.y - FirstTouch.y) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, angle, transform.rotation.z));
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isShooting)
            StartCoroutine(Shoot());

    }

    private void FixedUpdate()
    {
        if (moveOn == true)
            Move();
        else
            ResetMove();
    }

    private void LateUpdate()
    {
        DetectEnemy();
    }

    WaitForSeconds wfs = new WaitForSeconds(1.2f);
    IEnumerator Shoot() // reload 시 쏘지 않기
    {
        isShooting = true;
        ResetAnimation();
        playerAnimator.SetBool("Shoot", true);

        StartCoroutine(camShake.DoShake()); // 카메라 쉐이크 처리
        bullet--;

        if (bullet <= 0 && !isReloading)
        {
            isReloading = true;
            bullet = 0;
            playerAnimator.SetTrigger("Reload");
        }
        yield return wfs;
        ResetShootAnimation();
    }

    void Move()
    {
        timer += Time.deltaTime;
        ResetAnimation();
        if (timer >= 1)
            speed = timer >= 3 ? 3 : timer;

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))
            dir = hit.point - transform.position;

        transform.Translate(dir * speed * Time.deltaTime, Space.World);
        playerAnimator.SetFloat("RunSpeed", speed);
    }

    void ResetMove()
    {
        playerAnimator.SetFloat("RunSpeed", 0);
        speed = 1;
        timer = 0;
    }


    [SerializeField] float radius = 1;
    [SerializeField] int enemyLayer;
    [SerializeField] int wallLayer;
    [SerializeField] Transform spineTr;
    void DetectEnemy()
    {
        // overlap을 통해서 주변 적 감지
        Collider[] enemies = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        if (enemies.Length > 0)
        {
            Transform closeEnemy = GetClosetEnemy(enemies);
            if (ReferenceEquals((object)closeEnemy, null))
            {
                print($"clos is null");
                return;
            }

            Transform targetboneTR;
            targetboneTR = closeEnemy.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.Spine);
            if (ReferenceEquals((object)targetboneTR, null))
            {
                print($"targetboneTR is null");
                return;
            }

            spineTr.LookAt(targetboneTR); // lookrotation 으로 방향을 구해서 사용
            // 항상 바라보는 타겟을 생성하고, LookAt을 통해 타겟을 항상 바라보고, 적의 위치에 따라 타겟을 따라가게

            // 총 쏘기
            if (!isShooting)
            {
                StartCoroutine(Shoot());
                weaponDmg = UnityEngine.Random.Range(10, 21);
                // 방향 계산해서주기 
                Vector3 dir = closeEnemy.position - transform.position;
                // dir.Normalize();
                closeEnemy.GetComponent<Enemy>().GotHit(weaponDmg, dir);
            }
        }
    }

    // 가까운적 찾으면서 벽도 있는지 체크
    Transform GetClosetEnemy(Collider[] enemies)
    {
        Transform closeEnemy = null;
        float closeDistance = 1000;
        for (int i = 0; i < enemies.Length; i++)
        {
            if (!Physics.Linecast(spineTr.position, enemies[i].transform.position, wallLayer)) // 벽이 있는지 체크 원점이 발일 수 있음
            {
                float curDistance = Vector3.Distance(transform.position, enemies[i].transform.position);
                if (closeDistance > curDistance)
                {
                    closeDistance = curDistance;
                    closeEnemy = enemies[i].transform;
                }
            }
            else
            {
                print($"true? name: {enemies[i].transform.name}");
                enemies[i] = null;
            }
        }
        return closeEnemy;
    }

    public void OnExitReload()
    {
        ResetShootAnimation();
        bullet = max_bullet;
    }


    bool isActiveIdle = true;
    WaitForSeconds waitflag2 = new WaitForSeconds(2);
    IEnumerator CallRendonAnimaition()
    {
        if (isActiveIdle)
        {
            yield return waitflag2;
            int num = UnityEngine.Random.Range(0, 2);
            switch (num)
            {
                case 0:
                    {
                        playerAnimator.SetBool("Idle", true);
                        playerAnimator.SetBool("LookAround", false); break;
                    }
                case 1:
                    {
                        playerAnimator.SetBool("LookAround", true);
                        playerAnimator.SetBool("Idle", false); break;
                    }

                default: print("번호가 잘못 출력됨 num: " + num); break;
            }
            yield return waitflag2;
            ResetAnimation();
        }
        StartCoroutine(CallRendonAnimaition());
    }

    private void ResetAnimation()
    {
        playerAnimator.SetBool("Idle", false);
        playerAnimator.SetBool("LookAround", false);
    }

    private void ResetShootAnimation()
    {
        isReloading = false;
        isShooting = false;
        playerAnimator.SetBool("Shoot", false);
    }
}
