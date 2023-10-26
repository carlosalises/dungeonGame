using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossDetectionView : MonoBehaviour
{

    public Coroutine _bossActionCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Human"))
        {
            if(_bossActionCoroutine == null)
            {
                gameObject.GetComponentInParent<BossController>().StartBossAction();  
            }

        }
    }
}
