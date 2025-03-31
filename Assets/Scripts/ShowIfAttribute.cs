using UnityEngine;

public class ShowIfAttribute : PropertyAttribute
{
	public string conditionField;

	public ShowIfAttribute(string conditionField)
	{
		this.conditionField = conditionField;
	}
}