using UnityEngine;

public class Billboard : MonoBehaviour
{
	[SerializeField]
	private Camera m_Camera;

	[SerializeField]
	private bool ignoreY;

    private void Start()
    {
		m_Camera = Camera.main;
    }

    private void LateUpdate()
	{
		if (m_Camera == null) return;
		if (ignoreY)
		{
			Vector3 forward = m_Camera.transform.forward;
			forward.y = 0f;
			base.transform.rotation = Quaternion.LookRotation(forward);
		}
		else if (!(m_Camera == null))
		{
			Vector3 worldPosition = base.transform.position + m_Camera.transform.rotation * Vector3.forward;
			base.transform.LookAt(worldPosition, m_Camera.transform.rotation * Vector3.up);
		}
	}
}
