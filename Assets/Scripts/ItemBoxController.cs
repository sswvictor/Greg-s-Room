using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ItemBoxController : MonoBehaviour
{
    public GameObject buttonPrefab;    
    public Transform contentParent;     
    public int slotCount = 8;        
    
    public int columns = 2;             
    public float buttonWidth = 120f;    
    public float buttonHeight = 100f;  
    public float spacing = 20f;         

    public float columnSpacing = 40f; 

    public float startYPosition = 0f;   

    private List<GameObject> buttonInstances = new List<GameObject>();

    public void ShowItems(List<Sprite> icons, List<GameObject> prefabs)
    {
        ClearItems();

        for (int i = 0; i < slotCount; i++)
        {
            var buttonGO = Instantiate(buttonPrefab, contentParent, false);
            buttonInstances.Add(buttonGO);

            var rect = buttonGO.GetComponent<RectTransform>();
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0.5f, 1f);  
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);      

            rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            int row = i / columns;
            int col = i % columns;

            float totalWidth = buttonWidth * columns + columnSpacing * (columns - 1);
            float xStart = -totalWidth / 2f + buttonWidth / 2f;
            float xPos = xStart + col * (buttonWidth + columnSpacing);


            float yPos = -row * 250;

            rect.anchoredPosition = new Vector2(xPos, yPos);


            var button = buttonGO.GetComponent<ItemSlotController>();
            var image = buttonGO.GetComponent<Image>();

            if (i < icons.Count && i < prefabs.Count)
            {
                image.sprite = icons[i];
                button.modelPrefab = prefabs[i];
                image.enabled = true;
            }
            else
            {
                image.sprite = null;
                button.modelPrefab = null;
                image.enabled = false;
            }
        }
    }

    public void ShowButtons(List<GameObject> buttonPrefabs)
    {
        ClearItems();

        for (int i = 0; i < buttonPrefabs.Count; i++)
        {
            var prefab = buttonPrefabs[i];
            if (prefab == null) continue;

            var buttonGO = Instantiate(prefab, contentParent, false);
            buttonInstances.Add(buttonGO);

            var rect = buttonGO.GetComponent<RectTransform>();
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
            rect.anchorMin = new Vector2(0.5f, 1f);  
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);   

            rect.sizeDelta = new Vector2(buttonWidth, buttonHeight);

            int row = i / columns;
            int col = i % columns;

            float totalWidth = buttonWidth * columns + columnSpacing * (columns - 1);
            float xStart = -totalWidth / 2f + buttonWidth / 2f;
            float xPos = xStart + col * (buttonWidth + columnSpacing);

            float yPos = -row * 250;

            rect.anchoredPosition = new Vector2(xPos, yPos);
        }

        gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void ClearItems()
    {
        foreach (var go in buttonInstances)
        {
            Destroy(go);
        }
        buttonInstances.Clear();
    }
}
