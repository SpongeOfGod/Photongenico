using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;

public class DisconnectFeedback : MonoBehaviour
{
    [HideInInspector] public PhotonView PhotonView;
    [SerializeField] private float TimeBetweenFeedback;
    [SerializeField] private TextVisualEffects Prefab;
    [SerializeField] private Transform Holder;
    private Queue<string> UIFeedback = new();
    private Coroutine FeedbackCoroutine;

    private void Awake()
    {
        PhotonView = GetComponent<PhotonView>();
    }

    void Update()
    {
        if (FeedbackCoroutine == null && UIFeedback.Count > 0) 
            FeedbackCoroutine = StartCoroutine(FeedbackRoutine());
    }

    [PunRPC]
    public void AddFeedback(string message) => UIFeedback.Enqueue(message);

    IEnumerator FeedbackRoutine() 
    {
        while (UIFeedback.Count > 0)
        {
            var item = Instantiate(Prefab, Holder);
            string message = UIFeedback.Dequeue();
            item.Text.text = message;
            item.gameObject.SetActive(true);
            yield return new WaitForSeconds(TimeBetweenFeedback);
        }

        FeedbackCoroutine = null;
    }
}
