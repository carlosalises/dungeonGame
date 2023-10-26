using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] public Information _playerInfo;

    [Header("List game items")]
    [SerializeField] private Items[] _shopItems;

    [Header("Initialize player values")]
    [SerializeField] private int _keysMax;

    public delegate void UpdateMoney(int actualMoney);
    public static UpdateMoney onUpdateMoney;

    public delegate void SetItemQuantity(string itemName);
    public static SetItemQuantity onSetItemQuantity;

    public delegate void EndGame();
    public static EndGame onEndGame;

    public delegate void StartGame();
    public static StartGame onStartGame;

    public delegate void GetKey();
    public static GetKey onGetKey;

    public delegate void GetMoney(int quantity);
    public static GetMoney onGetMoney;

    public delegate void StartExterior();
    public static StartExterior onStartExterior;


    public delegate void EnablePurchase(string item);
    public static EnablePurchase onEnablePurchase;

    [SerializeField] private TextMeshProUGUI _gameOver;
    [SerializeField] private GameObject _startMenu;
    [SerializeField] private Camera _gameCamera;
    [SerializeField] private GameObject _controlPanel;
    private bool _enableControls;

    [SerializeField] private ScriptableInteger _playerLife;

    [SerializeField] private GameObject _exitDoor;

    private Coroutine _incrementMoneyCoroutine;

    public static bool _endGame = false;

    private void Start()
    {

        onSetItemQuantity += UpdateItemQuantity;


        if (SceneManager.GetActiveScene().name.Equals("DungeonScene"))
        {
            ActiveElements(false);
        }

        if (SceneManager.GetActiveScene().name.Equals("ExteriorScene"))
        {
            Debug.Log("asdasd");
            InitializeManagerConfigInExterior();
        }

    }

    private void OnDisable()
    {
        UIView.onBuyItem -= BuySelectedItem;
        onGetKey -= GetDungeonKey;
        onEndGame -= () => _endGame = true;
        onGetMoney -= GiveMoney;
        onEnablePurchase -= EnableItemPurchase;

    }

    private void InitializeManagerConfigInDungeon()
    {
        _enableControls = false;
        _playerLife.valorActual = _playerLife.valorMax;
        InitializeShopItems();
        _playerInfo.keys = 0;
        _playerInfo.money = 0;
        _incrementMoneyCoroutine = StartCoroutine(IncrementDungeonMoney());
        onUpdateMoney?.Invoke(_playerInfo.money);
        UIView.onBuyItem += BuySelectedItem;
        onGetKey += GetDungeonKey;
        onEndGame += () => _endGame = true;
        onGetMoney += GiveMoney;
        onEnablePurchase += EnableItemPurchase;
    }

    private void InitializeManagerConfigInExterior()
    {
        _playerLife.valorActual = _playerLife.valorMax;
        onUpdateMoney?.Invoke(_playerInfo.money);
        onEndGame += () => _gameOver.gameObject.SetActive(true);
        UIView.onBuyItem += BuySelectedItem;
        onEnablePurchase += EnableItemPurchase;
        onUpdateMoney?.Invoke(_playerInfo.money);
    }

    private void Update()
    {
        ExitDungeonTrap();
    }


    public bool BuySelectedItem(string item)
    {

        Items selectedItem = _shopItems.Where(x => x._itemName == item).FirstOrDefault();

        if (_playerInfo.money >= selectedItem._prize)
        {

            if (selectedItem.name.Equals("Potion"))
            {
                if (_playerLife.valorActual < _playerLife.valorMax)
                {
                    UIView.onSetQuantityOnItem(selectedItem);
                    _playerInfo.money -= selectedItem._prize;
                    onUpdateMoney?.Invoke(_playerInfo.money);
                    PlayerController.onUsePotion?.Invoke();
                    return true;

                }

                UIView.onActiveText?.Invoke("Tienes la vida al maximo");
                return false;
            }

            else if (selectedItem._canBuy == true)
            {
                if (selectedItem.name.Equals("Sword"))
                {
                    
                    PlayerController.onUseSpecialAtack?.Invoke();
                }

                selectedItem._canBuy = false;
                selectedItem._quantity++;
                UIView.onSetQuantityOnItem(selectedItem);
                _playerInfo.money -= selectedItem._prize;
                onUpdateMoney?.Invoke(_playerInfo.money);
                return true;
            }
            else
            {
                UIView.onActiveText?.Invoke("Ya tienes una " + selectedItem.name + " en el inventario");
                return false;
            }
        }

        UIView.onActiveText?.Invoke("Dinero Insuficiente");
        return false;
    }

    private void InitializeShopItems()
    {
        for (int x = 0, c = _shopItems.Length; x < c; x++)
        {
            _shopItems[x]._quantity = 0;
            _shopItems[x]._canBuy = true;
        }
    }


    private IEnumerator IncrementDungeonMoney()
    {
        yield return new WaitForSeconds(3f);
        while (!_endGame)
        {
            _playerInfo.money += 1;
            onUpdateMoney?.Invoke(_playerInfo.money);
            yield return new WaitForSeconds(1f);
        }
    }

    private void GiveMoney(int quantity)
    {
        _playerInfo.money += quantity;
        onUpdateMoney?.Invoke(_playerInfo.money);
    }


    private void GetDungeonKey()
    {
        _playerInfo.keys++;

        if (_playerInfo.keys == 3)
        {
            StopCoroutine(_incrementMoneyCoroutine);
            PlayerController.onDungeonGetOut?.Invoke();
            _exitDoor.SetActive(true);
            _exitDoor.gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        }
    }

    private void ExitDungeonTrap()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            // Esta suscrito New Player Controller
            PlayerController.onDungeonGetOut?.Invoke();
            _exitDoor.SetActive(true);
            _exitDoor.transform.GetChild(0).gameObject.GetComponent<BoxCollider2D>().isTrigger = true;
        }
    }

    private void EnableItemPurchase(string itemName)
    {
        Items item = _shopItems.Where(x => x._itemName.Equals(itemName)).FirstOrDefault();
        item._canBuy = true;
    }

    public void ActiveElements(bool enable)
    {
        GameObject[] objetosEscena = Resources.FindObjectsOfTypeAll<GameObject>();

        // Desactiva cada objeto de la escena
        foreach (GameObject objeto in objetosEscena)
        {
            if (objeto.name.Equals("Human") || objeto.name.Equals("Enemigos"))
            {
                Debug.Log("yyy");
                objeto.SetActive(enable);
            }
        }

    }

    public void InitializeDungeonGame()
    {
        _startMenu.SetActive(false);
        ActiveElements(true);
        _gameCamera.gameObject.SetActive(false);
        InitializeManagerConfigInDungeon();
        onStartGame?.Invoke();
    }


    public void EnableControlsPanel()
    {
        _enableControls = !_enableControls;
        _controlPanel.SetActive(_enableControls);
    }


    private void UpdateItemQuantity(string itemName)
    {
        Items item = _shopItems.Where(x => x._itemName.Equals(itemName)).FirstOrDefault();
        item._quantity--;
        UIView.onSetQuantityOnItem(item);
    }

}

