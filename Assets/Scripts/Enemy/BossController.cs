using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{

    private Vector2 centerPosition;
    private bool movingClockwise = true;
    public bool _follow;
    private bool _dead = false;
    private float time;
    private bool _bossDead = false;

    private enum States { PATRULLA, ATAC, HIT, DEAD };
    private States _enemyState;
    private float stateDeltaTime;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;

    [SerializeField] private Transform _player;

    [Header("Life variables")]
    [SerializeField] private float _lifeMax = 10;
    private float _lifeActual;

    public Coroutine _bossActionCoroutine;

    [SerializeField] private GameObject _fireBall;
    [SerializeField] private GameObject [] _esbirros;

    private void Start()
    {

        _lifeActual = _lifeMax;
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        centerPosition = transform.position;

        GameManager.onEndGame += () =>
        {
            StopCoroutine(_bossActionCoroutine);
            gameObject.SetActive(false);
        };
    }

    public void DecreaseBossLife(int damage)
    {
        _animator.Play("Hit");

        if (_lifeActual - damage < 0)
        {
            _lifeActual = 0;
        }
        else
        {
            _lifeActual -= damage;
        }

        if (_lifeActual == 0)
        {
            _animator.Play("Dead");
            _bossDead = true;
            StartCoroutine(DeadAction());
        }
    }


    private void EnableEsbirros(bool enable)
    {
        for(int x = 0, c = _esbirros.Length; x < c; x++)
        {
            _esbirros[x].SetActive(enable);
        }
    }
        

    void OnCollisionEnter2D(Collision2D collision)
    {
        movingClockwise = !movingClockwise;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
    }


    private IEnumerator DeadAction()
    {
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;
        _dead = true;
        yield return new WaitForSeconds(5f);
        Destroy(gameObject);
    }

    private IEnumerator BossAction()
    {
        while (_bossDead != true)
        {
            StartCoroutine(MoverObjeto(gameObject));
            yield return new WaitForSeconds(5f);
            yield return StartCoroutine(ShootFireBalls());
        }
    }

    private IEnumerator ShootFireBalls()
    {

        int balls = 10;
        Debug.Log("Fire");
        while (balls > 0)
        {
            GameObject fireBall = Instantiate(_fireBall);
            fireBall.transform.position = transform.position;
            StartCoroutine(MoverObjeto(fireBall, true));
            balls--;
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator MoverObjeto(GameObject objeto, bool destroy = false)
    {
        Vector3 posicionInicial = transform.position;
        Vector3 posicionObjetivo = _player.transform.position;
        float tiempoPasado = 0f;
        int it = 0;

        while (tiempoPasado < 5f)
        {
            Vector3 nuevaPosicion = Vector3.Lerp(posicionInicial, posicionObjetivo, tiempoPasado / 10f * 5f);
            objeto.transform.position = new Vector2(nuevaPosicion.x, transform.position.y);
            tiempoPasado += Time.deltaTime;

            if (it < 1)
            {
                if (tiempoPasado > 4f)
                {
                    if (objeto.CompareTag("Boss"))
                    {
                        _animator.Play("Atack");
                        it++;
                    }
                }
            }

            yield return null;
        }

        if (destroy == true)
        {
            Destroy(objeto.gameObject);
        }
    }

    public void StartBossAction()
    {
        _bossActionCoroutine = StartCoroutine(BossAction());
    }

}
