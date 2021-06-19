using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonController : MonoBehaviour {

	[SerializeField] Button button;
	[SerializeField] Transform buttonTransform;

	[SerializeField] AudioSource soundOnSelect;
	[SerializeField] AudioSource soundOnClick;

	[SerializeField] GameObject selectionIcon;

	[SerializeField] AnimationCurve selectScaleCurve;
	[SerializeField] AnimationCurve selectionRotationCurve;
	[SerializeField] AnimationCurve clickScaleCurve;
	[SerializeField] AnimationCurve selectionIconCurve;

	bool isSelected = false;
	float changeSelectedTimer = 100;
	float selectedTimer = 0;
	float onClickTimer = 100;

	private Vector3 selectionIconPosition;

	void Start() {
		selectionIconPosition = selectionIcon.transform.localPosition;

		selectionIcon.gameObject.SetActive(false);

		button.onClick.AddListener(() => {
			soundOnClick.Play();

			onClickTimer = 0;
		});
	}

	void Update() {
		{
			bool isNowSelected = EventSystem.current.currentSelectedGameObject == button.gameObject;

			if (!isSelected && isNowSelected) {
				soundOnSelect.Play();

				changeSelectedTimer = 0;

				selectedTimer = 0;

				selectionIcon.gameObject.SetActive(true);
			} else if (isSelected && !isNowSelected) {
				buttonTransform.localRotation = Quaternion.identity;

				selectionIcon.transform.localPosition = selectionIconPosition;

				selectionIcon.gameObject.SetActive(false);
			}
			isSelected = isNowSelected;
		}

		{
			if (onClickTimer < clickScaleCurve.keys[clickScaleCurve.length - 1].time) {
				buttonTransform.localScale = Vector3.one * clickScaleCurve.Evaluate(onClickTimer);
				onClickTimer += Time.deltaTime;
			} else if (changeSelectedTimer < selectScaleCurve.keys[selectScaleCurve.length - 1].time) {
				buttonTransform.localScale = Vector3.one * selectScaleCurve.Evaluate(changeSelectedTimer);
				changeSelectedTimer += Time.deltaTime;
			}

			if (isSelected) {
				buttonTransform.localRotation = Quaternion.AngleAxis(selectionRotationCurve.Evaluate(selectedTimer), Vector3.forward);

				selectedTimer += Time.deltaTime;
				selectedTimer %= selectionRotationCurve.keys[selectionRotationCurve.length - 1].time;

				selectionIcon.transform.localPosition = selectionIconPosition + Vector3.right * selectionIconCurve.Evaluate(selectedTimer);
			}
		}

	}
}
