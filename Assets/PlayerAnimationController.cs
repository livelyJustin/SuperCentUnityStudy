using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    [SerializeField] CameraShake camShake;
    [SerializeField] GunManager gun;
    private bool isShooting, isReloading = false;
    private bool moveOn = false;
    private bool isActiveIdle = true;
    private float weaponDmg = 0;
    [SerializeField] private float speed = 1;
    [SerializeField] private float speedadjust = 0.01f;
    private float timer = 0;
    [SerializeField] private float radius = 3;
    private int enemyLayer;
    private int wallLayer;
    [SerializeField] private int bullet;
    [SerializeField] private int max_bullet = 7;
    [SerializeField] Transform spineTr;
    [SerializeField] Image joyStick_Pad;
    [SerializeField] Image joyStick_Frame;
    [SerializeField] CanvasGroup joyStick_Group;
    private Vector3 moveDir;
    private Vector3 FirstTouch;
    [SerializeField] private Vector3 LastTouch;
    private Ray ray;
    private RaycastHit hit;
    private WaitForSeconds wfs = new WaitForSeconds(1.2f);

    private int hash_RunSpeed, hash_Shoot, hash_Reload, hash_Idle, hash_LookAround;
    void Start()
    {
        StartCoroutine(CallRendonAnimaition());
        bullet = max_bullet;
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
        //if(statinfo.shortNameHash == hash_Shoot) // hash의 이름만. full path( .idle 까지)
        //{
        //    // stateinfo.normalizedTime -> 0~1까지 재생한 시간
        //}
    }
    // float joystick_radius => joyStick_Frame.rectTransform.rect.width * 0.5f;
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

            // 끝나면 없애주기
            Vector2 normal = LastTouch * joystick_adjust;
            joyStick_Pad.rectTransform.anchoredPosition = Vector2.ClampMagnitude(normal, 100); // 시작 위치가 적절하지 않아 고쳐야함

            // print($"anchored x: {joyStick_Pad.rectTransform.anchoredPosition.x} y: {joyStick_Pad.rectTransform.anchoredPosition.y} ");
            // print($"position X: {joyStick_Pad.rectTransform.position.x} y: {joyStick_Pad.rectTransform.position.y} ");
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
        DetectEnemy(); // 애니메이션은 late전에 동작하기에 lateupdata에서 실행해주어야함
    }

    IEnumerator Shoot() // reload 시 쏘지 않기
    {
        isShooting = true;
        ResetAnimation();
        playerAnimator.SetBool(hash_Shoot, true);

        StartCoroutine(camShake.DoShake()); // 카메라 쉐이크 처리
        bullet--;

        if (bullet <= 0 && !isReloading)
        {
            isReloading = true;
            bullet = 0;
            playerAnimator.SetTrigger(hash_Reload);
        }
        yield return wfs;
        ResetShootAnimation();
    }

    void Move(Vector3 dir)
    {
        timer += Time.deltaTime;
        ResetAnimation();
        if (timer >= 1)
            speed = timer >= 3 ? 3 : timer;

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
        // overlap을 통해서 주변 적 감지
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

            Transform targetboneTR = enemy_animator.GetBoneTransform(HumanBodyBones.Spine);
            if (ReferenceEquals((object)targetboneTR, null))
            {
                print($"targetboneTR is null");
                return;
            }

            // spineTr.LookAt(targetboneTR); // lookrotation 으로 방향을 구해서 사용

            // spineTr.rotation = Quaternion.LookRotation(targetboneTR.position - spineTr.position);
            // Vector3 screenpointEnemy = Camera.main.WorldToScreenPoint(targetboneTR.position);
            // Vector3 screenpointMe = Camera.main.WorldToScreenPoint(spineTr.position);
            // Vector3 rotdir = screenpointEnemy - screenpointMe;
            // float rotdir2 = Mathf.Atan2(rotdir.y, rotdir.x) * Mathf.Rad2Deg;
            // Debug.Log($" rot:  {rotdir2} -rot: {-rotdir2}");
            // // float finalAngle = rotdir2 > 180 ? -180 : rotdir2;
            // // spineTr.rotation = Quaternion.AngleAxis(rotdir2, Vector3.forward);
            // spineTr.transform.rotation = Quaternion.AngleAxis(-rotdir2, Vector3.up);
            // spineTr.transform.eulerAngles = new Vector3(0, 0, Mathf.LerpAngle(spineTr.transform.eulerAngles.z, rotdir2, 1 * Time.deltaTime));

            Vector3 rotdir = targetboneTR.position - spineTr.position;
            // 왜 되는가? 3D 좌표에서랑 2D 좌표에서의 아크 탄젠트에 넣어줘야하는 값이 다르기 때문에ㅎ
            float rotdir2 = Mathf.Atan2(rotdir.x, rotdir.z) * Mathf.Rad2Deg;
            // spineTr.rotation = Quaternion.Euler(0, rotdir2, 0);
            Debug.Log($" rot:  {rotdir2} -rot: {-rotdir2}");
            spineTr.rotation = Quaternion.AngleAxis(rotdir2, Vector3.up);

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
            var normalize = enemies[i].transform.position + new Vector3(0, 0.5f, 0);
            if (!Physics.Linecast(spineTr.position, (normalize), wallLayer)) // 벽이 있는지 체크 원점이 발일 수 있음
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

                default: print("번호가 잘못 출력됨 num: " + num); break;
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
        isReloading = false;
        isShooting = false;
        playerAnimator.SetBool(hash_Shoot, false);
    }
}
