using UnityEngine;
using UnityEngine.UI;

public class TracePointSpawner : MonoBehaviour
{
    public RectTransform canvasRect;
    public GameObject fadePointPrefab;
    public float spawnInterval = 0.05f;

    private float spawnTimer = 0f;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            spawnTimer += Time.deltaTime;

            if (spawnTimer >= spawnInterval)
            {
                Vector2 pos;
                RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    canvasRect,
                    Input.mousePosition,
                    null,
                    out pos
                );

                GameObject dot = Instantiate(fadePointPrefab, canvasRect);
                dot.GetComponent<RectTransform>().anchoredPosition = pos;

                spawnTimer = 0f;
            }
        }
        else
        {
            spawnTimer = spawnInterval;
        }
    }
}