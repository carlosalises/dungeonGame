using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordView : MonoBehaviour
{

    private int _damage;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Enemy") || collision.gameObject.tag.Equals("Boss"))
        {
            GameObject obj_collision = collision.gameObject;

            if (gameObject.transform.parent.tag == "Human")
            {
                if (gameObject.GetComponentInParent<PlayerController>()._specialAtack == false)
                {
                    _damage = 1;
                }
                else
                {
                    _damage = 2;
                }

                if (obj_collision.tag.Equals("Enemy"))
                    collision.gameObject.GetComponent<EnemyController>().DecreaseOrcLife(_damage);
                else
                {
                    obj_collision.GetComponent<BossController>().DecreaseBossLife(_damage);
                }
            }
        }

        if (collision.gameObject.CompareTag("Human"))
        {
            Debug.Log("DAMAGE");
            if (gameObject.transform.parent.tag == "Enemy")
            {
                Debug.Log("DAMAGE");
                collision.gameObject.GetComponent<PlayerController>().DecreaseLife(1);
            }
        }
    }
}
