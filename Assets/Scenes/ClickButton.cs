using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private Image _img;
    [SerializeField] private Sprite _default, _pressed;
    [SerializeField] private AudioClip _compressClip, _uncompressClip;
    [SerializeField] private AudioSource _source;

    private RectTransform _rectTransform;
    private float _originalHeight;

    void Start()
    {
        _rectTransform = _img.GetComponent<RectTransform>();
        _originalHeight = _rectTransform.sizeDelta.y;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _img.sprite = _pressed;
        _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _originalHeight - 5);
        _source.PlayOneShot(_compressClip);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        _img.sprite = _default;
        _rectTransform.sizeDelta = new Vector2(_rectTransform.sizeDelta.x, _originalHeight);
        _source.PlayOneShot(_uncompressClip);
    }
}
