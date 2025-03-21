using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
	public TMP_Text text;

	public void Show(string message, Color color)
	{
		text.text = message;
		text.color = color;

		transform.DOMoveY(transform.position.y + 1f, 1f).SetEase(Ease.OutQuad);
		text.DOFade(0f, 1f).OnComplete(() => Destroy(gameObject));
	}
}