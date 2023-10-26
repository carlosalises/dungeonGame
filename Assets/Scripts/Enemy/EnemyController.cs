using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private float _speed = 2f;
    private float _angularSpeed = 2f;
    private float _force = 2f;
    private Vector2 centerPosition;
    private float radius = -1f;
    private Vector2 _direction;
    private int _enemyDirection;
    private bool movingClockwise = true;
    public bool _follow;
    private bool _dead = false;
    private float time;

    private enum States { PATRULLA, ATAC, HIT, DEAD };
    private States _enemyState;
    private float stateDeltaTime;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;

    [SerializeField] private Transform _player;

    [Header("Life variables")]
    [SerializeField] private float _lifeMax = 2;
    private float _lifeActual;

    private Coroutine _atackCorutine;
    private Coroutine _deadCorutine;

    private void Start()
    {

        _lifeActual = _lifeMax;

        if(Random.Range(0, 2) == 1)
        {
            _enemyDirection = -1;
        }
        else
        {
            _enemyDirection = 1;
        }


        _enemyState = States.PATRULLA;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        centerPosition = transform.position;
    }

    private void Update()
    {
       
        UpdateState(_enemyState);
        
    }

    private void ChangeState(States newState)
    {
        if(_dead == false)
        {
            if (newState == _enemyState)
                return;

            ExitState(_enemyState);
            InitState(newState);
        }

    }

    private void InitState(States initState)
    {
        _enemyState = initState;
        stateDeltaTime = 0;

        switch (_enemyState)
        {
            case States.PATRULLA:
                _animator.Play("Patrulla");
                break;
            case States.ATAC:
                _animator.Play("Walk");
               _atackCorutine = StartCoroutine(EnemyAtack());
                break;
            case States.HIT:
                _animator.Play("Hit");
                break;
            case States.DEAD:
                break;
            default:
                break;
        }
    }

    private void UpdateState(States updateState)
    {
        stateDeltaTime += Time.deltaTime;

        switch (updateState)
        {
            case States.PATRULLA:
                PatrolMovement();
                break;
            case States.ATAC:
                MovementToPlayer();
                ChangeSpriteView();
                break;
            case States.HIT:
    
                if (stateDeltaTime >= 0.3f)
                {
                    if (_follow) 
                    { 
                       ChangeState(States.ATAC);
                       _animator.SetBool("Walk", true);

                    }
                    else
                    {
                        ChangeState(States.PATRULLA);
                        _animator.SetBool("Patrulla", true);
                    }
                }
                break;
            case States.DEAD:
                Debug.Log("SS");
                Destroy(gameObject);
                break;
            default:
                break;
        }
    }

    private void ExitState(States exitState)
    {
        switch (exitState)
        {
            case States.PATRULLA:
                break;
            case States.ATAC:
                StopCoroutine(_atackCorutine);
                break;
            case States.HIT:
                break;
            case States.DEAD:
                //Destroy(gameObject);
                break;
            default:
                break;
        }
    }


    private void PatrolMovement()
    {
        /*float angle = Time.time * _angularSpeed;

        _enemyDirection = movingClockwise ? 1 : -1;

        Vector2 offset = new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * radius * _enemyDirection;
        transform.position = centerPosition + offset;*/

        _rb.velocity = Vector2.zero;

    }

    private void ChangeSpriteView()
    {
        _direction = _player.position - transform.position;
        float angle = Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg;
        if (angle > -90 && angle < 90)
        {
            _spriteRenderer.flipX = false; 
        }
        else
        {
            _spriteRenderer.flipX = true; 
        }
    }


    private void MovementToPlayer()
    {
        _direction = (_player.position - transform.position).normalized;
        _rb.velocity = _direction * _speed;    
    }

    public void IntroAtackState()
    {
        ChangeState(States.ATAC);
    }

    public IEnumerator IntroPatrolState()
    {
        yield return new WaitForSeconds(2f);
        centerPosition = transform.position;
        ChangeState(States.PATRULLA);
    }

    public void DecreaseOrcLife(int damage)
    {
        ChangeState(States.HIT);

        if(_lifeActual - damage < 0)
        {
            _lifeActual = 0;
        }
        else
        {
            _lifeActual -= damage;
        }

        if(_lifeActual == 0)
        {
            _animator.Play("Dead");
            GameManager.onGetMoney(3);
            StartCoroutine(DeadAction());
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        movingClockwise = !movingClockwise;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag.Equals("HurtBox"))
        {
            
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.CompareTag("Human"))
        {
            Debug.Log("Human");
        }
    }


    private IEnumerator EnemyAtack()
    {
        while(true)
        {
            _animator.Play("Atack");
            yield return new WaitForSeconds(5f);
        }
    }

    private IEnumerator DeadAction()
    {
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;
        _dead = true;
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }


}
