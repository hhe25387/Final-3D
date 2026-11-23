using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;



public class GameController : MonoBehaviour
{
    [SerializeField] private Player _player;
    [SerializeField] private float _maxTimeSeconds;
    [SerializeField] private GameObject _startGameUI;
    [SerializeField] private GameObject _gameOverUI;
    [SerializeField] private TMP_Text _timerText;
    [SerializeField] private TMP_Text _pointsText;
    [SerializeField] private TMP_Text _finalCommonText;
    [SerializeField] private TMP_Text _finalUncommonText;
    [SerializeField] private TMP_Text _finalRareText;
    [SerializeField] private TMP_Text _totalText;

    // Add member variables to store references to all of the UI text.

    private int _commonPoints;
    private int _uncommonPoints;
    private int _rarePoints;

    private float _timeRemaining;
    private bool _runGame;

    // ------------------------------------------------------------------------
    // You don't need to code anything in this method :)
    private void Start()
    {
        Time.timeScale = 1f;
        SetGameEnabled(false);
        UpdatePointsText();
        _timeRemaining = _maxTimeSeconds;
        UpdateTimerText();
    }

    // ------------------------------------------------------------------------
    private void Update()
    {
        if (_runGame)
        {
            _timeRemaining -= Time.deltaTime;
            
             UpdateTimerText();

            if (_timeRemaining <= 0)
            {
                EndGame();
            }
        }
    }

    // ------------------------------------------------------------------------
    public void StartGame()
    {
        Time.timeScale = 1f;
        _startGameUI.SetActive(false);
        _gameOverUI.SetActive(false);
        
        SetGameEnabled(true);

        _timeRemaining = _maxTimeSeconds;
        
        UpdateTimerText();
    }

    // ------------------------------------------------------------------------
    // You don't need to code anything in this method :)
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(0);
    }

    // ------------------------------------------------------------------------
    private void EndGame()
    {
        SetGameEnabled(false);

        _finalCommonText.text = _commonPoints.ToString();
        _finalUncommonText.text = _uncommonPoints.ToString();
        _finalRareText.text = _rarePoints.ToString();

        int total = _commonPoints
            + _uncommonPoints * 2
            + _rarePoints * 3;

        _totalText.text = $"Total: {total}";
        _gameOverUI.SetActive(true);
        Time.timeScale = 0f;
    }

    // ------------------------------------------------------------------------
    // You don't need to code anything in this method :)
    private void SetGameEnabled(bool enabled)
    {
        Cursor.lockState = enabled ? CursorLockMode.Locked : CursorLockMode.None;
        _player.enabled = enabled;
        _runGame = enabled;
    }

    // ------------------------------------------------------------------------
    // Add a paramter to this method to represent the item's rarity.
    public void CollectItem(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                _commonPoints++;
                break;

            case ItemRarity.Uncommon:
                _uncommonPoints++;
                break;

            case ItemRarity.Rare:
                _rarePoints++;
                break;
        }

        UpdatePointsText();
    }

    // ------------------------------------------------------------------------
    public void RemoveItem(ItemRarity rarity)
    {
        switch (rarity)
        {
            case ItemRarity.Common:
                _commonPoints = Mathf.Max(0, _commonPoints - 1);
                break;

            case ItemRarity.Uncommon:
                _uncommonPoints = Mathf.Max(0, _uncommonPoints - 1);
                break;

            case ItemRarity.Rare:
                _rarePoints = Mathf.Max(0, _rarePoints - 1);
                break;
        }

        UpdatePointsText();
    }

    private void UpdatePointsText()
    {
        _pointsText.text =
            $"Common: {_commonPoints} ({_commonPoints} pts)\n" +
            $"Uncommon: {_uncommonPoints} ({_uncommonPoints * 2} pts)\n" +
            $"Rare: {_rarePoints} ({_rarePoints * 3} pts)";
    }
    private void UpdateTimerText()
    {
        int seconds = Mathf.CeilToInt(Mathf.Max(0f, _timeRemaining));
        _timerText.text = $"Time: {seconds}";
    }
}