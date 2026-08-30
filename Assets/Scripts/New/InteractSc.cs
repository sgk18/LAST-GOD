using UnityEngine;
using UnityEngine.InputSystem;

public class InteractSc : MonoBehaviour
{

    [Header("Interactives")]
    private bool isInteracted;
    [SerializeField] private InputActionReference interact;
    private GameObject player;

    [Header("GOs")]
    [SerializeField] private GameObject onGO;
    [SerializeField] private GameObject offGO;

    private bool status = false;

    void Awake()
    {
        player = GameObject.FindWithTag("Player");
    }
    void OnEnable()
    {
        interact.action.Enable();
    }

    void Update()
    {
        //isInteracted = true and false when? .. distance between player and go is less than 2
        isInteracted = Vector3.Distance(player.transform.position, transform.position) < 2f;

        //Calling
        if (interact.action.WasPressedThisFrame())
        {
            InteractIt();
        }
    }

    public void InteractIt()
    {
        if (isInteracted)
        {
            status = !status;
            if (status)
            {
                On();
            }
            else
            {
                Off();
            }
        }
        else
        {
            Debug.Log("Not Interacted to anything");
        }
    }
    private void On()
    {
        onGO.SetActive(true);
        offGO.SetActive(false);
    }
    private void Off()
    {
        offGO.SetActive(true);
        onGO.SetActive(false);
    }
}
