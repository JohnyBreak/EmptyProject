using UnityEngine;

namespace Common.Canvases.External
{
    public interface IMainCanvas
    {
        Transform GetTransform();
        Canvas GetCanvas();
    }
}