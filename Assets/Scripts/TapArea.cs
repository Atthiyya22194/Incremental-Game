using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Audio;

public class TapArea : MonoBehaviour, IPointerDownHandler
{
    public AudioSource sfxCoin;
    public void OnPointerDown(PointerEventData eventData)
    {
        GameManager.Instance.CollectByTap(eventData.position, transform);
        sfxCoin.Play();
    }
}
