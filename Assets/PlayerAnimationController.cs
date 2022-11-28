using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] Animator playerAnimator;
    [SerializeField] CameraShake camShake;
    [SerializeField] int bullet;
    [SerializeField] int max_bullet = 7;



    void Start()
    {
        StartCoroutine(CallRendonAnimaition());
        bullet = max_bullet;

        
        //var hash_Shoot = playerAnimator.StringToHash("RunSpeed");
        //var statinfo = playerAnimator.GetCurrentAnimatorClipInfo(0); // 실시간 갱신, 애니메이션은 한 프레임 늦게 됨
        //statinfo.isTag; // 
        //if(statinfo.shortNameHash == hash_Shoot) // hash의 이름만. full path( .idle 까지)
        //{
        //    // stateinfo.normalizedTime -> 0~1까지 재생한 시간
        //}
    }

    Vector3 dir;
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
        {
            timer += Time.deltaTime;
            ResetAnimation();
            if (timer >= 1)
            {
                speed = timer;
            }

            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
                dir = hit.point - transform.position;

            transform.Translate(dir * speed * Time.deltaTime, Space.World);

            playerAnimator.SetFloat("RunSpeed", speed);
        }

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
            Debug.Log(angle + 180);

            if (angle + 180 == 180)
            {
                angle += 180;
            }

            transform.rotation = Quaternion.Euler(transform.rotation.x, angle + 180, transform.rotation.z);
        }
        if (Input.GetKeyUp(KeyCode.Mouse0))
        {
            playerAnimator.SetFloat("RunSpeed", 0);
            speed = 1;
            timer = 0;
        }

        if(Input.GetKeyDown(KeyCode.Space) && !isShooting)
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
        }


    }
    


    public void OnExitShot()
    {
        ResetShootAnimation();
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
        if(isActiveIdle)
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
