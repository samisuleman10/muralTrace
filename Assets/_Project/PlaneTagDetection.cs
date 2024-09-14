using UnityEngine;
using TMPro;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace MuralTrace
{
    [RequireComponent(typeof(ARPlane))]
    public class PlaneTagDetection : MonoBehaviour
    {
        [SerializeField] TMP_Text m_AlignmentText;

        [SerializeField] TMP_Text m_ClassificationText;

        [SerializeField] private GameObject traceImage;

        ARPlane m_Plane;

        void OnEnable()
        {
            m_Plane = GetComponent<ARPlane>();
            m_Plane.boundaryChanged += OnBoundaryChanged;
        }

        void OnDisable()
        {
            m_Plane.boundaryChanged -= OnBoundaryChanged;
        }

        void OnBoundaryChanged(ARPlaneBoundaryChangedEventArgs eventArgs)
        {
            m_ClassificationText.text = m_Plane.classification.ToString();
            m_AlignmentText.text = m_Plane.alignment.ToString();

            if (m_Plane.classification == PlaneClassification.Wall)
            {
                traceImage.SetActive(true);
                m_ClassificationText.gameObject.transform.parent.gameObject.SetActive(false);
                m_AlignmentText.gameObject.transform.parent.gameObject.SetActive(false);
            }
            else
            {
                traceImage.SetActive(false);
                m_ClassificationText.gameObject.transform.parent.gameObject.SetActive(true);
                m_AlignmentText.gameObject.transform.parent.gameObject.SetActive(true);
            }

            transform.position = m_Plane.center;
        }
    }
}