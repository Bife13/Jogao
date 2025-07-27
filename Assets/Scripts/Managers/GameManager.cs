using UnityEngine;

[RequireComponent(typeof(CombatManager))]
[RequireComponent(typeof(CombatUIManager))]
[RequireComponent(typeof(TurnManager))]
[RequireComponent(typeof(UnitManager))]
public class GameManager : MonoBehaviour
{
	private static GameManager _instance;
	public CombatManager combatManager;
	public CombatUIManager combatUIManager;
	public TurnManager turnManager;
	public UnitManager unitManager;
	public ItemSelection itemSelection;

	public static GameManager Instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = FindFirstObjectByType<GameManager>();
				if (_instance == null)
				{
					GameObject singletonObject = new GameObject("CombatManager");
					_instance = singletonObject.AddComponent<GameManager>();
				}
			}

			return _instance;
		}
	}

	private void Awake()
	{
		if (_instance == null)
		{
			_instance = this;
			DontDestroyOnLoad(gameObject); // Optionally preserve this object between scenes
		}
		else
		{
			Destroy(gameObject); // If a duplicate exists, destroy this instance
		}

		combatManager = GetComponent<CombatManager>();
		combatUIManager = GetComponent<CombatUIManager>();
		turnManager = GetComponent<TurnManager>();
		unitManager = GetComponent<UnitManager>();
		itemSelection = GetComponent<ItemSelection>();
	}

	public void InitializeGameManager()
	{
		combatManager.Initialize();
		combatUIManager.Initialize();
		turnManager.Initialize();
		itemSelection.Initialize();
	}
}