using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private EventSystem sys;
    private UIItem current;
    private float time;
    private bool i;

    void Start()
    {
        sys = EventSystem.current;
        current = sys.firstSelectedGameObject.GetComponent<UIItem>();
        time = 0;
        i = false;
    }

    void FixedUpdate()
    {
        time += Time.fixedDeltaTime;
        if (time >= 0.5) i = true;
    }

    public void Navigate(Vector2 input)
    {
        if (input.x == 0 && input.y == 0){}
        else sys.SetSelectedGameObject(null);
        if (input.x > 0)
        {
            if (current.right)
            {
                sys.SetSelectedGameObject(current.right);
                current = current.right.GetComponent<UIItem>();
            } else
            {
                current.gameObject.GetComponent<Slider>().value++;
            }
        }
        if (input.x < 0) 
        {
            if (current.left)
            {
                sys.SetSelectedGameObject(current.left);
                current = current.left.GetComponent<UIItem>();
            } else
            {
                current.gameObject.GetComponent<Slider>().value--;
            }
        }
        
        if (input.y > 0 && current.up) 
        {
            sys.SetSelectedGameObject(current.up);
            current = current.up.GetComponent<UIItem>();
        }
        if (input.y < 0 && current.down)
        {
            sys.SetSelectedGameObject(current.down);
            current = current.down.GetComponent<UIItem>();
        }
    }

    public void Submit(bool input)
    {
        // Guard against null selection or menu
        if (!i)
            return;
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

        // If this UIItem represents a box type selection, update the menu selection
        if (!string.IsNullOrEmpty(current.boxType))
        {
            current.menu.UpdateBoxType(current.boxType);
        }
        else
        {
            // Distinguish between an "Add to Order" button and an "Order/Place Order" button.
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
            }
            else
            {
                // Fallback: call PlaceOrder for action items if we can't determine exactly
                current.menu.PlaceOrder();
                Debug.LogWarning($"UIController.Submit: ambiguous action for '{current.gameObject.name}', falling back to PlaceOrder().");
            }
        }

        // reset debounce
        i = false;
        time = 0;
    }

    public void Cancel(bool input)
    {
        // handled outside of here, easier to just monitor this event than send this class a bunch of data it uses once
        // handled specifically in NoahOrderHandlerTrigger.cs, ln 36 (in Update()).
    }
}
