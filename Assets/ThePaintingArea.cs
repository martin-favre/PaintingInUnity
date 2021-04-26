using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ThePaintingArea : MonoBehaviour
{
    private Object LoadPrefab(string path)
    {
        Object obj = Resources.Load(path) as Object;
        if (obj == null)
        {
            Debug.LogWarning("Could not load resource " + path);
        }
        return obj;
    }
    Texture2D texture;
    Image image;
    bool mouseDown = false;
    public Vector2Int brushSize = new Vector2Int(5, 5);
    Vector2Int lastPosition;
    bool lastPositionSet = false;
    Color[] arr;
    RectTransform rectTransform;

    List<Texture2D> previousTexts;

    void Start()
    {
        image = GetComponent<Image>();
        arr = new Color[brushSize.x * brushSize.y];
        SetColor(Color.black);

        if (ColorPickerSingle.Instance)
        {
            SetColor(ColorPickerSingle.Instance.CurrentColor);
            ColorPickerSingle.Instance.onValueChanged.AddListener(color => SetColor(color));
        }
        previousTexts = new List<Texture2D>();
        SpawnNewTexture();
    }

    private void SetColor(Color color)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            arr[i] = color;
        }
    }

    void SpawnNewTexture()
    {
        Texture2D text = (LoadPrefab("Ruta") as Texture2D);
        texture = new Texture2D(text.width, text.height);
        texture.SetPixels(text.GetPixels());
        texture.Apply();
        image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 1);
        rectTransform = GetComponent<RectTransform>();
        previousTexts.Clear();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            mouseDown = true;
            Texture2D previousTexture = new Texture2D(texture.width, texture.height);
            previousTexture.SetPixels(texture.GetPixels());
            previousTexture.Apply();
            previousTexts.Add(previousTexture);
            if (previousTexts.Count > 100)
            {
                previousTexts.RemoveAt(0);
            }

        }
        if (Input.GetMouseButtonUp(0))
        {
            mouseDown = false;
            lastPositionSet = false;
        }

        if (Input.GetMouseButtonUp(1))
        {
            if (previousTexts.Count > 0)
            {
                texture = previousTexts[previousTexts.Count - 1];
                previousTexts.RemoveAt(previousTexts.Count - 1);
                image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0), 1);
            }
        }

        if (Input.GetKeyDown(KeyCode.K))
        {
            SpawnNewTexture();
        }

        if (mouseDown)
        {
            Vector2Int mousePos = GetMousePos();
            print(mousePos);

            PaintUntilCursor(mousePos);
            PaintArea(mousePos);
            texture.Apply();
            lastPositionSet = true;
            lastPosition = mousePos;
        }
    }

    Vector2Int GetMousePos()
    {
        Vector3 mousePosV3 = Input.mousePosition;
        var screenPoint = Camera.main.ViewportToScreenPoint(mousePosV3);

        Vector2Int mousePos = new Vector2Int(Mathf.RoundToInt(mousePosV3.x), Mathf.RoundToInt(mousePosV3.y));
        Vector2 localPoint;
        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, Input.mousePosition, null, out localPoint);

        int px = (int)(((localPoint.x - rectTransform.rect.x) * texture.width) / rectTransform.rect.width);
        int py = (int)(((localPoint.y - rectTransform.rect.y) * texture.height) / rectTransform.rect.height);
        return new Vector2Int(px, py);
    }

    void PaintUntilCursor(Vector2Int newPos)
    {
        int avoidInfLoopCntr = 0;
        if (lastPositionSet && lastPosition != newPos)
        {
            Vector2 diff = newPos - lastPosition;
            Vector2 dir = diff.normalized;
            Vector2 currentPos = lastPosition;
            while (diff.magnitude > 1.5f)
            {
                avoidInfLoopCntr++;
                currentPos += dir;
                PaintArea(new Vector2Int(Mathf.RoundToInt(currentPos.x), Mathf.RoundToInt(currentPos.y)));
                diff = newPos - currentPos;
                if (avoidInfLoopCntr > 1000)
                {
                    Debug.LogWarning("Error horror get me outta here");
                    break;
                }
            }
        }

    }

    void PaintArea(Vector2Int pos)
    {
        if (pos.x > 0 && pos.y > 0 && (pos.x + brushSize.x) < texture.width && (pos.y + brushSize.y) < texture.height)
        {
            texture.SetPixels(pos.x, pos.y, brushSize.x, brushSize.y, arr);
        }

    }
}
