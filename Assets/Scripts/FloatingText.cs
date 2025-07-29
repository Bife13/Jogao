using DG.Tweening;
using TMPro;
using UnityEngine;

public class FloatingText : MonoBehaviour
{
	public TMP_Text text;

	[SerializeField]
	private float distanceToTravel = 1f;

	[SerializeField]
	private float travelDuration = 1f;

	[SerializeField]
	private float fadeDuration = 1.5f;

	public void Show(string message, Color color)
	{
		text.text = message;
		text.color = color;

		transform.DOMoveY(transform.position.y + distanceToTravel, travelDuration).SetEase(Ease.OutQuad);
		text.DOFade(0f, fadeDuration).OnComplete(() => Destroy(gameObject));
	}
}