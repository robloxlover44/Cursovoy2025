using UnityEngine;
using static LeanTween;
public class PanelController : MonoBehaviour
{
    public RectTransform leftPanel, rightPanel;
    public Collider2D leftTrigger, rightTrigger;

    public float slideDistance = 200f; // Насколько выезжает панель
    public float slideSpeed = 0.5f; // Скорость анимации

    private Vector2 leftPanelStartPos, rightPanelStartPos;
    private bool isLeftOpen = false;
    private bool isRightOpen = false;

    private void Start()
    {
        // Запоминаем стартовые позиции
        leftPanelStartPos = leftPanel.anchoredPosition;
        rightPanelStartPos = rightPanel.anchoredPosition;
        
        // Убираем панели за экран
        leftPanel.anchoredPosition = leftPanelStartPos - new Vector2(slideDistance, 0);
        rightPanel.anchoredPosition = rightPanelStartPos + new Vector2(slideDistance, 0);
    }

    private void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(mousePos);

        if (hit == leftTrigger && !isLeftOpen)
        {
            isLeftOpen = true;
            LeanTween.moveX(leftPanel, leftPanelStartPos.x, slideSpeed).setEaseOutExpo();
        }
        else if (hit != leftTrigger && isLeftOpen)
        {
            isLeftOpen = false;
            LeanTween.moveX(leftPanel, leftPanelStartPos.x - slideDistance, slideSpeed).setEaseInExpo();
        }

        if (hit == rightTrigger && !isRightOpen)
        {
            isRightOpen = true;
            LeanTween.moveX(rightPanel, rightPanelStartPos.x, slideSpeed).setEaseOutExpo();
        }
        else if (hit != rightTrigger && isRightOpen)
        {
            isRightOpen = false;
            LeanTween.moveX(rightPanel, rightPanelStartPos.x + slideDistance, slideSpeed).setEaseInExpo();
        }
    }
}
