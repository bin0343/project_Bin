using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AutoScrollToSelected : MonoBehaviour
{
    private ScrollRect scrollRect;
    private RectTransform contentPanel;
    private RectTransform scrollrectTransform;
    private RectTransform viewportRect;

    [Header("스크롤 속도")]
    public float scrollSpeed = 10f;

    private void Start()
    {
        scrollRect = GetComponent<ScrollRect>();
        contentPanel = scrollRect.content;
        scrollrectTransform = scrollRect.GetComponent<RectTransform>();
        viewportRect = scrollRect.viewport;
    }
    private void Update()
    {
        GameObject selected = EventSystem.current.currentSelectedGameObject;

        if (selected != null && selected.transform.IsChildOf(contentPanel))
        {
            UpdateScrollPosition(selected.GetComponent<RectTransform>());
        }
    }

    void UpdateScrollPosition(RectTransform target)
    {
        Vector3 viewportCenter = scrollrectTransform.position;      //스크롤 뷰의 중앙 지점
        Vector3 targetPos = target.position;        //타겟의 위치
        Vector3 difference = viewportCenter - targetPos; //두 지점의 차이
        difference.z = 0; //z축 차이는 무시

        if (difference.magnitude > 0.1f) //차이가 어느정도 이상일 때만 스크롤
        {
            //스크롤 뷰의 크기와 컨텐츠 패널의 크기를 고려하여 스크롤 양 계산
            float scrollAmount = difference.y / (contentPanel.rect.height - scrollrectTransform.rect.height);
            //현재 스크롤 위치에 스크롤 양을 더함
            float newScrollPos = Mathf.Clamp01(scrollRect.verticalNormalizedPosition + scrollAmount * scrollSpeed * Time.deltaTime);
            scrollRect.verticalNormalizedPosition = newScrollPos;

            contentPanel.position = Vector3.Lerp(contentPanel.position, difference + contentPanel.position, scrollSpeed * Time.unscaledDeltaTime);
        }

        /*if (target == null) return;

        // 1. 타겟을 중앙에 오게 하기 위한 목표 위치(World Space) 계산
        Vector3 viewportCenter = viewportRect.position;
        Vector3 targetPos = target.position;
        Vector3 difference = viewportCenter - targetPos;
        difference.z = 0;

        // 목표로 하는 Content의 World Position
        Vector3 desiredWorldPos = contentPanel.position + difference;

        // 2. 목표 위치를 Viewport의 Local Space로 변환 (계산 편의를 위해)
        Vector3 desiredLocalPos = viewportRect.InverseTransformPoint(desiredWorldPos);

        // 3. 클램핑(Clamping): 지도가 화면 밖으로 벗어나지 않도록 제한
        //    (Pivots, Anchors 설정에 상관없이 작동하도록 Rect 좌표계 사용)

        // 현재 Content와 Viewport의 크기
        float contentWidth = contentPanel.rect.width;
        float contentHeight = contentPanel.rect.height;
        float viewportWidth = viewportRect.rect.width;
        float viewportHeight = viewportRect.rect.height;

        // Content의 Pivot에 따른 로컬 좌표계 보정
        Vector2 contentPivot = contentPanel.pivot;

        // 목표 위치에서의 Content 좌/우/상/하 끝점 계산 (Local Space 기준)
        float newContentLeft = desiredLocalPos.x - (contentWidth * contentPivot.x);
        float newContentRight = desiredLocalPos.x + (contentWidth * (1 - contentPivot.x));
        float newContentBottom = desiredLocalPos.y - (contentHeight * contentPivot.y);
        float newContentTop = desiredLocalPos.y + (contentHeight * (1 - contentPivot.y));

        // Viewport의 좌/우/상/하 끝점 (Local Space 기준)
        // Viewport Rect는 (0,0)이 중심이 아닐 수 있으므로 rect.xMin/xMax 사용
        float viewLeft = viewportRect.rect.xMin;
        float viewRight = viewportRect.rect.xMax;
        float viewBottom = viewportRect.rect.yMin;
        float viewTop = viewportRect.rect.yMax;

        // 보정값 계산 (Clamping)
        float offsetX = 0f;
        float offsetY = 0f;

        // 가로 보정: 지도가 화면보다 작으면 중앙 정렬, 크면 빈공간 안 보이게
        if (contentWidth > viewportWidth)
        {
            // 왼쪽 빈공간이 보이면? (지도가 너무 오른쪽으로 감) -> 왼쪽으로 당김
            if (newContentLeft > viewLeft) offsetX = viewLeft - newContentLeft;
            // 오른쪽 빈공간이 보이면? (지도가 너무 왼쪽으로 감) -> 오른쪽으로 당김
            else if (newContentRight < viewRight) offsetX = viewRight - newContentRight;
        }

        // 세로 보정
        if (contentHeight > viewportHeight)
        {
            // 아래 빈공간이 보이면? -> 아래로 당김
            if (newContentBottom > viewBottom) offsetY = viewBottom - newContentBottom;
            // 위 빈공간이 보이면? -> 위로 당김
            else if (newContentTop < viewTop) offsetY = viewTop - newContentTop;
        }

        // 보정된 최종 목표 위치 (Local Space)
        Vector3 clampedLocalPos = desiredLocalPos + new Vector3(offsetX, offsetY, 0);

        // 4. 최종 이동 (World Space로 변환하여 적용)
        //    Time.unscaledDeltaTime을 사용하여 일시정지 상태에서도 부드럽게 이동
        Vector3 clampedWorldPos = viewportRect.TransformPoint(clampedLocalPos);

        if (Vector3.Distance(contentPanel.position, clampedWorldPos) > 0.1f)
        {
            contentPanel.position = Vector3.Lerp(
                contentPanel.position,
                clampedWorldPos,
                scrollSpeed * Time.unscaledDeltaTime
            );
        }*/
    }
}
