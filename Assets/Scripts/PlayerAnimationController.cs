using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    [SerializeField] CameraShake camShake;
    // [SerializeField] GunManager gun;
    [SerializeField] Bullet bullet;
    private bool isShooting, isReloading = false;
    private bool moveOn = false;
    private bool isActiveIdle = true;
    [SerializeField] private float speed = 1;
    [SerializeField] private float speedadjust = 0.01f;
    private float timer = 0;
    [SerializeField] private float radius = 3;
    private int enemyLayer;
    private int wallLayer;
    [SerializeField] private int bullet_Count;
    [SerializeField] private int max_bullet = 7;
    [SerializeField] Transform spineTr;

    // JoyStick Field
    [SerializeField] Image joyStick_Pad;
    [SerializeField] Image joyStick_Frame;
    [SerializeField] CanvasGroup joyStick_Group;

    // Attack Particle
    [SerializeField] ParticleSystem[] ps_Hits;

    [SerializeField] Transform gunPos;
    private Vector3 moveDir;
    private Vector3 FirstTouch;
    private Vector3 LastTouch;
    private Ray ray;
    private RaycastHit hit;
    private WaitForSeconds wfs1dot2sec = new WaitForSeconds(1.2f);
    private WaitForSeconds wfs05sec = new WaitForSeconds(0.5f);

    private int hash_RunSpeed, hash_Shoot, hash_Reload, hash_Idle, hash_LookAround;
    void Start()
    {
        StartCoroutine(CallRendonAnimaition());
        bullet_Count = max_bullet;
        enemyLayer = 1 << LayerMask.NameToLayer("Enemy");
        wallLayer = 1 << LayerMask.NameToLayer("Wall");
        #region  Animator Hash Setting
        hash_RunSpeed = Animator.StringToHash("RunSpeed");
        hash_Shoot = Animator.StringToHash("Shoot");
        hash_Reload = Animator.StringToHash("Reload");
        hash_Idle = Animator.StringToHash("Idle");
        hash_LookAround = Animator.StringToHash("LookAround");
        #endregion

        spineTr = transform.GetComponent<Animator>().GetBoneTransform(HumanBodyBones.UpperChest);
        //statinfo.isTag; // 
        //if(statinfo.shortNameHash == hash_Shoot) // hash??? ?????????. full path( .idle ??????)
        //{
        //    // stateinfo.normalizedTime -> 0~1?????? ????????? ??????
        //}
    }
    [SerializeField] float joystick_adjust = 0.1f;
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            FirstTouch.x = Input.mousePosition.x;
            FirstTouch.y = Input.mousePosition.y;

            joyStick_Group.alpha = 1;
            joyStick_Frame.rectTransform.position = FirstTouch;
        }
        if (Input.GetMouseButton(0))
        {
            moveOn = true;
            LastTouch.x = Input.mousePosition.x;
            LastTouch.y = Input.mousePosition.y;

            // ????????? ????????????
            Vector2 normal = LastTouch * joystick_adjust;
            joyStick_Pad.rectTransform.anchoredPosition = Vector2.ClampMagnitude(normal, 100); // ?????? ????????? ???????????? ?????? ????????????

            moveDir = LastTouch - FirstTouch;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            joyStick_Group.alpha = 0;
            moveOn = false;
        }

        if (Input.GetKeyDown(KeyCode.Space) && !isShooting)
            StartCoroutine(Shoot());
    }

    private void FixedUpdate()
    {
        if (moveOn == true)
            Move(moveDir);
        else
            ResetMove();

        RotatebyMouse();
    }

    private void LateUpdate()
    {
        DetectEnemy(); // ?????????????????? late?????? ??????????????? lateupdata?????? ?????????????????????
    }

    IEnumerator Shoot() // reload ??? ?????? ??????
    {
        isShooting = true;
        ResetAnimation();
        playerAnimator.SetBool(hash_Shoot, true);

        StartCoroutine(camShake.DoShake()); // ????????? ????????? ??????
        bullet_Count--;

        if (bullet_Count <= 0 && !isReloading)
        {
            isReloading = true;
            SoundManager.Instance.PlayOneShot(SoundList.Reload);
            bullet_Count = 0;
            playerAnimator.SetTrigger(hash_Reload);
            yield break;
        }

        yield return wfs1dot2sec;
        ResetShootAnimation();
    }


    void Move(Vector3 dir)
    {
        timer += Time.deltaTime;
        ResetAnimation();
        if (timer >= 1)
        {
            speed = timer >= 3 ? 3 : timer;
        }

        var temp = new Vector3(dir.x, transform.position.y, dir.y);
        transform.Translate(temp * (speed * speedadjust) * Time.deltaTime, Space.World);
        playerAnimator.SetFloat(hash_RunSpeed, speed);
    }

    void ResetMove()
    {
        playerAnimator.SetFloat(hash_RunSpeed, 0);
        speed = 1;
        timer = 0;
    }

    void RotatebyMouse()
    {
        float angle = Mathf.Atan2(LastTouch.x - FirstTouch.x, LastTouch.y - FirstTouch.y) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.up); // Quaternion.Euler(new Vector3(transform.rotation.x, angle, transform.rotation.z));
    }

    void DetectEnemy()
    {
        // overlap??? ????????? ?????? ??? ??????
        Collider[] enemies = Physics.OverlapSphere(transform.position, radius, enemyLayer);

        if (enemies.Length > 0)
        {
            Transform closeEnemy = GetClosetEnemy(enemies);
            if (ReferenceEquals((object)closeEnemy, null))
            {
                print($"close is null");
                return;
            }
            Animator enemy_animator = closeEnemy.GetComponent<Animator>();

            Transform targetboneTR = enemy_animator.GetBoneTransform(HumanBodyBones.Chest);
            if (ReferenceEquals((object)targetboneTR, null))
            {
                print($"targetboneTR is null");
                return;
            }

            // spineTr.LookAt(targetboneTR); // lookrotation ?????? ????????? ????????? ??????

            Vector3 rotdir = targetboneTR.position - spineTr.position;
            // ??? ?????????? 3D ??????????????? 2D ??????????????? ?????? ???????????? ?????????????????? ?????? ????????? ????????????
            float angle = Mathf.Atan2(rotdir.x, rotdir.z) * Mathf.Rad2Deg;
            spineTr.rotation = Quaternion.AngleAxis(angle, Vector3.up);


            // ??? ??????
            if (!isShooting)
            {
                StartCoroutine(Shoot());
                bullet.myColider.enabled = true;
                bullet.StartCoroutine(bullet.GoToEnemy(targetboneTR));
            }
        }
        enemies = null;
    }

    // ???????????? ???????????? ?????? ????????? ??????
    Transform GetClosetEnemy(Collider[] enemies)
    {
        Transform closeEnemy = null;
        float closeDistance = 1000;
        for (int i = 0; i < enemies.Length; i++)
        {
            var normalize = enemies[i].transform.position + new Vector3(0, 0.5f, 0);
            if (!Physics.Linecast(spineTr.position, (normalize), wallLayer)) // ?????? ????????? ?????? ????????? ?????? ??? ??????
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
                print($"?????? ?????? name: {enemies[i].transform.name}");
                enemies[i] = null;
            }
        }
        return closeEnemy;
    }

    public void OnExitReload()
    {
        ResetShootAnimation();
        isReloading = false;
        bullet_Count = max_bullet;
    }


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
                        playerAnimator.SetBool(hash_Idle, true);
                        playerAnimator.SetBool(hash_LookAround, false); break;
                    }
                case 1:
                    {
                        playerAnimator.SetBool(hash_LookAround, true);
                        playerAnimator.SetBool(hash_Idle, false); break;
                    }

                default: print("????????? ?????? ????????? num: " + num); break;
            }
            yield return waitflag2;
            ResetAnimation();
        }
        StartCoroutine(CallRendonAnimaition());
    }

    private void ResetAnimation()
    {
        playerAnimator.SetBool(hash_Idle, false);
        playerAnimator.SetBool(hash_LookAround, false);
    }

    private void ResetShootAnimation()
    {
        // isReloading = false;
        isShooting = false;
        playerAnimator.SetBool(hash_Shoot, false);
    }

    private void OnDisable()
    {
        joyStick_Group.alpha = 0;
    }
}
