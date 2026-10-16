using UnityEngine;

public class MinimapIconFollow : MonoBehaviour
{
    public Transform target;

    private void LateUpdate()
    {
        if (BattleManager.instance != null)
        {
            GameObject activeCharacter = BattleManager.instance.GetActiveCharacter();

            if (activeCharacter != null)
            {
                Player_Action action = activeCharacter.GetComponent<Player_Action>();

                if (action != null && action.animator != null)
                {
                    target = action.animator.transform;
                }
                else
                {
                    target = activeCharacter.transform;
                }
            }
        }

        if (target == null) return;

        transform.position = new Vector3(target.position.x, transform.position.y, target.position.z);

        transform.rotation = Quaternion.Euler(0f, target.eulerAngles.y + 180f, 0f);
    }
}