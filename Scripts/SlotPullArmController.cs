using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class SlotPullArmController : MonoBehaviour
{
    [SerializeField] private SlotArmButtonController buttonController;
    [SerializeField] private Transform pinTransform;
    [SerializeField] private float offset;

    [SerializeField] private UpgradeMenuController upgradeMenuController;
    [SerializeField] private RerollButton rerollButton;

    [SerializeField] private PlayerController playerController;
    [SerializeField] private GameObject costGroup;
    [SerializeField] private TextMeshProUGUI costText;

    private bool startPullAnimation = false;

    void OnDisable()
    {
        costGroup.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (buttonController.isHolding)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Vector3 direction = mousePosition - pinTransform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            pinTransform.rotation = Quaternion.Euler(0, 0, angle + offset);

            if (angle + offset < -45f && !upgradeMenuController.animationStarted)
            {
                DoReroll();
            }
        }

        if (buttonController.isSelected)
        {
            if (playerController.dashButtonPressed && !upgradeMenuController.animationStarted)
            {
                startPullAnimation = true;
                playerController.dashButtonPressed = false;
                DoReroll();
            }
        }

        if (startPullAnimation)
        {
            Quaternion currentRot = pinTransform.rotation;
            Quaternion endRot = Quaternion.Euler(0, 0, -55f);
            pinTransform.rotation = Quaternion.Lerp(currentRot, endRot, 0.03f);

            if (Mathf.Abs(Quaternion.Dot(currentRot, endRot)) >= 1 - 0.01f) startPullAnimation = false;
        }
        else
        {
            Quaternion currentAngle = pinTransform.rotation;
            Quaternion desiredAngle = Quaternion.Euler(0, 0, 10f);
            pinTransform.rotation = Quaternion.Lerp(currentAngle, desiredAngle, 0.05f);
        }
    }

    private void DoReroll()
    {
        costGroup.SetActive(true);
        costText.text = rerollButton.cost.ToString();

        rerollButton.StartReroll();
    }
}
