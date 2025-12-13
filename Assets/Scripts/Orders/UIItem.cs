using UnityEngine;

public class UIItem : MonoBehaviour
{
    // Selectable GameObject above
    public GameObject up;
    // Selectable GameObject below
    public GameObject down;
    // Selectable GameObject to the left
    public GameObject left;
    // Selectable GameObject to the right
    public GameObject right;
    // Reference to the order menu script, used by UIController to call public methods
    public ProductOrderingMenu menu;
    // Type of box the button represents, set to empty string on everything else
    public string boxType; 
}
