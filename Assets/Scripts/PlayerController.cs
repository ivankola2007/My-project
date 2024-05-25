using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public const string EQUIPE_NOT_SELECTED_TEXT = "EquipeNotSelected";
    private const float gravityScale = 9.8f, speedScale = 5f, jumpForce = 5f, turnSpeed = 90f;
    private const float hitScaleSpeed = 15f;
    private float verticalSpeed, mouseX, mouseY, currentCameraAngleX,hitLastTime;
    private int inversion = -1;
    [SerializeField]
    private CharacterController characterController;
    [SerializeField]
    public string itemYouCanEquipeName = EQUIPE_NOT_SELECTED_TEXT;
    private GameObject playerCamera;

    [SerializeField]
    private GameObject particleBlockObject;
    private GameObject  currentEquipedItem;

    [HideInInspector]
    public List<ItemData> inventoryItems, currentChestItems;
    private bool canMove = true;

    public static PlayerController instance;
    [SerializeField]
    private GameObject[] equipableItems;

    private void Awake()
    {
        instance = this; 
    }

    private void Start()
    {
        inventoryItems = new List<ItemData>();
        currentChestItems = new List<ItemData>();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        InventoryManager.instance.CreateItem(0, inventoryItems);
    }

    private void FixedUpdate()
    {
        if (canMove) Rotate();
    }

    private void Update()
    {
        if (canMove)
        {
            Move();
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, 5f))
            {
                if (Input.GetMouseButton(0))
                {
                    ObjectInteraction(hit.transform.gameObject);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            OpenInventory();
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            CloseInventoryPanel();
        }
    }

    private void Rotate()
    {
        mouseX = Input.GetAxis("Mouse X");
        mouseY = Input.GetAxis("Mouse Y");
        transform.Rotate(new Vector3(0f, mouseX * turnSpeed * Time.fixedDeltaTime, 0f));

        currentCameraAngleX += mouseY * Time.fixedDeltaTime * turnSpeed * inversion;
        currentCameraAngleX = Mathf.Clamp(currentCameraAngleX, -60f, 60f);
        playerCamera.transform.localEulerAngles = new Vector3(currentCameraAngleX, 0f, 0f);
    }

    private void Move()
    {
        Vector3 velocity = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        velocity = transform.TransformDirection(velocity) * speedScale;
        if (characterController.isGrounded)
        {
            verticalSpeed = 0f;
            if (Input.GetKeyDown(KeyCode.Space)) 
            {
                verticalSpeed = jumpForce;
            }
        }
        verticalSpeed -= gravityScale * Time.deltaTime;
        velocity.y = verticalSpeed;
        characterController.Move(velocity * Time.deltaTime);
    }

    private void Dig(Block block)
    {
        if(Time.time - hitLastTime > 1 / hitScaleSpeed)
        {
            currentEquipedItem.GetComponent<Animator>().SetTrigger("attack");
            hitLastTime = Time.time;
            block.health -= currentEquipedItem.GetComponent<Tool>().damageToBlock;
            GameObject particleObj = Instantiate(particleBlockObject, block.transform.position, Quaternion.identity);
            particleObj.GetComponent<ParticleSystemRenderer>().material = block.GetComponent<MeshRenderer>().material;

            if (block.health <= 0)
            {
                block.DestroyBehavior();
            }
        }
    }

    private void ObjectInteraction(GameObject currentObj)
    {
        switch (currentObj.tag)
        {
            case "Block":
                Dig(currentObj.GetComponent<Block>());
                break;
            case "Enemy":
                break;
            case "Chest":
                currentChestItems = currentObj.GetComponent<Chest>().chestItems;
                OpenChest();
                break;
        }
    }

    private void OpenInventory()
    {
        var inventoryManager = InventoryManager.instance;

        if (!inventoryManager.GetInventoryPanel().activeSelf) 
        {
            SwitchCursor(true, CursorLockMode.Confined);

            inventoryManager.GetInventoryPanel().SetActive(true);
            if (inventoryItems.Count > 0)
            {
                for (int i = 0; i < inventoryItems.Count; i++)
                {
                    inventoryManager.InstantiateItem(inventoryItems[i],
                        inventoryManager.GetInventoryContent().transform,
                        inventoryManager.inventorySlots);
                }
            }
        }
    }

    private void OpenChest()
    {
        var inventoryManager = InventoryManager.instance;        

        if (!inventoryManager.GetChestPanel().activeSelf)
        {
            SwitchCursor(true, CursorLockMode.Confined);
            inventoryManager.GetChestPanel().SetActive(true);
            for (int i = 0; i < currentChestItems.Count; i++)
            {
                inventoryManager.InstantiateItem(currentChestItems[i],
                    inventoryManager.GetChestContent().transform,
                    inventoryManager.currentChestSlots);
            }
        }
    }

    private void SwitchCursor(bool active, CursorLockMode lockMode) 
    {
        Cursor.visible = active;
        Cursor.lockState = lockMode;
        canMove = !active;
    }

    private void CloseInventoryPanel()
    {
        var inventoryManager = InventoryManager.instance;

        SwitchCursor(false, CursorLockMode.Locked);
        foreach (GameObject slot in inventoryManager.currentChestSlots)
        {
            Destroy(slot);
        }
        foreach (GameObject slot in inventoryManager.inventorySlots)
        {
            Destroy(slot);
        }
        inventoryManager.currentChestSlots.Clear();
        inventoryManager.inventorySlots.Clear();
        inventoryManager.GetChestPanel().SetActive(false);
        inventoryManager.GetInventoryPanel().SetActive(false);
    }

    private void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.name.StartsWith("mini"))
        {
            InventoryManager.instance.CreateItem(2, inventoryItems);
            Destroy(col.gameObject);
        }
    }
}
