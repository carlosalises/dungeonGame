using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIView : MonoBehaviour
{

    [SerializeField] private Image[] _hearts;
    [SerializeField] private Image[] _keys;

    private int _heartsQuantity;
    private int _keysQuantity;

    [SerializeField] private GameObject _store;
    private bool _enableStore;

    [SerializeField] private TextMeshProUGUI _moneyText;
    [SerializeField] private TextMeshProUGUI _insufficientMoneyText;

    [Header("Shield variables")]
    [SerializeField] private GameObject _shieldSystem;
    [SerializeField] private Image _shieldBar;

    public delegate bool BuyItem(string item);
    public static BuyItem onBuyItem;

    public delegate IEnumerator InitializeShieldBar();
    public static InitializeShieldBar onInitializeShieldBar;

    public delegate void ActiveText(string text);
    public static ActiveText onActiveText;

    public delegate void SetQuantityOnItem(Items items);
    public static SetQuantityOnItem onSetQuantityOnItem;

    [SerializeField] private TextMeshProUGUI _shieldQuantity;
    [SerializeField] private TextMeshProUGUI _potionQuantity;
    [SerializeField] private TextMeshProUGUI _atackQuantity;

    private void Awake()
    {
        onSetQuantityOnItem += SetItemQuantity;
        GameManager.onStartGame += InitializeUIElementsInDungeon;

        if (SceneManager.GetActiveScene().name.Equals("ExteriorScene"))
        {
            InitializeUIElementsExit();
        }
    }

    private void OnDisable()
    {
        PlayerController.onHumanRespawn -= RestoreHearts;
        PlayerController.onLostLife -= SetHeart;
        GameManager.onUpdateMoney -= SetMoney;
        GameManager.onGetKey -= SetKeys;
        onInitializeShieldBar -= InitializeBar;
        onActiveText -= EnableActionText;

    }

    private void InitializeUIElementsInDungeon()
    {
        _keysQuantity = 0;
        _shieldSystem.SetActive(false);
        PlayerController.onHumanRespawn += RestoreHearts;
        PlayerController.onLostLife += SetHeart;
        _heartsQuantity = 5;
        EnableHearts(true);
        EnableKeys(false);
        _enableStore = false;
        _store.SetActive(false);
        GameManager.onUpdateMoney += SetMoney;
        GameManager.onGetKey += SetKeys;
        onInitializeShieldBar += InitializeBar;
        onActiveText += EnableActionText;

    }

    private void InitializeUIElementsExit()
    {
        _shieldSystem.SetActive(false);
        PlayerController.onLostLife += SetHeart;
        _heartsQuantity = 5;
        EnableHearts(true);
        _enableStore = false;
        _store.SetActive(false);
        onInitializeShieldBar += InitializeBar;
        onActiveText += EnableActionText;
        GameManager.onUpdateMoney += SetMoney;
    }


    private void Update()
    {
        EnableStore();
    }

    private void RestoreHearts()
    {
        Debug.Log("restore");
        _heartsQuantity = _hearts.Length;
        EnableHearts(true);
    }


    private void EnableHearts(bool enable)
    {
        for (int x = 0, c = _hearts.Length; x < c; x++)
        {
            _hearts[x].gameObject.SetActive(enable);
        }
    }

    private void EnableKeys(bool enable)
    {
        for (int x = 0, c = _keys.Length; x < c; x++)
        {
            _keys[x].gameObject.SetActive(enable);
        }
    }


    private void SetHeart(int quantity, bool boolean)
    {
        if (boolean == false)
        {
            _heartsQuantity -= quantity;
        }
        else
        {
            _heartsQuantity += quantity;
        }

        for (int i = 0, c = _hearts.Length; i < c; i++)
        {
            if (i < _heartsQuantity)
            {
                _hearts[i].gameObject.SetActive(true);
            }
            else
            {
                _hearts[i].gameObject.SetActive(false);
            }
        }

    }

    private void SetKeys()
    {
        _keysQuantity += 1;

        for (int i = 0; i < _keys.Length; i++)
        {
            if (i < _keysQuantity)
            {
                _keys[i].gameObject.SetActive(true);
            }
        }
    }

    private void EnableStore()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _enableStore = !_enableStore;
            _store.SetActive(_enableStore);
        }

        _ = _enableStore == true ? Time.timeScale = 0 : Time.timeScale = 1;

    }


    public void PerformButtonAction()
    {
        GameObject clickedButton = EventSystem.current.currentSelectedGameObject;
        bool canBuy = true;

        switch (clickedButton.tag)
        {
            case "ButtonShield":
                canBuy = (bool)(onBuyItem?.Invoke("Shield"));
                break;
            case "ButtonAtack":
                canBuy = (bool)(onBuyItem?.Invoke("Sword"));
                break;
            case "ButtonPotion":
                canBuy = (bool)(onBuyItem?.Invoke("Potion"));
                break;
        }
    }

    private void SetMoney(int moneyActual)
    {
        Debug.Log("fsdfdf");
        _moneyText.text = moneyActual.ToString();
    }


    private void EnableActionText(string text)
    {
        StartCoroutine(EnableText(text));
    }

    private IEnumerator EnableText(string text)
    {
        _insufficientMoneyText.gameObject.SetActive(true);
        _insufficientMoneyText.text = text;
        yield return new WaitForSeconds(.2f);
        _insufficientMoneyText.gameObject.SetActive(false);
    }


    private IEnumerator InitializeBar()
    {
        _shieldSystem.SetActive(true);
        var actualTime = 10f;

        while (actualTime != 0)
        {
            actualTime -= 1;
            _shieldBar.fillAmount = actualTime / 10;
            yield return new WaitForSeconds(1);
        }
        _shieldSystem.SetActive(false);
    }

    private void SetItemQuantity(Items item)
    {

        switch (item._itemName)
        {
            case "Sword":
                _atackQuantity.text = item._quantity.ToString();
                break;
            case "Potion":
                _potionQuantity.text = item._quantity.ToString();
                break;
            case "Shield":
                _shieldQuantity.text = item._quantity.ToString();
                break;
            default:
                break;

        }
    }

}

