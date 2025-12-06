using System.Collections.Generic;
using UnityEngine;

public class TextBoxShowHide : MonoBehaviour
{
    private List<GameObject> textBoxContainers = new();

    void Update()
    {
        var boxList = GameObject.FindGameObjectsWithTag("TutorialTextBox");

        foreach (GameObject box in boxList) {
            if (!textBoxContainers.Contains(box)) {
                textBoxContainers.Add(box);
            }
        }
    }

    public void HideTutorialBoxes() {
        foreach (GameObject box in textBoxContainers) {
            box.SetActive(false);
        }
    }

    public void ShowTutorialBoxes() {
        foreach (GameObject box in textBoxContainers) {
            box.SetActive(true);
        }
    }
}
