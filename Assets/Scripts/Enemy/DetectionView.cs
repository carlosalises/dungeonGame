using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionView : MonoBehaviour
{

    [SerializeField] private EnemyController _enemy;
    private Coroutine _patrolCoroutine;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Human"))
        {
            if(_patrolCoroutine != null)
                StopCoroutine(_patrolCoroutine);
            _enemy.IntroAtackState();
            _enemy._follow = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Human"))
        {
            _patrolCoroutine = StartCoroutine(_enemy.IntroPatrolState());
            _enemy._follow = false;
        }
    }
}
