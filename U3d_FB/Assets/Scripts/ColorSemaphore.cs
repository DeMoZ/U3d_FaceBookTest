using UniRx;
using UnityEngine;
using UnityEngine.UI;

public enum SemaphoreColor
{
    Red,
    Yellow,
    Green
}

public class ColorSemaphore : MonoBehaviour
{
    public struct Ctx
    {
        public ReactiveCommand<SemaphoreColor> onSetColor;
    }
    
    [SerializeField] private Image _g;
    [SerializeField] private Image _y;
    [SerializeField] private Image _r;

    private Ctx _ctx;

    public void SetCtx(Ctx ctx)
    {
        _ctx = ctx;
        _ctx.onSetColor.Subscribe(SetColor).AddTo(this);
    }
    
    private void SetColor(SemaphoreColor color)
    {
        _g.gameObject.SetActive(color == SemaphoreColor.Green);
        _y.gameObject.SetActive(color == SemaphoreColor.Yellow);
        _r.gameObject.SetActive(color == SemaphoreColor.Red);
    }
}