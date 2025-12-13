using System.Collections.Generic;
using UnityEngine;

public class TextBoxShowHide : MonoBehaviour
{
    // List of all the containers of the text boxes, used to show and hide them
    private List<GameObject> textBoxContainers = new();

    // Ensures all tutorial text boxes are in the list, since they are a part of the warehouse prefab
    void Update()
    {
        var boxList = GameObject.FindGameObjectsWithTag("TutorialTextBox");

        foreach (GameObject box in boxList) {
            if (!textBoxContainers.Contains(box)) {
                textBoxContainers.Add(box);
            }
        }
    }

    // Hides all tutorial boxes by setting them inactive
    public void HideTutorialBoxes() {
        foreach (GameObject box in textBoxContainers) {
            box.SetActive(false);
        }
    }

    // Shows all tutorial boxes by setting them active again
    public void ShowTutorialBoxes() {
        foreach (GameObject box in textBoxContainers) {
            box.SetActive(true);
        }
    }
}
