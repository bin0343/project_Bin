using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Equipment : MonoBehaviour
{
    public static Player_Equipment instance;
    private Player_Action playerAction;

    [Header("캐릭터 무기")]
    public WeaponCategory usableWeaponCategory = WeaponCategory.OneHandedSword; //캐릭터 전용 무기 종류

    [Header("Equipment Setup")]
    [SerializeField] private Transform weaponMountPoint;
    private GameObject currentWeaponObject;

    [Header("전투 (무기 표시) 설정")]
    public float combatCooldown = 5f; // 마지막 공격/피격 후 무기가 사라지기까지의 시간
    public bool isInCombat = false;
    private float combatTimer = 0f;

    //무기 사라짐 효과
    private Coroutine weaponFadeCoroutine;
    private Vector3 originalWeaponScale = Vector3.one;

    public ItemHolder[] equipmentSlots = new ItemHolder[System.Enum.GetValues(typeof(EquipmentType)).Length];

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(gameObject);
        instance = this;
        playerAction = GetComponent<Player_Action>();
    }

    private void Start()
    {
        InitializeEquipment();
    }

    private void Update()
    {
        if (isInCombat)
        {
            combatTimer -= Time.deltaTime;
            if (combatTimer <= 0f)
            {
                ExitCombatState();
            }
        }
    }

    private void InitializeEquipment()
    {
        Player_Stat stat = GetComponent<Player_Stat>();

        for (int i = 0; i < equipmentSlots.Length; i++)
        {
            // 슬롯에 아이템이 있고, 데이터가 유효하다면
            if (equipmentSlots[i] != null && equipmentSlots[i].ItemData != null)
            {
                Item_Equipment equipmentData = equipmentSlots[i].ItemData as Item_Equipment;
                if (equipmentData == null) continue;

                // 무기 프리팹 생성 (무기 슬롯인 경우 혹은 무기 프리팹이 있는 경우)
                if (equipmentData.weaponPrefab != null)
                {
                    // 기존 무기가 있다면 제거 (혹시 모를 중복 방지)
                    if (currentWeaponObject != null) Destroy(currentWeaponObject);

                    currentWeaponObject = Instantiate(equipmentData.weaponPrefab, weaponMountPoint);
                    originalWeaponScale = currentWeaponObject.transform.localScale;
                    Weapon_Player newWeaponController = currentWeaponObject.GetComponentInChildren<Weapon_Player>();
                    playerAction.SetCurrentWeapon(newWeaponController);

                    currentWeaponObject.SetActive(isInCombat);
                }

                // 스탯 적용
                if (stat != null)
                {
                    stat.AddEquipmentStat(STAT.Attack, equipmentData.attackBonus);
                }
            }
        }
    }

    public void EnterCombatState()
    {
        isInCombat = true;
        combatTimer = combatCooldown;

        if (currentWeaponObject != null)
        {
            // 사라지고 있던 중이었다면 멈춤
            if (weaponFadeCoroutine != null)
            {
                StopCoroutine(weaponFadeCoroutine);
                weaponFadeCoroutine = null;
            }

            currentWeaponObject.SetActive(true);

            currentWeaponObject.transform.localScale = originalWeaponScale;
        }
    }

    public void ExitCombatState()
    {
        isInCombat = false;

        if (currentWeaponObject != null && currentWeaponObject.activeInHierarchy)
        {
            if (weaponFadeCoroutine != null) StopCoroutine(weaponFadeCoroutine);
            weaponFadeCoroutine = StartCoroutine(FadeOutWeaponCoroutine());
        }
    }

    public void Equip(ItemHolder itemToEquip, SlotType sourceType, int sourceIndex)
    {
        if (itemToEquip == null || itemToEquip.ItemData.itemType != ITEMTYPE.Equipment) return;

        Item_Equipment equipmentData = itemToEquip.ItemData as Item_Equipment;
        if (equipmentData == null) return;

        if (equipmentData.weaponCategory != usableWeaponCategory)
        {
            Debug.LogWarning($"장착 실패: 이 캐릭터는 [{usableWeaponCategory}] 전용입니다. ({equipmentData.weaponCategory} 장착 불가)");

            if (UI_Manager.instance != null)
            {
                UI_Manager.instance.ShowMessage("이 캐릭터가 장착할 수 없는 무기 종류입니다.");
            }
            return; 
        }

        ItemHolder previouslyEquipped = UnEquip(EquipmentType.Weapon);
        int slotIndex = 0;

        equipmentSlots[slotIndex] = itemToEquip;

        if (sourceType == SlotType.INVENTORY)
        {
            Player_Inventory.instance.inventorySlots[sourceIndex] = previouslyEquipped;
        }
        else if (sourceType == SlotType.QUICKSLOT)
        {
            Player_Inventory.instance.quickSlots[sourceIndex] = previouslyEquipped;
        }

        if (equipmentData.weaponPrefab != null)
        {
            currentWeaponObject = Instantiate(equipmentData.weaponPrefab, weaponMountPoint);

            Weapon_Player newWeaponController = currentWeaponObject.GetComponentInChildren<Weapon_Player>();

            playerAction.SetCurrentWeapon(newWeaponController);
        }

        // 스탯 적용
        Player_Stat stat = GetComponent<Player_Stat>();
        stat.AddEquipmentStat(STAT.Attack, equipmentData.attackBonus);
        Debug.Log($"{equipmentData.itemName}을(를) 장착했습니다.");

        RefreshUI();
    }

    public ItemHolder UnEquip(EquipmentType slotToUnEquip)
    {
        // CHANGED: 함수가 호출될 때마다 Player_Stat을 직접 찾아옵니다.
        Player_Stat stat = GetComponent<Player_Stat>();
        if (stat == null)
        {
            Debug.LogError("UnEquip 실패: Player_Stat 컴포넌트를 찾을 수 없습니다!");
            return null;
        }

        int slotIndex = 0;
        ItemHolder itemToUnEquip = equipmentSlots[slotIndex];

        if (itemToUnEquip != null)
        {
            Item_Equipment equipmentData = itemToUnEquip.ItemData as Item_Equipment;
            if (equipmentData != null)
            {
                // 스탯 해제
                stat.RemoveEquipmentStat(STAT.Attack, equipmentData.attackBonus);
            }

            equipmentSlots[slotIndex] = null;
        }

        if (currentWeaponObject != null)
        {
            Destroy(currentWeaponObject);
            currentWeaponObject = null;
        }

        playerAction.SetCurrentWeapon(null);
        
        return itemToUnEquip;
    }

    private void RefreshUI()
    {
        if (UI_Manager.instance?.UI_Status?.uiEquipmentPanel != null)
        {
            UI_Manager.instance.UI_Status.uiEquipmentPanel.RefreshUI();
        }
        UI_WeaponTab weaponTab = FindObjectOfType<UI_WeaponTab>();
        if (weaponTab != null && weaponTab.gameObject.activeInHierarchy)
        {
            weaponTab.RefreshTab();
        }
    }

    private IEnumerator FadeOutWeaponCoroutine()
    {
        float duration = 0.2f; // 0.2초 동안 아주 빠르게 스르륵! (원하는 속도로 조절하세요)
        float elapsed = 0f;
        Vector3 startScale = currentWeaponObject.transform.localScale;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            // 시작 크기에서 Vector3.zero(크기 0)으로 부드럽게 변환
            currentWeaponObject.transform.localScale = Vector3.Lerp(startScale, Vector3.zero, elapsed / duration);
            yield return null; // 다음 프레임까지 대기
        }

        currentWeaponObject.SetActive(false);
        currentWeaponObject.transform.localScale = originalWeaponScale;
    }
}
