using UnityEngine;

public class SkillRangePreview : MonoBehaviour
{
    [Header("Preview Target")]
    public Animator modelAnimator;
    public Skill_InstantAttack skill;

    [Header("Gizmo Option")]
    public bool drawGizmo = true;
    public Color gizmoColor = new Color(1f, 0f, 0f, 0.25f);
    public Color wireColor = Color.red;

    private void OnDrawGizmos()
    {
        if (!drawGizmo) return;
        if (skill == null) return;

        Transform modelTransform = null;

        if (modelAnimator != null)
        {
            modelTransform = modelAnimator.transform;
        }
        else
        {
            Animator foundAnimator = GetComponentInChildren<Animator>();
            if (foundAnimator != null)
                modelTransform = foundAnimator.transform;
        }

        if (modelTransform == null) return;

        Vector3 center = modelTransform.position + modelTransform.TransformDirection(skill.spawnOffset);
        Quaternion rotation = modelTransform.rotation;

        Matrix4x4 oldMatrix = Gizmos.matrix;

        Gizmos.matrix = Matrix4x4.TRS(center, rotation, Vector3.one);

        Gizmos.color = gizmoColor;
        Gizmos.DrawCube(Vector3.zero, skill.hitboxSize);

        Gizmos.color = wireColor;
        Gizmos.DrawWireCube(Vector3.zero, skill.hitboxSize);

        Gizmos.matrix = oldMatrix;
    }
}