using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Reflection;

public class InputBridge : MonoBehaviour
{

    public MonoBehaviour currentController;
    public PlayerInput playerInput;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    public void SetController(MonoBehaviour controller)
    {
        currentController = controller;
        if (playerInput == null) return;

        // Subscribe to all actions in all action maps
        foreach (var map in playerInput.actions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                // Exclude "forklift interact" from all subscriptions
                if (string.Equals(action.name, "interact", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(map.name, "Forklift", StringComparison.OrdinalIgnoreCase))
                {
                    // undo earlier performed subscription and skip adding others
                    continue;
                }

                // Subscribe to other phases for all other actions
                action.performed += OnInputAction;
                action.canceled += OnInputAction;
            }
        
        }
    }

    public void ClearController()
    {
        foreach (var map in playerInput.actions.actionMaps)
        {
            foreach (var action in map.actions)
            {
                action.performed -= OnInputAction;
            }
        }
        currentController = null;
    }

    // This is a generic input callback from PlayerInput
    public void OnInputAction(InputAction.CallbackContext context)
    {
        if (currentController == null) return;

        // Use the action name as the method name
        string methodName = context.action.name;

        CallMethodIfExists(methodName, context);
    }

    // General reflection helper
    private void CallMethodIfExists(string methodName, InputAction.CallbackContext context)
    {

        Type controllerType = currentController.GetType();

        // Look for public instance methods
        MethodInfo method = controllerType.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public);
        if (method == null)
        {
            Debug.LogWarning($"{controllerType.Name} does not have a method named {methodName}");
            return;
        }

        // Get method parameters
        var parameters = method.GetParameters();
        object[] args = new object[parameters.Length];

        for (int i = 0; i < parameters.Length; i++)
        {
            var paramType = parameters[i].ParameterType;

            if (paramType == typeof(InputAction.CallbackContext))
                args[i] = context;
            else if (paramType == typeof(float))
                args[i] = context.ReadValue<float>();
            else if (paramType == typeof(Vector2))
                args[i] = context.ReadValue<Vector2>();
            else
                args[i] = null; // unsupported type
        }

        try
        {
            method.Invoke(currentController, args);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error invoking {methodName} on {controllerType.Name}: {e}");
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
