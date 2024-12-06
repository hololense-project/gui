using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Threading.Tasks;

public class MenuManager : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform buttonContainer;
    public ObjectLoader objectLoader;

    private List<string> objectNames = new List<string> { "Object1", "Object2", "Object3" }; // Przykładowe nazwy obiektów

    private void Start()
    {
        GenerateMenu();
    }

    private void GenerateMenu()
    {
        foreach (string objectName in objectNames)
        {
            GameObject button = Instantiate(buttonPrefab, buttonContainer);
            button.GetComponentInChildren<Text>().text = objectName;
            button.GetComponent<Button>().onClick.AddListener(() => OnObjectButtonClicked(objectName));
        }
    }

    private void OnObjectButtonClicked(string objectName)
    {
        objectLoader.LoadObjectFromServer(objectName);
    }
}