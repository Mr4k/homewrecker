using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(HandTool))]
public class FirstPersonPlayer : MonoBehaviour
{
    public FirstPersonCharacterController Character;
    public Transform CameraTransform;
    public float LookSensitivity = 2f;
    private float _pitch;

    public List<BaseTool> tools = new List<BaseTool>();
    private int activeToolIndex = 0;

    public TextMeshProUGUI ToolNameText;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        tools.AddRange(GetComponents<BaseTool>());
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
            Cursor.lockState = CursorLockMode.Locked;

        Vector2 look = Cursor.lockState == CursorLockMode.Locked
            ? new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * LookSensitivity
            : Vector2.zero;

        FirstPersonInputs inputs = new FirstPersonInputs
        {
            Move = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")),
            LookYaw = look.x,
            Jump = Input.GetKeyDown(KeyCode.Space),
        };
        Character.SetInputs(ref inputs);

        _pitch = Mathf.Clamp(_pitch - look.y, -89f, 89f);
        CameraTransform.localRotation = Quaternion.Euler(_pitch, 0f, 0f);

        activeToolIndex = activeToolIndex % tools.Count;
        tools[activeToolIndex].ActiveToolUpdate(CameraTransform);

        ToolNameText.text = tools[activeToolIndex].GetName();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            activeToolIndex -= 1;
            if (activeToolIndex < 0)
            {
                activeToolIndex += tools.Count;
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            activeToolIndex += 1;
            activeToolIndex = activeToolIndex % tools.Count;
        }
    }

    private void FixedUpdate()
    {
        activeToolIndex = activeToolIndex % tools.Count;
        tools[activeToolIndex].ActiveToolFixedUpdate(Character);
    }
}
