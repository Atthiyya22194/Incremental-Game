using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if(_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
            }
            return _instance;
        }
    }

    public float AutoCollectPercentage = 0.1f;
    public ResourceConfig[] ResourcesConfigs;
    public Sprite[] ResourcesSpites;

    public Transform ResourcesParent;
    public ResourcesController ResourcePrefab;
    public TapText TapTextPrefab;

    public Transform CoinIcon;
    public Text GoldInfo;
    public Text AutoCollectInfo;

    private List<ResourcesController> _activeResources = new List<ResourcesController>();
    private float _collectSecond;
    private List<TapText> _tapTextPool = new List<TapText>();

    public double TotalGold { get; private set; }

    private void Start()
    {
        AddAllResources();
    }

    private void Update()
    {
        _collectSecond += Time.unscaledDeltaTime;
        if(_collectSecond >= 1f)
        {
            CollectPerSecond();
            _collectSecond = 0f;
        }

        CheckResourceCost();

        CoinIcon.transform.localScale = Vector3.LerpUnclamped(CoinIcon.transform.localScale, Vector3.one * 2f, 0.15f);
        CoinIcon.transform.Rotate(0f, 0f, Time.deltaTime * -100f);
    }

    private void AddAllResources()
    {
        bool showResources = true;
        foreach (ResourceConfig config in ResourcesConfigs)
        {
            GameObject obj = Instantiate(ResourcePrefab.gameObject, ResourcesParent, false);
            ResourcesController resources = obj.GetComponent<ResourcesController>();

            resources.SetCofig(config);
            obj.gameObject.SetActive(showResources);
            if(showResources && !resources.IsUnlocked)
            {
                showResources = false;
            }
            _activeResources.Add(resources);
        }
    }

    public void ShowNextResource()
    {
        foreach(ResourcesController resources in _activeResources)
        {
            if (!resources.gameObject.activeSelf)
            {
                resources.gameObject.SetActive(true);
                break;
            }
        }
    }

    private void CollectPerSecond()
    {
        double output = 0;
        foreach(ResourcesController resources in _activeResources)
        {
            if (resources.IsUnlocked)
            {
                output = +resources.GetOutput();
            }
        }
        output *= AutoCollectPercentage;
        AutoCollectInfo.text = $"Auto Collect: {output.ToString("F1")}/second";
        AddGold(output);
    }

    public void AddGold(double value)
    {
        TotalGold += value;
        GoldInfo.text = $"Gold:{TotalGold.ToString("0")}";
    }

    public void CollectByTap(Vector3 tapPoisition, Transform parent)
    {
        double output = 0;
        foreach (ResourcesController resources in _activeResources)
        {
            if (resources.IsUnlocked)
            {
                output = +resources.GetOutput();
            }
        }

        TapText tapText = GetOrCreateTapText();
        tapText.transform.SetParent(parent, false);
        tapText.transform.position = tapPoisition;

        tapText.Text.text = $"+{output.ToString("0")}";
        tapText.gameObject.SetActive(true);
        CoinIcon.transform.localScale = Vector3.one * 1.75f;
        AddGold(output);
    }

    private TapText GetOrCreateTapText()
    {
        TapText tapText = _tapTextPool.Find(t => !t.gameObject.activeSelf);
        if(tapText == null)
        {
            tapText = Instantiate(TapTextPrefab).GetComponent<TapText>();
            _tapTextPool.Add(tapText);
        }
        return tapText;
    }

    private void CheckResourceCost()
    {
        foreach(ResourcesController resources in _activeResources)
        {
            bool isBuyable = false;
            if (resources.IsUnlocked)
            {
                isBuyable = TotalGold >= resources.GetUpgradeCost();
            }
            else
            {
                isBuyable = TotalGold >= resources.GetUnlockCost();
            }
        }
    }
}

[System.Serializable] public struct ResourceConfig
{
    public string Name;
    public double UnlockCost;
    public double UpgradeCost;
    public double Output;
}
