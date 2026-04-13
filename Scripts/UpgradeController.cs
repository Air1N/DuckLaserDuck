using UnityEngine;
using System.Collections;

public class UpgradeController : MonoBehaviour
{
	public GameObject openAnimPrefab;

	public float menuDelay;

	private bool pickedUp = false;

	public bool playOpenAnimation;

	private GameObject upgradeMenu;
	[SerializeField] private MainMenuController mainMenuController;

	private void Start()
	{
		upgradeMenu = FindObjectOfType<UpgradeMenuController>(includeInactive: true).gameObject;
		mainMenuController = FindObjectOfType<MainMenuController>();
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (col.gameObject.CompareTag("Player"))
			PickUp();
	}

	private void PickUp()
	{
		if (playOpenAnimation && !pickedUp)
			Instantiate(openAnimPrefab, transform.position, Quaternion.identity, transform.Find("Paint Layer 3(pasted)"));
		StartCoroutine(WaitForAnimationThenDestroy());
		pickedUp = true;
	}

	private IEnumerator WaitForAnimationThenDestroy()
	{

		Time.timeScale = 0f;
		yield return new WaitForSecondsRealtime(menuDelay);

		upgradeMenu.SetActive(true);
		mainMenuController.OpenUpgradeMenu(); // Select the upgrade menu button for controller/keyboard navigation

		yield return new WaitUntil(() => !upgradeMenu.activeSelf);
		Destroy(gameObject);

		Time.timeScale = 1f;
	}
}