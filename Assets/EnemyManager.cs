using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyManager : MonoBehaviour
{
    public static EnemyManager instance;
    [SerializeField] Button retryButton;
    [SerializeField] CanvasGroup retryGroup;
    [SerializeField] TextMeshProUGUI text_EnemyCount;
    [SerializeField] PlayerAnimationController player; // 다른 방법 고안하기
    WaitForSeconds wfs2sec = new WaitForSeconds(1);
    private int enemy_Count;
    private int enemy_MaxCount;
    public int Enemy_Count
    {
        get { return enemy_Count; }
        set { enemy_Count = value; }
    }

    private void Awake()
    {
        if (instance == null)
            instance = this;

        // 싱글톤 예외 처리
    }

    IEnumerator Start() // Init함수를 만들어서 따로 통제하는 방안을 고려해볼 필요 있음
    {
        yield return wfs2sec;
        retryGroup.alpha = 0;
        enemy_MaxCount = Enemy_Count;
        text_EnemyCount.text = $"{enemy_Count:0}/{enemy_MaxCount:#.0}";  //string.Format("{0}/{1}", enemy_Count, enemy_MaxCount);
    }


    public void CountDownEnemy()
    {
        Enemy_Count--;
        if (enemy_Count <= 0)
            // 엔딩 UI 출력
            StartCoroutine(ReLoadScene());
        text_EnemyCount.text = string.Format("{0}/{1}", enemy_Count, enemy_MaxCount);
    }

    // 추후 다른 스크립트에서 관리
    IEnumerator ReLoadScene()
    {
        yield return wfs2sec;
        player.enabled = false;
        Time.timeScale = 0;
        retryGroup.alpha = 1;
        retryButton.onClick.AddListener(() => UnityEngine.SceneManagement.SceneManager.LoadScene(0));
    }

}
