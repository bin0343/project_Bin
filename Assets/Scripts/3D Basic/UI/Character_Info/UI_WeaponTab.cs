using UnityEngine;
using UnityEngine.UI;

public class UI_WeaponTab : MonoBehaviour
{
    [Header("무기 정보 UI")]
    public Image weaponIcon;
    public Text weaponNameText;
    public Text currentAttackText;
    public Text nextAttackText;
    public Text descriptionText;

    [Header("3D 무기 생성 위치")]
    public Transform uiWeaponMountPoint;

    [Header("무기 교체/선택 패널")]
    public GameObject weaponSelectionPanel;
    public GameObject changeButton;       // 기본 상태일 때 보이는 [교체] 버튼
    public GameObject confirmEquipButton; // 미리보기 상태일 때 보이는 [장착 확정] 버튼

    private GameObject currentUIWeaponObject; 

    private bool isPreviewMode = false;
    private ItemHolder previewItemHolder;
    private int previewInventoryIndex;

    private Character_Data targetCharacterData;

    public void RefreshTab(Character_Data characterData = null)
    {
        if (characterData != null)
        {
            targetCharacterData = characterData;
        }

        CharacterStatus cStatus = Character_Manager.Instance.GetCharacterStatus(targetCharacterData.characterID);
        Player_Equipment activeEquip = GetEquipmentOfCharacter(targetCharacterData);

        ItemHolder targetHolder = null;

        if (isPreviewMode && previewItemHolder != null)
        {
            targetHolder = previewItemHolder;
        }
        else
        {
            if (activeEquip != null)
            {
                targetHolder = activeEquip.equipmentSlots[0];
                if (cStatus != null) cStatus.equippedWeapon = targetHolder; // 최신화 데이터 백업
            }
            else if (cStatus != null)
            {
                targetHolder = cStatus.equippedWeapon;
            }
        }

        if (targetHolder != null && targetHolder.ItemData != null)
        {
            Item_Equipment weaponData = targetHolder.ItemData as Item_Equipment;
            if (weaponData != null)
            {
                weaponIcon.sprite = weaponData.itemIcon;
                weaponIcon.gameObject.SetActive(true);
                string refineText = targetHolder.refinementStage > 1 ? $" <color=yellow>+{targetHolder.refinementStage - 1}</color>" : "";
                weaponNameText.text = $"Lv.{targetHolder.weaponLevel} {weaponData.itemName}{refineText}";
                int finalAttack = targetHolder.GetTotalWeaponAttack();
                currentAttackText.text = $"공격력: {finalAttack}";
                if (nextAttackText != null) nextAttackText.gameObject.SetActive(false);
                descriptionText.text = weaponData.itemDescription;

                GameObject previewPrefab = weaponData.weaponPreviewPrefab != null ? weaponData.weaponPreviewPrefab : weaponData.weaponPrefab;

                Update3DModel(previewPrefab);
            }
        }
        else
        {
            // 장착된 무기가 없을 때 (미리보기 중이 아닐 때만)
            weaponIcon.gameObject.SetActive(false);
            weaponNameText.text = "장착된 무기 없음";
            currentAttackText.text = "공격력: 0";
            nextAttackText.text = "-";
            descriptionText.text = "무기를 장착해 주세요.";
            Update3DModel(null);
        }

        if (changeButton != null) changeButton.SetActive(!isPreviewMode);
        if (confirmEquipButton != null) confirmEquipButton.SetActive(isPreviewMode);
    }

    private void Update3DModel(GameObject prefabToSpawn)
    {
        if (uiWeaponMountPoint == null) return;

        if (currentUIWeaponObject != null)
        {
            Destroy(currentUIWeaponObject);
            currentUIWeaponObject = null;
        }

        if (prefabToSpawn != null)
        {
            currentUIWeaponObject = Instantiate(prefabToSpawn, uiWeaponMountPoint);
            currentUIWeaponObject.transform.localPosition = Vector3.zero;
            currentUIWeaponObject.transform.localRotation = Quaternion.identity; // 각도 정면 초기화

            Weapon_Player wp = currentUIWeaponObject.GetComponentInChildren<Weapon_Player>();
            if (wp != null) Destroy(wp);

            Collider col = currentUIWeaponObject.GetComponentInChildren<Collider>();
            if (col != null) Destroy(col);
        }
    }

    public void OnClickEnhanceWeapon()
    {
        Player_Equipment activeEquip = GetActiveEquipment();
        if (activeEquip == null) return;

        ItemHolder currentWeapon = activeEquip.equipmentSlots[0];

        if (currentWeapon != null && currentWeapon.ItemData != null)
        {
            if (UI_WeaponEnhancement.instance != null)
            {
                UI_WeaponEnhancement.instance.OpenEnhancementScreen(currentWeapon);
            }
            else
            {
                Debug.LogError("UI_WeaponEnhancement.instance가 씬에 없습니다! 패널이 비활성화 되어 있어도 Awake가 실행되게 하려면 최상위 캔버스를 껐다 켜보세요.");
            }
        }
        CloseSelectionPanel();
    }

    public void OnClickChangeWeapon()
    {
        if (weaponSelectionPanel != null)
        {
            weaponSelectionPanel.SetActive(true);
        }
    }

    public void ShowPreview(ItemHolder holder, int invIndex)
    {
        isPreviewMode = true;
        previewItemHolder = holder;
        previewInventoryIndex = invIndex;
        RefreshTab(); 
    }

    public void OnClickConfirmEquip()
    {
        if (isPreviewMode && previewItemHolder != null)
        {
            Player_Equipment activeEquip = GetActiveEquipment();
            CharacterStatus cStatus = Character_Manager.Instance.GetCharacterStatus(targetCharacterData.characterID);

            if (activeEquip != null)
            {
                activeEquip.Equip(previewItemHolder, SlotType.INVENTORY, previewInventoryIndex);
                if (cStatus != null) cStatus.equippedWeapon = activeEquip.equipmentSlots[0];
            }
            else
            {
                if (cStatus != null && Player_Inventory.Instance != null)
                {
                    if (previewInventoryIndex >= 0 && previewInventoryIndex < Player_Inventory.Instance.inventorySlots.Count)
                    {
                        ItemHolder oldWeapon = cStatus.equippedWeapon;
                        ItemHolder newWeapon = Player_Inventory.Instance.inventorySlots[previewInventoryIndex];

                        cStatus.equippedWeapon = newWeapon;

                        if (oldWeapon != null && oldWeapon.ItemData != null)
                        {
                            Player_Inventory.Instance.inventorySlots[previewInventoryIndex] = oldWeapon;
                        }
                        else
                        {
                            Player_Inventory.Instance.inventorySlots.RemoveAt(previewInventoryIndex);
                        }

                        Player_Inventory.Instance.CleanUpInventory();
                        Player_Inventory.Instance.RefreshAllUI();
                    }
                }
            }

                CloseSelectionPanel();
        }
    }

    public void CloseSelectionPanel()
    {
        isPreviewMode = false;
        previewItemHolder = null;

        if (weaponSelectionPanel != null)
        {
            weaponSelectionPanel.SetActive(false);
        }
        RefreshTab(); // 장착 중인 원래 무기로 되돌려서 다시 그리기
    }

    private Player_Equipment GetActiveEquipment()
    {
        return GetEquipmentOfCharacter(targetCharacterData);
    }

    private Player_Equipment GetEquipmentOfCharacter(Character_Data characterData)
    {
        if (characterData == null || BattleManager.Instance == null || BattleManager.Instance.SpawnedCharacters == null) return null;

        foreach (GameObject charObj in BattleManager.Instance.SpawnedCharacters)
        {
            if (charObj != null)
            {
                Character_Stat stat = charObj.GetComponent<Character_Stat>();
                if (stat != null && stat.characterData != null && stat.characterData.characterID == characterData.characterID)
                {
                    return charObj.GetComponent<Player_Equipment>();
                }
            }
        }

        return null;
    }
}