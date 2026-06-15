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

    private GameObject currentUIWeaponObject; // 현재 띄워진 3D 모델을 기억할 변수

    private bool isPreviewMode = false;
    private ItemHolder previewItemHolder;
    private int previewInventoryIndex;

    public void RefreshTab()
    {
        if (Player_Equipment.instance == null) return;

        ItemHolder targetHolder = null;

        // 상태에 따라 보여줄 아이템 결정 (미리보기 중이면 미리보기 아이템, 아니면 장착 중인 아이템)
        if (isPreviewMode && previewItemHolder != null)
        {
            targetHolder = previewItemHolder;
        }
        else
        {
            targetHolder = Player_Equipment.instance.equipmentSlots[0];
        }

        if (targetHolder != null && targetHolder.ItemData != null)
        {
            Item_Equipment weaponData = targetHolder.ItemData as Item_Equipment;
            if (weaponData != null)
            {
                weaponIcon.sprite = weaponData.itemIcon;
                weaponIcon.gameObject.SetActive(true);
                weaponNameText.text = weaponData.itemName;
                currentAttackText.text = $"공격력: {weaponData.attackBonus}";
                nextAttackText.text = $"->  {weaponData.attackBonus + 15}";
                descriptionText.text = weaponData.itemDescription;

                Update3DModel(weaponData.weaponPrefab);
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
        if (Player_Equipment.instance == null) return;

        ItemHolder currentWeapon = Player_Equipment.instance.equipmentSlots[0];

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
            Player_Equipment.instance.Equip(previewItemHolder, SlotType.INVENTORY, previewInventoryIndex);

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
}