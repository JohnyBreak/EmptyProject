using UnityEngine;

namespace Common.Canvases.External
{
    public class MainCanvas : MonoBehaviour, IMainCanvas
    {
        private Canvas m_Canvas;

        public Transform GetTransform()
        {
            return GetCanvas().transform;
        }

        public Canvas GetCanvas()
        {
            if (m_Canvas == null)
            {
                m_Canvas = GetComponent<Canvas>();
            }

            return m_Canvas;
        }
    }
}