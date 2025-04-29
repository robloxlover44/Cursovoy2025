using UnityEngine;
using UnityEngine.UI; // ��� ������ � UI

public class ResourceButton : MonoBehaviour
{
    public enum ResourceType { Money, Shards }
    public ResourceType resourceType;
    public int amount = 1; // ���������� ��������
    private bool hasBeenClicked = false; // ����, ����� ���������� ������ ���� ���

    private void Start()
    {
        Button button = GetComponent<Button>();

        if (button == null)
        {
            Debug.LogError("[ResourceButton] ������: ������ ����� �� �� ������!");
            return;
        }

        button.onClick.AddListener(GiveResource);
    }

    private void GiveResource()
    {
        if (hasBeenClicked) return; // ���� ��� �������� � �������

        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("[ResourceButton] ������: PlayerDataManager.Instance == null!");
            return;
        }

        switch (resourceType)
        {
            case ResourceType.Money:
                PlayerDataManager.Instance.AddMoney(amount);
                break;
            case ResourceType.Shards:
                PlayerDataManager.Instance.AddShards(amount);
                break;
        }

        Debug.Log($"[ResourceButton] {amount} {resourceType} ���������!");
        hasBeenClicked = true; // ������ ������ ������
    }
}