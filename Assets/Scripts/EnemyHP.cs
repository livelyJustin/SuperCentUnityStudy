// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;

// public class EnemyHP : MonoBehaviour
// {
//     [SerializeField] private CanvasGroup canvasGroup;
//     [SerializeField] private Image img_HP;
//     [SerializeField] private Image img_HP_Red; // 빌보드 처리 해주기
//     WaitForSeconds wfs3sec = new WaitForSeconds(3);
//     WaitForSeconds wfs05sec = new WaitForSeconds(0.5f);

//     [SerializeField] float fadeTime = 2;

//     IEnumerator DownHP(float dmg)
//     {
//         canvasGroup.alpha = 1;
//         float tagetValue = (img_HP.fillAmount - (MathF.Abs(dmg) / maxHp)) <= 0 ? 0 : (img_HP.fillAmount - (MathF.Abs(dmg) / maxHp));
//         img_HP.fillAmount = tagetValue;
//         yield return wfs05sec;
//         float timer = 0;

//         while (timer < 1.5f)
//         {
//             yield return null;
//             timer += Time.deltaTime;
//             img_HP_Red.fillAmount = Mathf.Lerp(img_HP_Red.fillAmount, tagetValue, 0.005f); // deltatime 값이 고려되지 않아 프레임 따라 속도가 달라질 수 있음
//         }

//         yield return null;
//         timer = 0;
//         while (timer < 1f)
//         {
//             yield return null;
//             timer += Time.deltaTime;
//             canvasGroup.alpha = Mathf.Lerp(canvasGroup.alpha, 0, 0.02f);
//         }
//     }
// }
