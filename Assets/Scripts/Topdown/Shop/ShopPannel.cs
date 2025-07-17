using System.Collections;
using System.Collections.Generic;
using Kinnly;
using UnityEngine;
using UnityEngine.UI;

public class ShopPannel : MonoBehaviour
{
    public Text DescriptionText_Hp;
    public Text DescriptionText_Mp;
    public Text HPPotionCount_Shop;
    public Text MPPotionCount_Shop;
    public Text BuyPrice_Hp;
    public Text BuyPrice_Mp;
    public Text SellPrice_Hp;
    public Text SellPrice_Mp;
    public Text CurrentGold;
    public ItemData HpPotion;
    public ItemData MpPotion;
    private Inventory Inventory;
    private PlayerStat stat;

    public Button HpPotionPlusButton;
    public Button HpPotionMinusButton;
    public Button MpPotionPlusButton;
    public Button MpPotionMinusButton;

    void Start()
    {
        Inventory = FindObjectOfType<Inventory>();
        stat = FindObjectOfType<PlayerStat>();

        HpPotionPlusButton.onClick.AddListener(BuyHpPotion);
        HpPotionMinusButton.onClick.AddListener(SellHpPotion);
        MpPotionPlusButton.onClick.AddListener(BuyMpPotion);
        MpPotionMinusButton.onClick.AddListener(SellMpPotion);

        UpdateUI();
    }

    void BuyHpPotion()
    {
        if (stat.Gold >= HpPotion.BuyPrice)
        {
            stat.Gold -= HpPotion.BuyPrice;
            Inventory.AddItem(HpPotion);
            UpdateUI();
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }
    }

    void SellHpPotion()
    {
        int count = Inventory.GetItemCount(HpPotion);
        if (count > 0)
        {
            Inventory.RemoveItem(HpPotion, 1);
            stat.Gold += HpPotion.SellPrice;
            UpdateUI();
        }
        else
        {
            Debug.Log("판매할 아이템이 없습니다.");
        }
    }

    void BuyMpPotion()
    {
        if (stat.Gold >= MpPotion.BuyPrice)
        {
            stat.Gold -= MpPotion.BuyPrice;
            Inventory.AddItem(MpPotion);
            UpdateUI();
        }
        else
        {
            Debug.Log("골드가 부족합니다.");
        }
    }

    void SellMpPotion()
    {
        int count = Inventory.GetItemCount(MpPotion);
        if (count > 0)
        {
            Inventory.RemoveItem(MpPotion, 1);
            stat.Gold += MpPotion.SellPrice;
            UpdateUI();
        }
        else
        {
            Debug.Log("판매할 아이템이 없습니다.");
        }
    }

    void UpdateUI()
    {
        HPPotionCount_Shop.text = $"{Inventory.GetItemCount(HpPotion)}";
        MPPotionCount_Shop.text = $"{Inventory.GetItemCount(MpPotion)}";

        DescriptionText_Hp.text = HpPotion.itemName;
        DescriptionText_Mp.text = MpPotion.itemName;
        CurrentGold.text = $"현재 골드 : {stat.Gold}";

        BuyPrice_Hp.text = $"{HpPotion.BuyPrice}G";
        BuyPrice_Mp.text = $"{MpPotion.BuyPrice}G";
        SellPrice_Hp.text = $"{HpPotion.SellPrice}G";
        SellPrice_Mp.text = $"{MpPotion.SellPrice}G";
    }
}
