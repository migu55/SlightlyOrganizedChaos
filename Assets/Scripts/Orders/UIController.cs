using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    // Reference to the current event system, so that the custom navigation can change what the selected GameObject is
    private EventSystem sys;
    // Currently selected UI item
    private UIItem current;
    // Time tracker, used for preventing a bouncing input that causes a single input to be read as multiple
    private float time;
    // Bool set to true when the time tracker passes 0.5seconds, used as shorthand instead of if (time >= 0.5)
    private bool i;

    // Initialization; set time to 0 and i to false, find the EventSystem and set the current selected object to the
    // first selected one that I set in the Inspector to the A type box button
    void Start()
    {
        sys = EventSystem.current;
        current = sys.firstSelectedGameObject.GetComponent<UIItem>();
        time = 0;
        i = false;
    }

    // Just increments time and sets i to true if time >= 0.5
    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time >= 0.5) i = true;
    }

    // Custom navigation implementation compatible with the InputBridge setup
    public void Navigate(Vector2 input)
    {
        // If no input do nothing, else set selected object to null so it can be changed later
        if (input.x == 0 && input.y == 0){}
        else sys.SetSelectedGameObject(null);

        // Handles left/right input. If a left or right side selectable object exists, change to that.
        // Otherwise we assume we're on the slider and adjust its value accordingly
        if (input.x > 0)
        {
            SFXController.Instance.PlayClip(SFXController.Instance.uiInput, true);
            if (current.right)
            {
                sys.SetSelectedGameObject(current.right);
                current = current.right.GetComponent<UIItem>();
            } else
            {
                current.gameObject.GetComponent<Slider>().value++;
                sys.SetSelectedGameObject(current.gameObject);
            }
        }
        if (input.x < 0) 
        {
            SFXController.Instance.PlayClip(SFXController.Instance.uiInput, true);
            if (current.left)
            {
                sys.SetSelectedGameObject(current.left);
                current = current.left.GetComponent<UIItem>();
            } else
            {
                current.gameObject.GetComponent<Slider>().value--;
                sys.SetSelectedGameObject(current.gameObject);
            }
        }
        
        // Handles up/down input, selecting the appropriate object.
        if (input.y > 0 && current.up) 
        {
            SFXController.Instance.PlayClip(SFXController.Instance.uiInput, true);
            sys.SetSelectedGameObject(current.up);
            current = current.up.GetComponent<UIItem>();
        }
        if (input.y < 0 && current.down)
        {
            SFXController.Instance.PlayClip(SFXController.Instance.uiInput, true);
            sys.SetSelectedGameObject(current.down);
            current = current.down.GetComponent<UIItem>();
        }
    }

    // Handles confirm/submit input, pressing the appropriate button within this custom system
    public void Submit(bool input)
    {
        // Guard against bouncing input
        if (!i)
            return;
        // Guard against null selection or menu
        if (current == null)
        {
            Debug.LogWarning("UIController.Submit: no current UIItem selected.");
            return;
        }
        if (current.menu == null)
        {
            Debug.LogWarning($"UIController.Submit: selected UIItem '{current.gameObject.name}' has no ProductOrderingMenu assigned.");
            return;
        }

        SFXController.Instance.PlayClip(SFXController.Instance.uiSelect);
        // If this UIItem represents a box type selection, update the menu selection
        if (!string.IsNullOrEmpty(current.boxType))
        {
            current.menu.UpdateBoxType(current.boxType);
        }
        else
        {
            // Distinguish between an "Add to Order" button, an "Order/Place Order" button, and a "Clear Order" button.
            string goName = (current.gameObject != null) ? current.gameObject.name.ToLowerInvariant() : "";
            if (goName.Contains("add"))
            {
                // e.g. GameObject named "AddButton"
                current.menu.AddToOrder();
            }
            else if (goName.Contains("order") || goName.Contains("place"))
            {
                // e.g. GameObject named "OrderButton" or "PlaceOrder"
                current.menu.PlaceOrder();
                current = sys.firstSelectedGameObject.GetComponent<UIItem>();
            }
            else if (goName.Contains("clear"))
            {
                // e.g. GameObject named "ClearOrder" or "ClearButton"
                current.menu.ClearOrder();
            }
            else
            {
                // Fallback: call PlaceOrder for action items if we can't determine exactly
                current.menu.PlaceOrder();
                current = sys.firstSelectedGameObject.GetComponent<UIItem>();
                Debug.LogWarning($"UIController.Submit: ambiguous action for '{current.gameObject.name}', falling back to PlaceOrder().");
            }
        }

        // Reset debounce
        i = false;
        time = 0;
    }

    // Required to be placed here for the system to work in Unity, but handled outside as mentioned here
    public void Cancel(bool input)
    {
        // handled outside of here, easier to just monitor this event than send this class a bunch of data it uses once
        // handled specifically in NoahOrderHandlerTrigger.cs, ln 44 to 54 (in Update()).
    }
}
