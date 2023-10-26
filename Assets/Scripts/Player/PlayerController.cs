using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    private Rigidbody2D rb;
    private Animator _animator;
    private enum States { IDLE, WALK, ATAC, HIT };
    private States _humanState;
    private States _beforeState;
    private Vector2 _direction;
    private float stateDeltaTime;

    private Coroutine _coldownAtack;


    [Header("Life Human")]
    [SerializeField] private ScriptableInteger _playerLife;
    [SerializeField] private Information _playerInfo;
    [Header("Items")]
    [SerializeField] private Items[] _items;
    [Header("Respawn Waipoint")]
    [SerializeField] private Transform _respawnWaypoint;


    // Player delegate
    public delegate void LostLife(int quantity, bool boolean);
    public static LostLife onLostLife;
    //
    public delegate void UsePotion();
    public static UsePotion onUsePotion;

    public delegate void UseSpecialAtack();
    public static UseSpecialAtack onUseSpecialAtack;

    public delegate void DungeonGetOut();
    public static DungeonGetOut onDungeonGetOut;

    public delegate void HumanResoawn();
    public static HumanResoawn onHumanRespawn;

    // Player booleans
    private bool _canAtack;
    public bool _specialAtack = false;
    private bool _invulnerability = false;
    private bool _canUseShield = true;
    private bool _canExitDungeon = false;
    public bool _dead = false;
    private bool _canMove;
    private bool _onExterior;
    private bool _canJump;

    float _horizontalInput;
    float _verticalInput;

    private Coroutine _shieldAction;

    void Start()
    {
        _canMove = true;
        _canAtack = true;
        _animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        _humanState = States.IDLE;
        _onExterior = true;
        _canJump = true;

        // Subscripcion a delegado que avisa al jugador del uso de una pocion 
        onUsePotion += UsePotionHealth;
        // Subscripcion a delegado que avisa al jugador del uso del ataque especial 
        onUseSpecialAtack += UseSpecialPlayerAtack;

        onDungeonGetOut += () => _canExitDungeon = true;

    }

    private void OnDisable()
    {
        onUsePotion -= UsePotionHealth;
        onUseSpecialAtack -= UseSpecialPlayerAtack;
        onDungeonGetOut -= () => _canExitDungeon = true;
    }

    void Update()
    {

        // Movimiento del jugador
        if (_canMove == true)
        {
            _horizontalInput = Input.GetAxis("Horizontal");
            _verticalInput = Input.GetAxis("Vertical");

            if (_horizontalInput < 0) GetComponent<SpriteRenderer>().flipX = true;
            if (_horizontalInput > 0) GetComponent<SpriteRenderer>().flipX = false;
        


            Vector2 _direction = new Vector2(_horizontalInput, _onExterior == false ? _verticalInput : 0);
            _direction.Normalize();

            rb.velocity = _direction * speed;

	    }

        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("Salto");
            rb.AddForce(Vector2.up * 20f, ForceMode2D.Impulse);
        }

        // Actualiza el estado del player constantemente 
        UpdateState(_humanState);
        // Input para hacer uso del escudo
        UseShield();

    }

    private void ChangeState(States newState)
    {
        if (newState == _humanState)
            return;

        ExitState(_humanState);
        InitState(newState);
    }

    private void InitState(States initState)
    {
        _humanState = initState;
        stateDeltaTime = 0;

        switch (_humanState)
        {
            case States.IDLE:
                _animator.Play("Idle");
                break;
            case States.WALK:
                _animator.Play("Walk");
                break;
            case States.ATAC:

                if (_specialAtack == false)
                {
                    _animator.Play("Atack");
                }
                else
                {
                    GameManager.onSetItemQuantity("Sword");
                    _animator.Play("SpecialAtack");
                }

                _coldownAtack = StartCoroutine(ColdownAtack());
                break;
            case States.HIT:
                _animator.Play("Hit");
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
            case States.IDLE:

                if (rb.velocity != Vector2.zero)
                    ChangeState(States.WALK);
                else if (Input.GetKeyDown(KeyCode.Space) && _canAtack)
                    ChangeState(States.ATAC);

                break;
            case States.WALK:

                if (rb.velocity == Vector2.zero)
                    ChangeState(States.IDLE);

                else if (Input.GetKeyDown(KeyCode.Space) && _canAtack)
                    ChangeState(States.ATAC);

                break;
            case States.ATAC:
                _animator.SetBool("Walk", false);
                _animator.SetBool("Idle", false);

                if (stateDeltaTime >= 0.3f)
                {
                    if (rb.velocity != Vector2.zero)
                    {
                        _animator.SetBool("Walk", true);
                        ChangeState(States.WALK);
                    }
                    else
                    {
                        _animator.SetBool("Idle", true);
                        ChangeState(States.IDLE);
                    }

                    if (_specialAtack == true)
                    {
                        _specialAtack = false;
                        GameManager.onEnablePurchase?.Invoke("Sword");
                    }

                }
                break;
            case States.HIT:

                if (stateDeltaTime >= 0.2f)
                {
                    if (rb.velocity != Vector2.zero)
                    {
                        _animator.SetBool("Walk", true);
                        ChangeState(States.WALK);
                    }
                    else
                    {
                        _animator.SetBool("Idle", true);
                        ChangeState(States.IDLE);
                    }
                }
                break;
            default:
                break;
        }
    }

    private void ExitState(States exitState)
    {
        switch (exitState)
        {
            case States.IDLE:
                break;
            case States.WALK:
                break;
            case States.ATAC:
                //StopCoroutine(_coldownAtack);
                break;
            case States.HIT:
                break;
            default:
                break;
        }
    }

    private void AnimationFinished()
    {
        Debug.Log("La animación ha terminado.");
    }

    private IEnumerator ColdownAtack()
    {
        _canAtack = false;
        yield return new WaitForSeconds(2);
        _canAtack = true;
    }


    public void DecreaseLife(int damage)
    {
        if (_invulnerability == false)
        {
            ChangeState(States.HIT);

            if (_playerLife.valorActual - damage < 0)
            {
                _playerLife.valorActual = 0;
                onLostLife?.Invoke(0, false);
            }
            else
            {
                _playerLife.valorActual -= damage;
                onLostLife?.Invoke(damage, false);
            }
        }

        if (_playerLife.valorActual == 0)
        {
            if (SceneManager.GetActiveScene().name.Equals("DungeonScene"))
            {
                StartCoroutine(RespawnHuman());
            }
            else
            {
                GameManager.onEndGame?.Invoke();
                gameObject.SetActive(false);
            }
        }
    }

    private IEnumerator RespawnHuman()
    {
        yield return new WaitForSeconds(1f);
        transform.position = _respawnWaypoint.position;
        _canMove = false;
        _invulnerability = true;
        _playerLife.valorActual = _playerLife.valorMax;
        onHumanRespawn?.Invoke();
        yield return new WaitForSeconds(3f);
        _canMove = true;
        if (_shieldAction == null)
        {
            _invulnerability = false;
        }
    }


    private void UseShield()
    {
        if (Input.GetKeyDown("1"))
        {
            if (ComproveCanUse("Shield"))
            {
                GameManager.onSetItemQuantity("Shield");
                _shieldAction = StartCoroutine(ShieldAction());
            }
        }
    }


    private bool ComproveCanUse(string nomItem)
    {
        Items item = _items.Where(x => x._itemName.Equals(nomItem)).FirstOrDefault();

        if (item._quantity > 0)
        {
            return true;
        }

        return false;
    }

    private void UsePotionHealth()
    {
        _playerLife.valorActual++;
        onLostLife?.Invoke(1, true);
    }

    private void UseSpecialPlayerAtack()
    {

        _specialAtack = true;
    }


    private IEnumerator ShieldAction()
    {
        _invulnerability = true;
        _canUseShield = false;
        yield return UIView.onInitializeShieldBar?.Invoke();
        _invulnerability = false;
        _canUseShield = true;
        GameManager.onEnablePurchase?.Invoke("Shield");
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Key"))
        {
            GameManager.onGetKey?.Invoke();
            _respawnWaypoint = collision.gameObject.GetComponentInParent<CofreView>()._waypoint;
            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.CompareTag("HandFulMoney"))
        {
            GameManager.onGetMoney?.Invoke(20);
            collision.gameObject.SetActive(false);
        }

        if (collision.gameObject.CompareTag("Boss"))
        {
            DecreaseLife(1);
        }

        if (collision.gameObject.CompareTag("Ground"))
        {
            _canJump = true;
        }

    }

    private void OnTriggerEnter2D(Collider2D collision)
    {

        GameObject element = collision.gameObject;

        if (element.CompareTag("Trap"))
        {
            DecreaseLife(1);
        }

        if (element.CompareTag("Hole"))
        {
            DecreaseLife(1);
        }

        if (element.CompareTag("Electric"))
        {
            DecreaseLife(1);
        }

        if (element.CompareTag("FireBall"))
        {
            DecreaseLife(1);
            Destroy(element.gameObject);
        }

        if (element.CompareTag("ExitDoor"))
        {
            if (_canExitDungeon == true)
            {
                StartCoroutine(IETransition());
                _onExterior = true;
                SceneManager.LoadScene("ExteriorScene");
            }
        }
    }

    private IEnumerator IETransition()
    {

        //Iniciar escena con un play
        yield return new WaitForSeconds(.30f);
        //SceneManager.LoadScene("ExteriorScene");

    }
}
